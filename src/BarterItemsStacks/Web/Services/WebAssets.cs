using System.Reflection;

namespace BarterItemsStacks.Web.Services;

public static class WebAssets
{
    private static readonly Lazy<string> _styles = new(BuildStyles);
    private static readonly Lazy<string> _scripts = new(BuildScripts);

    public static string Styles => _styles.Value;

    public static string Scripts => _scripts.Value;

    public static byte[] ReadEmbeddedBytes(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
                            ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public static string ReadText(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
                            ?? throw new InvalidOperationException($"Embedded resource stream not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string BuildStyles()
    {
        var asm = Assembly.GetExecutingAssembly();
        var names = asm.GetManifestResourceNames();

        var cssRes = names.FirstOrDefault(n => n.EndsWith("bis.css", StringComparison.OrdinalIgnoreCase));
        var fontRes = names.FirstOrDefault(n => n.EndsWith("bender.otf", StringComparison.OrdinalIgnoreCase));

        if (cssRes is null)
        {
            throw new InvalidOperationException("Embedded resource 'bis.css' not found. Check that it is under Web\\Assets and marked as EmbeddedResource.");
        }

        if (fontRes is null)
        {
            throw new InvalidOperationException("Embedded resource 'bender.otf' not found. Check that it is under Web\\Assets and marked as EmbeddedResource.");
        }

        var css = ReadText(asm, cssRes);
        var fontBytes = ReadEmbeddedBytes(asm, fontRes);
        var b64 = Convert.ToBase64String(fontBytes);

        var fontFace =
            $@"
            @font-face {{
              font-family: 'Bender';
              src: url('data:font/otf;base64,{b64}') format('opentype');
              font-weight: 400;
              font-style: normal;
              font-display: swap;
            }}
            ";

        return fontFace + "\n" + css;
    }

    private static string BuildScripts()
    {
        var asm = Assembly.GetExecutingAssembly();
        var names = asm.GetManifestResourceNames();

        var jsRes = names.FirstOrDefault(n => n.EndsWith("inputFilters.js", StringComparison.OrdinalIgnoreCase));

        if (jsRes is null)
        {
            throw new InvalidOperationException("Embedded resource 'inputFilters.js' not found. Check that it is under Web\\Assets and marked as EmbeddedResource.");
        }

        return ReadText(asm, jsRes);
    }
}
