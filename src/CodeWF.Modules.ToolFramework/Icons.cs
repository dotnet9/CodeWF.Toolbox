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

    public static string WithToolBadge(string baseIcon, string id)
    {
        return $"{baseIcon} {CreateBadge(StableHash(id))}";
    }

    private static uint StableHash(string value)
    {
        const uint offset = 2166136261;
        const uint prime = 16777619;
        var hash = offset;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= prime;
        }

        return hash;
    }

    private static string CreateBadge(uint hash)
    {
        var variant = (int)(hash % 8);
        var cx = 744 + (int)((hash >> 3) % 5) * 22;
        var cy = 734 + (int)((hash >> 7) % 5) * 22;
        var r = 48 + (int)((hash >> 11) % 4) * 6;
        var s = r * 2;
        var left = cx - r;
        var top = cy - r;

        return variant switch
        {
            0 => $"M{left} {cy}a{r} {r} 0 1 0 {s} 0a{r} {r} 0 1 0 -{s} 0",
            1 => $"M{left} {top}h{s}v{s}h-{s}z",
            2 => $"M{cx} {top}l{r} {r}l-{r} {r}l-{r}-{r}z",
            3 => $"M{cx} {top}l{r} {s}h-{s}z",
            4 => $"M{cx - 18} {top}h36v{s}h-36z M{left} {cy - 18}h{s}v36h-{s}z",
            5 => $"M{cx} {top}l20 {r - 10}l{r - 8} 8l-{r + 2} 18l-10 {r}l-22-{r - 16}l-{r} -12l{r + 2}-16z",
            6 => $"M{left} {top + s - 28}h28v28h-28z M{cx - 14} {top + 28}h28v{s - 28}h-28z M{left + s - 28} {top}h28v{s}h-28z",
            _ => $"M{left} {top + 16}h{s}v28h-{s}z M{left + 16} {cy - 14}h{s - 32}v28h-{s - 32}z M{left} {top + s - 44}h{s}v28h-{s}z"
        };
    }
}
