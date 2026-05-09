using System.Text;

namespace CodeWF.Modules.ToolFramework;

internal static class Icons
{
    public const string Tool =
        "M128 128h768v128H128V128z m0 320h768v128H128V448z m0 320h768v128H128V768z";

    public const string Crypto =
        "M512 64l320 128v256c0 208-128 397.333-320 512-192-114.667-320-304-320-512V192L512 64z m0 96L256 262.4V448c0 160 92.8 311.467 256 411.733C675.2 759.467 768 608 768 448V262.4L512 160z";

    public const string Converter =
        "M512 1024a32 32 0 0 1 0-64 448.736 448.736 0 0 0 442.688-378.976l-151.424 116.352a32 32 0 0 1-38.976-50.752l208.224-160A32 32 0 0 1 1024 512a512.608 512.608 0 0 1-512 512zM32 544a32 32 0 0 1-32-32A512.608 512.608 0 0 1 512 0a32 32 0 0 1 0 64A448.736 448.736 0 0 0 69.248 443.392l153.376-116.832a32 32 0 1 1 38.816 50.88l-210.048 160A31.808 31.808 0 0 1 32 544z";

    public const string Development =
        "M318.577778 819.2L17.066667 512l301.511111-307.2 45.511111 45.511111L96.711111 512l267.377778 261.688889zM705.422222 819.2l-45.511111-45.511111L927.288889 512l-267.377778-261.688889 45.511111-45.511111L1006.933333 512zM540.785778 221.866667l55.751111 11.150222L483.157333 802.133333l-55.751111-11.093333z";

    public const string Network =
        "M512 96a160 160 0 0 1 160 160c0 72.213-47.787 133.248-113.493 153.173V512H832v128H704v128H576v160H448V768H320V640H192V512h273.493V409.173A160.085 160.085 0 0 1 512 96z";

    public const string Text =
        "M128 192h768v96H576v544H448V288H128V192z";

    public const string Media =
        "M128 192h768v640H128V192z m96 96v448h576V288H224z m96 96l128 128 96-96 160 224H320l96-128-96-128z";

    public const string Math =
        "M192 192h640v128H192V192z m0 256h640v128H192V448z m0 256h256v128H192V704z m384 0h256v128H576V704z";

    public const string Web =
        "M512 64a448 448 0 1 0 0 896 448 448 0 0 0 0-896z m0 96c52 62 88 146 101 256H411c13-110 49-194 101-256zM168 576h169c6 86 27 166 61 232a352 352 0 0 1-230-232z m169-128H168a352 352 0 0 1 230-232c-34 66-55 146-61 232z m175 416c-52-62-88-146-101-256h202c-13 110-49 194-101 256z m119-416H393c11-121 55-216 119-288 64 72 108 167 119 288z m0 128c-11 121-55 216-119 288-64-72-108-167-119-288h238z m-5 232c34-66 55-146 61-232h169a352 352 0 0 1-230 232z m61-360c-6-86-27-166-61-232a352 352 0 0 1 230 232H687z";

    public const string Data =
        "M512 96c212 0 384 72 384 160v512c0 88-172 160-384 160S128 856 128 768V256c0-88 172-160 384-160z m0 96c-176 0-288 49-288 64s112 64 288 64 288-49 288-64-112-64-288-64z m288 210c-70 43-174 70-288 70s-218-27-288-70v110c0 15 112 64 288 64s288-49 288-64V402z m0 256c-70 43-174 70-288 70s-218-27-288-70v110c0 15 112 64 288 64s288-49 288-64V658z";

    public const string Measurement =
        "M512 128a384 384 0 0 1 384 384c0 136-72 264-188 333H316C200 776 128 648 128 512a384 384 0 0 1 384-384z m0 96a288 288 0 0 0-252 427l96-56a176 176 0 0 1-20-83h-96v-80h104a176 176 0 0 1 54-77l-52-90 70-40 53 92c14-3 28-5 43-5s29 2 43 5l53-92 70 40-52 90a176 176 0 0 1 54 77h104v80h-96c0 29-7 57-20 83l96 56A288 288 0 0 0 512 224z m0 176a112 112 0 0 0-96 170l96-56 96 56A112 112 0 0 0 512 400z";

    public static string ToolGlyph(string id)
    {
        return id switch
        {
            "base64-file-converter" => Compose(Document(BinaryMark(336, 420), Digits64(516, 600)), ArrowRight(704, 424, 160, 160)),
            "base64-string-converter" => Compose(TextLines(176, 256, 520, 4), Digits64(632, 560)),
            "case-converter" => CaseGlyph(),
            "color-converter" => PaletteGlyph(),
            "date-time-converter" => Compose(CalendarGlyph(), ClockMini(640, 584)),
            "integer-base-converter" => Compose(CalculatorGlyph(), Digits(592, 560, 1, 0), Digits(736, 720, 1, 6)),
            "json-to-toml" => ConvertGlyph(JsonMini(128, 336), TomlMini(664, 340)),
            "json-to-xml" => ConvertGlyph(JsonMini(128, 336), XmlMini(664, 352)),
            "json-to-yaml-converter" => ConvertGlyph(JsonMini(128, 336), YamlMini(664, 336)),
            "list-converter" => Compose(ListGlyph(), SwapGlyph()),
            "markdown-to-html" => ConvertGlyph(MarkdownMini(128, 352), HtmlMini(664, 352)),
            "roman-numeral-converter" => Compose(RomanGlyph(), SwapGlyph()),
            "temperature-converter" => ThermometerGlyph(),
            "text-to-binary" => ConvertGlyph(TextMini(128, 352), BinaryMark(680, 360)),
            "text-to-nato-alphabet" => Compose(SpeechGlyph(), TextLines(424, 344, 392, 3), Circle(816, 688, 48)),
            "text-to-unicode" => ConvertGlyph(TextMini(128, 352), UnicodeGlyph(640, 312)),
            "toml-to-json" => ConvertGlyph(TomlMini(128, 340), JsonMini(664, 336)),
            "toml-to-yaml" => ConvertGlyph(TomlMini(128, 340), YamlMini(664, 336)),
            "xml-to-json" => ConvertGlyph(XmlMini(128, 352), JsonMini(664, 336)),
            "yaml-to-json-converter" => ConvertGlyph(YamlMini(128, 336), JsonMini(664, 336)),
            "yaml-to-toml" => ConvertGlyph(YamlMini(128, 336), TomlMini(664, 340)),

            "chmod-calculator" => Compose(LockGlyph(), PermissionGrid(560, 304)),
            "crontab-generator" => Compose(CalendarGlyph(), ClockMini(616, 560)),
            "docker-run-to-docker-compose-converter" => ConvertGlyph(DockerGlyph(128, 344), YamlMini(664, 336)),
            "email-normalizer" => Compose(MailGlyph(), FunnelGlyph(640, 392)),
            "git-memo" => GitGlyph(),
            "json-diff" => Compose(JsonMini(120, 336), JsonMini(656, 336), DiffGlyph()),
            "json-minify" => Compose(JsonMini(256, 304), CompressGlyph()),
            "json-to-csv" => ConvertGlyph(JsonMini(128, 336), CsvMini(664, 340)),
            "json-viewer" => Compose(JsonMini(208, 336), EyeGlyph(560, 432)),
            "random-port-generator" => Compose(PlugGlyph(), DiceGlyph(608, 420)),
            "regex-memo" => Compose(CardGlyph(), RegexGlyph(312, 408)),
            "regex-tester" => Compose(RegexGlyph(232, 344), CheckGlyph(600, 520)),
            "sql-prettify" => Compose(DatabaseGlyph(), PenGlyph(608, 288)),
            "xml-formatter" => Compose(XmlMini(184, 352), TextLines(520, 344, 320, 3)),
            "yaml-viewer" => Compose(YamlMini(208, 336), EyeGlyph(560, 432)),

            "bcrypt" => Compose(LockGlyph(), HashGlyph(560, 352, 280)),
            "bip39-generator" => Compose(WordListGlyph(), Digits(656, 576, 3, 9)),
            "encryption" => Compose(LockGlyph(), KeyGlyph(520, 464)),
            "hash-text" => HashGlyph(256, 240, 480),
            "hmac-generator" => Compose(HashGlyph(176, 240, 384), KeyGlyph(560, 520)),
            "password-strength-analyser" => Compose(ShieldGlyph(), MeterGlyph(392, 600)),
            "pdf-signature-checker" => Compose(Document(TextLines(336, 360, 312, 2)), PenGlyph(560, 560), CheckGlyph(656, 280)),
            "rsa-key-pair-generator" => KeyPairGlyph(),
            "token-generator" => Compose(TicketGlyph(), SparkGlyph(656, 288)),
            "ulid-generator" => Compose(IdCardGlyph(), ClockMini(600, 536)),
            "uuid-generator" => Compose(IdCardGlyph(), Blocks(568, 560, 2, 2, 72, 20)),

            "basic-auth-generator" => Compose(KeyGlyph(192, 408), HeaderGlyph(520, 296)),
            "device-information" => MonitorGlyph(),
            "html-entities" => Compose(HtmlMini(144, 352), AmpersandGlyph(560, 320)),
            "html-wysiwyg-editor" => Compose(HtmlMini(144, 352), PenGlyph(568, 352)),
            "http-status-codes" => Compose(BrowserFrame(), Digits(496, 480, 4, 0, 4)),
            "jwt-parser" => Compose(TokenSegmentsGlyph(), JsonMini(608, 360)),
            "keycode-info" => KeyboardGlyph(),
            "meta-tag-generator" => Compose(TagGlyph(), HtmlMini(592, 376)),
            "mime-types" => Compose(Document(TextLines(336, 360, 312, 2)), TagGlyph(560, 568)),
            "otp-code-generator-and-validator" => Compose(ClockGlyph(), Digits(608, 560, 6)),
            "safelink-decoder" => Compose(ShieldGlyph(), LinkGlyph(544, 512)),
            "slugify-string" => Compose(TextLines(176, 320, 384, 2), LinkGlyph(560, 512)),
            "url-encoder" => Compose(LinkGlyph(184, 448), PercentGlyph(576, 312)),
            "url-parser" => Compose(LinkGlyph(160, 448), SplitNodesGlyph(568, 352)),
            "user-agent-parser" => Compose(BrowserFrame(), UserGlyph(600, 440)),

            "qr-code-generator" => QrGlyph(224, 224, 576),
            "svg-placeholder-generator" => Compose(ImageFrameGlyph(), XmlMini(592, 392)),
            "wifi-qr-code-generator" => Compose(WifiGlyph(160, 312), QrGlyph(560, 360, 304)),

            "ipv4-address-converter" => Compose(NetworkNodesGlyph(), Digits(656, 560, 4)),
            "ipv4-range-expander" => Compose(RangeGlyph(), Digits(696, 536, 4)),
            "ipv4-subnet-calculator" => Compose(NetworkTreeGlyph(), Digits(672, 560, 4)),
            "ipv6-ula-generator" => Compose(NetworkNodesGlyph(), Digits(656, 560, 6)),
            "mac-address-generator" => Compose(NicGlyph(), PlusGlyph(640, 320)),
            "mac-address-lookup" => Compose(NicGlyph(), SearchGlyph(608, 432)),

            "eta-calculator" => Compose(ClockGlyph(), ProgressGlyph(576, 608)),
            "math-evaluator" => Compose(CalculatorGlyph(), OperatorGlyph(608, 312)),
            "percentage-calculator" => PercentGlyph(256, 208),

            "benchmark-builder" => GaugeGlyph(),
            "chronometer" => StopwatchGlyph(),

            "ascii-text-drawer" => Compose(BlockTextGlyph(), Blocks(600, 336, 3, 3, 56, 18)),
            "emoji-picker" => FaceGlyph(),
            "lorem-ipsum-generator" => ParagraphGlyph(),
            "numeronym-generator" => Compose(Digits(160, 392, 1), TextLines(320, 408, 300, 1), Digits(688, 392, 8)),
            "string-obfuscator" => Compose(TextLines(176, 352, 520, 2), MaskDotsGlyph(560, 560)),
            "text-diff" => Compose(TextLines(152, 288, 296, 3), TextLines(576, 288, 296, 3), PlusMinusGlyph()),
            "text-statistics" => Compose(TextLines(152, 264, 384, 3), BarChartGlyph(584, 360)),

            "iban-validator-and-parser" => Compose(BankCardGlyph(), CheckGlyph(640, 520)),
            "phone-parser-and-formatter" => Compose(PhoneGlyph(), IdCardGlyph(560, 384)),
            _ => Tool
        };
    }

    private static string Compose(params string[] parts)
    {
        var builder = new StringBuilder();
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(part);
        }

        return builder.ToString();
    }

    private static string Rect(int x, int y, int width, int height) => $"M{x} {y}h{width}v{height}h-{width}z";

    private static string Circle(int cx, int cy, int r) =>
        $"M{cx - r} {cy}a{r} {r} 0 1 0 {r * 2} 0a{r} {r} 0 1 0 -{r * 2} 0";

    private static string Diamond(int cx, int cy, int r) =>
        $"M{cx} {cy - r}l{r} {r}l-{r} {r}l-{r}-{r}z";

    private static string ArrowRight(int x, int y, int width, int height)
    {
        var head = width / 3;
        var shaft = height / 4;
        var shaftY = y + (height - shaft) / 2;
        return Compose(Rect(x, shaftY, width - head, shaft), $"M{x + width - head} {y}l{head} {height / 2}l-{head} {height / 2}z");
    }

    private static string ConvertGlyph(string left, string right) => Compose(left, ArrowRight(412, 448, 200, 128), right);

    private static string FileFrame() => Compose(
        Rect(208, 96, 392, 72),
        Rect(208, 96, 72, 832),
        Rect(208, 856, 608, 72),
        Rect(744, 320, 72, 608),
        Rect(600, 96, 72, 224),
        Rect(600, 248, 216, 72));

    private static string Document(params string[] content) => Compose(FileFrame(), Compose(content));

    private static string TextLines(int x, int y, int width, int rows)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < rows; i++)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(Rect(x, y + i * 120, i == rows - 1 ? width * 2 / 3 : width, 48));
        }

        return builder.ToString();
    }

    private static string TextMini(int x, int y) => TextLines(x, y, 260, 3);

    private static string Blocks(int x, int y, int columns, int rows, int size, int gap)
    {
        var builder = new StringBuilder();
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(Rect(x + column * (size + gap), y + row * (size + gap), size, size));
            }
        }

        return builder.ToString();
    }

    private static string BinaryMark(int x, int y) => Compose(
        Blocks(x, y, 2, 2, 72, 24),
        Rect(x + 192, y, 56, 168),
        Rect(x + 288, y, 56, 168));

    private static string Digits64(int x, int y) => Compose(Digit(x, y, 6, 96, 160, 24), Digit(x + 112, y, 4, 96, 160, 24));

    private static string Digits(int x, int y, params int[] digits)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < digits.Length; i++)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(Digit(x + i * 112, y, digits[i], 96, 160, 24));
        }

        return builder.ToString();
    }

    private static string Digit(int x, int y, int digit, int width, int height, int thick)
    {
        var builder = new StringBuilder();
        foreach (var segment in SegmentsForDigit(digit))
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(segment switch
            {
                0 => Rect(x + thick, y, width - thick * 2, thick),
                1 => Rect(x + width - thick, y + thick, thick, height / 2 - thick),
                2 => Rect(x + width - thick, y + height / 2, thick, height / 2 - thick),
                3 => Rect(x + thick, y + height - thick, width - thick * 2, thick),
                4 => Rect(x, y + height / 2, thick, height / 2 - thick),
                5 => Rect(x, y + thick, thick, height / 2 - thick),
                _ => Rect(x + thick, y + height / 2 - thick / 2, width - thick * 2, thick)
            });
        }

        return builder.ToString();
    }

    private static int[] SegmentsForDigit(int digit)
    {
        return digit switch
        {
            0 => [0, 1, 2, 3, 4, 5],
            1 => [1, 2],
            2 => [0, 1, 6, 4, 3],
            3 => [0, 1, 6, 2, 3],
            4 => [5, 6, 1, 2],
            5 => [0, 5, 6, 2, 3],
            6 => [0, 5, 6, 4, 2, 3],
            7 => [0, 1, 2],
            8 => [0, 1, 2, 3, 4, 5, 6],
            9 => [0, 1, 2, 3, 5, 6],
            _ => []
        };
    }

    private static string TableGlyph(int x, int y, int width, int height) => Compose(
        Rect(x, y, width, 48),
        Rect(x, y, 48, height),
        Rect(x, y + height - 48, width, 48),
        Rect(x + width - 48, y, 48, height),
        Rect(x + width / 3, y, 32, height),
        Rect(x + width * 2 / 3, y, 32, height),
        Rect(x, y + height / 3, width, 32),
        Rect(x, y + height * 2 / 3, width, 32));

    private static string JsonMini(int x, int y) => Compose(
        Rect(x + 64, y, 72, 72),
        Rect(x + 64, y, 52, 120),
        Rect(x + 16, y + 136, 120, 72),
        Rect(x + 64, y + 192, 52, 128),
        Rect(x + 64, y + 296, 72, 72),
        Rect(x + 216, y, 72, 72),
        Rect(x + 236, y, 52, 120),
        Rect(x + 216, y + 136, 120, 72),
        Rect(x + 236, y + 192, 52, 128),
        Rect(x + 216, y + 296, 72, 72));

    private static string XmlMini(int x, int y) => Compose(
        $"M{x + 140} {y}l-120 160l120 160l56-48l-84-112l84-112z",
        $"M{x + 220} {y}l120 160l-120 160l-56-48l84-112l-84-112z",
        $"M{x + 244} {y - 32}l72 24l-176 416l-72-24z");

    private static string HtmlMini(int x, int y) => Compose(XmlMini(x, y), Rect(x + 132, y + 336, 96, 48));

    private static string YamlMini(int x, int y) => Compose(
        Circle(x + 168, y + 56, 52),
        Circle(x + 72, y + 272, 52),
        Circle(x + 264, y + 272, 52),
        Rect(x + 144, y + 104, 48, 160),
        $"M{x + 168} {y + 208}l-96 64l-32-48l128-88z",
        $"M{x + 168} {y + 208}l96 64l32-48l-128-88z");

    private static string TomlMini(int x, int y) => TableGlyph(x, y, 280, 320);

    private static string CsvMini(int x, int y) => Compose(TableGlyph(x, y, 280, 320), Circle(x + 344, y + 248, 28), Circle(x + 416, y + 248, 28));

    private static string MarkdownMini(int x, int y) => Compose(
        Rect(x, y, 300, 56),
        Rect(x, y, 56, 280),
        Rect(x + 244, y, 56, 280),
        Rect(x, y + 224, 300, 56),
        Rect(x + 80, y + 96, 48, 128),
        $"M{x + 144} {y + 96}l56 80l56-80v128h-48v-48l-56 72l-56-72v48h-48V{y + 96}z");

    private static string UnicodeGlyph(int x, int y) => Compose(
        Rect(x, y, 72, 248),
        Rect(x + 224, y, 72, 248),
        Rect(x + 72, y + 248, 224, 72),
        Rect(x + 72, y + 320, 72, 72),
        Rect(x + 224, y + 320, 72, 72));

    private static string CaseGlyph() => Compose(
        $"M160 816l176-608h120l176 608H512l-32-128H312l-32 128z M336 576h116l-56-232z",
        Circle(720, 640, 112),
        Rect(800, 480, 72, 336));

    private static string PaletteGlyph() => Compose(
        Circle(512, 512, 352),
        Circle(352, 392, 48),
        Circle(512, 320, 48),
        Circle(664, 416, 48),
        Circle(432, 616, 48),
        $"M616 704c80-16 168 24 168 96c0 88-92 128-204 128H512v-96h72c52 0 72-12 72-32c0-16-20-28-56-20z");

    private static string CalendarGlyph() => Compose(
        Rect(176, 192, 672, 88),
        Rect(176, 192, 72, 656),
        Rect(776, 192, 72, 656),
        Rect(176, 776, 672, 72),
        Rect(312, 112, 72, 160),
        Rect(640, 112, 72, 160),
        Blocks(312, 376, 3, 3, 72, 72));

    private static string ClockGlyph() => Compose(
        Rect(472, 128, 80, 128),
        Rect(472, 768, 80, 128),
        Rect(128, 472, 128, 80),
        Rect(768, 472, 128, 80),
        Rect(472, 288, 80, 256),
        Rect(512, 512, 224, 80));

    private static string ClockMini(int x, int y) => Compose(
        Circle(x, y, 96),
        Rect(x - 16, y - 80, 32, 96),
        Rect(x, y, 96, 32));

    private static string ThermometerGlyph() => Compose(
        Rect(456, 144, 112, 520),
        Circle(512, 760, 152),
        Rect(616, 224, 160, 48),
        Rect(616, 384, 128, 48),
        Rect(616, 544, 160, 48));

    private static string CalculatorGlyph() => Compose(
        Rect(256, 128, 512, 72),
        Rect(256, 128, 72, 768),
        Rect(696, 128, 72, 768),
        Rect(256, 824, 512, 72),
        Rect(352, 248, 320, 96),
        Blocks(352, 432, 3, 3, 72, 56));

    private static string ListGlyph() => Compose(
        Circle(184, 280, 36),
        Circle(184, 448, 36),
        Circle(184, 616, 36),
        Circle(184, 784, 36),
        TextLines(280, 248, 500, 4));

    private static string SwapGlyph() => Compose(
        ArrowRight(528, 256, 288, 120),
        $"M496 768l-288-120l288-120v80h320v80H496z");

    private static string RomanGlyph() => Compose(
        Rect(176, 224, 80, 576),
        $"M336 224l128 576h96l128-576h-96l-80 376l-80-376z",
        $"M760 224l-120 288l120 288h104l-120-288l120-288z");

    private static string SpeechGlyph() => Compose(
        Rect(144, 224, 520, 72),
        Rect(144, 224, 72, 360),
        Rect(592, 224, 72, 360),
        Rect(144, 512, 520, 72),
        $"M320 584l-96 176v-176z");

    private static string LockGlyph() => Compose(
        Rect(288, 448, 448, 352),
        Rect(384, 320, 80, 160),
        Rect(560, 320, 80, 160),
        Rect(384, 240, 256, 80),
        Circle(512, 616, 44),
        Rect(488, 640, 48, 96));

    private static string PermissionGrid(int x, int y) => Compose(
        Blocks(x, y, 3, 3, 72, 40),
        CheckGlyph(x + 120, y + 296));

    private static string DockerGlyph(int x, int y) => Compose(
        Blocks(x + 40, y, 4, 2, 56, 16),
        Blocks(x + 112, y - 72, 2, 1, 56, 16),
        Rect(x, y + 160, 344, 80),
        $"M{x + 344} {y + 160}l72 40l-72 40z");

    private static string MailGlyph() => Compose(
        Rect(144, 256, 736, 72),
        Rect(144, 256, 72, 512),
        Rect(808, 256, 72, 512),
        Rect(144, 696, 736, 72),
        $"M216 328l296 232l296-232v96L512 656L216 424z");

    private static string FunnelGlyph(int x, int y) => $"M{x} {y}h320l-120 144v176h-80V{y + 144}z";

    private static string GitGlyph() => Compose(
        Circle(320, 256, 80),
        Circle(320, 768, 80),
        Circle(704, 512, 80),
        Rect(288, 320, 64, 384),
        $"M352 352l288 128l-40 72l-288-128z");

    private static string DiffGlyph() => Compose(
        Rect(464, 328, 96, 368),
        Rect(424, 464, 176, 64),
        Rect(424, 616, 176, 64),
        Rect(448, 304, 128, 64));

    private static string CompressGlyph() => Compose(
        $"M352 192h80v240H192v-80h104z",
        $"M672 832h-80V592h240v80H728z");

    private static string EyeGlyph(int x, int y) => Compose(
        $"M{x - 256} {y}c104-144 208-216 256-216s152 72 256 216c-104 144-208 216-256 216S{x - 152} {y + 144} {x - 256} {y}z",
        Circle(x, y, 88));

    private static string PlugGlyph() => Compose(
        Rect(312, 176, 80, 240),
        Rect(536, 176, 80, 240),
        Rect(248, 400, 432, 96),
        Rect(344, 496, 240, 192),
        Rect(456, 688, 80, 160));

    private static string DiceGlyph(int x, int y) => Compose(
        Rect(x, y, 248, 248),
        Circle(x + 64, y + 64, 24),
        Circle(x + 184, y + 64, 24),
        Circle(x + 124, y + 124, 24),
        Circle(x + 64, y + 184, 24),
        Circle(x + 184, y + 184, 24));

    private static string CardGlyph() => Compose(
        Rect(176, 216, 672, 72),
        Rect(176, 216, 72, 592),
        Rect(776, 216, 72, 592),
        Rect(176, 736, 672, 72));

    private static string RegexGlyph(int x, int y) => Compose(
        Circle(x, y + 220, 44),
        Rect(x + 104, y + 88, 64, 264),
        Rect(x + 40, y + 188, 192, 64),
        $"M{x + 328} {y + 88}l56 112l120-40l24 80l-120 40l72 104l-72 48l-72-104l-72 104l-72-48l72-104l-120-40l24-80l120 40z");

    private static string DatabaseGlyph() => Data;

    private static string HashGlyph(int x, int y, int size)
    {
        var t = size / 7;
        return Compose(
            Rect(x + size / 3, y, t, size),
            Rect(x + size * 2 / 3, y, t, size),
            Rect(x, y + size / 3, size, t),
            Rect(x, y + size * 2 / 3, size, t));
    }

    private static string WordListGlyph() => Compose(
        Rect(160, 160, 704, 72),
        Rect(160, 792, 704, 72),
        Rect(160, 160, 72, 704),
        Rect(792, 160, 72, 704),
        TextLines(288, 288, 400, 4),
        Blocks(688, 288, 2, 4, 40, 80));

    private static string KeyGlyph(int x, int y) => Compose(
        Circle(x, y, 104),
        Rect(x + 88, y - 32, 288, 64),
        Rect(x + 288, y + 32, 64, 96),
        Rect(x + 184, y + 32, 64, 72));

    private static string ShieldGlyph() => "M512 96l304 128v248c0 192-112 348-304 456c-192-108-304-264-304-456V224z";

    private static string MeterGlyph(int x, int y) => Compose(
        Rect(x, y + 160, 72, 128),
        Rect(x + 112, y + 96, 72, 192),
        Rect(x + 224, y + 24, 72, 264));

    private static string PenGlyph(int x, int y) => Compose(
        $"M{x + 192} {y}l96 96l-360 360l-128 32l32-128z",
        $"M{x + 224} {y - 32}l64-64l96 96l-64 64z");

    private static string CheckGlyph(int x, int y) => $"M{x} {y + 136}l72-72l112 112l256-256l72 72l-328 328z";

    private static string KeyPairGlyph() => Compose(KeyGlyph(232, 336), KeyGlyph(520, 592), Rect(488, 488, 80, 80));

    private static string TicketGlyph() => Compose(
        Rect(176, 320, 672, 72),
        Rect(176, 632, 672, 72),
        Rect(176, 320, 72, 384),
        Rect(776, 320, 72, 384),
        Circle(248, 512, 48),
        Circle(776, 512, 48),
        Rect(480, 392, 64, 240));

    private static string SparkGlyph(int x, int y) => Compose(
        Diamond(x, y, 88),
        Diamond(x + 160, y + 160, 56),
        Diamond(x - 80, y + 208, 40));

    private static string IdCardGlyph() => IdCardGlyph(176, 288);

    private static string IdCardGlyph(int x, int y) => Compose(
        Rect(x, y, 672, 72),
        Rect(x, y, 72, 448),
        Rect(x + 600, y, 72, 448),
        Rect(x, y + 376, 672, 72),
        Circle(x + 184, y + 192, 72),
        TextLines(x + 320, y + 136, 240, 2));

    private static string HeaderGlyph(int x, int y) => Compose(
        Rect(x, y, 360, 72),
        TextLines(x, y + 136, 300, 3));

    private static string MonitorGlyph() => Compose(
        Rect(160, 176, 704, 72),
        Rect(160, 176, 72, 512),
        Rect(792, 176, 72, 512),
        Rect(160, 616, 704, 72),
        Rect(464, 688, 96, 120),
        Rect(352, 808, 320, 72));

    private static string BrowserFrame() => Compose(
        Rect(144, 176, 736, 88),
        Rect(144, 176, 72, 648),
        Rect(808, 176, 72, 648),
        Rect(144, 752, 736, 72),
        Circle(264, 220, 24),
        Circle(352, 220, 24),
        Circle(440, 220, 24));

    private static string AmpersandGlyph(int x, int y) => Compose(
        Circle(x + 112, y + 96, 88),
        Circle(x + 104, y + 280, 96),
        $"M{x + 232} {y + 184}l64 56l-240 240l-64-56z",
        $"M{x + 224} {y + 336}l160 136l-56 64l-160-136z");

    private static string TokenSegmentsGlyph() => Compose(
        Rect(128, 408, 200, 104),
        Rect(412, 408, 200, 104),
        Rect(696, 408, 200, 104),
        Circle(368, 460, 24),
        Circle(656, 460, 24));

    private static string KeyboardGlyph() => Compose(
        Rect(128, 280, 768, 72),
        Rect(128, 280, 72, 464),
        Rect(824, 280, 72, 464),
        Rect(128, 672, 768, 72),
        Blocks(240, 392, 8, 2, 56, 28),
        Rect(336, 588, 352, 48));

    private static string TagGlyph() => TagGlyph(176, 304);

    private static string TagGlyph(int x, int y) => Compose(
        $"M{x} {y}h336l224 224l-336 336L{x - 112} {y + 448}z",
        Circle(x + 256, y + 144, 44));

    private static string SearchGlyph(int x, int y) => Compose(Circle(x, y, 112), $"M{x + 80} {y + 80}l192 192l-64 64l-192-192z");

    private static string LinkGlyph(int x, int y) => Compose(
        Circle(x, y, 112),
        Circle(x + 224, y, 112),
        Rect(x, y - 32, 224, 64));

    private static string PercentGlyph(int x, int y) => Compose(
        Circle(x + 112, y + 112, 72),
        Circle(x + 416, y + 416, 72),
        $"M{x + 456} {y}l80 48l-416 576l-80-48z");

    private static string SplitNodesGlyph(int x, int y) => Compose(
        Circle(x, y + 160, 56),
        Circle(x + 240, y, 56),
        Circle(x + 240, y + 320, 56),
        Rect(x + 52, y + 128, 188, 48),
        $"M{x + 88} {y + 184}l152 96l-40 64l-152-96z");

    private static string UserGlyph(int x, int y) => Compose(Circle(x, y, 96), $"M{x - 176} {y + 288}c32-120 112-176 176-176s144 56 176 176z");

    private static string QrGlyph(int x, int y, int size)
    {
        var cell = size / 7;
        return Compose(
            QrCorner(x, y, cell),
            QrCorner(x + cell * 4, y, cell),
            QrCorner(x, y + cell * 4, cell),
            Rect(x + cell * 3, y + cell * 3, cell, cell),
            Rect(x + cell * 5, y + cell * 3, cell, cell),
            Rect(x + cell * 3, y + cell * 5, cell, cell),
            Rect(x + cell * 4, y + cell * 6, cell, cell),
            Rect(x + cell * 6, y + cell * 5, cell, cell));
    }

    private static string QrCorner(int x, int y, int cell) => Compose(
        Rect(x, y, cell * 3, cell),
        Rect(x, y, cell, cell * 3),
        Rect(x, y + cell * 2, cell * 3, cell),
        Rect(x + cell * 2, y, cell, cell * 3),
        Rect(x + cell, y + cell, cell, cell));

    private static string ImageFrameGlyph() => Compose(
        Rect(160, 224, 704, 72),
        Rect(160, 224, 72, 576),
        Rect(792, 224, 72, 576),
        Rect(160, 728, 704, 72),
        Circle(688, 384, 56),
        $"M232 728l208-240l152 152l96-104l104 192z");

    private static string WifiGlyph(int x, int y) => Compose(
        $"M{x} {y + 136}c192-168 384-168 576 0l-72 72c-144-120-288-120-432 0z",
        $"M{x + 128} {y + 304}c104-88 216-88 320 0l-72 72c-56-48-120-48-176 0z",
        Circle(x + 288, y + 512, 64));

    private static string NetworkNodesGlyph() => Compose(
        Circle(256, 512, 80),
        Circle(512, 280, 80),
        Circle(768, 512, 80),
        Circle(512, 744, 80),
        $"M320 488l160-144l48 56l-160 144z",
        $"M576 344l160 144l-48 56l-160-144z",
        $"M320 536l160 144l-48 56l-160-144z",
        $"M704 536l-160 144l48 56l160-144z");

    private static string RangeGlyph() => Compose(
        Circle(216, 512, 72),
        Circle(808, 512, 72),
        Rect(288, 480, 448, 64),
        Rect(456, 376, 112, 272));

    private static string NetworkTreeGlyph() => Compose(
        Circle(512, 224, 72),
        Circle(256, 672, 72),
        Circle(512, 672, 72),
        Circle(768, 672, 72),
        Rect(480, 296, 64, 168),
        Rect(256, 464, 512, 64),
        Rect(224, 464, 64, 136),
        Rect(480, 464, 64, 136),
        Rect(736, 464, 64, 136));

    private static string NicGlyph() => Compose(
        Rect(192, 288, 640, 80),
        Rect(192, 288, 80, 448),
        Rect(752, 288, 80, 448),
        Rect(192, 656, 640, 80),
        Blocks(336, 416, 4, 1, 64, 40),
        Rect(320, 736, 64, 128),
        Rect(640, 736, 64, 128));

    private static string PlusGlyph(int x, int y) => Compose(Rect(x + 104, y, 72, 280), Rect(x, y + 104, 280, 72));

    private static string ProgressGlyph(int x, int y) => Compose(Rect(x, y, 320, 56), Rect(x, y + 120, 224, 56), Rect(x, y + 240, 128, 56));

    private static string OperatorGlyph(int x, int y) => Compose(
        PlusGlyph(x, y),
        Rect(x + 368, y + 120, 280, 56),
        Rect(x + 456, y + 32, 56, 232));

    private static string GaugeGlyph() => Compose(
        $"M192 720c0-176 144-320 320-320s320 144 320 320h-96c0-124-100-224-224-224S288 596 288 720z",
        $"M512 704l176-256l72 48l-176 256z",
        Circle(512, 720, 72));

    private static string StopwatchGlyph() => Compose(
        Rect(448, 96, 128, 72),
        Rect(472, 168, 80, 96),
        ClockGlyph());

    private static string BlockTextGlyph() => Compose(
        Rect(176, 248, 96, 528),
        Rect(176, 248, 336, 80),
        Rect(176, 472, 272, 80),
        Rect(176, 696, 336, 80));

    private static string FaceGlyph() => Compose(
        Circle(512, 512, 352),
        Circle(384, 416, 48),
        Circle(640, 416, 48),
        $"M328 600h80c24 72 72 112 104 112s80-40 104-112h80c-32 136-128 216-184 216s-152-80-184-216z");

    private static string ParagraphGlyph() => Compose(
        Rect(256, 184, 512, 80),
        Rect(256, 344, 432, 64),
        Rect(256, 488, 512, 64),
        Rect(256, 632, 360, 64),
        Rect(256, 776, 440, 64));

    private static string MaskDotsGlyph(int x, int y) => Compose(Circle(x, y, 44), Circle(x + 128, y, 44), Circle(x + 256, y, 44));

    private static string PlusMinusGlyph() => Compose(
        Rect(456, 328, 112, 48),
        Rect(488, 296, 48, 112),
        Rect(456, 632, 112, 48));

    private static string BarChartGlyph(int x, int y) => Compose(Rect(x, y + 240, 72, 192), Rect(x + 128, y + 120, 72, 312), Rect(x + 256, y, 72, 432));

    private static string BankCardGlyph() => Compose(
        Rect(144, 280, 736, 80),
        Rect(144, 280, 72, 464),
        Rect(808, 280, 72, 464),
        Rect(144, 664, 736, 80),
        Rect(144, 424, 736, 72),
        TextLines(272, 560, 360, 1));

    private static string PhoneGlyph() =>
        "M312 128c-56 24-104 72-136 136c64 240 224 400 464 464c64-32 112-80 136-136l-160-96l-96 72c-88-48-152-112-200-200l72-96z";
}
