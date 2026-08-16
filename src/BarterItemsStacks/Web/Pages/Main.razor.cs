using System.Reflection;
using System.Text;
using BarterItemsStacks.Configs;
using BarterItemsStacks.Web.Config;
using BarterItemsStacks.Web.Models;
using BarterItemsStacks.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace BarterItemsStacks.Web.Pages;

public partial class Main : ComponentBase, IDisposable
{
    private const string OtherCategoryName = "Other/NS/NT";
    private const int SearchResultLimit = 14;
    private const int HighlightDurationMs = 2000;
    private const int ToastDurationMs = 2500;
    private const int ToastErrorDurationMs = 4000;

    [Inject] private ModHelper _modHelper { get; set; } = default!;
    [Inject] private DatabaseServer _databaseServer { get; set; } = default!;
    [Inject] private IJSRuntime _js { get; set; } = default!;
    [Inject] private LocaleService _localeService { get; set; } = default!;

    private string? _error;

    private int _searchRefreshToken;

    private readonly Debouncer _toastClearDebouncer = new();
    private readonly Debouncer _highlightClearDebouncer = new();

    private readonly ItemImages _images = new();

    private ItemsDbIndex? _itemsIndex;
    private CategoryResolver? _resolver;
    private ItemsConfig? _cfg;
    private string? _pathToMod;

    private bool _isSaving;
    private bool _showCategoryEditor;
    private readonly List<PresetEntry> _presets = new();
    private string _selectedPreset = "";
    private bool _presetsOpen;
    private string? _presetConfirmName;
    private string _weightMultiplierText = "";

    private readonly HashSet<string> _collapsedCategories = new(StringComparer.Ordinal);

    private readonly Dictionary<string, ConfigItemRow> _configItemsById = new(StringComparer.Ordinal);
    private readonly HashSet<string> _inConfig = new(StringComparer.Ordinal);

    private readonly HashSet<string> _knownParentTplIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _parentOverrideIds = new(StringComparer.Ordinal);

    private List<ConfigItemRow> _parentOverrides = new();
    private List<CategoryGroup>? _viewCategories;

    private string? _pendingScrollTplId;
    private string? _highlightTplId;

    private string? _toastMessage;
    private bool _toastVisible;

    private static string EmbeddedStyles => WebAssets.Styles;
    private static string EmbeddedScripts => WebAssets.Scripts;

    private string ItemImageSrc(string tplId) => _images.Src(tplId);

    private static readonly FieldInfo? RuleStackField =
        typeof(ItemsConfig.ItemRule).GetField("StackSize", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? RuleMaxResField =
        typeof(ItemsConfig.ItemRule).GetField("MaxResource", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? RuleHeightField =
        typeof(ItemsConfig.ItemRule).GetField("ItemHeight", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? RuleWidthField =
        typeof(ItemsConfig.ItemRule).GetField("ItemWidth", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? RuleWeightField =
        typeof(ItemsConfig.ItemRule).GetField("WeightMultiplier", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? RulePriceField =
        typeof(ItemsConfig.ItemRule).GetField("PriceMultiplier", BindingFlags.Instance | BindingFlags.NonPublic);

    protected override void OnInitialized()
    {
        try
        {
            _pathToMod = _modHelper.GetAbsolutePathToModFolder(typeof(ItemsConfig).Assembly);
            _cfg = _modHelper.GetJsonDataFromFile<ItemsConfig>(_pathToMod, ItemsConfig.FileName);
            ScanPresets();
            LoadWeightMultiplier();

            var localeKey = _localeService.GetDesiredGameLocale();
            var localeLocalized = _localeService.GetLocaleDb();
            var localeEn = _localeService.GetLocaleDb("en");

            CategoriesConfig.EnsureExists(_pathToMod);
            var categoriesCfg = _modHelper.GetJsonDataFromFile<CategoriesConfig>(_pathToMod, CategoriesConfig.FileName);
            _resolver = CategoryResolver.Build(categoriesCfg.Categories);

            _itemsIndex = new ItemsDbIndex(_databaseServer, OtherCategoryName, localeLocalized, localeEn, _resolver);

            _images.BuildIndex();

            var itemsDb = _databaseServer.GetTables().Templates.Items;
            foreach (var kvp in itemsDb)
            {
                if (string.Equals(kvp.Value.Type, "Node", StringComparison.OrdinalIgnoreCase))
                    _knownParentTplIds.Add(kvp.Key.ToString());
            }

            LoadItemsFromConfig(_cfg);
        }
        catch (Exception ex)
        {
            _error = ex.ToString();
        }
    }

    public void Dispose()
    {
        _toastClearDebouncer?.Dispose();
        _highlightClearDebouncer?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _js.InvokeVoidAsync(
                "eval",
                @"(function () {
                  document.querySelectorAll('summary').forEach(function (s) {
                    s.querySelectorAll('input, select, textarea, a, label')
                     .forEach(function (el) {
                       el.addEventListener('pointerdown', function (e) { e.stopPropagation(); });
                       el.addEventListener('click', function (e) { e.stopPropagation(); });
                       el.addEventListener('keydown', function (e) { e.stopPropagation(); });
                     });
                  });
                })();"
            );
        }

        if (!string.IsNullOrWhiteSpace(_pendingScrollTplId))
        {
            var tpl = _pendingScrollTplId!;
            _pendingScrollTplId = null;

            await _js.InvokeVoidAsync(
                "eval",
                $"document.getElementById('row-{tpl}')?.scrollIntoView({{behavior:'smooth', block:'center'}});"
            );
        }
    }

    private static ItemsConfig.ItemRule CreateRule(int? stackSize, int? maxResource, int? height, int? width, double? weight, double? price)
    {
        var rule = new ItemsConfig.ItemRule();
        RuleStackField?.SetValue(rule, stackSize);
        RuleMaxResField?.SetValue(rule, maxResource);
        RuleHeightField?.SetValue(rule, height);
        RuleWidthField?.SetValue(rule, width);
        RuleWeightField?.SetValue(rule, weight);
        RulePriceField?.SetValue(rule, price);
        return rule;
    }

    private void RebuildView()
    {
        _parentOverrides = _parentOverrideIds
            .Where(id => _configItemsById.ContainsKey(id))
            .Select(id => _configItemsById[id])
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.TemplateId, StringComparer.Ordinal)
            .ToList();

        var itemsOnly = GetNonParentItems();

        _viewCategories = ViewBuilder.Build(itemsOnly, OtherCategoryName, _resolver!);
    }

    private IEnumerable<ConfigItemRow> GetNonParentItems()
    {
        return _configItemsById.Values.Where(x => !_parentOverrideIds.Contains(x.TemplateId));
    }

    private async Task OpenCategoryEditor()
    {
        _showCategoryEditor = true;
        await _js.InvokeVoidAsync("eval", "document.documentElement.style.overflow='hidden';document.body.style.overflow='hidden';");
    }

    private async Task CloseCategoryEditor()
    {
        _showCategoryEditor = false;
        await _js.InvokeVoidAsync("eval", "document.documentElement.style.overflow='';document.body.style.overflow='';");
    }

    private void RefreshCategories()
    {
        if (_pathToMod is null)
        {
            return;
        }

        try
        {
            CategoriesConfig.EnsureExists(_pathToMod);
            var cfg = _modHelper.GetJsonDataFromFile<CategoriesConfig>(_pathToMod, CategoriesConfig.FileName);
            var resolver = CategoryResolver.Build(cfg.Categories);
            _resolver = resolver;

            var localeLocalized = _localeService.GetLocaleDb();
            var localeEn = _localeService.GetLocaleDb("en");
            _itemsIndex = new ItemsDbIndex(_databaseServer, OtherCategoryName, localeLocalized, localeEn, resolver);

            foreach (var row in _configItemsById.Values)
            {
                row.Category = resolver.Resolve(row.TemplateId, row.Parent, OtherCategoryName);
            }

            _searchRefreshToken++;
            RebuildView();
        }
        catch
        {
        }
    }

    private void LoadItemsFromConfig(ItemsConfig cfg)
    {
        _configItemsById.Clear();
        _inConfig.Clear();
        _parentOverrideIds.Clear();
        _parentOverrides.Clear();
        _categoryBulk.Clear();

        foreach (var kvp in cfg.Items)
        {
            var tplId = kvp.Key;
            var rule = kvp.Value;

            if (_itemsIndex != null && _itemsIndex.TryGet(tplId, out var db))
            {
                _configItemsById[tplId] = new ConfigItemRow(
                    tplId,
                    db.Name,
                    db.Parent,
                    db.Category,
                    rule.Stack,
                    rule.Resource,
                    rule.Height,
                    rule.Width,
                    rule.Weight,
                    rule.Price
                );
            }
            else
            {
                _configItemsById[tplId] = new ConfigItemRow(
                    tplId,
                    "unknown",
                    "unknown",
                    OtherCategoryName,
                    rule.Stack,
                    rule.Resource,
                    rule.Height,
                    rule.Width,
                    rule.Weight,
                    rule.Price
                );
            }

            _inConfig.Add(tplId);

            if (_knownParentTplIds.Contains(tplId))
            {
                _parentOverrideIds.Add(tplId);
            }
        }

        _searchRefreshToken++;
        RebuildView();
    }

    private void ScanPresets()
    {
        _presets.Clear();

        if (_pathToMod is null)
        {
            return;
        }

        var dir = Path.Combine(_pathToMod, "presets");
        if (!Directory.Exists(dir))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(dir))
        {
            var ext = Path.GetExtension(file);
            if (!ext.Equals(".json", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".jsonc", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _presets.Add(new PresetEntry(Path.GetFileNameWithoutExtension(file), Path.GetFileName(file)));
        }
    }

    private void ApplyPreset(string presetName)
    {
        if (_pathToMod is null || _itemsIndex is null)
        {
            return;
        }

        var entry = _presets.FirstOrDefault(p => string.Equals(p.Name, presetName, StringComparison.Ordinal));
        if (entry is null)
        {
            return;
        }

        try
        {
            var preset = _modHelper.GetJsonDataFromFile<ItemsConfig>(Path.Combine(_pathToMod, "presets"), entry.File);
            _cfg = preset;
            LoadItemsFromConfig(_cfg);
        }
        catch (Exception ex)
        {
            ShowToast("Preset load failed: " + ex.Message, ToastErrorDurationMs);
        }
    }

    private void TogglePresets()
    {
        _presetsOpen = !_presetsOpen;
    }

    private void ClosePresets()
    {
        _presetsOpen = false;
    }

    private void SelectPreset(string name)
    {
        _presetsOpen = false;
        if (!string.IsNullOrWhiteSpace(name))
        {
            _presetConfirmName = name;
        }
    }

    private void ConfirmApplyPreset()
    {
        var name = _presetConfirmName;
        _presetConfirmName = null;
        if (!string.IsNullOrWhiteSpace(name))
        {
            ApplyPreset(name);
        }
    }

    private void CancelApplyPreset()
    {
        _presetConfirmName = null;
    }

    private void LoadWeightMultiplier()
    {
        if (_pathToMod is null)
        {
            return;
        }

        var paramsPath = Path.Combine(_pathToMod, ParamsConfig.FileName);
        if (!File.Exists(paramsPath))
        {
            DefaultConfigs.CreateParamsConfig(paramsPath);
        }

        var paramsCfg = _modHelper.GetJsonDataFromFile<ParamsConfig>(_pathToMod, ParamsConfig.FileName);
        _weightMultiplierText = FormatWeight(paramsCfg.WeightLimitRaw ?? 1f);
    }

    private void OnWeightInput(ChangeEventArgs e)
    {
        _weightMultiplierText = e.Value?.ToString() ?? "";
    }

    private void OnWeightBlur()
    {
        if (float.TryParse(_weightMultiplierText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
        {
            _weightMultiplierText = FormatWeight(v);
        }
        else
        {
            _weightMultiplierText = "1.0";
        }
    }

    private static string FormatWeight(float v)
    {
        var s = v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return s.Contains('.') ? s : s + ".0";
    }

    private sealed record PresetEntry(string Name, string File);

    // --- bulk ---
    private readonly Dictionary<string, CategoryBulk> _categoryBulk = new(StringComparer.Ordinal);

    private enum BulkField
    {
        MaxStackSize,
        MaxResources,
        Height,
        Width,
        Weight,
        Price
    }

    private sealed class CategoryBulk
    {
        public string MaxStackSize { get; set; } = "";
        public string MaxResources { get; set; } = "";
        public string Height { get; set; } = "";
        public string Width { get; set; } = "";
        public string Weight { get; set; } = "";
        public string Price { get; set; } = "";
    }

    private CategoryBulk GetCategoryBulk(string categoryName)
    {
        return _categoryBulk.GetValueOrDefault(categoryName) ?? (_categoryBulk[categoryName] = new CategoryBulk());
    }

    private void OnCategoryBulkInput(string categoryName, BulkField field, ChangeEventArgs e)
    {
        var text = e.Value?.ToString() ?? "";
        var bulk = GetCategoryBulk(categoryName);

        var targets = GetNonParentItems()
            .Where(x => string.Equals(x.Category, categoryName, StringComparison.Ordinal));

        switch (field)
        {
            case BulkField.MaxStackSize:
                bulk.MaxStackSize = text;
                foreach (var item in targets) item.MaxStackSizeText = text;
                break;

            case BulkField.MaxResources:
                bulk.MaxResources = text;
                foreach (var item in targets) item.MaxResourcesText = text;
                break;

            case BulkField.Height:
                bulk.Height = text;
                foreach (var item in targets) item.HeightText = text;
                break;

            case BulkField.Width:
                bulk.Width = text;
                foreach (var item in targets) item.WidthText = text;
                break;

            case BulkField.Weight:
                bulk.Weight = text;
                foreach (var item in targets) item.WeightText = text;
                break;

            case BulkField.Price:
                bulk.Price = text;
                foreach (var item in targets) item.PriceText = text;
                break;
        }

        // StateHasChanged();
    }

    // --- categories ---
    private bool IsCategoryOpen(string categoryName) => !_collapsedCategories.Contains(categoryName);

    private void ToggleCategory(string categoryName)
    {
        if (_collapsedCategories.Contains(categoryName))
            _collapsedCategories.Remove(categoryName);
        else
            _collapsedCategories.Add(categoryName);
    }

    private void RemoveCategoryItems(string categoryName)
    {
        var itemsToRemove = GetNonParentItems()
            .Where(x => string.Equals(x.Category, categoryName, StringComparison.Ordinal))
            .ToList();

        if (itemsToRemove.Count == 0)
        {
            return;
        }

        foreach (var item in itemsToRemove)
        {
            _configItemsById.Remove(item.TemplateId);
            _inConfig.Remove(item.TemplateId);
            _cfg?.Items.Remove(item.TemplateId);
        }

        _categoryBulk.Remove(categoryName);
        _searchRefreshToken++;

        RebuildView();
        StateHasChanged();
    }

    // ---- search / add / remove ----
    private void SelectSuggestion(string tplId)
    {
        _ = TryAddToConfig(tplId);

        if (_configItemsById.TryGetValue(tplId, out var row) && !_parentOverrideIds.Contains(tplId))
            _collapsedCategories.Remove(row.Category);

        _highlightTplId = tplId;
        _pendingScrollTplId = tplId;

        _highlightClearDebouncer.Debounce(HighlightDurationMs, () =>
        {
            if (string.Equals(_highlightTplId, tplId, StringComparison.Ordinal))
                _highlightTplId = null;

            return InvokeAsync(StateHasChanged);
        });

        StateHasChanged();
    }

    private bool TryAddToConfig(string tplId)
    {
        if (_cfg is null || _itemsIndex is null)
        {
            return false;
        }

        if (_inConfig.Contains(tplId))
        {
            return false;
        }

        _cfg.Items[tplId] = CreateRule(stackSize: null, maxResource: null, height: null, width: null, weight: null, price: null);

        if (_itemsIndex.TryGet(tplId, out var db))
        {
            _configItemsById[tplId] = new ConfigItemRow(tplId, db.Name, db.Parent, db.Category, 0, 0, 0, 0, 0,0);

            if (string.Equals(db.Category, "Parent", StringComparison.Ordinal))
            {
                _parentOverrideIds.Add(tplId);
            }
        }
        else
        {
            _configItemsById[tplId] = new ConfigItemRow(tplId, "unknown", "unknown", OtherCategoryName, 0, 0,0,0,0,0);
        }

        _inConfig.Add(tplId);

        _searchRefreshToken++;

        RebuildView();
        return true;
    }

    private Task<List<Suggestion>> SearchItemsAsync(string query)
    {
        if (_itemsIndex is null)
        {
            return Task.FromResult(new List<Suggestion>());
        }

        return Task.FromResult(_itemsIndex.Search(query, _inConfig, SearchResultLimit));
    }

    private void RemoveItem(string tplId)
    {
        _configItemsById.Remove(tplId);
        _inConfig.Remove(tplId);
        _cfg?.Items.Remove(tplId);

        _parentOverrideIds.Remove(tplId);

        _searchRefreshToken++;

        if (string.Equals(_highlightTplId, tplId, StringComparison.Ordinal))
        {
            _highlightTplId = null;
        }

        RebuildView();
        StateHasChanged();
    }

    // ---- save ----
    private async Task SaveAsync()
    {
        if (_pathToMod is null)
            return;

        try
        {
            _isSaving = true;
            await InvokeAsync(StateHasChanged);

            var newCfg = new ItemsConfig
            {
                Items = new Dictionary<string, ItemsConfig.ItemRule>(StringComparer.Ordinal)
            };

            foreach (var row in _parentOverrides.OrderBy(x => x.TemplateId, StringComparer.Ordinal))
            {
                int? stack = row.MaxStackSize > 0 ? row.MaxStackSize : null;
                int? res = row.MaxResource > 0 ? row.MaxResource : null;
                int? height = row.Height > 0 ? row.Height : null;
                int? width = row.Width > 0 ? row.Width : null;
                double? weight = row.Weight > 0 ? row.Weight : null;
                double? price = row.Price > 0 ? row.Price : null;

                newCfg.Items[row.TemplateId] = CreateRule(stack, res, height, width, weight, price);
            }

            foreach (var row in GetNonParentItems().OrderBy(x => x.TemplateId, StringComparer.Ordinal))
            {
                int? stack = row.MaxStackSize > 0 ? row.MaxStackSize : null;
                int? res = row.MaxResource > 0 ? row.MaxResource : null;
                int? height = row.Height > 0 ? row.Height : null;
                int? width = row.Width > 0 ? row.Width : null;
                double? weight = row.Weight > 0 ? row.Weight : null;
                double? price = row.Price > 0 ? row.Price : null;

                newCfg.Items[row.TemplateId] = CreateRule(stack, res, height, width, weight, price);
            }

            var text = BuildConfigJsonc();

            var dst = Path.Combine(_pathToMod, ItemsConfig.FileName);

            await using (var fs = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            await using (var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                await sw.WriteAsync(text);
                await sw.FlushAsync();
                fs.Flush(flushToDisk: true);
            }

            File.SetLastWriteTimeUtc(dst, DateTime.UtcNow);

            await SaveParamsConfig();

            ShowToast("Saved.", ToastDurationMs);
        }
        catch (Exception ex)
        {
            ShowToast("Save failed: " + ex.Message, ToastErrorDurationMs);
        }
        finally
        {
            _isSaving = false;

            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SaveParamsConfig()
    {
        if (_pathToMod is null)
        {
            return;
        }

        if (!float.TryParse(_weightMultiplierText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var weightMult))
        {
            return;
        }

        var text = "{\n\t\"WeightLimitMultiplier\": " + weightMult.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n}";
        var dst = Path.Combine(_pathToMod, ParamsConfig.FileName);

        await using (var fs = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        await using (var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
        {
            await sw.WriteAsync(text);
            await sw.FlushAsync();
            fs.Flush(flushToDisk: true);
        }

        File.SetLastWriteTimeUtc(dst, DateTime.UtcNow);
    }

    private string BuildConfigJsonc()
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"Items\": {");

        var blocks = CollectConfigBlocks();
        var totalItems = blocks.Sum(b => b.Rows.Count);
        var remaining = totalItems;

        foreach (var block in blocks)
        {
            AppendCategoryBlock(sb, block, ref remaining);
        }

        sb.AppendLine("  }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private List<(string? Header, List<ConfigItemRow> Rows)> CollectConfigBlocks()
    {
        var blocks = new List<(string? Header, List<ConfigItemRow> Rows)>();

        if (_parentOverrides.Count > 0)
        {
            blocks.Add(("Parents", _parentOverrides.OrderBy(x => x.TemplateId, StringComparer.Ordinal).ToList()));
        }

        if (_viewCategories is not null)
        {
            foreach (var cat in _viewCategories)
            {
                if (cat.Items.Count == 0) continue;
                blocks.Add((cat.Name, cat.Items.OrderBy(x => x.TemplateId, StringComparer.Ordinal).ToList()));
            }
        }

        return blocks;
    }

    private void AppendCategoryBlock(StringBuilder sb, (string? Header, List<ConfigItemRow> Rows) block, ref int remaining)
    {
        if (block.Header is not null)
        {
            var comment = string.Equals(block.Header, "Parents", StringComparison.Ordinal)
                ? "Parents"
                : SanitizeComment(block.Header);
            sb.AppendLine($"    // {comment}");
        }

        foreach (var row in block.Rows)
        {
            remaining--;
            AppendItemRuleLine(sb, row, addComma: remaining > 0);
        }
    }

    private static void AppendItemRuleLine(StringBuilder sb, ConfigItemRow row, bool addComma)
    {
        sb.Append("    \"");
        sb.Append(row.TemplateId);
        sb.Append("\": ");
        sb.Append(BuildRuleInline(row));

        if (addComma)
        {
            sb.Append(",");
        }

        if (!string.IsNullOrWhiteSpace(row.Name) && !string.Equals(row.Name, "null", StringComparison.OrdinalIgnoreCase))
        {
            sb.Append("\t// ");
            sb.Append(SanitizeComment(row.Name));
        }

        sb.AppendLine();
    }

    private static string BuildRuleInline(ConfigItemRow row)
    {
        int? stack = row.MaxStackSize > 0 ? row.MaxStackSize : null;
        int? res = row.MaxResource > 0 ? row.MaxResource : null;

        int? height = row.Height > 0 ? row.Height : null;
        int? width = row.Width > 0 ? row.Width : null;

        double? weight = row.Weight > 0 ? row.Weight : null;
        double? price = row.Price > 0 ? row.Price : null;

        var parts = new List<string>();

        if (stack is not null) parts.Add($"\"StackSize\": {stack.Value}");
        if (res is not null) parts.Add($"\"MaxResource\": {res.Value}");
        if (height is not null) parts.Add($"\"ItemHeight\": {height.Value}");
        if (width is not null) parts.Add($"\"ItemWidth\": {width.Value}");
        if (weight is not null) parts.Add($"\"WeightMultiplier\": {weight.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (price is not null) parts.Add($"\"PriceMultiplier\": {price.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        return parts.Count == 0
            ? "{ }"
            : "{ " + string.Join(", ", parts) + " }";
    }

    private static string SanitizeComment(string s)
    {
        return (s ?? string.Empty)
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .Trim();
    }

    private void ShowToast(string message, int durationMs = 1000)
    {
        _toastMessage = message;
        _toastVisible = true;

        _ = InvokeAsync(StateHasChanged);

        _toastClearDebouncer.Debounce(durationMs, () =>
        {
            return HideToastAsync();
        });
    }

    private async Task HideToastAsync()
    {
        _toastVisible = false;
        await InvokeAsync(StateHasChanged);

        await Task.Delay(300);

        _toastMessage = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ScrollToTopAsync()
    {
        await _js.InvokeVoidAsync("eval", "window.scrollTo({top:0,behavior:\"smooth\"});");
    }

}
