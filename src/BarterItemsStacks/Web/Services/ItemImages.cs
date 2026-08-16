using System.Reflection;

namespace BarterItemsStacks.Web.Services;

public sealed class ItemImages
{
    private const string FallbackUnknown = "data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=";

    private readonly Dictionary<string, string> _resById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _dataUriCache = new(StringComparer.Ordinal);
    private string _unknown = FallbackUnknown;

    public void BuildIndex()
    {
        _resById.Clear();
        _dataUriCache.Clear();
        _unknown = FallbackUnknown;

        var asm = Assembly.GetExecutingAssembly();
        var names = asm.GetManifestResourceNames();

        foreach (var res in names)
        {
            if (!res.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (res.IndexOf(".items.", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            var parts = res.Split('.');
            if (parts.Length < 2)
            {
                continue;
            }

            var id = parts[^2];
            if (!string.IsNullOrWhiteSpace(id))
            {
                _resById[id] = res;
            }
        }

        if (_resById.TryGetValue("unknown", out var unkRes))
        {
            _unknown = ToWebpDataUri(WebAssets.ReadEmbeddedBytes(asm, unkRes));
        }
    }

    public string Src(string? tplId)
    {
        if (string.IsNullOrWhiteSpace(tplId))
        {
            return _unknown;
        }

        if (_dataUriCache.TryGetValue(tplId, out var cached))
        {
            return cached;
        }

        if (!_resById.TryGetValue(tplId, out var resName))
        {
            return _unknown;
        }

        var uri = ToWebpDataUri(WebAssets.ReadEmbeddedBytes(Assembly.GetExecutingAssembly(), resName));
        _dataUriCache[tplId] = uri;
        return uri;
    }

    private static string ToWebpDataUri(byte[] bytes) => "data:image/webp;base64," + Convert.ToBase64String(bytes);
}
