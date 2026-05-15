using Avalonia.Media.Imaging;
using CodeWF.Modules.ToolFramework.Models;
using CronExpressionDescriptor;
using DiffPlex;
using Figgle;
using Figgle.Fonts;
using Markdig;
using NBitcoin;
using NUlid;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using OtpNet;
using PhoneNumbers;
using QRCoder;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Tomlyn;
using UAParser;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CodeWF.Modules.ToolFramework.Services;

public static partial class ToolAlgorithms
{
    private static readonly DefaultJsonTypeInfoResolver JsonTypeInfoResolver = new();
    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = JsonTypeInfoResolver
    };
    private static readonly JsonSerializerOptions CompactJsonOptions = new()
    {
        WriteIndented = false,
        TypeInfoResolver = JsonTypeInfoResolver
    };
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex SlugUnsafeRegex = new(@"[^a-zA-Z0-9\-_\s]", RegexOptions.Compiled);

    private static readonly Lazy<Dictionary<string, string>> OuiData = new(() =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(ReadResource("oui-data.json"), CompactJsonOptions) ?? []);

    private static readonly Lazy<Dictionary<string, MimeDbEntry>> MimeDb = new(() =>
        JsonSerializer.Deserialize<Dictionary<string, MimeDbEntry>>(ReadResource("mime-db.json"), CompactJsonOptions) ?? []);

    private static readonly Lazy<Dictionary<string, EmojiInfo>> EmojiData = new(() =>
        JsonSerializer.Deserialize<Dictionary<string, EmojiInfo>>(ReadResource("emoji-data.json"), CompactJsonOptions) ?? []);

    private static readonly Dictionary<string, string> CssColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["black"] = "#000000", ["silver"] = "#c0c0c0", ["gray"] = "#808080", ["white"] = "#ffffff",
        ["maroon"] = "#800000", ["red"] = "#ff0000", ["purple"] = "#800080", ["fuchsia"] = "#ff00ff",
        ["green"] = "#008000", ["lime"] = "#00ff00", ["olive"] = "#808000", ["yellow"] = "#ffff00",
        ["navy"] = "#000080", ["blue"] = "#0000ff", ["teal"] = "#008080", ["aqua"] = "#00ffff",
        ["orange"] = "#ffa500", ["transparent"] = "#00000000"
    };

    public static Task Base64FileAsync(ToolRunContext context, CancellationToken token)
    {
        var mode = context.Option("mode");
        if (mode == "File to Base64")
        {
            var file = context.Text("file");
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                context.SetText("result", "Choose an existing input file.");
                return Task.CompletedTask;
            }

            var bytes = File.ReadAllBytes(file);
            context.SetText("result", Convert.ToBase64String(bytes));
            return Task.CompletedTask;
        }

        var base64 = StripDataUrlPrefix(context.Text("base64").Trim());
        if (string.IsNullOrWhiteSpace(base64))
        {
            context.SetText("result", "Paste a base64 string.");
            return Task.CompletedTask;
        }

        var output = context.Text("save");
        var decoded = Convert.FromBase64String(PadBase64(base64));
        if (!string.IsNullOrWhiteSpace(output))
        {
            File.WriteAllBytes(output, decoded);
            context.SetText("result", $"Wrote {decoded.Length:N0} bytes to {output}");
        }
        else
        {
            context.SetText("result", $"Decoded {decoded.Length:N0} bytes. Choose a save path to write the file.");
        }

        return Task.CompletedTask;
    }

    public static Task Base64StringAsync(ToolRunContext context, CancellationToken token)
    {
        var input = context.Text("text");
        var urlSafe = context.Bool("urlSafe");
        if (context.Option("mode") == "Encode")
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            context.SetText("result", urlSafe ? MakeBase64UrlSafe(encoded) : encoded);
            return Task.CompletedTask;
        }

        var normalized = urlSafe ? UndoBase64UrlSafe(input.Trim()) : input.Trim();
        var bytes = Convert.FromBase64String(PadBase64(normalized));
        context.SetText("result", Encoding.UTF8.GetString(bytes));
        return Task.CompletedTask;
    }

    public static Task CaseConverterAsync(ToolRunContext context, CancellationToken token)
    {
        var words = SplitWords(context.Text("text")).ToList();
        string JoinPascal() => string.Concat(words.Select(CultureInfo.InvariantCulture.TextInfo.ToTitleCase));
        var camel = JoinPascal();
        if (camel.Length > 0)
        {
            camel = char.ToLowerInvariant(camel[0]) + camel[1..];
        }

        var lines = new[]
        {
            $"Lower case: {string.Join(' ', words).ToLowerInvariant()}",
            $"Upper case: {string.Join(' ', words).ToUpperInvariant()}",
            $"Sentence case: {SentenceCase(words)}",
            $"Title Case: {CultureInfo.InvariantCulture.TextInfo.ToTitleCase(string.Join(' ', words).ToLowerInvariant())}",
            $"camelCase: {camel}",
            $"PascalCase: {JoinPascal()}",
            $"snake_case: {string.Join('_', words).ToLowerInvariant()}",
            $"kebab-case: {string.Join('-', words).ToLowerInvariant()}",
            $"CONSTANT_CASE: {string.Join('_', words).ToUpperInvariant()}",
            $"dot.case: {string.Join('.', words).ToLowerInvariant()}",
        };
        context.SetText("result", string.Join(Environment.NewLine, lines));
        return Task.CompletedTask;
    }

    public static Task ColorConverterAsync(ToolRunContext context, CancellationToken token)
    {
        if (!TryParseColor(context.Text("color"), out var r, out var g, out var b, out var a))
        {
            context.SetText("result", "Unable to parse color. Try #3498db, rgb(52,152,219), hsl(204,70%,53%) or blue.");
            return Task.CompletedTask;
        }

        var (h, s, l) = RgbToHsl(r, g, b);
        var name = CssColors.FirstOrDefault(kv => string.Equals(kv.Value, $"#{r:x2}{g:x2}{b:x2}", StringComparison.OrdinalIgnoreCase)).Key;
        context.SetText("result", string.Join(Environment.NewLine,
            $"HEX: #{r:x2}{g:x2}{b:x2}{(a < 255 ? a.ToString("x2") : string.Empty)}",
            $"RGB: rgb({r}, {g}, {b})",
            $"RGBA: rgba({r}, {g}, {b}, {Math.Round(a / 255d, 3).ToString(CultureInfo.InvariantCulture)})",
            $"HSL: hsl({Math.Round(h)}, {Math.Round(s * 100)}%, {Math.Round(l * 100)}%)",
            $"CSS name: {(string.IsNullOrWhiteSpace(name) ? "(none)" : name)}"));
        return Task.CompletedTask;
    }

    public static Task DateTimeConverterAsync(ToolRunContext context, CancellationToken token)
    {
        var input = context.Text("date").Trim();
        var date = string.IsNullOrWhiteSpace(input)
            ? DateTimeOffset.Now
            : ParseDate(input, context.Option("format"));

        var unix = date.ToUnixTimeSeconds();
        var millis = date.ToUnixTimeMilliseconds();
        var excel = date.UtcDateTime.Subtract(new DateTime(1899, 12, 30, 0, 0, 0, DateTimeKind.Utc)).TotalDays;
        var objectId = $"{unix:x8}0000000000000000";
        context.SetText("result", string.Join(Environment.NewLine,
            $"Local: {date.LocalDateTime:yyyy-MM-dd HH:mm:ss zzz}",
            $"UTC: {date.UtcDateTime:yyyy-MM-dd HH:mm:ss}Z",
            $"ISO 8601: {date:O}",
            $"RFC 3339: {date.UtcDateTime:yyyy-MM-dd'T'HH:mm:ss'Z'}",
            $"RFC 7231: {date.UtcDateTime:R}",
            $"Unix timestamp: {unix}",
            $"Timestamp milliseconds: {millis}",
            $"Mongo ObjectID: {objectId}",
            $"Excel date/time: {excel.ToString("0.########", CultureInfo.InvariantCulture)}"));
        return Task.CompletedTask;
    }

    public static Task IntegerBaseConverterAsync(ToolRunContext context, CancellationToken token)
    {
        var fromBase = Math.Clamp(context.Int("fromBase"), 2, 36);
        var toBase = Math.Clamp(context.Int("toBase"), 2, 36);
        var value = Convert.ToInt64(context.Text("number").Trim(), fromBase);
        context.SetText("result", string.Join(Environment.NewLine,
            $"Decimal: {value}",
            $"Hexadecimal: {Convert.ToString(value, 16).ToUpperInvariant()}",
            $"Octal: {Convert.ToString(value, 8)}",
            $"Binary: {Convert.ToString(value, 2)}",
            $"Base {toBase}: {ConvertToBase(value, toBase)}",
            $"Base64 bytes: {Convert.ToBase64String(BitConverter.GetBytes(value))}"));
        return Task.CompletedTask;
    }

    public static Task JsonToTomlAsync(ToolRunContext context, CancellationToken token)
    {
        using var doc = JsonDocument.Parse(context.Text("json"));
        var model = JsonElementToPlain(doc.RootElement);
        context.SetText("result", TomlSerializer.Serialize(model, model?.GetType() ?? typeof(object)));
        return Task.CompletedTask;
    }

    public static Task JsonToXmlAsync(ToolRunContext context, CancellationToken token)
    {
        using var doc = JsonDocument.Parse(context.Text("json"));
        var root = new XElement("root", JsonElementToXml(doc.RootElement, "item"));
        context.SetText("result", root.ToString());
        return Task.CompletedTask;
    }

    public static Task JsonToYamlAsync(ToolRunContext context, CancellationToken token)
    {
        using var doc = JsonDocument.Parse(context.Text("json"));
        var model = JsonElementToPlain(doc.RootElement);
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        context.SetText("result", serializer.Serialize(model));
        return Task.CompletedTask;
    }

    public static Task ListConverterAsync(ToolRunContext context, CancellationToken token)
    {
        var lines = SplitLines(context.Text("list")).ToList();
        var operation = context.Option("operation");
        lines = operation switch
        {
            "Sort" => lines.OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).ToList(),
            "Reverse" => lines.AsEnumerable().Reverse().ToList(),
            "Lowercase" => lines.Select(x => x.ToLowerInvariant()).ToList(),
            "Uppercase" => lines.Select(x => x.ToUpperInvariant()).ToList(),
            "Unique" => lines.Distinct(StringComparer.CurrentCultureIgnoreCase).ToList(),
            "Transpose" => TransposeLines(lines),
            _ => lines
        };

        var prefix = context.Text("prefix");
        var suffix = context.Text("suffix");
        var truncate = context.Int("truncate");
        if (!string.IsNullOrEmpty(prefix) || !string.IsNullOrEmpty(suffix) || truncate > 0)
        {
            lines = lines.Select(line =>
            {
                var value = truncate > 0 && line.Length > truncate ? line[..truncate] : line;
                return $"{prefix}{value}{suffix}";
            }).ToList();
        }

        context.SetText("result", string.Join(Environment.NewLine, lines));
        return Task.CompletedTask;
    }

    public static Task MarkdownToHtmlAsync(ToolRunContext context, CancellationToken token)
    {
        context.SetText("result", Markdown.ToHtml(context.Text("markdown")));
        return Task.CompletedTask;
    }

    public static Task RomanNumeralAsync(ToolRunContext context, CancellationToken token)
    {
        var value = context.Text("value").Trim();
        context.SetText("result", context.Option("mode") == "Number to Roman"
            ? ToRoman(int.Parse(value, CultureInfo.InvariantCulture))
            : FromRoman(value).ToString(CultureInfo.InvariantCulture));
        return Task.CompletedTask;
    }

    public static Task TemperatureAsync(ToolRunContext context, CancellationToken token)
    {
        var celsius = ToCelsius((double)context.Number("value"), context.Option("unit"));
        var lines = new[]
        {
            $"Celsius: {celsius:0.###} C",
            $"Kelvin: {celsius + 273.15:0.###} K",
            $"Fahrenheit: {celsius * 9 / 5 + 32:0.###} F",
            $"Rankine: {(celsius + 273.15) * 9 / 5:0.###} R",
            $"Delisle: {(100 - celsius) * 3 / 2:0.###} De",
            $"Newton: {celsius * 33 / 100:0.###} N",
            $"Reaumur: {celsius * 4 / 5:0.###} Re",
            $"Romer: {celsius * 21 / 40 + 7.5:0.###} Ro",
        };
        context.SetText("result", string.Join(Environment.NewLine, lines));
        return Task.CompletedTask;
    }

    public static Task TextBinaryAsync(ToolRunContext context, CancellationToken token)
    {
        if (context.Option("mode") == "Text to Binary")
        {
            context.SetText("result", string.Join(' ', Encoding.UTF8.GetBytes(context.Text("text")).Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))));
            return Task.CompletedTask;
        }

        var bytes = context.Text("text").Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Select(binary => Convert.ToByte(binary, 2))
            .ToArray();
        context.SetText("result", Encoding.UTF8.GetString(bytes));
        return Task.CompletedTask;
    }

    public static Task NatoAsync(ToolRunContext context, CancellationToken token)
    {
        var words = new Dictionary<char, string>
        {
            ['a'] = "Alfa", ['b'] = "Bravo", ['c'] = "Charlie", ['d'] = "Delta", ['e'] = "Echo", ['f'] = "Foxtrot",
            ['g'] = "Golf", ['h'] = "Hotel", ['i'] = "India", ['j'] = "Juliett", ['k'] = "Kilo", ['l'] = "Lima",
            ['m'] = "Mike", ['n'] = "November", ['o'] = "Oscar", ['p'] = "Papa", ['q'] = "Quebec", ['r'] = "Romeo",
            ['s'] = "Sierra", ['t'] = "Tango", ['u'] = "Uniform", ['v'] = "Victor", ['w'] = "Whiskey", ['x'] = "X-ray",
            ['y'] = "Yankee", ['z'] = "Zulu"
        };
        context.SetText("result", string.Join(' ', context.Text("text").ToLowerInvariant().Select(c => words.GetValueOrDefault(c, c.ToString()))));
        return Task.CompletedTask;
    }

    public static Task UnicodeAsync(ToolRunContext context, CancellationToken token)
    {
        if (context.Option("mode") == "Text to Unicode")
        {
            context.SetText("result", string.Concat(context.Text("text").Select(c => $"\\u{(int)c:x4}")));
            return Task.CompletedTask;
        }

        context.SetText("result", Regex.Replace(context.Text("text"), @"\\u([0-9a-fA-F]{4})", match => ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString()));
        return Task.CompletedTask;
    }

    public static Task TomlToJsonAsync(ToolRunContext context, CancellationToken token)
    {
        var model = TomlSerializer.Deserialize<Dictionary<string, object?>>(context.Text("toml"));
        context.SetText("result", JsonSerializer.Serialize(model, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task TomlToYamlAsync(ToolRunContext context, CancellationToken token)
    {
        var model = TomlSerializer.Deserialize<Dictionary<string, object?>>(context.Text("toml"));
        var serializer = new SerializerBuilder().Build();
        context.SetText("result", serializer.Serialize(model));
        return Task.CompletedTask;
    }

    public static Task XmlToJsonAsync(ToolRunContext context, CancellationToken token)
    {
        var doc = XDocument.Parse(context.Text("xml"));
        context.SetText("result", JsonSerializer.Serialize(XmlElementToObject(doc.Root!), PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task YamlToJsonAsync(ToolRunContext context, CancellationToken token)
    {
        var deserializer = new DeserializerBuilder().Build();
        var obj = deserializer.Deserialize<object>(context.Text("yaml"));
        context.SetText("result", JsonSerializer.Serialize(obj, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task YamlToTomlAsync(ToolRunContext context, CancellationToken token)
    {
        var deserializer = new DeserializerBuilder().Build();
        var obj = deserializer.Deserialize<object>(context.Text("yaml"));
        context.SetText("result", TomlSerializer.Serialize(obj, obj?.GetType() ?? typeof(object)));
        return Task.CompletedTask;
    }

    public static Task ChmodAsync(ToolRunContext context, CancellationToken token)
    {
        var owner = Bits(context.Bool("ur"), context.Bool("uw"), context.Bool("ux"));
        var group = Bits(context.Bool("gr"), context.Bool("gw"), context.Bool("gx"));
        var other = Bits(context.Bool("or"), context.Bool("ow"), context.Bool("ox"));
        var symbolic = $"{Symbol(context.Bool("ur"), context.Bool("uw"), context.Bool("ux"))}{Symbol(context.Bool("gr"), context.Bool("gw"), context.Bool("gx"))}{Symbol(context.Bool("or"), context.Bool("ow"), context.Bool("ox"))}";
        context.SetText("result", $"Mode: {owner}{group}{other}{Environment.NewLine}Symbolic: {symbolic}{Environment.NewLine}Command: chmod {owner}{group}{other} <path>");
        return Task.CompletedTask;
    }

    public static Task CrontabAsync(ToolRunContext context, CancellationToken token)
    {
        var cron = context.Text("cron").Trim();
        var fields = cron.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var valid = fields.Length is 5 or 6;
        var description = valid ? ExpressionDescriptor.GetDescription(cron) : "Cron expression must have 5 or 6 fields.";
        context.SetText("result", $"Valid: {valid}{Environment.NewLine}Description: {description}");
        return Task.CompletedTask;
    }

    public static Task DockerComposeAsync(ToolRunContext context, CancellationToken token)
    {
        var tokens = ShellSplit(context.Text("command")).ToList();
        var runIndex = tokens.FindIndex(t => t.Equals("run", StringComparison.OrdinalIgnoreCase));
        if (runIndex >= 0)
        {
            tokens = tokens.Skip(runIndex + 1).ToList();
        }
        else if (tokens.FirstOrDefault()?.Equals("docker", StringComparison.OrdinalIgnoreCase) == true)
        {
            tokens = tokens.Skip(1).ToList();
        }

        var name = "app";
        var image = string.Empty;
        var ports = new List<string>();
        var volumes = new List<string>();
        var env = new List<string>();
        var command = new List<string>();

        for (var i = 0; i < tokens.Count; i++)
        {
            var tokenValue = tokens[i];
            string? Next() => i + 1 < tokens.Count ? tokens[++i] : null;
            switch (tokenValue)
            {
                case "-d":
                case "--rm":
                case "-it":
                    break;
                case "--name":
                    name = Next() ?? name;
                    break;
                case "-p":
                case "--publish":
                    ports.Add(Next() ?? string.Empty);
                    break;
                case "-v":
                case "--volume":
                    volumes.Add(Next() ?? string.Empty);
                    break;
                case "-e":
                case "--env":
                    env.Add(Next() ?? string.Empty);
                    break;
                default:
                    if (tokenValue.StartsWith("--name=", StringComparison.Ordinal))
                    {
                        name = tokenValue["--name=".Length..];
                    }
                    else if (tokenValue.StartsWith("-p", StringComparison.Ordinal) && tokenValue.Length > 2)
                    {
                        ports.Add(tokenValue[2..]);
                    }
                    else if (tokenValue.StartsWith("-v", StringComparison.Ordinal) && tokenValue.Length > 2)
                    {
                        volumes.Add(tokenValue[2..]);
                    }
                    else if (image.Length == 0 && !tokenValue.StartsWith('-', StringComparison.Ordinal))
                    {
                        image = tokenValue;
                    }
                    else
                    {
                        command.Add(tokenValue);
                    }

                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(image))
        {
            image = "image:latest";
        }

        var sb = new StringBuilder();
        sb.AppendLine("services:");
        sb.AppendLine($"  {SanitizeServiceName(name)}:");
        sb.AppendLine($"    image: {image}");
        if (ports.Count > 0)
        {
            sb.AppendLine("    ports:");
            foreach (var port in ports.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                sb.AppendLine($"      - \"{port}\"");
            }
        }
        if (volumes.Count > 0)
        {
            sb.AppendLine("    volumes:");
            foreach (var volume in volumes.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                sb.AppendLine($"      - \"{volume.Replace("\\", "\\\\")}\"");
            }
        }
        if (env.Count > 0)
        {
            sb.AppendLine("    environment:");
            foreach (var variable in env.Where(v => !string.IsNullOrWhiteSpace(v)))
            {
                var parts = variable.Split('=', 2);
                sb.AppendLine(parts.Length == 2 ? $"      {parts[0]}: \"{parts[1]}\"" : $"      - {variable}");
            }
        }
        if (command.Count > 0)
        {
            sb.AppendLine($"    command: {string.Join(' ', command.Select(QuoteYaml))}");
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task DockerImageTagParserAsync(ToolRunContext context, CancellationToken token)
    {
        var reference = context.Text("image").Trim();
        if (string.IsNullOrWhiteSpace(reference))
        {
            context.SetText("result", "Enter a Docker image reference.");
            return Task.CompletedTask;
        }

        var digest = string.Empty;
        var digestIndex = reference.IndexOf('@', StringComparison.Ordinal);
        if (digestIndex >= 0)
        {
            digest = reference[(digestIndex + 1)..];
            reference = reference[..digestIndex];
        }

        var lastSlash = reference.LastIndexOf('/');
        var lastColon = reference.LastIndexOf(':');
        var tag = lastColon > lastSlash ? reference[(lastColon + 1)..] : "latest";
        var path = lastColon > lastSlash ? reference[..lastColon] : reference;
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var registry = parts.Length > 1 && (parts[0].Contains('.') || parts[0].Contains(':') || parts[0].Equals("localhost", StringComparison.OrdinalIgnoreCase))
            ? parts[0]
            : "docker.io";
        var repositoryParts = registry == "docker.io" ? parts : parts.Skip(1).ToArray();
        var repository = repositoryParts.Length == 1 && registry == "docker.io"
            ? $"library/{repositoryParts[0]}"
            : string.Join('/', repositoryParts);

        context.SetText("result", string.Join(Environment.NewLine,
            $"Registry: {registry}",
            $"Repository: {repository}",
            $"Image name: {repositoryParts.LastOrDefault() ?? string.Empty}",
            $"Tag: {tag}",
            $"Digest: {(string.IsNullOrWhiteSpace(digest) ? "(none)" : digest)}"));
        return Task.CompletedTask;
    }

    public static Task EmailNormalizerAsync(ToolRunContext context, CancellationToken token)
    {
        var results = SplitLines(context.Text("emails"))
            .Select(NormalizeEmail)
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);
        context.SetText("result", string.Join(Environment.NewLine, results));
        return Task.CompletedTask;
    }

    public static Task GitMemoAsync(ToolRunContext context, CancellationToken token)
    {
        context.SetText("result", ReadResource("git-memo.content.md"));
        return Task.CompletedTask;
    }

    public static Task JsonDiffAsync(ToolRunContext context, CancellationToken token)
    {
        var left = PrettyJson(context.Text("left"));
        var right = PrettyJson(context.Text("right"));
        context.SetText("result", BuildLineDiff(left, right));
        return Task.CompletedTask;
    }

    public static Task JsonMinifyAsync(ToolRunContext context, CancellationToken token)
    {
        using var doc = JsonDocument.Parse(context.Text("json"));
        context.SetText("result", JsonSerializer.Serialize(doc.RootElement, CompactJsonOptions));
        return Task.CompletedTask;
    }

    public static Task JsonToCsvAsync(ToolRunContext context, CancellationToken token)
    {
        using var doc = JsonDocument.Parse(context.Text("json"));
        var rows = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray().ToList()
            : [doc.RootElement];
        var headers = rows
            .Where(row => row.ValueKind == JsonValueKind.Object)
            .SelectMany(row => row.EnumerateObject().Select(prop => prop.Name))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (headers.Count == 0)
        {
            headers.Add("value");
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers.Select(CsvEscape)));
        foreach (var row in rows)
        {
            var values = headers.Select(header =>
            {
                if (row.ValueKind == JsonValueKind.Object && row.TryGetProperty(header, out var value))
                {
                    return CsvEscape(JsonScalarToString(value));
                }

                return header == "value" ? CsvEscape(JsonScalarToString(row)) : string.Empty;
            });
            sb.AppendLine(string.Join(',', values));
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task CsvToJsonAsync(ToolRunContext context, CancellationToken token)
    {
        var rows = ParseDelimitedRows(context.Text("csv"), DelimiterFromOption(context.Option("delimiter")));
        if (rows.Count == 0)
        {
            context.SetText("result", "[]");
            return Task.CompletedTask;
        }

        if (context.Bool("header"))
        {
            var headers = MakeUniqueHeaders(rows[0]
                .Select((header, index) => string.IsNullOrWhiteSpace(header) ? $"column{index + 1}" : header.Trim())
                .ToList());
            var objects = rows.Skip(1)
                .Where(row => row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                .Select(row => headers.ToDictionary(
                    header => header,
                    header => row.ElementAtOrDefault(headers.IndexOf(header)) ?? string.Empty,
                    StringComparer.Ordinal));
            context.SetText("result", JsonSerializer.Serialize(objects, PrettyJsonOptions));
            return Task.CompletedTask;
        }

        context.SetText("result", JsonSerializer.Serialize(rows, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task CsvToMarkdownAsync(ToolRunContext context, CancellationToken token)
    {
        var rows = ParseDelimitedRows(context.Text("csv"), DelimiterFromOption(context.Option("delimiter")));
        context.SetText("result", FormatMarkdownTable(rows, null));
        return Task.CompletedTask;
    }

    public static Task DataUrlParserAsync(ToolRunContext context, CancellationToken token)
    {
        var value = context.Text("dataUrl").Trim();
        if (!value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            context.SetText("result", "Input must start with data:.");
            return Task.CompletedTask;
        }

        var comma = value.IndexOf(',');
        if (comma < 0)
        {
            context.SetText("result", "Data URL is missing the comma separator.");
            return Task.CompletedTask;
        }

        var metadata = value[5..comma];
        var payload = value[(comma + 1)..];
        var metadataParts = metadata.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var mediaType = metadataParts.FirstOrDefault(part => part.Contains('/')) ?? "text/plain";
        var isBase64 = metadataParts.Any(part => part.Equals("base64", StringComparison.OrdinalIgnoreCase));
        var charset = metadataParts.FirstOrDefault(part => part.StartsWith("charset=", StringComparison.OrdinalIgnoreCase)) ?? "(not specified)";
        var bytes = isBase64
            ? Convert.FromBase64String(PadBase64(payload.Trim()))
            : Encoding.UTF8.GetBytes(WebUtility.UrlDecode(payload));
        var preview = LooksTextual(mediaType)
            ? Encoding.UTF8.GetString(bytes.Take(4096).ToArray())
            : Convert.ToHexString(bytes.Take(256).ToArray()).ToLowerInvariant();

        context.SetText("result", string.Join(Environment.NewLine,
            $"Media type: {mediaType}",
            $"Charset: {charset}",
            $"Base64: {isBase64}",
            $"Payload bytes: {bytes.Length:N0}",
            "Preview:",
            preview));
        return Task.CompletedTask;
    }

    public static Task EnvToJsonAsync(ToolRunContext context, CancellationToken token)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in SplitLines(context.Text("env")))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.Ordinal))
            {
                line = line[7..].TrimStart();
            }

            var equals = line.IndexOf('=');
            if (equals <= 0)
            {
                continue;
            }

            var key = line[..equals].Trim();
            var value = StripOptionalQuotes(line[(equals + 1)..].Trim());
            values[key] = value;
        }

        context.SetText("result", JsonSerializer.Serialize(values, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task HexDumpAsync(ToolRunContext context, CancellationToken token)
    {
        var bytes = context.Option("mode") == "Hex"
            ? ParseHexBytes(context.Text("input"))
            : Encoding.UTF8.GetBytes(context.Text("input"));
        var bytesPerLine = Math.Clamp(context.Int("bytesPerLine"), 4, 64);
        context.SetText("result", FormatHexDump(bytes, bytesPerLine));
        return Task.CompletedTask;
    }

    public static Task HttpHeaderParserAsync(ToolRunContext context, CancellationToken token)
    {
        var headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var startLine = string.Empty;
        foreach (var line in SplitLines(context.Text("headers")))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var colon = line.IndexOf(':');
            if (colon <= 0)
            {
                startLine = string.IsNullOrWhiteSpace(startLine) ? line.Trim() : startLine;
                continue;
            }

            var name = line[..colon].Trim();
            var value = line[(colon + 1)..].Trim();
            if (!headers.TryGetValue(name, out var values))
            {
                values = [];
                headers[name] = values;
            }

            values.Add(value);
        }

        var payload = new
        {
            StartLine = startLine,
            Headers = headers.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Count == 1 ? (object)pair.Value[0] : pair.Value,
                StringComparer.OrdinalIgnoreCase)
        };
        context.SetText("result", JsonSerializer.Serialize(payload, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task IniToJsonAsync(ToolRunContext context, CancellationToken token)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var currentSection = "default";
        result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawLine in SplitLines(context.Text("ini")))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line[1..^1].Trim();
                result.TryAdd(currentSection, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator < 0)
            {
                separator = line.IndexOf(':');
            }

            if (separator <= 0)
            {
                continue;
            }

            result[currentSection][line[..separator].Trim()] = StripOptionalQuotes(line[(separator + 1)..].Trim());
        }

        if (result["default"].Count == 0)
        {
            result.Remove("default");
        }

        context.SetText("result", JsonSerializer.Serialize(result, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task JsonPathExtractorAsync(ToolRunContext context, CancellationToken token)
    {
        using var doc = JsonDocument.Parse(context.Text("json"));
        var element = ResolveJsonPath(doc.RootElement, context.Text("path"));
        context.SetText("result", JsonSerializer.Serialize(element, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task MarkdownTableGeneratorAsync(ToolRunContext context, CancellationToken token)
    {
        var delimiter = DelimiterFromOption(context.Option("delimiter"));
        var headers = ParseDelimitedRows(context.Text("headers"), delimiter).FirstOrDefault() ?? [];
        var rows = ParseDelimitedRows(context.Text("rows"), delimiter);
        if (headers.Count == 0)
        {
            headers = rows.FirstOrDefault() ?? [];
            rows = rows.Skip(1).ToList();
        }

        var allRows = new List<List<string>> { headers };
        allRows.AddRange(rows);
        context.SetText("result", FormatMarkdownTable(allRows, context.Option("alignment")));
        return Task.CompletedTask;
    }

    public static Task NanoidAsync(ToolRunContext context, CancellationToken token)
    {
        var alphabet = context.Text("alphabet");
        if (string.IsNullOrEmpty(alphabet))
        {
            alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz-";
        }

        var length = Math.Clamp(context.Int("length"), 1, 256);
        var count = Math.Clamp(context.Int("count"), 1, 1000);
        context.SetText("result", string.Join(Environment.NewLine, Enumerable.Range(0, count).Select(_ => RandomString(alphabet, length))));
        return Task.CompletedTask;
    }

    public static Task QueryStringBuilderAsync(ToolRunContext context, CancellationToken token)
    {
        var pairs = ParseKeyValueLines(context.Text("pairs"))
            .Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}");
        var query = string.Join('&', pairs);
        if (context.Bool("includeQuestionMark") && query.Length > 0)
        {
            query = "?" + query;
        }

        context.SetText("result", query);
        return Task.CompletedTask;
    }

    public static Task QueryStringParserAsync(ToolRunContext context, CancellationToken token)
    {
        var input = context.Text("query").Trim();
        var query = Uri.TryCreate(input, UriKind.Absolute, out var uri) ? uri.Query : input;
        var values = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pieces = part.Split('=', 2);
            var key = WebUtility.UrlDecode(pieces[0]) ?? string.Empty;
            var value = pieces.Length > 1 ? WebUtility.UrlDecode(pieces[1]) ?? string.Empty : string.Empty;
            if (!values.TryGetValue(key, out var list))
            {
                list = [];
                values[key] = list;
            }

            list.Add(value);
        }

        var payload = values.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Count == 1 ? (object)pair.Value[0] : pair.Value,
            StringComparer.OrdinalIgnoreCase);
        context.SetText("result", JsonSerializer.Serialize(payload, PrettyJsonOptions));
        return Task.CompletedTask;
    }

    public static Task JsonViewerAsync(ToolRunContext context, CancellationToken token)
    {
        context.SetText("result", PrettyJson(context.Text("json")));
        return Task.CompletedTask;
    }

    public static Task RandomPortAsync(ToolRunContext context, CancellationToken token)
    {
        var count = Math.Clamp(context.Int("count"), 1, 1000);
        var min = Math.Clamp(context.Int("min"), 1, 65535);
        var max = Math.Clamp(context.Int("max"), min, 65535);
        var values = new SortedSet<int>();
        while (values.Count < count && values.Count < max - min + 1)
        {
            values.Add(RandomNumberGenerator.GetInt32(min, max + 1));
        }

        context.SetText("result", string.Join(Environment.NewLine, values));
        return Task.CompletedTask;
    }

    public static Task RegexMemoAsync(ToolRunContext context, CancellationToken token)
    {
        context.SetText("result", ReadResource("regex-memo.content.md"));
        return Task.CompletedTask;
    }

    public static Task RegexReplacerAsync(ToolRunContext context, CancellationToken token)
    {
        var options = RegexOptions.None;
        if (context.Bool("ignoreCase"))
        {
            options |= RegexOptions.IgnoreCase;
        }
        if (context.Bool("multiline"))
        {
            options |= RegexOptions.Multiline;
        }

        var regex = new Regex(context.Text("pattern"), options);
        var input = context.Text("text");
        var count = regex.Matches(input).Count;
        context.SetText("result", string.Join(Environment.NewLine,
            $"Replacements: {count}",
            regex.Replace(input, context.Text("replacement"))));
        return Task.CompletedTask;
    }

    public static Task RegexTesterAsync(ToolRunContext context, CancellationToken token)
    {
        var options = RegexOptions.None;
        if (context.Bool("ignoreCase"))
        {
            options |= RegexOptions.IgnoreCase;
        }
        if (context.Bool("multiline"))
        {
            options |= RegexOptions.Multiline;
        }

        var regex = new Regex(context.Text("pattern"), options);
        var matches = regex.Matches(context.Text("text"));
        var sb = new StringBuilder();
        sb.AppendLine($"Matches: {matches.Count}");
        foreach (Match match in matches.Take(200))
        {
            sb.AppendLine($"[{match.Index}, {match.Length}] {match.Value}");
            for (var i = 1; i < match.Groups.Count; i++)
            {
                sb.AppendLine($"  group {i}: {match.Groups[i].Value}");
            }
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task SemVerComparatorAsync(ToolRunContext context, CancellationToken token)
    {
        var left = ParseSemVersion(context.Text("left"));
        var right = ParseSemVersion(context.Text("right"));
        var comparison = CompareSemVersions(left, right);
        var symbol = comparison < 0 ? "<" : comparison > 0 ? ">" : "=";
        context.SetText("result", $"{left.Original} {symbol} {right.Original}");
        return Task.CompletedTask;
    }

    public static Task SemVerInspectorAsync(ToolRunContext context, CancellationToken token)
    {
        var version = ParseSemVersion(context.Text("version"));
        context.SetText("result", string.Join(Environment.NewLine,
            $"Major: {version.Major}",
            $"Minor: {version.Minor}",
            $"Patch: {version.Patch}",
            $"Pre-release: {(string.IsNullOrWhiteSpace(version.PreRelease) ? "(none)" : version.PreRelease)}",
            $"Build metadata: {(string.IsNullOrWhiteSpace(version.Build) ? "(none)" : version.Build)}",
            $"Next patch: {version.Major}.{version.Minor}.{version.Patch + 1}",
            $"Next minor: {version.Major}.{version.Minor + 1}.0",
            $"Next major: {version.Major + 1}.0.0"));
        return Task.CompletedTask;
    }

    public static Task SqlPrettifyAsync(ToolRunContext context, CancellationToken token)
    {
        var sql = context.Text("sql");
        var keywords = new[]
        {
            "select", "from", "where", "inner join", "left join", "right join", "full join", "join", "group by",
            "order by", "having", "limit", "offset", "values", "set", "insert into", "update", "delete from"
        };
        foreach (var keyword in keywords.OrderByDescending(k => k.Length))
        {
            sql = Regex.Replace(sql, $@"\b{Regex.Escape(keyword)}\b", match => Environment.NewLine + match.Value.ToUpperInvariant(), RegexOptions.IgnoreCase);
        }
        sql = Regex.Replace(sql, @",\s*", "," + Environment.NewLine + "  ");
        context.SetText("result", sql.Trim());
        return Task.CompletedTask;
    }

    public static Task StringEscapeAsync(ToolRunContext context, CancellationToken token)
    {
        var text = context.Text("text");
        var escape = context.Option("mode") == "Escape";
        var result = context.Option("format") switch
        {
            "JSON" => escape
                ? JsonSerializer.Serialize(text, CompactJsonOptions)
                : JsonSerializer.Deserialize<string>(text, CompactJsonOptions) ?? string.Empty,
            "C#" => escape ? EscapeCSharpString(text) : UnescapeCStyle(text),
            "HTML" => escape ? WebUtility.HtmlEncode(text) : WebUtility.HtmlDecode(text),
            "URL" => escape ? WebUtility.UrlEncode(text) : WebUtility.UrlDecode(text),
            _ => text
        };
        context.SetText("result", result ?? string.Empty);
        return Task.CompletedTask;
    }

    public static Task UuidV5Async(ToolRunContext context, CancellationToken token)
    {
        var namespaceBytes = GuidToNetworkBytes(Guid.Parse(context.Text("namespace")));
        var nameBytes = Encoding.UTF8.GetBytes(context.Text("name"));
        var input = new byte[namespaceBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, input, 0, namespaceBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, input, namespaceBytes.Length, nameBytes.Length);
        var hash = SHA1.HashData(input);
        var uuidBytes = hash.Take(16).ToArray();
        uuidBytes[6] = (byte)((uuidBytes[6] & 0x0f) | 0x50);
        uuidBytes[8] = (byte)((uuidBytes[8] & 0x3f) | 0x80);
        context.SetText("result", NetworkBytesToGuid(uuidBytes).ToString());
        return Task.CompletedTask;
    }

    public static Task XmlFormatterAsync(ToolRunContext context, CancellationToken token)
    {
        var doc = XDocument.Parse(context.Text("xml"), LoadOptions.PreserveWhitespace);
        context.SetText("result", doc.ToString());
        return Task.CompletedTask;
    }

    public static Task XmlXPathTesterAsync(ToolRunContext context, CancellationToken token)
    {
        var doc = new XmlDocument();
        doc.LoadXml(context.Text("xml"));
        var nodes = doc.SelectNodes(context.Text("xpath"));
        var sb = new StringBuilder();
        sb.AppendLine($"Matches: {nodes?.Count ?? 0}");
        if (nodes != null)
        {
            foreach (XmlNode node in nodes.Cast<XmlNode>().Take(200))
            {
                sb.AppendLine(node is XmlAttribute ? $"{node.Name}=\"{node.Value}\"" : node.OuterXml);
            }
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task YamlViewerAsync(ToolRunContext context, CancellationToken token)
    {
        var deserializer = new DeserializerBuilder().Build();
        var serializer = new SerializerBuilder().Build();
        var obj = deserializer.Deserialize<object>(context.Text("yaml"));
        context.SetText("result", serializer.Serialize(obj));
        return Task.CompletedTask;
    }

    public static Task BCryptAsync(ToolRunContext context, CancellationToken token)
    {
        if (context.Option("mode") == "Compare")
        {
            var ok = BCrypt.Net.BCrypt.Verify(context.Text("text"), context.Text("hash"));
            context.SetText("result", ok ? "The text matches the hash." : "The text does not match the hash.");
            return Task.CompletedTask;
        }

        var rounds = Math.Clamp(context.Int("rounds"), 4, 31);
        context.SetText("result", BCrypt.Net.BCrypt.HashPassword(context.Text("text"), rounds));
        return Task.CompletedTask;
    }

    public static Task Bip39Async(ToolRunContext context, CancellationToken token)
    {
        var wordCount = context.Int("words") switch
        {
            <= 12 => WordCount.Twelve,
            <= 15 => WordCount.Fifteen,
            <= 18 => WordCount.Eighteen,
            <= 21 => WordCount.TwentyOne,
            _ => WordCount.TwentyFour
        };
        var mnemonic = new Mnemonic(Wordlist.English, wordCount);
        var seed = mnemonic.DeriveSeed(context.Text("passphrase"));
        context.SetText("result", $"Mnemonic:{Environment.NewLine}{mnemonic}{Environment.NewLine}{Environment.NewLine}Seed (hex):{Environment.NewLine}{Convert.ToHexString(seed).ToLowerInvariant()}");
        return Task.CompletedTask;
    }

    public static Task EncryptionAsync(ToolRunContext context, CancellationToken token)
    {
        using SymmetricAlgorithm algorithm = context.Option("algorithm") == "TripleDES" ? TripleDES.Create() : Aes.Create();
        var keyLength = algorithm.KeySize / 8;
        var ivLength = algorithm.BlockSize / 8;
        var derived = Rfc2898DeriveBytes.Pbkdf2(
            context.Text("password"),
            Encoding.UTF8.GetBytes("CodeWF.ToolFramework.Salt"),
            100_000,
            HashAlgorithmName.SHA256,
            keyLength + ivLength);
        algorithm.Key = derived[..keyLength];
        algorithm.IV = derived[keyLength..];
        algorithm.Mode = CipherMode.CBC;
        algorithm.Padding = PaddingMode.PKCS7;

        if (context.Option("mode") == "Encrypt")
        {
            using var encryptor = algorithm.CreateEncryptor();
            var plaintext = Encoding.UTF8.GetBytes(context.Text("text"));
            context.SetText("result", Convert.ToBase64String(encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length)));
            return Task.CompletedTask;
        }

        using var decryptor = algorithm.CreateDecryptor();
        var cipher = Convert.FromBase64String(context.Text("text"));
        context.SetText("result", Encoding.UTF8.GetString(decryptor.TransformFinalBlock(cipher, 0, cipher.Length)));
        return Task.CompletedTask;
    }

    public static Task HashTextAsync(ToolRunContext context, CancellationToken token)
    {
        var bytes = Encoding.UTF8.GetBytes(context.Text("text"));
        context.SetText("result", ToHex(ComputeDigest(context.Option("algorithm"), bytes)));
        return Task.CompletedTask;
    }

    public static Task HmacAsync(ToolRunContext context, CancellationToken token)
    {
        var message = Encoding.UTF8.GetBytes(context.Text("text"));
        var key = Encoding.UTF8.GetBytes(context.Text("key"));
        var algorithm = context.Option("algorithm");
        byte[] result;
        if (algorithm.StartsWith("SHA3", StringComparison.OrdinalIgnoreCase) || algorithm == "RIPEMD160")
        {
            var digest = CreateDigest(algorithm);
            var hmac = new HMac(digest);
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(message, 0, message.Length);
            result = new byte[hmac.GetMacSize()];
            hmac.DoFinal(result, 0);
        }
        else
        {
            using HMAC hmac = algorithm switch
            {
                "MD5" => new HMACMD5(key),
                "SHA1" => new HMACSHA1(key),
                "SHA384" => new HMACSHA384(key),
                "SHA512" => new HMACSHA512(key),
                _ => new HMACSHA256(key)
            };
            result = hmac.ComputeHash(message);
        }

        context.SetText("result", ToHex(result));
        return Task.CompletedTask;
    }

    public static Task PasswordStrengthAsync(ToolRunContext context, CancellationToken token)
    {
        var password = context.Text("password");
        var charset = 0;
        if (password.Any(char.IsLower)) charset += 26;
        if (password.Any(char.IsUpper)) charset += 26;
        if (password.Any(char.IsDigit)) charset += 10;
        if (password.Any(c => !char.IsLetterOrDigit(c))) charset += 33;
        var entropy = password.Length == 0 || charset == 0 ? 0 : password.Length * Math.Log2(charset);
        var guesses = Math.Pow(2, entropy);
        var crackSeconds = guesses / 1_000_000_000d;
        var score = entropy switch
        {
            < 28 => "Very weak",
            < 36 => "Weak",
            < 60 => "Reasonable",
            < 80 => "Strong",
            _ => "Very strong"
        };
        context.SetText("result", string.Join(Environment.NewLine,
            $"Score: {score}",
            $"Length: {password.Length}",
            $"Character set size: {charset}",
            $"Entropy: {entropy:0.##} bits",
            $"Offline crack estimate at 1B guesses/s: {FormatDuration(TimeSpan.FromSeconds(Math.Min(crackSeconds, TimeSpan.MaxValue.TotalSeconds)))}"));
        return Task.CompletedTask;
    }

    public static Task PdfSignatureAsync(ToolRunContext context, CancellationToken token)
    {
        var file = context.Text("file");
        if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
        {
            context.SetText("result", "Choose a PDF file.");
            return Task.CompletedTask;
        }

        var bytes = File.ReadAllBytes(file);
        var text = Encoding.Latin1.GetString(bytes);
        var matches = Regex.Matches(text, @"/ByteRange\s*\[\s*(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s*\].*?/Contents\s*<([0-9A-Fa-f\s]+)>", RegexOptions.Singleline);
        var sb = new StringBuilder();
        sb.AppendLine($"PDF: {Path.GetFileName(file)}");
        sb.AppendLine($"Signatures found: {matches.Count}");
        foreach (Match match in matches)
        {
            var ranges = match.Groups.Values.Skip(1).Take(4).Select(g => long.Parse(g.Value, CultureInfo.InvariantCulture)).ToArray();
            var signatureHex = Regex.Replace(match.Groups[5].Value, @"\s+", "");
            sb.AppendLine();
            sb.AppendLine($"ByteRange: [{string.Join(", ", ranges)}]");
            sb.AppendLine($"Signature container bytes: {signatureHex.Length / 2:N0}");
            sb.AppendLine($"Covered bytes: {ranges[1] + ranges[3]:N0} of {bytes.Length:N0}");
        }
        if (matches.Count == 0)
        {
            sb.AppendLine("No /ByteRange signature dictionary was found.");
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task RsaAsync(ToolRunContext context, CancellationToken token)
    {
        var size = Math.Clamp(context.Int("size"), 1024, 8192);
        using var rsa = RSA.Create(size);
        var privatePem = rsa.ExportPkcs8PrivateKeyPem();
        var publicPem = rsa.ExportSubjectPublicKeyInfoPem();
        context.SetText("result", $"{privatePem}{Environment.NewLine}{publicPem}");
        return Task.CompletedTask;
    }

    public static Task TokenAsync(ToolRunContext context, CancellationToken token)
    {
        var chars = new StringBuilder();
        if (context.Bool("upper")) chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        if (context.Bool("lower")) chars.Append("abcdefghijklmnopqrstuvwxyz");
        if (context.Bool("numbers")) chars.Append("0123456789");
        if (context.Bool("symbols")) chars.Append("!#$%&()*+,-./:;<=>?@[]^_{|}~");
        if (chars.Length == 0) chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
        var length = Math.Clamp(context.Int("length"), 1, 4096);
        context.SetText("result", RandomString(chars.ToString(), length));
        return Task.CompletedTask;
    }

    public static Task UlidAsync(ToolRunContext context, CancellationToken token)
    {
        var count = Math.Clamp(context.Int("count"), 1, 1000);
        context.SetText("result", string.Join(Environment.NewLine, Enumerable.Range(0, count).Select(_ => Ulid.NewUlid().ToString())));
        return Task.CompletedTask;
    }

    public static Task UuidAsync(ToolRunContext context, CancellationToken token)
    {
        var count = Math.Clamp(context.Int("count"), 1, 1000);
        var values = Enumerable.Range(0, count).Select(_ => Guid.NewGuid().ToString("D"));
        if (context.Bool("uppercase"))
        {
            values = values.Select(v => v.ToUpperInvariant());
        }

        context.SetText("result", string.Join(Environment.NewLine, values));
        return Task.CompletedTask;
    }

    public static Task BasicAuthAsync(ToolRunContext context, CancellationToken token)
    {
        var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{context.Text("username")}:{context.Text("password")}"));
        context.SetText("result", $"Authorization: Basic {value}");
        return Task.CompletedTask;
    }

    public static Task DeviceInfoAsync(ToolRunContext context, CancellationToken token)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"OS: {Environment.OSVersion}");
        sb.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        sb.AppendLine($"64-bit process: {Environment.Is64BitProcess}");
        sb.AppendLine($".NET: {Environment.Version}");
        sb.AppendLine($"Machine: {Environment.MachineName}");
        sb.AppendLine($"User: {Environment.UserName}");
        sb.AppendLine($"Processor count: {Environment.ProcessorCount}");
        sb.AppendLine($"Working set: {Environment.WorkingSet / 1024 / 1024:N0} MB");
        sb.AppendLine($"System directory: {Environment.SystemDirectory}");
        sb.AppendLine($"Current directory: {Environment.CurrentDirectory}");
        sb.AppendLine();
        sb.AppendLine("Network interfaces:");
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up))
        {
            sb.AppendLine($"- {networkInterface.Name}: {networkInterface.NetworkInterfaceType}, {networkInterface.GetPhysicalAddress()}");
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task HtmlEntitiesAsync(ToolRunContext context, CancellationToken token)
    {
        var text = context.Text("text");
        context.SetText("result", context.Option("mode") == "Escape" ? WebUtility.HtmlEncode(text) : WebUtility.HtmlDecode(text));
        return Task.CompletedTask;
    }

    public static Task HtmlEditorAsync(ToolRunContext context, CancellationToken token)
    {
        var lines = SplitLines(context.Text("content")).ToList();
        var html = context.Option("mode") switch
        {
            "Unordered list" => $"<ul>{Environment.NewLine}{string.Join(Environment.NewLine, lines.Select(line => $"  <li>{WebUtility.HtmlEncode(line)}</li>"))}{Environment.NewLine}</ul>",
            "Ordered list" => $"<ol>{Environment.NewLine}{string.Join(Environment.NewLine, lines.Select(line => $"  <li>{WebUtility.HtmlEncode(line)}</li>"))}{Environment.NewLine}</ol>",
            "Raw HTML preview" => context.Text("content"),
            _ => string.Join(Environment.NewLine, lines.Select(line => $"<p>{WebUtility.HtmlEncode(line)}</p>"))
        };
        context.SetText("result", html);
        return Task.CompletedTask;
    }

    public static Task HttpStatusAsync(ToolRunContext context, CancellationToken token)
    {
        var query = context.Text("query").Trim();
        var statuses = HttpStatuses()
            .Where(status => string.IsNullOrWhiteSpace(query)
                             || status.Code.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase)
                             || status.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || status.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(status => $"{status.Code} {status.Name} - {status.Description}");
        context.SetText("result", string.Join(Environment.NewLine, statuses));
        return Task.CompletedTask;
    }

    public static Task JwtAsync(ToolRunContext context, CancellationToken token)
    {
        var parts = context.Text("jwt").Trim().Split('.');
        if (parts.Length < 2)
        {
            context.SetText("result", "JWT must contain at least header and payload sections.");
            return Task.CompletedTask;
        }

        var header = DecodeJwtPart(parts[0]);
        var payload = DecodeJwtPart(parts[1]);
        context.SetText("result", $"Header:{Environment.NewLine}{PrettyJson(header)}{Environment.NewLine}{Environment.NewLine}Payload:{Environment.NewLine}{PrettyJson(payload)}{Environment.NewLine}{Environment.NewLine}Signature present: {parts.Length > 2 && parts[2].Length > 0}");
        return Task.CompletedTask;
    }

    public static Task KeycodeInfoAsync(ToolRunContext context, CancellationToken token)
    {
        context.SetText("result", "Focus this tool panel and press any key.");
        return Task.CompletedTask;
    }

    public static Task MetaTagsAsync(ToolRunContext context, CancellationToken token)
    {
        var title = WebUtility.HtmlEncode(context.Text("title"));
        var description = WebUtility.HtmlEncode(context.Text("description"));
        var url = WebUtility.HtmlEncode(context.Text("url"));
        var image = WebUtility.HtmlEncode(context.Text("image"));
        var site = WebUtility.HtmlEncode(context.Text("site"));
        var type = WebUtility.HtmlEncode(context.Option("type"));
        var tags = new[]
        {
            $"""<meta property="og:type" content="{type}" />""",
            $"""<meta property="og:title" content="{title}" />""",
            $"""<meta property="og:description" content="{description}" />""",
            $"""<meta property="og:url" content="{url}" />""",
            $"""<meta property="og:image" content="{image}" />""",
            $"""<meta property="og:site_name" content="{site}" />""",
            $"""<meta name="twitter:card" content="summary_large_image" />""",
            $"""<meta name="twitter:title" content="{title}" />""",
            $"""<meta name="twitter:description" content="{description}" />""",
            $"""<meta name="twitter:image" content="{image}" />""",
        };
        context.SetText("result", string.Join(Environment.NewLine, tags));
        return Task.CompletedTask;
    }

    public static Task MimeTypesAsync(ToolRunContext context, CancellationToken token)
    {
        var query = context.Text("query").Trim().TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(query))
        {
            context.SetText("result", "Enter a MIME type or extension.");
            return Task.CompletedTask;
        }

        var matches = MimeDb.Value
            .Where(kv => kv.Key.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || kv.Value.Extensions?.Any(ext => ext.Equals(query, StringComparison.OrdinalIgnoreCase)) == true)
            .Take(200)
            .Select(kv => $"{kv.Key}    extensions: {(kv.Value.Extensions == null ? "(none)" : string.Join(", ", kv.Value.Extensions))}");
        context.SetText("result", string.Join(Environment.NewLine, matches));
        return Task.CompletedTask;
    }

    public static Task OtpAsync(ToolRunContext context, CancellationToken token)
    {
        var secretText = context.Text("secret").Trim();
        if (string.IsNullOrWhiteSpace(secretText))
        {
            var random = RandomNumberGenerator.GetBytes(20);
            secretText = Base32Encoding.ToString(random);
        }

        var secret = Base32Encoding.ToBytes(secretText);
        var digits = Math.Clamp(context.Int("digits"), 6, 8);
        var period = Math.Clamp(context.Int("period"), 5, 300);
        var totp = new Totp(secret, period, OtpHashMode.Sha1, digits);
        var code = totp.ComputeTotp();
        var remaining = totp.RemainingSeconds();
        var sb = new StringBuilder();
        sb.AppendLine($"Secret: {secretText}");
        sb.AppendLine($"Current code: {code}");
        sb.AppendLine($"Remaining seconds: {remaining}");
        var codeToValidate = context.Text("code").Trim();
        if (!string.IsNullOrWhiteSpace(codeToValidate))
        {
            var valid = totp.VerifyTotp(codeToValidate, out var timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);
            sb.AppendLine($"Validation: {(valid ? $"valid (time step {timeStepMatched})" : "invalid")}");
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task SafelinkAsync(ToolRunContext context, CancellationToken token)
    {
        var input = context.Text("url").Trim();
        if (!Uri.TryCreate(input, UriKind.Absolute, out var uri))
        {
            context.SetText("result", "Invalid URL.");
            return Task.CompletedTask;
        }

        var query = ParseQuery(uri.Query);
        context.SetText("result", query.TryGetValue("url", out var decoded) ? WebUtility.UrlDecode(decoded) : input);
        return Task.CompletedTask;
    }

    public static Task SlugifyAsync(ToolRunContext context, CancellationToken token)
    {
        var text = context.Text("text").Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var ch in text)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }
        var slug = SlugUnsafeRegex.Replace(builder.ToString(), "");
        slug = WhitespaceRegex.Replace(slug.Trim(), "-");
        slug = Regex.Replace(slug, "-{2,}", "-");
        if (context.Bool("lower"))
        {
            slug = slug.ToLowerInvariant();
        }

        context.SetText("result", slug);
        return Task.CompletedTask;
    }

    public static Task UrlEncoderAsync(ToolRunContext context, CancellationToken token)
    {
        context.SetText("result", context.Option("mode") == "Encode"
            ? WebUtility.UrlEncode(context.Text("text"))
            : WebUtility.UrlDecode(context.Text("text")));
        return Task.CompletedTask;
    }

    public static Task UrlParserAsync(ToolRunContext context, CancellationToken token)
    {
        if (!Uri.TryCreate(context.Text("url"), UriKind.Absolute, out var uri))
        {
            context.SetText("result", "Invalid absolute URL.");
            return Task.CompletedTask;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Scheme: {uri.Scheme}");
        sb.AppendLine($"Host: {uri.Host}");
        sb.AppendLine($"Port: {uri.Port}");
        sb.AppendLine($"Path: {uri.AbsolutePath}");
        sb.AppendLine($"Query: {uri.Query}");
        sb.AppendLine($"Fragment: {uri.Fragment}");
        sb.AppendLine($"User info: {uri.UserInfo}");
        sb.AppendLine("Query parameters:");
        foreach (var pair in ParseQuery(uri.Query))
        {
            sb.AppendLine($"  {pair.Key}: {pair.Value}");
        }

        context.SetText("result", sb.ToString());
        return Task.CompletedTask;
    }

    public static Task UserAgentAsync(ToolRunContext context, CancellationToken token)
    {
        var parser = Parser.GetDefault();
        var client = parser.Parse(context.Text("userAgent"));
        context.SetText("result", string.Join(Environment.NewLine,
            $"Browser: {client.UA.Family} {client.UA.Major}.{client.UA.Minor}.{client.UA.Patch}",
            $"OS: {client.OS.Family} {client.OS.Major}.{client.OS.Minor}.{client.OS.Patch}",
            $"Device: {client.Device.Family} {client.Device.Brand} {client.Device.Model}"));
        return Task.CompletedTask;
    }

    public static Task QrCodeAsync(ToolRunContext context, CancellationToken token)
    {
        var text = context.Text("text");
        if (string.IsNullOrWhiteSpace(text))
        {
            context.SetText("result", "Enter text to encode.");
            context.SetImage("image", null);
            return Task.CompletedTask;
        }

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        var png = qr.GetGraphic(Math.Clamp(context.Int("pixels"), 2, 40));
        context.SetImage("image", BytesToBitmap(png));
        context.SetText("result", text);
        return Task.CompletedTask;
    }

    public static Task SvgPlaceholderAsync(ToolRunContext context, CancellationToken token)
    {
        var width = Math.Max(1, context.Int("width"));
        var height = Math.Max(1, context.Int("height"));
        var text = WebUtility.HtmlEncode(context.Text("text"));
        var bg = WebUtility.HtmlEncode(context.Text("bg"));
        var fg = WebUtility.HtmlEncode(context.Text("fg"));
        var svg = $"""<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}"><rect width="100%" height="100%" fill="{bg}"/><text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle" fill="{fg}" font-family="Arial, sans-serif" font-size="{Math.Max(12, Math.Min(width, height) / 10)}">{text}</text></svg>""";
        context.SetText("result", svg);
        return Task.CompletedTask;
    }

    public static Task WifiQrAsync(ToolRunContext context, CancellationToken token)
    {
        var payload = $"WIFI:T:{context.Option("auth")};S:{EscapeWifi(context.Text("ssid"))};P:{EscapeWifi(context.Text("password"))};H:{context.Bool("hidden").ToString().ToLowerInvariant()};;";
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        context.SetImage("image", BytesToBitmap(qr.GetGraphic(10)));
        context.SetText("result", payload);
        return Task.CompletedTask;
    }

    public static Task Ipv4AddressAsync(ToolRunContext context, CancellationToken token)
    {
        var value = ParseIpv4(context.Text("ip"));
        var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
        context.SetText("result", string.Join(Environment.NewLine,
            $"Dotted decimal: {FormatIpv4(value)}",
            $"Decimal: {value}",
            $"Hexadecimal: 0x{value:X8}",
            $"Binary: {string.Join(".", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')))}",
            $"IPv6 mapped: ::ffff:{FormatIpv4(value)}"));
        return Task.CompletedTask;
    }

    public static Task Ipv4RangeAsync(ToolRunContext context, CancellationToken token)
    {
        var start = ParseIpv4(context.Text("start"));
        var end = ParseIpv4(context.Text("end"));
        if (start > end)
        {
            (start, end) = (end, start);
        }

        context.SetText("result", string.Join(Environment.NewLine, RangeToCidrs(start, end)));
        return Task.CompletedTask;
    }

    public static Task Ipv4SubnetAsync(ToolRunContext context, CancellationToken token)
    {
        var parts = context.Text("cidr").Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var prefix) || prefix is < 0 or > 32)
        {
            context.SetText("result", "CIDR must look like 192.168.1.0/24.");
            return Task.CompletedTask;
        }

        var ip = ParseIpv4(parts[0]);
        var mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
        var network = ip & mask;
        var broadcast = network | ~mask;
        var hosts = prefix >= 31 ? (ulong)(broadcast - network + 1) : broadcast - network - 1;
        context.SetText("result", string.Join(Environment.NewLine,
            $"Network: {FormatIpv4(network)}/{prefix}",
            $"Netmask: {FormatIpv4(mask)}",
            $"Wildcard: {FormatIpv4(~mask)}",
            $"Broadcast: {FormatIpv4(broadcast)}",
            $"First host: {FormatIpv4(prefix >= 31 ? network : network + 1)}",
            $"Last host: {FormatIpv4(prefix >= 31 ? broadcast : broadcast - 1)}",
            $"Hosts: {hosts:N0}"));
        return Task.CompletedTask;
    }

    public static Task Ipv6UlaAsync(ToolRunContext context, CancellationToken token)
    {
        var count = Math.Clamp(context.Int("count"), 1, 100);
        var lines = Enumerable.Range(0, count).Select(_ =>
        {
            var bytes = RandomNumberGenerator.GetBytes(5);
            return $"fd{bytes[0]:x2}:{bytes[1]:x2}{bytes[2]:x2}:{bytes[3]:x2}{bytes[4]:x2}::/48";
        });
        context.SetText("result", string.Join(Environment.NewLine, lines));
        return Task.CompletedTask;
    }

    public static Task MacGeneratorAsync(ToolRunContext context, CancellationToken token)
    {
        var count = Math.Clamp(context.Int("count"), 1, 1000);
        var prefixBytes = ParseMacPrefix(context.Text("prefix"));
        var lines = Enumerable.Range(0, count).Select(_ =>
        {
            var bytes = new byte[6];
            RandomNumberGenerator.Fill(bytes);
            for (var i = 0; i < prefixBytes.Length; i++)
            {
                bytes[i] = prefixBytes[i];
            }
            var mac = string.Join(":", bytes.Select(b => b.ToString(context.Bool("uppercase") ? "X2" : "x2", CultureInfo.InvariantCulture)));
            return mac;
        });
        context.SetText("result", string.Join(Environment.NewLine, lines));
        return Task.CompletedTask;
    }

    public static Task MacLookupAsync(ToolRunContext context, CancellationToken token)
    {
        var prefix = NormalizeMac(context.Text("mac"));
        if (prefix.Length < 6)
        {
            context.SetText("result", "Enter at least the first 6 hexadecimal MAC digits.");
            return Task.CompletedTask;
        }

        prefix = prefix[..6].ToUpperInvariant();
        context.SetText("result", OuiData.Value.TryGetValue(prefix, out var vendor)
            ? $"{prefix}{Environment.NewLine}{vendor}"
            : "Vendor not found in embedded OUI data.");
        return Task.CompletedTask;
    }

    public static Task EtaAsync(ToolRunContext context, CancellationToken token)
    {
        var done = (double)context.Number("done");
        var total = (double)context.Number("total");
        var rate = (double)context.Number("rate");
        if (total <= 0 || rate <= 0)
        {
            context.SetText("result", "Total and rate must be positive.");
            return Task.CompletedTask;
        }
        var remaining = Math.Max(0, total - done) / rate;
        var end = DateTime.Now.AddSeconds(remaining);
        context.SetText("result", string.Join(Environment.NewLine,
            $"Progress: {done / total:P2}",
            $"Remaining units: {Math.Max(0, total - done):0.###}",
            $"Remaining time: {FormatDuration(TimeSpan.FromSeconds(remaining))}",
            $"Estimated end: {end:yyyy-MM-dd HH:mm:ss}"));
        return Task.CompletedTask;
    }

    public static Task MathEvalAsync(ToolRunContext context, CancellationToken token)
    {
        var value = new ExpressionParser(context.Text("expression")).Parse();
        context.SetText("result", value.ToString("G15", CultureInfo.InvariantCulture));
        return Task.CompletedTask;
    }

    public static Task PercentageAsync(ToolRunContext context, CancellationToken token)
    {
        var a = (double)context.Number("a");
        var b = (double)context.Number("b");
        context.SetText("result", string.Join(Environment.NewLine,
            $"{a} is {(b == 0 ? double.NaN : a / b * 100):0.###}% of {b}",
            $"{b}% of {a} is {a * b / 100:0.###}",
            $"Increase from {a} to {b}: {(a == 0 ? double.NaN : (b - a) / a * 100):0.###}%"));
        return Task.CompletedTask;
    }

    public static Task BenchmarkAsync(ToolRunContext context, CancellationToken token)
    {
        var iterations = Math.Clamp(context.Int("iterations"), 1, 100_000_000);
        var task = context.Option("task");
        var sw = Stopwatch.StartNew();
        switch (task)
        {
            case "String concat":
                var text = "";
                for (var i = 0; i < Math.Min(iterations, 100_000); i++) text += "x";
                break;
            case "StringBuilder":
                var sb = new StringBuilder();
                for (var i = 0; i < iterations; i++) sb.Append('x');
                break;
            case "Random numbers":
                var sum = 0;
                for (var i = 0; i < iterations; i++) sum += RandomNumberGenerator.GetInt32(0, 1000);
                break;
            default:
                using (var sha = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes("benchmark");
                    for (var i = 0; i < iterations; i++) sha.ComputeHash(bytes);
                }
                break;
        }
        sw.Stop();
        context.SetText("result", string.Join(Environment.NewLine,
            $"Task: {task}",
            $"Iterations: {iterations:N0}",
            $"Elapsed: {sw.Elapsed}",
            $"Mean: {sw.Elapsed.TotalMilliseconds / iterations:0.########} ms/op"));
        return Task.CompletedTask;
    }

    public static Task ChronometerAsync(ToolRunContext context, CancellationToken token)
    {
        var start = ParseLooseDate(context.Text("start"), DateTimeOffset.Now);
        var end = ParseLooseDate(context.Text("end"), DateTimeOffset.Now);
        var duration = end - start;
        context.SetText("result", string.Join(Environment.NewLine,
            $"Start: {start:yyyy-MM-dd HH:mm:ss zzz}",
            $"End: {end:yyyy-MM-dd HH:mm:ss zzz}",
            $"Duration: {FormatDuration(duration.Duration())}",
            $"Seconds: {duration.TotalSeconds:0.###}"));
        return Task.CompletedTask;
    }

    public static Task AsciiArtAsync(ToolRunContext context, CancellationToken token)
    {
        var font = context.Option("font") switch
        {
            "Slant" => FiggleFonts.Slant,
            "Small" => FiggleFonts.Small,
            "Big" => FiggleFonts.Big,
            _ => FiggleFonts.Standard
        };
        context.SetText("result", font.Render(context.Text("text")));
        return Task.CompletedTask;
    }

    public static Task EmojiAsync(ToolRunContext context, CancellationToken token)
    {
        var query = context.Text("query").Trim();
        var results = EmojiData.Value
            .Where(kv => string.IsNullOrWhiteSpace(query)
                         || kv.Key.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || kv.Value.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
                         || kv.Value.Group?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            .Take(250)
            .Select(kv => $"{kv.Key}  {kv.Value.Name}  U+{string.Join(" U+", kv.Key.EnumerateRunes().Select(r => r.Value.ToString("X")))}  {kv.Value.Group}");
        context.SetText("result", string.Join(Environment.NewLine, results));
        return Task.CompletedTask;
    }

    public static Task LoremAsync(ToolRunContext context, CancellationToken token)
    {
        const string paragraph = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer vitae velit non justo facilisis volutpat. Donec vitae mi sed mauris pharetra bibendum. Suspendisse potenti.";
        var count = Math.Clamp(context.Int("paragraphs"), 1, 100);
        context.SetText("result", string.Join(Environment.NewLine + Environment.NewLine, Enumerable.Repeat(paragraph, count)));
        return Task.CompletedTask;
    }

    public static Task NumeronymAsync(ToolRunContext context, CancellationToken token)
    {
        var lines = SplitLines(context.Text("text"))
            .SelectMany(SplitWords)
            .Where(word => word.Length > 2)
            .Select(word => $"{word}: {word[0]}{word.Length - 2}{word[^1]}");
        context.SetText("result", string.Join(Environment.NewLine, lines));
        return Task.CompletedTask;
    }

    public static Task StringObfuscatorAsync(ToolRunContext context, CancellationToken token)
    {
        var value = context.Text("text");
        var keepStart = Math.Clamp(context.Int("keepStart"), 0, value.Length);
        var keepEnd = Math.Clamp(context.Int("keepEnd"), 0, Math.Max(0, value.Length - keepStart));
        var mask = string.IsNullOrEmpty(context.Text("mask")) ? "*" : context.Text("mask");
        var hiddenLength = Math.Max(0, value.Length - keepStart - keepEnd);
        context.SetText("result", value[..keepStart] + string.Concat(Enumerable.Repeat(mask, hiddenLength)) + (keepEnd > 0 ? value[^keepEnd..] : string.Empty));
        return Task.CompletedTask;
    }

    public static Task TextDiffAsync(ToolRunContext context, CancellationToken token)
    {
        context.SetText("result", BuildLineDiff(context.Text("left"), context.Text("right")));
        return Task.CompletedTask;
    }

    public static Task TextStatsAsync(ToolRunContext context, CancellationToken token)
    {
        var text = context.Text("text");
        var words = Regex.Matches(text, @"\b[\p{L}\p{N}_'-]+\b").Count;
        context.SetText("result", string.Join(Environment.NewLine,
            $"Characters: {text.Length:N0}",
            $"Unicode scalars: {text.EnumerateRunes().Count():N0}",
            $"Words: {words:N0}",
            $"Lines: {SplitLines(text).Count():N0}",
            $"Sentences: {Regex.Matches(text, @"[.!?]+").Count:N0}",
            $"UTF-8 bytes: {Encoding.UTF8.GetByteCount(text):N0}"));
        return Task.CompletedTask;
    }

    public static Task IbanAsync(ToolRunContext context, CancellationToken token)
    {
        var iban = Regex.Replace(context.Text("iban").ToUpperInvariant(), @"\s+", "");
        var valid = IsValidIban(iban);
        context.SetText("result", string.Join(Environment.NewLine,
            $"Valid: {valid}",
            $"Country: {(iban.Length >= 2 ? iban[..2] : string.Empty)}",
            $"Check digits: {(iban.Length >= 4 ? iban[2..4] : string.Empty)}",
            $"BBAN: {(iban.Length > 4 ? iban[4..] : string.Empty)}",
            $"Friendly: {Regex.Replace(iban, ".{4}", "$0 ").Trim()}"));
        return Task.CompletedTask;
    }

    public static Task PhoneAsync(ToolRunContext context, CancellationToken token)
    {
        var util = PhoneNumberUtil.GetInstance();
        var number = util.Parse(context.Text("phone"), context.Text("region").ToUpperInvariant());
        var valid = util.IsValidNumber(number);
        context.SetText("result", string.Join(Environment.NewLine,
            $"Valid: {valid}",
            $"Region: {util.GetRegionCodeForNumber(number)}",
            $"Type: {util.GetNumberType(number)}",
            $"E.164: {util.Format(number, PhoneNumberFormat.E164)}",
            $"International: {util.Format(number, PhoneNumberFormat.INTERNATIONAL)}",
            $"National: {util.Format(number, PhoneNumberFormat.NATIONAL)}",
            $"RFC3966: {util.Format(number, PhoneNumberFormat.RFC3966)}"));
        return Task.CompletedTask;
    }

    private static string ReadResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .First(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static string StripDataUrlPrefix(string value)
    {
        var comma = value.IndexOf(',', StringComparison.Ordinal);
        return value.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0 ? value[(comma + 1)..] : value;
    }

    private static string PadBase64(string value)
    {
        var padding = value.Length % 4;
        return padding == 0 ? value : value.PadRight(value.Length + 4 - padding, '=');
    }

    private static string MakeBase64UrlSafe(string value) => value.TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string UndoBase64UrlSafe(string value) => value.Replace('-', '+').Replace('_', '/');

    private static IEnumerable<string> SplitWords(string text)
    {
        return Regex.Matches(text, @"[\p{L}\p{N}]+")
            .Select(match => match.Value)
            .Where(word => word.Length > 0);
    }

    private static string SentenceCase(IReadOnlyList<string> words)
    {
        if (words.Count == 0)
        {
            return string.Empty;
        }

        var sentence = string.Join(' ', words).ToLowerInvariant();
        return char.ToUpperInvariant(sentence[0]) + sentence[1..];
    }

    private static bool TryParseColor(string input, out int r, out int g, out int b, out int a)
    {
        input = input.Trim();
        if (CssColors.TryGetValue(input, out var named))
        {
            input = named;
        }

        var hex = Regex.Match(input, "^#?([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$");
        if (hex.Success)
        {
            var value = hex.Groups[1].Value;
            if (value.Length == 3)
            {
                value = string.Concat(value.Select(ch => $"{ch}{ch}"));
            }

            r = Convert.ToInt32(value[..2], 16);
            g = Convert.ToInt32(value[2..4], 16);
            b = Convert.ToInt32(value[4..6], 16);
            a = value.Length == 8 ? Convert.ToInt32(value[6..8], 16) : 255;
            return true;
        }

        var rgb = Regex.Match(input, @"rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*([0-9.]+))?\)", RegexOptions.IgnoreCase);
        if (rgb.Success)
        {
            r = Math.Clamp(int.Parse(rgb.Groups[1].Value, CultureInfo.InvariantCulture), 0, 255);
            g = Math.Clamp(int.Parse(rgb.Groups[2].Value, CultureInfo.InvariantCulture), 0, 255);
            b = Math.Clamp(int.Parse(rgb.Groups[3].Value, CultureInfo.InvariantCulture), 0, 255);
            a = rgb.Groups[4].Success ? Math.Clamp((int)(double.Parse(rgb.Groups[4].Value, CultureInfo.InvariantCulture) * 255), 0, 255) : 255;
            return true;
        }

        var hsl = Regex.Match(input, @"hsl\(([-0-9.]+),\s*([-0-9.]+)%?,\s*([-0-9.]+)%?\)", RegexOptions.IgnoreCase);
        if (hsl.Success)
        {
            var h = double.Parse(hsl.Groups[1].Value, CultureInfo.InvariantCulture);
            var s = double.Parse(hsl.Groups[2].Value, CultureInfo.InvariantCulture) / 100d;
            var l = double.Parse(hsl.Groups[3].Value, CultureInfo.InvariantCulture) / 100d;
            (r, g, b) = HslToRgb(h, s, l);
            a = 255;
            return true;
        }

        r = g = b = 0;
        a = 255;
        return false;
    }

    private static (double H, double S, double L) RgbToHsl(int r, int g, int b)
    {
        var rd = r / 255d;
        var gd = g / 255d;
        var bd = b / 255d;
        var max = Math.Max(rd, Math.Max(gd, bd));
        var min = Math.Min(rd, Math.Min(gd, bd));
        var h = 0d;
        var l = (max + min) / 2d;
        var d = max - min;
        var s = d == 0 ? 0 : d / (1 - Math.Abs(2 * l - 1));
        if (d != 0)
        {
            h = max == rd ? 60 * (((gd - bd) / d) % 6)
                : max == gd ? 60 * (((bd - rd) / d) + 2)
                : 60 * (((rd - gd) / d) + 4);
            if (h < 0)
            {
                h += 360;
            }
        }

        return (h, s, l);
    }

    private static (int R, int G, int B) HslToRgb(double h, double s, double l)
    {
        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var x = c * (1 - Math.Abs((h / 60d) % 2 - 1));
        var m = l - c / 2;
        var (r1, g1, b1) = h switch
        {
            < 60 => (c, x, 0d),
            < 120 => (x, c, 0d),
            < 180 => (0d, c, x),
            < 240 => (0d, x, c),
            < 300 => (x, 0d, c),
            _ => (c, 0d, x)
        };
        return ((int)Math.Round((r1 + m) * 255), (int)Math.Round((g1 + m) * 255), (int)Math.Round((b1 + m) * 255));
    }

    private static DateTimeOffset ParseDate(string input, string format)
    {
        if (format == "Unix timestamp" || (format == "Auto" && Regex.IsMatch(input, @"^\d{1,10}$")))
        {
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(input, CultureInfo.InvariantCulture));
        }
        if (format == "Timestamp milliseconds" || (format == "Auto" && Regex.IsMatch(input, @"^\d{11,13}$")))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(input, CultureInfo.InvariantCulture));
        }
        if (format == "Mongo ObjectId" || (format == "Auto" && Regex.IsMatch(input, @"^[0-9a-fA-F]{24}$")))
        {
            return DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(input[..8], 16));
        }
        if (format == "Excel date/time")
        {
            var days = double.Parse(input, CultureInfo.InvariantCulture);
            return new DateTimeOffset(new DateTime(1899, 12, 30, 0, 0, 0, DateTimeKind.Utc).AddDays(days));
        }

        return DateTimeOffset.Parse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
    }

    private static string ConvertToBase(long value, int targetBase)
    {
        const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (value == 0)
        {
            return "0";
        }

        var negative = value < 0;
        var current = Math.Abs(value);
        var result = new Stack<char>();
        while (current > 0)
        {
            result.Push(digits[(int)(current % targetBase)]);
            current /= targetBase;
        }

        return (negative ? "-" : "") + new string(result.ToArray());
    }

    private static object? JsonElementToXml(JsonElement element, string itemName)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .Select(prop => new XElement(XmlConvert.EncodeName(prop.Name), JsonElementToXml(prop.Value, "item"))),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(item => new XElement(itemName, JsonElementToXml(item, "item"))),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty
        };
    }

    private static object? JsonElementToPlain(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(prop => prop.Name, prop => JsonElementToPlain(prop.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToPlain).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var integer) ? integer : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static object? XmlElementToObject(XElement element)
    {
        if (!element.Elements().Any())
        {
            return element.Value;
        }

        var dict = new Dictionary<string, object?>();
        foreach (var group in element.Elements().GroupBy(child => child.Name.LocalName))
        {
            var values = group.Select(XmlElementToObject).ToList();
            dict[group.Key] = values.Count == 1 ? values[0] : values;
        }

        foreach (var attr in element.Attributes())
        {
            dict[$"@{attr.Name.LocalName}"] = attr.Value;
        }

        return dict;
    }

    private static IEnumerable<string> SplitLines(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
    }

    private static List<string> TransposeLines(IReadOnlyList<string> lines)
    {
        var cells = lines.Select(line => line.Split([',', ';', '\t'], StringSplitOptions.None)).ToList();
        var width = cells.Count == 0 ? 0 : cells.Max(row => row.Length);
        var result = new List<string>();
        for (var col = 0; col < width; col++)
        {
            result.Add(string.Join('\t', cells.Select(row => col < row.Length ? row[col] : string.Empty)));
        }

        return result;
    }

    private static string ToRoman(int number)
    {
        if (number is <= 0 or > 3999)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Roman numerals support 1 through 3999.");
        }

        (int Value, string Numeral)[] map =
        [
            (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"), (100, "C"), (90, "XC"),
            (50, "L"), (40, "XL"), (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
        ];
        var sb = new StringBuilder();
        foreach (var (value, numeral) in map)
        {
            while (number >= value)
            {
                sb.Append(numeral);
                number -= value;
            }
        }

        return sb.ToString();
    }

    private static int FromRoman(string roman)
    {
        var map = new Dictionary<char, int> { ['I'] = 1, ['V'] = 5, ['X'] = 10, ['L'] = 50, ['C'] = 100, ['D'] = 500, ['M'] = 1000 };
        var total = 0;
        var previous = 0;
        foreach (var value in roman.ToUpperInvariant().Reverse().Select(ch => map[ch]))
        {
            total += value < previous ? -value : value;
            previous = Math.Max(previous, value);
        }

        return total;
    }

    private static double ToCelsius(double value, string unit)
    {
        return unit switch
        {
            "Kelvin" => value - 273.15,
            "Fahrenheit" => (value - 32) * 5 / 9,
            "Rankine" => (value - 491.67) * 5 / 9,
            "Delisle" => 100 - value * 2 / 3,
            "Newton" => value * 100 / 33,
            "Reaumur" => value * 5 / 4,
            "Romer" => (value - 7.5) * 40 / 21,
            _ => value
        };
    }

    private static int Bits(bool read, bool write, bool execute) => (read ? 4 : 0) + (write ? 2 : 0) + (execute ? 1 : 0);

    private static string Symbol(bool read, bool write, bool execute) => $"{(read ? 'r' : '-')}{(write ? 'w' : '-')}{(execute ? 'x' : '-')}";

    private static IEnumerable<string> ShellSplit(string command)
    {
        var current = new StringBuilder();
        var quote = '\0';
        for (var i = 0; i < command.Length; i++)
        {
            var ch = command[i];
            if (quote != '\0')
            {
                if (ch == quote)
                {
                    quote = '\0';
                }
                else
                {
                    current.Append(ch);
                }
            }
            else if (ch is '"' or '\'')
            {
                quote = ch;
            }
            else if (char.IsWhiteSpace(ch))
            {
                if (current.Length > 0)
                {
                    yield return current.ToString();
                    current.Clear();
                }
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
        {
            yield return current.ToString();
        }
    }

    private static string SanitizeServiceName(string name) => Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9_-]+", "-").Trim('-');

    private static string QuoteYaml(string value) => value.Contains(' ') ? $"\"{value.Replace("\"", "\\\"")}\"" : value;

    private static string NormalizeEmail(string email)
    {
        email = email.Trim().ToLowerInvariant();
        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return string.Empty;
        }

        var local = parts[0];
        var domain = parts[1];
        var plus = local.IndexOf('+', StringComparison.Ordinal);
        if (plus >= 0)
        {
            local = local[..plus];
        }
        if (domain is "gmail.com" or "googlemail.com")
        {
            local = local.Replace(".", string.Empty);
            domain = "gmail.com";
        }

        return $"{local}@{domain}";
    }

    private static string PrettyJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement, PrettyJsonOptions);
    }

    private static string BuildLineDiff(string left, string right)
    {
        var leftLines = SplitLines(left).ToList();
        var rightLines = SplitLines(right).ToList();
        var max = Math.Max(leftLines.Count, rightLines.Count);
        var sb = new StringBuilder();
        for (var i = 0; i < max; i++)
        {
            var oldLine = i < leftLines.Count ? leftLines[i] : null;
            var newLine = i < rightLines.Count ? rightLines[i] : null;
            if (oldLine == newLine)
            {
                sb.AppendLine($"  {oldLine}");
            }
            else
            {
                if (oldLine != null) sb.AppendLine($"- {oldLine}");
                if (newLine != null) sb.AppendLine($"+ {newLine}");
            }
        }

        return sb.ToString();
    }

    private static string CsvEscape(string value)
    {
        return value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    private static string JsonScalarToString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.GetRawText(),
            JsonValueKind.Null => string.Empty,
            _ => element.GetRawText()
        };
    }

    private static char? DelimiterFromOption(string option)
    {
        return option switch
        {
            "Comma" => ',',
            "Semicolon" => ';',
            "Tab" => '\t',
            "Pipe" => '|',
            _ => null
        };
    }

    private static List<List<string>> ParseDelimitedRows(string text, char? delimiter)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var actualDelimiter = delimiter ?? DetectDelimiter(text);
        var rows = new List<List<string>>();
        var row = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;

        void AddCell()
        {
            row.Add(cell.ToString());
            cell.Clear();
        }

        void AddRow()
        {
            AddCell();
            rows.Add(row);
            row = [];
        }

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (quoted)
            {
                if (ch == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else if (ch == '"')
                {
                    quoted = false;
                }
                else
                {
                    cell.Append(ch);
                }

                continue;
            }

            if (ch == '"')
            {
                quoted = true;
            }
            else if (ch == actualDelimiter)
            {
                AddCell();
            }
            else if (ch is '\r' or '\n')
            {
                if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                AddRow();
            }
            else
            {
                cell.Append(ch);
            }
        }

        if (cell.Length > 0 || row.Count > 0 || !text.EndsWith('\n'))
        {
            AddRow();
        }

        return rows
            .Where(r => r.Any(cellValue => !string.IsNullOrEmpty(cellValue)))
            .ToList();
    }

    private static char DetectDelimiter(string text)
    {
        var line = SplitLines(text).FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? string.Empty;
        return new[] { ',', ';', '\t', '|' }
            .OrderByDescending(candidate => line.Count(ch => ch == candidate))
            .First();
    }

    private static List<string> MakeUniqueHeaders(IReadOnlyList<string> headers)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        var result = new List<string>(headers.Count);
        foreach (var header in headers)
        {
            var name = string.IsNullOrWhiteSpace(header) ? $"column{result.Count + 1}" : header;
            if (!counts.TryAdd(name, 1))
            {
                counts[name]++;
                name = $"{name}_{counts[name]}";
            }

            result.Add(name);
        }

        return result;
    }

    private static string FormatMarkdownTable(IReadOnlyList<List<string>> rows, string? alignment)
    {
        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var width = rows.Max(row => row.Count);
        var normalized = rows
            .Select(row => Enumerable.Range(0, width).Select(i => i < row.Count ? row[i] : string.Empty).ToList())
            .ToList();
        var separator = alignment switch
        {
            "Left" => Enumerable.Repeat(":---", width),
            "Center" => Enumerable.Repeat(":---:", width),
            "Right" => Enumerable.Repeat("---:", width),
            _ => Enumerable.Repeat("---", width)
        };
        var sb = new StringBuilder();
        sb.AppendLine("| " + string.Join(" | ", normalized[0].Select(EscapeMarkdownCell)) + " |");
        sb.AppendLine("| " + string.Join(" | ", separator) + " |");
        foreach (var row in normalized.Skip(1))
        {
            sb.AppendLine("| " + string.Join(" | ", row.Select(EscapeMarkdownCell)) + " |");
        }

        return sb.ToString();
    }

    private static string EscapeMarkdownCell(string value)
    {
        return value.Replace("\\", "\\\\").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }

    private static IEnumerable<KeyValuePair<string, string>> ParseKeyValueLines(string text)
    {
        foreach (var rawLine in SplitLines(text))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator < 0)
            {
                separator = line.IndexOf(':');
            }

            if (separator <= 0)
            {
                continue;
            }

            yield return new KeyValuePair<string, string>(
                line[..separator].Trim(),
                StripOptionalQuotes(line[(separator + 1)..].Trim()));
        }
    }

    private static string StripOptionalQuotes(string value)
    {
        return value.Length >= 2 && ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\''))
            ? value[1..^1]
            : value;
    }

    private static JsonElement ResolveJsonPath(JsonElement root, string path)
    {
        var current = root;
        var text = path.Trim();
        var index = text.StartsWith('$') ? 1 : 0;
        while (index < text.Length)
        {
            if (text[index] == '.')
            {
                index++;
            }

            if (index >= text.Length)
            {
                break;
            }

            if (text[index] == '[')
            {
                var end = text.IndexOf(']', index);
                if (end < 0)
                {
                    throw new FormatException("JSON path is missing a closing bracket.");
                }

                var token = text[(index + 1)..end].Trim().Trim('\'', '"');
                current = int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var arrayIndex)
                    ? current.EnumerateArray().ElementAt(arrayIndex)
                    : current.GetProperty(token);
                index = end + 1;
                continue;
            }

            var start = index;
            while (index < text.Length && text[index] != '.' && text[index] != '[')
            {
                index++;
            }

            var property = text[start..index];
            current = current.GetProperty(property);
        }

        return current;
    }

    private static byte[] ParseHexBytes(string text)
    {
        var hex = Regex.Replace(text, "[^0-9a-fA-F]", "");
        if (hex.Length % 2 != 0)
        {
            throw new FormatException("Hex input must contain an even number of digits.");
        }

        return Enumerable.Range(0, hex.Length / 2)
            .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
            .ToArray();
    }

    private static string FormatHexDump(byte[] bytes, int bytesPerLine)
    {
        var limit = Math.Min(bytes.Length, 65536);
        var sb = new StringBuilder();
        for (var offset = 0; offset < limit; offset += bytesPerLine)
        {
            var line = bytes.Skip(offset).Take(Math.Min(bytesPerLine, limit - offset)).ToArray();
            var hex = string.Join(' ', line.Select(value => value.ToString("X2", CultureInfo.InvariantCulture))).PadRight(bytesPerLine * 3 - 1);
            var ascii = new string(line.Select(value => value is >= 32 and <= 126 ? (char)value : '.').ToArray());
            sb.AppendLine($"{offset:X8}  {hex}  {ascii}");
        }

        if (bytes.Length > limit)
        {
            sb.AppendLine($"... truncated {bytes.Length - limit:N0} bytes");
        }

        return sb.ToString();
    }

    private static bool LooksTextual(string mediaType)
    {
        return mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
               || mediaType.Contains("json", StringComparison.OrdinalIgnoreCase)
               || mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase)
               || mediaType.Contains("svg", StringComparison.OrdinalIgnoreCase);
    }

    private static string EscapeCSharpString(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(ch switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\0' => "\\0",
                '\a' => "\\a",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\v' => "\\v",
                _ when char.IsControl(ch) => $"\\u{(int)ch:x4}",
                _ => ch
            });
        }

        return sb.ToString();
    }

    private static string UnescapeCStyle(string value)
    {
        var sb = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            if (ch != '\\' || i + 1 >= value.Length)
            {
                sb.Append(ch);
                continue;
            }

            var next = value[++i];
            sb.Append(next switch
            {
                '0' => '\0',
                'a' => '\a',
                'b' => '\b',
                'f' => '\f',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'v' => '\v',
                '\\' => '\\',
                '"' => '"',
                '\'' => '\'',
                'u' when i + 4 < value.Length && int.TryParse(value.Substring(i + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code) => ReadUnicodeEscape(value, ref i, code),
                _ => next
            });
        }

        return sb.ToString();
    }

    private static char ReadUnicodeEscape(string value, ref int index, int code)
    {
        index += 4;
        return (char)code;
    }

    private static byte[] GuidToNetworkBytes(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return
        [
            bytes[3], bytes[2], bytes[1], bytes[0],
            bytes[5], bytes[4],
            bytes[7], bytes[6],
            bytes[8], bytes[9], bytes[10], bytes[11], bytes[12], bytes[13], bytes[14], bytes[15]
        ];
    }

    private static Guid NetworkBytesToGuid(byte[] bytes)
    {
        return new Guid([
            bytes[3], bytes[2], bytes[1], bytes[0],
            bytes[5], bytes[4],
            bytes[7], bytes[6],
            bytes[8], bytes[9], bytes[10], bytes[11], bytes[12], bytes[13], bytes[14], bytes[15]
        ]);
    }

    private static SemVersion ParseSemVersion(string value)
    {
        var match = Regex.Match(value.Trim(), @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<pre>[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+(?<build>[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$");
        if (!match.Success)
        {
            throw new FormatException("Version must follow SemVer, for example 1.2.3-beta.1+build.5.");
        }

        return new SemVersion(
            int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["minor"].Value, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["patch"].Value, CultureInfo.InvariantCulture),
            match.Groups["pre"].Value,
            match.Groups["build"].Value,
            value.Trim());
    }

    private static int CompareSemVersions(SemVersion left, SemVersion right)
    {
        var core = left.Major.CompareTo(right.Major);
        if (core != 0) return core;
        core = left.Minor.CompareTo(right.Minor);
        if (core != 0) return core;
        core = left.Patch.CompareTo(right.Patch);
        if (core != 0) return core;
        if (left.PreRelease.Length == 0 && right.PreRelease.Length == 0) return 0;
        if (left.PreRelease.Length == 0) return 1;
        if (right.PreRelease.Length == 0) return -1;
        var leftParts = left.PreRelease.Split('.');
        var rightParts = right.PreRelease.Split('.');
        for (var i = 0; i < Math.Min(leftParts.Length, rightParts.Length); i++)
        {
            var part = CompareSemVerIdentifier(leftParts[i], rightParts[i]);
            if (part != 0) return part;
        }

        return leftParts.Length.CompareTo(rightParts.Length);
    }

    private static int CompareSemVerIdentifier(string left, string right)
    {
        var leftNumeric = int.TryParse(left, NumberStyles.None, CultureInfo.InvariantCulture, out var leftNumber);
        var rightNumeric = int.TryParse(right, NumberStyles.None, CultureInfo.InvariantCulture, out var rightNumber);
        return (leftNumeric, rightNumeric) switch
        {
            (true, true) => leftNumber.CompareTo(rightNumber),
            (true, false) => -1,
            (false, true) => 1,
            _ => string.CompareOrdinal(left, right)
        };
    }

    private static byte[] ComputeDigest(string algorithm, byte[] bytes)
    {
        return algorithm switch
        {
            "MD5" => MD5.HashData(bytes),
            "SHA1" => SHA1.HashData(bytes),
            "SHA256" => SHA256.HashData(bytes),
            "SHA384" => SHA384.HashData(bytes),
            "SHA512" => SHA512.HashData(bytes),
            _ => ComputeBcDigest(algorithm, bytes)
        };
    }

    private static byte[] ComputeBcDigest(string algorithm, byte[] bytes)
    {
        var digest = CreateDigest(algorithm);
        digest.BlockUpdate(bytes, 0, bytes.Length);
        var result = new byte[digest.GetDigestSize()];
        digest.DoFinal(result, 0);
        return result;
    }

    private static IDigest CreateDigest(string algorithm)
    {
        return algorithm switch
        {
            "SHA224" => new Sha224Digest(),
            "SHA3-512" => new Sha3Digest(512),
            "RIPEMD160" => new RipeMD160Digest(),
            _ => new Sha3Digest(256)
        };
    }

    private static string ToHex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 365)
        {
            return $"{duration.TotalDays / 365:0.##} years";
        }
        if (duration.TotalDays >= 1)
        {
            return $"{duration.TotalDays:0.##} days";
        }
        if (duration.TotalHours >= 1)
        {
            return $"{duration.TotalHours:0.##} hours";
        }
        if (duration.TotalMinutes >= 1)
        {
            return $"{duration.TotalMinutes:0.##} minutes";
        }

        return $"{duration.TotalSeconds:0.##} seconds";
    }

    private static string RandomString(string alphabet, int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        }

        return new string(chars);
    }

    private static string DecodeJwtPart(string part)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(UndoBase64UrlSafe(part))));
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        return query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(parts => WebUtility.UrlDecode(parts[0]), parts => parts.Length > 1 ? WebUtility.UrlDecode(parts[1]) : string.Empty, StringComparer.OrdinalIgnoreCase);
    }

    private static Bitmap BytesToBitmap(byte[] bytes)
    {
        return new Bitmap(new MemoryStream(bytes));
    }

    private static string EscapeWifi(string value)
    {
        return value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace(":", "\\:");
    }

    private static uint ParseIpv4(string value)
    {
        if (!IPAddress.TryParse(value.Trim(), out var address) || address.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new FormatException("Invalid IPv4 address.");
        }

        var bytes = address.GetAddressBytes();
        return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
    }

    private static string FormatIpv4(uint value)
    {
        return string.Join('.', new[]
        {
            (value >> 24) & 255,
            (value >> 16) & 255,
            (value >> 8) & 255,
            value & 255
        });
    }

    private static IEnumerable<string> RangeToCidrs(uint start, uint end)
    {
        var current = (ulong)start;
        var last = (ulong)end;
        while (current <= last)
        {
            var maxSize = current == 0 ? 1UL << 32 : current & (~current + 1);
            var remaining = last - current + 1;
            while (maxSize > remaining)
            {
                maxSize >>= 1;
            }

            var prefix = 32 - (int)Math.Log2(maxSize);
            yield return $"{FormatIpv4((uint)current)}/{prefix}";
            current += maxSize;
        }
    }

    private static byte[] ParseMacPrefix(string prefix)
    {
        var normalized = NormalizeMac(prefix);
        if (normalized.Length == 0)
        {
            return [];
        }

        if (normalized.Length % 2 == 1)
        {
            normalized = normalized[..^1];
        }

        return Enumerable.Range(0, Math.Min(normalized.Length / 2, 6))
            .Select(i => Convert.ToByte(normalized.Substring(i * 2, 2), 16))
            .ToArray();
    }

    private static string NormalizeMac(string value) => Regex.Replace(value, "[^0-9a-fA-F]", "");

    private static DateTimeOffset ParseLooseDate(string value, DateTimeOffset fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
    }

    private static bool IsValidIban(string iban)
    {
        if (!Regex.IsMatch(iban, "^[A-Z]{2}[0-9]{2}[A-Z0-9]{10,30}$"))
        {
            return false;
        }

        var rearranged = iban[4..] + iban[..4];
        var remainder = 0;
        foreach (var ch in rearranged)
        {
            if (char.IsDigit(ch))
            {
                remainder = (remainder * 10 + ch - '0') % 97;
            }
            else
            {
                var value = ch - 'A' + 10;
                remainder = (remainder * 100 + value) % 97;
            }
        }

        return remainder == 1;
    }

    private static IEnumerable<HttpStatusInfo> HttpStatuses()
    {
        yield return new(100, "Continue", "Request headers received; continue with body.");
        yield return new(101, "Switching Protocols", "Server is switching protocols.");
        yield return new(102, "Processing", "Request is being processed.");
        yield return new(103, "Early Hints", "Preload hints before final response.");
        yield return new(200, "OK", "Request succeeded.");
        yield return new(201, "Created", "Resource was created.");
        yield return new(202, "Accepted", "Request accepted for processing.");
        yield return new(204, "No Content", "Request succeeded with no response body.");
        yield return new(301, "Moved Permanently", "Resource moved permanently.");
        yield return new(302, "Found", "Temporary redirect.");
        yield return new(304, "Not Modified", "Cached representation is still valid.");
        yield return new(307, "Temporary Redirect", "Repeat request with same method.");
        yield return new(308, "Permanent Redirect", "Repeat request with same method permanently.");
        yield return new(400, "Bad Request", "Malformed request.");
        yield return new(401, "Unauthorized", "Authentication is required.");
        yield return new(403, "Forbidden", "Authenticated client is not allowed.");
        yield return new(404, "Not Found", "Resource was not found.");
        yield return new(405, "Method Not Allowed", "HTTP method is not allowed.");
        yield return new(409, "Conflict", "Request conflicts with current state.");
        yield return new(410, "Gone", "Resource is no longer available.");
        yield return new(418, "I'm a teapot", "April Fools teapot status.");
        yield return new(422, "Unprocessable Content", "Semantic validation failed.");
        yield return new(429, "Too Many Requests", "Rate limit exceeded.");
        yield return new(500, "Internal Server Error", "Generic server error.");
        yield return new(501, "Not Implemented", "Server does not support functionality.");
        yield return new(502, "Bad Gateway", "Invalid upstream response.");
        yield return new(503, "Service Unavailable", "Server temporarily unavailable.");
        yield return new(504, "Gateway Timeout", "Upstream timeout.");
        yield return new(511, "Network Authentication Required", "Client must authenticate to gain network access.");
    }

    private sealed class MimeDbEntry
    {
        public string[]? Extensions { get; set; }
    }

    private sealed class EmojiInfo
    {
        public string? Name { get; set; }

        public string? Group { get; set; }
    }

    private sealed record HttpStatusInfo(int Code, string Name, string Description);

    private sealed record SemVersion(int Major, int Minor, int Patch, string PreRelease, string Build, string Original);

    private sealed class ExpressionParser
    {
        private readonly string _text;
        private int _position;

        public ExpressionParser(string text)
        {
            _text = text;
        }

        public double Parse()
        {
            var value = ParseExpression();
            Skip();
            if (_position != _text.Length)
            {
                throw new FormatException($"Unexpected token at position {_position}.");
            }

            return value;
        }

        private double ParseExpression()
        {
            var value = ParseTerm();
            while (true)
            {
                Skip();
                if (Match('+')) value += ParseTerm();
                else if (Match('-')) value -= ParseTerm();
                else return value;
            }
        }

        private double ParseTerm()
        {
            var value = ParsePower();
            while (true)
            {
                Skip();
                if (Match('*')) value *= ParsePower();
                else if (Match('/')) value /= ParsePower();
                else if (Match('%')) value %= ParsePower();
                else return value;
            }
        }

        private double ParsePower()
        {
            var value = ParseUnary();
            Skip();
            if (Match('^'))
            {
                value = Math.Pow(value, ParsePower());
            }

            return value;
        }

        private double ParseUnary()
        {
            Skip();
            if (Match('+')) return ParseUnary();
            if (Match('-')) return -ParseUnary();
            return ParsePrimary();
        }

        private double ParsePrimary()
        {
            Skip();
            if (Match('('))
            {
                var value = ParseExpression();
                if (!Match(')')) throw new FormatException("Missing closing parenthesis.");
                return value;
            }

            if (char.IsLetter(Peek()))
            {
                var name = ParseName();
                if (string.Equals(name, "pi", StringComparison.OrdinalIgnoreCase)) return Math.PI;
                if (string.Equals(name, "e", StringComparison.OrdinalIgnoreCase)) return Math.E;
                if (!Match('(')) throw new FormatException($"Function '{name}' requires parentheses.");
                var first = ParseExpression();
                double? second = null;
                if (Match(','))
                {
                    second = ParseExpression();
                }
                if (!Match(')')) throw new FormatException("Missing closing parenthesis.");
                return name.ToLowerInvariant() switch
                {
                    "sqrt" => Math.Sqrt(first),
                    "sin" => Math.Sin(first),
                    "cos" => Math.Cos(first),
                    "tan" => Math.Tan(first),
                    "abs" => Math.Abs(first),
                    "log" => second.HasValue ? Math.Log(first, second.Value) : Math.Log10(first),
                    "ln" => Math.Log(first),
                    "exp" => Math.Exp(first),
                    "floor" => Math.Floor(first),
                    "ceil" => Math.Ceiling(first),
                    "round" => Math.Round(first),
                    "pow" => Math.Pow(first, second ?? 2),
                    _ => throw new FormatException($"Unknown function '{name}'.")
                };
            }

            return ParseNumber();
        }

        private double ParseNumber()
        {
            Skip();
            var start = _position;
            while (_position < _text.Length && (char.IsDigit(_text[_position]) || _text[_position] is '.'))
            {
                _position++;
            }

            if (start == _position)
            {
                throw new FormatException($"Expected number at position {_position}.");
            }

            return double.Parse(_text[start.._position], CultureInfo.InvariantCulture);
        }

        private string ParseName()
        {
            var start = _position;
            while (_position < _text.Length && char.IsLetter(_text[_position]))
            {
                _position++;
            }

            return _text[start.._position];
        }

        private void Skip()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }
        }

        private char Peek()
        {
            Skip();
            return _position < _text.Length ? _text[_position] : '\0';
        }

        private bool Match(char ch)
        {
            Skip();
            if (_position >= _text.Length || _text[_position] != ch)
            {
                return false;
            }

            _position++;
            return true;
        }
    }
}

