using CodeWF.Modules.ToolFramework.Models;

namespace CodeWF.Modules.ToolFramework.Services;

public static class ToolCatalog
{
    public static readonly string[] CategoryOrder =
    [
        ToolCategories.Converter,
        ToolCategories.Development,
        ToolCategories.Security,
        ToolCategories.Web,
        ToolCategories.Media,
        ToolCategories.Network,
        ToolCategories.Math,
        ToolCategories.Measurement,
        ToolCategories.Text,
        ToolCategories.Data
    ];

    public static IEnumerable<Func<ToolSpec>> CreateFactories()
    {
        yield return () => Spec("base64-file-converter", "Converter", "Base64 file converter", "Convert a file to base64 or decode base64 into a file.",
            [File("file", "Input file"), Multi("base64", "Base64 input"), SaveFile("save", "Save decoded file as"), Select("mode", "Mode", ["File to Base64", "Base64 to File"])],
            [Output("result", "Result")], () => ToolAlgorithms.Base64FileAsync, autoRun: false);
        yield return () => Spec("base64-string-converter", "Converter", "Base64 string encoder/decoder", "Encode and decode strings into base64, including URL-safe base64.",
            [Multi("text", "Input"), Bool("urlSafe", "URL safe"), Select("mode", "Mode", ["Encode", "Decode"])],
            [Output("result", "Result")], () => ToolAlgorithms.Base64StringAsync);
        yield return () => Spec("case-converter", "Converter", "Case converter", "Transform text between common naming conventions.",
            [Multi("text", "Text")], [Output("result", "Result")], () => ToolAlgorithms.CaseConverterAsync);
        yield return () => Spec("color-converter", "Converter", "Color converter", "Convert color between hex, RGB, HSL and known CSS names.",
            [Text("color", "Color", "#3498db")], [Output("result", "Result")], () => ToolAlgorithms.ColorConverterAsync);
        yield return () => Spec("date-time-converter", "Converter", "Date-time converter", "Convert date and time into ISO, RFC, timestamp, Mongo ObjectId and Excel formats.",
            [Text("date", "Date", "", "Leave empty for current time"), Select("format", "Input format", ["Auto", "ISO 8601", "Unix timestamp", "Timestamp milliseconds", "RFC 1123/RFC 7231", "Mongo ObjectId", "Excel date/time"])],
            [Output("result", "Converted formats")], () => ToolAlgorithms.DateTimeConverterAsync);
        yield return () => Spec("integer-base-converter", "Converter", "Integer base converter", "Convert a number between decimal, hexadecimal, binary, octal and base64.",
            [Text("number", "Number", "42"), Number("fromBase", "Input base", 10), Number("toBase", "Output base", 16)],
            [Output("result", "Result")], () => ToolAlgorithms.IntegerBaseConverterAsync);
        yield return () => Spec("json-to-toml", "Converter", "JSON to TOML", "Parse and convert JSON to TOML.",
            [Multi("json", "JSON")], [Output("result", "TOML")], () => ToolAlgorithms.JsonToTomlAsync);
        yield return () => Spec("json-to-xml", "Converter", "JSON to XML", "Convert JSON to XML.",
            [Multi("json", "JSON")], [Output("result", "XML")], () => ToolAlgorithms.JsonToXmlAsync);
        yield return () => Spec("json-to-yaml-converter", "Converter", "JSON to YAML converter", "Convert JSON to YAML.",
            [Multi("json", "JSON")], [Output("result", "YAML")], () => ToolAlgorithms.JsonToYamlAsync);
        yield return () => Spec("list-converter", "Converter", "List converter", "Apply sorting, reversing, prefix/suffix, casing and transpose operations to line-based data.",
            [Multi("list", "List"), Text("prefix", "Prefix"), Text("suffix", "Suffix"), Number("truncate", "Truncate length", 0), Select("operation", "Operation", ["None", "Sort", "Reverse", "Lowercase", "Uppercase", "Transpose", "Unique"])],
            [Output("result", "Result")], () => ToolAlgorithms.ListConverterAsync);
        yield return () => Spec("markdown-to-html", "Converter", "Markdown to HTML", "Convert Markdown to HTML.",
            [Multi("markdown", "Markdown")], [Output("result", "HTML")], () => ToolAlgorithms.MarkdownToHtmlAsync);
        yield return () => Spec("roman-numeral-converter", "Converter", "Roman numeral converter", "Convert Roman numerals to numbers and numbers to Roman numerals.",
            [Text("value", "Value", "2026"), Select("mode", "Mode", ["Number to Roman", "Roman to Number"])],
            [Output("result", "Result")], () => ToolAlgorithms.RomanNumeralAsync);
        yield return () => Spec("temperature-converter", "Measurement", "Temperature converter", "Convert Kelvin, Celsius, Fahrenheit, Rankine, Delisle, Newton, Reaumur and Romer.",
            [Number("value", "Value", 0), Select("unit", "Unit", ["Celsius", "Kelvin", "Fahrenheit", "Rankine", "Delisle", "Newton", "Reaumur", "Romer"])],
            [Output("result", "Result")], () => ToolAlgorithms.TemperatureAsync);
        yield return () => Spec("text-to-binary", "Converter", "Text to ASCII binary", "Convert text to its ASCII/UTF-8 binary representation and back.",
            [Multi("text", "Input"), Select("mode", "Mode", ["Text to Binary", "Binary to Text"])],
            [Output("result", "Result")], () => ToolAlgorithms.TextBinaryAsync);
        yield return () => Spec("text-to-nato-alphabet", "Converter", "Text to NATO alphabet", "Transform text into the NATO phonetic alphabet.",
            [Multi("text", "Text")], [Output("result", "Result")], () => ToolAlgorithms.NatoAsync);
        yield return () => Spec("text-to-unicode", "Converter", "Text to Unicode", "Convert text to Unicode escape sequences and back.",
            [Multi("text", "Input"), Select("mode", "Mode", ["Text to Unicode", "Unicode to Text"])],
            [Output("result", "Result")], () => ToolAlgorithms.UnicodeAsync);
        yield return () => Spec("toml-to-json", "Converter", "TOML to JSON", "Parse and convert TOML to JSON.",
            [Multi("toml", "TOML")], [Output("result", "JSON")], () => ToolAlgorithms.TomlToJsonAsync);
        yield return () => Spec("toml-to-yaml", "Converter", "TOML to YAML", "Parse and convert TOML to YAML.",
            [Multi("toml", "TOML")], [Output("result", "YAML")], () => ToolAlgorithms.TomlToYamlAsync);
        yield return () => Spec("xml-to-json", "Converter", "XML to JSON", "Convert XML to JSON.",
            [Multi("xml", "XML")], [Output("result", "JSON")], () => ToolAlgorithms.XmlToJsonAsync);
        yield return () => Spec("yaml-to-json-converter", "Converter", "YAML to JSON converter", "Convert YAML to JSON.",
            [Multi("yaml", "YAML")], [Output("result", "JSON")], () => ToolAlgorithms.YamlToJsonAsync);
        yield return () => Spec("yaml-to-toml", "Converter", "YAML to TOML", "Parse and convert YAML to TOML.",
            [Multi("yaml", "YAML")], [Output("result", "TOML")], () => ToolAlgorithms.YamlToTomlAsync);

        yield return () => Spec("chmod-calculator", "Development", "Chmod calculator", "Compute chmod permissions and commands.",
            [Bool("ur", "Owner read"), Bool("uw", "Owner write"), Bool("ux", "Owner execute"), Bool("gr", "Group read"), Bool("gw", "Group write"), Bool("gx", "Group execute"), Bool("or", "Others read"), Bool("ow", "Others write"), Bool("ox", "Others execute")],
            [Output("result", "Result")], () => ToolAlgorithms.ChmodAsync);
        yield return () => Spec("crontab-generator", "Development", "Crontab generator", "Validate crontab expressions and generate a human-readable description.",
            [Text("cron", "Cron expression", "*/5 * * * *")], [Output("result", "Result")], () => ToolAlgorithms.CrontabAsync);
        yield return () => Spec("csv-to-json", "Development", "CSV to JSON", "Convert CSV, TSV or delimited text into JSON.",
            [Multi("csv", "CSV"), Select("delimiter", "Delimiter", ["Auto", "Comma", "Semicolon", "Tab", "Pipe"]), Bool("header", "First row is header", true)],
            [Output("result", "JSON")], () => ToolAlgorithms.CsvToJsonAsync);
        yield return () => Spec("csv-to-markdown-table", "Development", "CSV to Markdown table", "Convert CSV, TSV or delimited text into a Markdown table.",
            [Multi("csv", "CSV"), Select("delimiter", "Delimiter", ["Auto", "Comma", "Semicolon", "Tab", "Pipe"])],
            [Output("result", "Markdown")], () => ToolAlgorithms.CsvToMarkdownAsync);
        yield return () => Spec("data-url-parser", "Development", "Data URL parser", "Inspect data: URLs and preview decoded payloads.",
            [Multi("dataUrl", "Data URL")], [Output("result", "Parsed data URL")], () => ToolAlgorithms.DataUrlParserAsync);
        yield return () => Spec("docker-run-to-docker-compose-converter", "Development", "Docker run to Docker compose converter", "Transform docker run commands into docker-compose YAML.",
            [Multi("command", "docker run command", "docker run -d --name app -p 8080:80 nginx")],
            [Output("result", "docker-compose.yml")], () => ToolAlgorithms.DockerComposeAsync);
        yield return () => Spec("docker-image-tag-parser", "Development", "Docker image tag parser", "Parse Docker image references into registry, repository, tag and digest.",
            [Text("image", "Image reference", "ghcr.io/dotnet9/codewf-toolbox:latest")],
            [Output("result", "Image parts")], () => ToolAlgorithms.DockerImageTagParserAsync);
        yield return () => Spec("email-normalizer", "Development", "Email normalizer", "Normalize email addresses for easier comparison and deduplication.",
            [Multi("emails", "Email addresses")], [Output("result", "Normalized")], () => ToolAlgorithms.EmailNormalizerAsync);
        yield return () => Spec("env-to-json", "Development", ".env to JSON", "Parse .env files into JSON objects.",
            [Multi("env", ".env content")], [Output("result", "JSON")], () => ToolAlgorithms.EnvToJsonAsync);
        yield return () => Spec("git-memo", "Development", "Git cheatsheet", "Quick access to common git commands.",
            [], [Output("result", "Cheatsheet")], () => ToolAlgorithms.GitMemoAsync);
        yield return () => Spec("hex-dump-viewer", "Development", "Hex dump viewer", "Render text or hexadecimal bytes as a classic hex dump.",
            [Multi("input", "Input"), Select("mode", "Mode", ["Text", "Hex"]), Number("bytesPerLine", "Bytes per line", 16)],
            [Output("result", "Hex dump")], () => ToolAlgorithms.HexDumpAsync, autoRun: false);
        yield return () => Spec("http-header-parser", "Development", "HTTP header parser", "Parse raw HTTP headers into structured JSON.",
            [Multi("headers", "Raw headers", "Content-Type: application/json\nX-Request-Id: abc")],
            [Output("result", "Headers")], () => ToolAlgorithms.HttpHeaderParserAsync);
        yield return () => Spec("ini-to-json", "Development", "INI to JSON", "Parse INI configuration text into JSON.",
            [Multi("ini", "INI content")], [Output("result", "JSON")], () => ToolAlgorithms.IniToJsonAsync);
        yield return () => Spec("json-diff", "Web", "JSON diff", "Compare two JSON values and show line-level differences after formatting.",
            [Multi("left", "Left JSON"), Multi("right", "Right JSON")], [Output("result", "Diff")], () => ToolAlgorithms.JsonDiffAsync);
        yield return () => Spec("json-minify", "Development", "JSON minify", "Minify JSON by removing unnecessary whitespace.",
            [Multi("json", "JSON")], [Output("result", "Minified JSON")], () => ToolAlgorithms.JsonMinifyAsync);
        yield return () => Spec("json-path-extractor", "Development", "JSON path extractor", "Extract a value using a simple dot and index path.",
            [Multi("json", "JSON"), Text("path", "Path", "$.items[0].name")],
            [Output("result", "Matched value")], () => ToolAlgorithms.JsonPathExtractorAsync);
        yield return () => Spec("json-to-csv", "Development", "JSON to CSV", "Convert JSON arrays or objects to CSV with automatic header detection.",
            [Multi("json", "JSON")], [Output("result", "CSV")], () => ToolAlgorithms.JsonToCsvAsync);
        yield return () => Spec("json-viewer", "Development", "JSON prettify and format", "Prettify JSON into a readable format.",
            [Multi("json", "JSON")], [Output("result", "Formatted JSON")], () => ToolAlgorithms.JsonViewerAsync);
        yield return () => Spec("markdown-table-generator", "Development", "Markdown table generator", "Build a Markdown table from headers and rows.",
            [Text("headers", "Headers", "Name,Value"), Multi("rows", "Rows", "CodeWF,Toolbox\nAvalonia,Prism"), Select("alignment", "Alignment", ["None", "Left", "Center", "Right"]), Select("delimiter", "Delimiter", ["Comma", "Semicolon", "Tab", "Pipe"])],
            [Output("result", "Markdown")], () => ToolAlgorithms.MarkdownTableGeneratorAsync);
        yield return () => Spec("nanoid-generator", "Development", "NanoID generator", "Generate compact URL-friendly random IDs.",
            [Number("length", "Length", 21), Number("count", "Count", 5), Text("alphabet", "Alphabet", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz-")],
            [Output("result", "IDs")], () => ToolAlgorithms.NanoidAsync, autoRun: false);
        yield return () => Spec("query-string-builder", "Development", "Query string builder", "Build an encoded query string from key-value lines.",
            [Multi("pairs", "Parameters", "page=1\nq=CodeWF"), Bool("includeQuestionMark", "Include leading ?", true)],
            [Output("result", "Query string")], () => ToolAlgorithms.QueryStringBuilderAsync);
        yield return () => Spec("query-string-parser", "Development", "Query string parser", "Decode query strings or URLs into structured parameters.",
            [Multi("query", "Query string or URL")], [Output("result", "Parameters")], () => ToolAlgorithms.QueryStringParserAsync);
        yield return () => Spec("random-port-generator", "Development", "Random port generator", "Generate random ports outside the known-port range.",
            [Number("count", "Count", 5), Number("min", "Min", 1024), Number("max", "Max", 65535)],
            [Output("result", "Ports")], () => ToolAlgorithms.RandomPortAsync, autoRun: false);
        yield return () => Spec("regex-memo", "Development", "Regex cheatsheet", "Regular expression cheatsheet.",
            [], [Output("result", "Cheatsheet")], () => ToolAlgorithms.RegexMemoAsync);
        yield return () => Spec("regex-replacer", "Development", "Regex replacer", "Preview regular-expression replacements against sample text.",
            [Text("pattern", "Pattern"), Text("replacement", "Replacement"), Multi("text", "Sample text"), Bool("ignoreCase", "Ignore case"), Bool("multiline", "Multiline")],
            [Output("result", "Result")], () => ToolAlgorithms.RegexReplacerAsync);
        yield return () => Spec("regex-tester", "Development", "Regex Tester", "Test regular expressions with sample text.",
            [Text("pattern", "Pattern"), Multi("text", "Sample text"), Bool("ignoreCase", "Ignore case"), Bool("multiline", "Multiline")],
            [Output("result", "Matches")], () => ToolAlgorithms.RegexTesterAsync);
        yield return () => Spec("semver-comparator", "Development", "SemVer comparator", "Compare two semantic versions using SemVer precedence rules.",
            [Text("left", "Left version", "1.2.3"), Text("right", "Right version", "1.2.4-beta.1")],
            [Output("result", "Comparison")], () => ToolAlgorithms.SemVerComparatorAsync);
        yield return () => Spec("semver-inspector", "Development", "SemVer inspector", "Parse a semantic version and show common bump targets.",
            [Text("version", "Version", "1.2.3-beta.1+build.5")],
            [Output("result", "Version details")], () => ToolAlgorithms.SemVerInspectorAsync);
        yield return () => Spec("sql-prettify", "Development", "SQL prettify and format", "Format and prettify SQL queries.",
            [Multi("sql", "SQL")], [Output("result", "Formatted SQL")], () => ToolAlgorithms.SqlPrettifyAsync);
        yield return () => Spec("string-escape-unescape", "Development", "String escape/unescape", "Escape or unescape text for JSON, C#, HTML and URLs.",
            [Multi("text", "Input"), Select("format", "Format", ["JSON", "C#", "HTML", "URL"]), Select("mode", "Mode", ["Escape", "Unescape"])],
            [Output("result", "Result")], () => ToolAlgorithms.StringEscapeAsync);
        yield return () => Spec("uuid-v5-generator", "Development", "UUID v5 generator", "Generate deterministic namespace/name UUID v5 identifiers.",
            [Text("namespace", "Namespace UUID", "6ba7b810-9dad-11d1-80b4-00c04fd430c8"), Text("name", "Name", "codewf.com")],
            [Output("result", "UUID")], () => ToolAlgorithms.UuidV5Async);
        yield return () => Spec("xml-xpath-tester", "Development", "XML XPath tester", "Evaluate XPath expressions against XML text.",
            [Multi("xml", "XML"), Text("xpath", "XPath", "//item")],
            [Output("result", "Matches")], () => ToolAlgorithms.XmlXPathTesterAsync);
        yield return () => Spec("xml-formatter", "Development", "XML formatter", "Prettify XML into a readable format.",
            [Multi("xml", "XML")], [Output("result", "Formatted XML")], () => ToolAlgorithms.XmlFormatterAsync);
        yield return () => Spec("yaml-viewer", "Development", "YAML prettify and format", "Prettify YAML into a readable format.",
            [Multi("yaml", "YAML")], [Output("result", "Formatted YAML")], () => ToolAlgorithms.YamlViewerAsync);

        yield return () => Spec("bcrypt", ToolCategories.Security, "Bcrypt", "Hash and compare text using bcrypt.",
            [Text("text", "Text"), Text("hash", "Hash to compare"), Number("rounds", "Rounds", 10), Select("mode", "Mode", ["Hash", "Compare"])],
            [Output("result", "Result")], () => ToolAlgorithms.BCryptAsync, autoRun: false);
        yield return () => Spec("bip39-generator", ToolCategories.Security, "BIP39 passphrase generator", "Generate a BIP39 mnemonic and seed.",
            [Number("words", "Word count", 12), Text("passphrase", "Passphrase")], [Output("result", "Mnemonic and seed")], () => ToolAlgorithms.Bip39Async, autoRun: false);
        yield return () => Spec("encryption", ToolCategories.Security, "Encrypt / decrypt text", "Encrypt clear text and decrypt ciphertext using AES or TripleDES.",
            [Multi("text", "Input"), Text("password", "Password"), Select("algorithm", "Algorithm", ["AES", "TripleDES"]), Select("mode", "Mode", ["Encrypt", "Decrypt"])],
            [Output("result", "Result")], () => ToolAlgorithms.EncryptionAsync, autoRun: false);
        yield return () => Spec("hash-text", ToolCategories.Security, "Hash text", "Hash a text string using MD5, SHA1, SHA2, SHA3 or RIPEMD160.",
            [Multi("text", "Text"), Select("algorithm", "Algorithm", ["MD5", "SHA1", "SHA224", "SHA256", "SHA384", "SHA512", "SHA3-256", "SHA3-512", "RIPEMD160"])],
            [Output("result", "Digest")], () => ToolAlgorithms.HashTextAsync);
        yield return () => Spec("hmac-generator", ToolCategories.Security, "Hmac generator", "Compute an HMAC with a secret key and selected hash function.",
            [Multi("text", "Message"), Text("key", "Secret key"), Select("algorithm", "Algorithm", ["MD5", "SHA1", "SHA256", "SHA384", "SHA512", "SHA3-256", "RIPEMD160"])],
            [Output("result", "HMAC")], () => ToolAlgorithms.HmacAsync);
        yield return () => Spec("password-strength-analyser", ToolCategories.Security, "Password strength analyser", "Estimate password strength, entropy and rough crack time.",
            [Text("password", "Password")], [Output("result", "Analysis")], () => ToolAlgorithms.PasswordStrengthAsync);
        yield return () => Spec("pdf-signature-checker", ToolCategories.Security, "PDF signature checker", "Inspect and verify embedded PDF signature containers when possible.",
            [File("file", "PDF file")], [Output("result", "Signature details")], () => ToolAlgorithms.PdfSignatureAsync, autoRun: false);
        yield return () => Spec("rsa-key-pair-generator", ToolCategories.Security, "RSA key pair generator", "Generate RSA private and public PEM key pairs.",
            [Number("size", "Key size", 2048)], [Output("result", "PEM keys")], () => ToolAlgorithms.RsaAsync, autoRun: false);
        yield return () => Spec("token-generator", ToolCategories.Security, "Token generator", "Generate random strings with chosen character sets.",
            [Number("length", "Length", 32), Bool("upper", "Uppercase"), Bool("lower", "Lowercase"), Bool("numbers", "Numbers"), Bool("symbols", "Symbols")],
            [Output("result", "Token")], () => ToolAlgorithms.TokenAsync, autoRun: false);
        yield return () => Spec("ulid-generator", ToolCategories.Security, "ULID generator", "Generate Universally Unique Lexicographically Sortable Identifiers.",
            [Number("count", "Count", 5)], [Output("result", "ULIDs")], () => ToolAlgorithms.UlidAsync, autoRun: false);
        yield return () => Spec("uuid-generator", ToolCategories.Security, "UUIDs generator", "Generate random UUID v4 identifiers.",
            [Number("count", "Count", 5), Bool("uppercase", "Uppercase")], [Output("result", "UUIDs")], () => ToolAlgorithms.UuidAsync, autoRun: false);

        yield return () => Spec("basic-auth-generator", "Web", "Basic auth generator", "Generate a Basic Authorization header from username and password.",
            [Text("username", "Username"), Text("password", "Password")], [Output("result", "Header")], () => ToolAlgorithms.BasicAuthAsync);
        yield return () => Spec("device-information", "Web", "Device information", "Get information about the current desktop runtime and system.",
            [], [Output("result", "Device information")], () => ToolAlgorithms.DeviceInfoAsync);
        yield return () => Spec("html-entities", "Web", "Escape HTML entities", "Escape or unescape HTML entities.",
            [Multi("text", "Input"), Select("mode", "Mode", ["Escape", "Unescape"])], [Output("result", "Result")], () => ToolAlgorithms.HtmlEntitiesAsync);
        yield return () => Spec("html-wysiwyg-editor", "Web", "HTML WYSIWYG editor", "Generate HTML from rich-text style content blocks.",
            [Multi("content", "Content"), Select("mode", "Block type", ["Paragraphs", "Unordered list", "Ordered list", "Raw HTML preview"])],
            [Output("result", "HTML source")], () => ToolAlgorithms.HtmlEditorAsync);
        yield return () => Spec("http-status-codes", "Web", "HTTP status codes", "List and search HTTP status codes.",
            [Text("query", "Search")], [Output("result", "Status codes")], () => ToolAlgorithms.HttpStatusAsync);
        yield return () => Spec("jwt-parser", "Web", "JWT parser", "Parse and decode JSON Web Tokens.",
            [Multi("jwt", "JWT")], [Output("result", "Decoded JWT")], () => ToolAlgorithms.JwtAsync);
        yield return () => Spec("keycode-info", "Web", "Keycode info", "Focus this view and press a key to inspect key information.",
            [], [Output("result", "Result")], () => ToolAlgorithms.KeycodeInfoAsync);
        yield return () => Spec("meta-tag-generator", "Web", "Open graph meta generator", "Generate Open Graph and Twitter meta tags.",
            [Text("title", "Title"), Text("description", "Description"), Text("url", "URL"), Text("image", "Image URL"), Text("site", "Site name"), Select("type", "Type", ["website", "article", "profile", "book", "music.song", "video.movie"])],
            [Output("result", "Meta tags")], () => ToolAlgorithms.MetaTagsAsync);
        yield return () => Spec("mime-types", "Web", "MIME types", "Convert MIME types to file extensions and vice versa.",
            [Text("query", "MIME type or extension")], [Output("result", "Matches")], () => ToolAlgorithms.MimeTypesAsync);
        yield return () => Spec("otp-code-generator-and-validator", "Web", "OTP code generator", "Generate and validate time-based one-time passwords.",
            [Text("secret", "Secret (Base32)"), Number("digits", "Digits", 6), Number("period", "Period seconds", 30), Text("code", "Code to validate")],
            [Output("result", "OTP")], () => ToolAlgorithms.OtpAsync);
        yield return () => Spec("safelink-decoder", "Web", "Outlook Safelink decoder", "Decode Outlook SafeLink URLs.",
            [Multi("url", "SafeLink URL")], [Output("result", "Decoded URL")], () => ToolAlgorithms.SafelinkAsync);
        yield return () => Spec("slugify-string", "Web", "Slugify string", "Make a string URL, filename and id safe.",
            [Text("text", "Text"), Bool("lower", "Lowercase")], [Output("result", "Slug")], () => ToolAlgorithms.SlugifyAsync);
        yield return () => Spec("url-encoder", "Web", "Encode/decode URL-formatted strings", "Encode text to percent-encoded format or decode from it.",
            [Multi("text", "Input"), Select("mode", "Mode", ["Encode", "Decode"])], [Output("result", "Result")], () => ToolAlgorithms.UrlEncoderAsync);
        yield return () => Spec("url-parser", "Web", "URL parser", "Parse a URL into its constituent parts.",
            [Text("url", "URL")], [Output("result", "URL parts")], () => ToolAlgorithms.UrlParserAsync);
        yield return () => Spec("user-agent-parser", "Web", "User-agent parser", "Parse browser, engine, OS, CPU and device data from a user-agent string.",
            [Multi("userAgent", "User-agent")], [Output("result", "Parsed user-agent")], () => ToolAlgorithms.UserAgentAsync);

        yield return () => Spec("qr-code-generator", ToolCategories.Media, "QR Code generator", "Generate a QR code for text or URL.",
            [Multi("text", "Text"), Number("pixels", "Pixels per module", 10)], [ImageOutput("image", "QR Code"), Output("result", "Payload")], () => ToolAlgorithms.QrCodeAsync);
        yield return () => Spec("svg-placeholder-generator", ToolCategories.Media, "SVG placeholder generator", "Generate SVG placeholder images.",
            [Number("width", "Width", 800), Number("height", "Height", 400), Text("text", "Text", "800 x 400"), Text("bg", "Background", "#dddddd"), Text("fg", "Foreground", "#555555")],
            [Output("result", "SVG")], () => ToolAlgorithms.SvgPlaceholderAsync);
        yield return () => Spec("wifi-qr-code-generator", ToolCategories.Media, "WiFi QR Code generator", "Generate QR codes for quick WiFi connections.",
            [Text("ssid", "SSID"), Text("password", "Password"), Select("auth", "Authentication", ["WPA", "WEP", "nopass"]), Bool("hidden", "Hidden SSID")],
            [ImageOutput("image", "QR Code"), Output("result", "WiFi payload")], () => ToolAlgorithms.WifiQrAsync);

        yield return () => Spec("ipv4-address-converter", "Network", "IPv4 address converter", "Convert an IPv4 address into decimal, binary, hexadecimal and IPv6-mapped forms.",
            [Text("ip", "IPv4 address", "192.168.1.1")], [Output("result", "Result")], () => ToolAlgorithms.Ipv4AddressAsync);
        yield return () => Spec("ipv4-range-expander", "Network", "IPv4 range expander", "Calculate CIDR blocks covering an IPv4 start/end range.",
            [Text("start", "Start IPv4", "192.168.1.0"), Text("end", "End IPv4", "192.168.1.255")],
            [Output("result", "CIDR ranges")], () => ToolAlgorithms.Ipv4RangeAsync);
        yield return () => Spec("ipv4-subnet-calculator", "Network", "IPv4 subnet calculator", "Parse IPv4 CIDR blocks and show subnet details.",
            [Text("cidr", "CIDR", "192.168.1.0/24")], [Output("result", "Subnet")], () => ToolAlgorithms.Ipv4SubnetAsync);
        yield return () => Spec("ipv6-ula-generator", "Network", "IPv6 ULA generator", "Generate local IPv6 ULA prefixes according to RFC 4193.",
            [Number("count", "Count", 3)], [Output("result", "ULA prefixes")], () => ToolAlgorithms.Ipv6UlaAsync, autoRun: false);
        yield return () => Spec("mac-address-generator", "Network", "MAC address generator", "Generate MAC addresses with an optional prefix.",
            [Number("count", "Count", 5), Text("prefix", "Prefix"), Bool("uppercase", "Uppercase")], [Output("result", "MAC addresses")], () => ToolAlgorithms.MacGeneratorAsync, autoRun: false);
        yield return () => Spec("mac-address-lookup", "Network", "MAC address lookup", "Find the vendor/manufacturer of a device by MAC address prefix.",
            [Text("mac", "MAC address")], [Output("result", "Vendor")], () => ToolAlgorithms.MacLookupAsync);

        yield return () => Spec("eta-calculator", "Math", "ETA calculator", "Estimate end time and duration from progress and rate.",
            [Number("done", "Done", 50), Number("total", "Total", 100), Number("rate", "Units per second", 1)],
            [Output("result", "ETA")], () => ToolAlgorithms.EtaAsync);
        yield return () => Spec("math-evaluator", "Math", "Math evaluator", "Evaluate mathematical expressions with common functions.",
            [Text("expression", "Expression", "sqrt(16)+cos(0)")], [Output("result", "Result")], () => ToolAlgorithms.MathEvalAsync);
        yield return () => Spec("percentage-calculator", "Math", "Percentage calculator", "Calculate percentages and percentage changes.",
            [Number("a", "Value A", 20), Number("b", "Value B", 100)], [Output("result", "Result")], () => ToolAlgorithms.PercentageAsync);

        yield return () => Spec("benchmark-builder", "Measurement", "Benchmark builder", "Run simple managed-code loop benchmarks.",
            [Number("iterations", "Iterations", 1000000), Select("task", "Task", ["String concat", "StringBuilder", "Random numbers", "SHA256 hash"])],
            [Output("result", "Benchmark")], () => ToolAlgorithms.BenchmarkAsync, autoRun: false);
        yield return () => Spec("chronometer", "Measurement", "Chronometer", "Measure elapsed time between two timestamps.",
            [Text("start", "Start time", ""), Text("end", "End time", "")], [Output("result", "Duration")], () => ToolAlgorithms.ChronometerAsync);

        yield return () => Spec("ascii-text-drawer", "Text", "ASCII Art Text Generator", "Create ASCII art text with Figlet fonts.",
            [Text("text", "Text", "CodeWF"), Select("font", "Font", ["Standard", "Slant", "Small", "Big"])],
            [Output("result", "ASCII art")], () => ToolAlgorithms.AsciiArtAsync);
        yield return () => Spec("emoji-picker", "Text", "Emoji picker", "Search emojis and inspect Unicode code points.",
            [Text("query", "Search")], [Output("result", "Emojis")], () => ToolAlgorithms.EmojiAsync);
        yield return () => Spec("lorem-ipsum-generator", "Text", "Lorem ipsum generator", "Generate placeholder text.",
            [Number("paragraphs", "Paragraphs", 3)], [Output("result", "Lorem ipsum")], () => ToolAlgorithms.LoremAsync, autoRun: false);
        yield return () => Spec("numeronym-generator", "Text", "Numeronym generator", "Generate numeronyms like i18n and a11y.",
            [Multi("text", "Words")], [Output("result", "Numeronyms")], () => ToolAlgorithms.NumeronymAsync);
        yield return () => Spec("string-obfuscator", "Text", "String obfuscator", "Obfuscate a secret while keeping it identifiable.",
            [Text("text", "Text"), Number("keepStart", "Keep start", 3), Number("keepEnd", "Keep end", 3), Text("mask", "Mask", "*")],
            [Output("result", "Obfuscated")], () => ToolAlgorithms.StringObfuscatorAsync);
        yield return () => Spec("text-diff", "Text", "Text diff", "Compare two texts and show line-level differences.",
            [Multi("left", "Left text"), Multi("right", "Right text")], [Output("result", "Diff")], () => ToolAlgorithms.TextDiffAsync);
        yield return () => Spec("text-statistics", "Text", "Text statistics", "Count characters, words, lines and bytes.",
            [Multi("text", "Text")], [Output("result", "Statistics")], () => ToolAlgorithms.TextStatsAsync);

        yield return () => Spec("iban-validator-and-parser", "Data", "IBAN validator and parser", "Validate and parse IBAN numbers.",
            [Text("iban", "IBAN")], [Output("result", "IBAN")], () => ToolAlgorithms.IbanAsync);
        yield return () => Spec("phone-parser-and-formatter", "Data", "Phone parser and formatter", "Parse, validate and format phone numbers.",
            [Text("phone", "Phone number"), Text("region", "Default region", "US")], [Output("result", "Phone")], () => ToolAlgorithms.PhoneAsync);
    }

    private static ToolSpec Spec(
        string id,
        string category,
        string name,
        string description,
        IEnumerable<ToolField> fields,
        IEnumerable<ToolOutput> outputs,
        Func<Func<ToolRunContext, CancellationToken, Task>> runFactory,
        bool autoRun = true)
    {
        var spec = new ToolSpec
        {
            Id = id,
            Category = category,
            Name = ToolLocalization.ToolTitle(id),
            Description = ToolLocalization.ToolDescription(id),
            Icon = IconForTool(id, category),
            RunAsync = LazyRun(runFactory),
            AutoRun = autoRun
        };
        spec.Fields.AddRange(fields);
        spec.Outputs.AddRange(outputs);
        ApplyLocalizationKeys(spec);
        return spec;
    }

    private static Func<ToolRunContext, CancellationToken, Task> LazyRun(
        Func<Func<ToolRunContext, CancellationToken, Task>> runFactory)
    {
        Func<ToolRunContext, CancellationToken, Task>? run = null;
        return (context, token) =>
        {
            run ??= runFactory();
            return run(context, token);
        };
    }

    public static IReadOnlyList<ToolSpec> GetByCategory(string category)
    {
        return CreateFactories()
            .Select(factory => factory())
            .Where(tool => string.Equals(tool.Category, category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(tool => tool.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static void ApplyLocalizationKeys(ToolSpec spec)
    {
        foreach (var field in spec.Fields)
        {
            field.Label = ToolLocalization.FieldLabel(spec.Id, field.Id);

            if (!string.IsNullOrWhiteSpace(field.Placeholder))
            {
                field.Placeholder = ToolLocalization.FieldPlaceholder(spec.Id, field.Id);
            }

            foreach (var option in field.Options)
            {
                option.Label = ToolLocalization.OptionLabel(spec.Id, field.Id, option.Value);
            }
        }

        foreach (var output in spec.Outputs)
        {
            output.Label = ToolLocalization.OutputLabel(spec.Id, output.Id);
        }
    }

    private static ToolField Text(string id, string label, string value = "", string placeholder = "") => new()
    {
        Id = id,
        Label = label,
        Text = value,
        Placeholder = placeholder,
        Kind = ToolFieldKind.Text
    };

    private static ToolField Multi(string id, string label, string value = "", string placeholder = "") => new()
    {
        Id = id,
        Label = label,
        Text = value,
        Placeholder = placeholder,
        Kind = ToolFieldKind.MultiLine
    };

    private static ToolField Number(string id, string label, decimal value) => new()
    {
        Id = id,
        Label = label,
        Number = value,
        Kind = ToolFieldKind.Number
    };

    private static ToolField Bool(string id, string label, bool value = false) => new()
    {
        Id = id,
        Label = label,
        Boolean = value,
        Kind = ToolFieldKind.Boolean
    };

    private static ToolField Select(string id, string label, IReadOnlyList<string> values)
    {
        var field = new ToolField
        {
            Id = id,
            Label = label,
            Kind = ToolFieldKind.Select
        };
        foreach (var value in values)
        {
            field.Options.Add(new ToolOption(value, value));
        }

        field.SelectedOption = field.Options.FirstOrDefault();
        return field;
    }

    private static ToolField File(string id, string label) => new()
    {
        Id = id,
        Label = label,
        Kind = ToolFieldKind.File
    };

    private static ToolField SaveFile(string id, string label) => new()
    {
        Id = id,
        Label = label,
        Kind = ToolFieldKind.SaveFile
    };

    private static ToolOutput Output(string id, string label) => new()
    {
        Id = id,
        Label = label
    };

    private static ToolOutput ImageOutput(string id, string label) => new()
    {
        Id = id,
        Label = label,
        IsMultiline = false
    };

    public static string IconForCategory(string category)
    {
        return category switch
        {
            ToolCategories.Security => Icons.Crypto,
            ToolCategories.Converter => Icons.Converter,
            ToolCategories.Development => Icons.Development,
            ToolCategories.Web => Icons.Web,
            ToolCategories.Network => Icons.Network,
            ToolCategories.Media => Icons.Media,
            ToolCategories.Math => Icons.Math,
            ToolCategories.Measurement => Icons.Measurement,
            ToolCategories.Text => Icons.Text,
            ToolCategories.Data => Icons.Data,
            _ => Icons.Tool
        };
    }

    private static string IconForTool(string id, string _)
    {
        return Icons.ToolGlyph(id);
    }
}
