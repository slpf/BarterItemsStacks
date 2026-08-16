using System.Text;
using BarterItemsStacks.Web.Config;
using BarterItemsStacks.Web.Models;
using BarterItemsStacks.Web.Services;
using Microsoft.AspNetCore.Components;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace BarterItemsStacks.Web.Pages;

public partial class CategoriesEditor : ComponentBase, IDisposable
{
    private const int ToastDurationMs = 2500;
    private const int ToastErrorDurationMs = 4000;
    private const int SearchResultLimit = 9;
    private const string OtherCategoryName = "Other/NS/NT";

    [Parameter] public EventCallback OnChanged { get; set; }

    [Parameter] public EventCallback OnClose { get; set; }

    [Inject] private ModHelper _modHelper { get; set; } = default!;

    [Inject] private DatabaseServer _databaseServer { get; set; } = default!;

    [Inject] private LocaleService _localeService { get; set; } = default!;

    private string? _error;
    private string? _pathToMod;
    private bool _isSaving;

    private readonly List<CategoryRow> _rows = new();

    private int? _dragIndex;

    private ItemsDbIndex? _itemsIndex;
    private readonly ItemImages _images = new();
    private readonly HashSet<string> _nodeIds = new(StringComparer.Ordinal);

    private EditorMode _mode = EditorMode.List;
    private CategoryRow? _editingRow;
    private readonly List<IdRow> _idRows = new();
    private readonly HashSet<string> _idSet = new(StringComparer.Ordinal);
    private int _idSearchToken;

    private readonly Debouncer _toastClearDebouncer = new();

    private string? _toastMessage;
    private bool _toastVisible;

    private string? _confirmMessage;
    private Func<Task>? _confirmAction;
    private bool _dirty;
    private string _confirmYesText = "Delete";

    private enum EditorMode
    {
        List,
        IdEdit
    }

    protected override void OnInitialized()
    {
        try
        {
            _pathToMod = _modHelper.GetAbsolutePathToModFolder(typeof(CategoriesConfig).Assembly);
            CategoriesConfig.EnsureExists(_pathToMod);

            var cfg = _modHelper.GetJsonDataFromFile<CategoriesConfig>(_pathToMod, CategoriesConfig.FileName);

            _rows.Clear();
            foreach (var category in cfg.Categories)
            {
                _rows.Add(new CategoryRow(category));
            }

            var resolver = CategoryResolver.Build(cfg.Categories);
            var localeLocalized = _localeService.GetLocaleDb();
            var localeEn = _localeService.GetLocaleDb("en");
            _itemsIndex = new ItemsDbIndex(_databaseServer, OtherCategoryName, localeLocalized, localeEn, resolver);

            _nodeIds.Clear();
            foreach (var kvp in _databaseServer.GetTables().Templates.Items)
            {
                if (string.Equals(kvp.Value.Type, "Node", StringComparison.OrdinalIgnoreCase))
                {
                    _nodeIds.Add(kvp.Key.ToString());
                }
            }

            _images.BuildIndex();
        }
        catch (Exception ex)
        {
            _error = ex.ToString();
        }
    }

    private void OnNameInput(CategoryRow row, ChangeEventArgs e)
    {
        row.Name = e.Value?.ToString() ?? "";
        _dirty = true;
    }

    private void DeleteRow(Guid id)
    {
        _rows.RemoveAll(r => r.Id == id);
        _dirty = true;
    }

    private void AddRow()
    {
        _rows.Add(new CategoryRow());
        _dirty = true;
    }

    private void OnDragStart(int index)
    {
        _dragIndex = index;
    }

    private void OnDragEnter(int index)
    {
        var src = _dragIndex;
        if (!src.HasValue || src.Value == index)
        {
            return;
        }

        var moved = _rows[src.Value];
        _rows.RemoveAt(src.Value);

        var target = index;
        if (target < 0)
        {
            target = 0;
        }
        if (target > _rows.Count)
        {
            target = _rows.Count;
        }

        _rows.Insert(target, moved);
        _dragIndex = target;
        _dirty = true;
    }

    private void OnDragEnd()
    {
        _dragIndex = null;
    }

    private void OnDrop()
    {
        _dragIndex = null;
    }

    private void EditIds(CategoryRow row)
    {
        _editingRow = row;
        _idRows.Clear();
        _idSet.Clear();

        foreach (var id in ParseIds(row.ParentIdsText).Concat(ParseIds(row.TemplateIdsText)))
        {
            if (_idSet.Add(id))
            {
                _idRows.Add(new IdRow(id, ResolveName(id), _nodeIds.Contains(id)));
            }
        }

        _idSearchToken++;
        _mode = EditorMode.IdEdit;
    }

    private void FlushIdEdits()
    {
        if (_editingRow is null)
        {
            return;
        }

        var parents = new List<string>();
        var templates = new List<string>();

        foreach (var r in _idRows)
        {
            if (_nodeIds.Contains(r.Id))
            {
                parents.Add(r.Id);
            }
            else
            {
                templates.Add(r.Id);
            }
        }

        _editingRow.ParentIdsText = string.Join(", ", parents);
        _editingRow.TemplateIdsText = string.Join(", ", templates);
    }

    private void BackToCategories()
    {
        FlushIdEdits();

        _editingRow = null;
        _idRows.Clear();
        _idSet.Clear();
        _mode = EditorMode.List;
    }

    private void AddId(string tplId)
    {
        if (string.IsNullOrWhiteSpace(tplId) || _idSet.Contains(tplId))
        {
            return;
        }

        _idSet.Add(tplId);
        _idRows.Add(new IdRow(tplId, ResolveName(tplId), _nodeIds.Contains(tplId)));
        _idSearchToken++;
        _dirty = true;
    }

    private void RemoveId(string id)
    {
        _idRows.RemoveAll(r => r.Id == id);
        _idSet.Remove(id);
        _idSearchToken++;
        _dirty = true;
    }

    private void AskDeleteCategory(CategoryRow row)
    {
        var label = string.IsNullOrWhiteSpace(row.Name) ? "(unnamed)" : row.Name;
        _confirmMessage = $"Delete category \"{label}\"?";
        _confirmYesText = "Delete";
        _confirmAction = () =>
        {
            DeleteRow(row.Id);
            return Task.CompletedTask;
        };
    }

    private void AskRemoveId(IdRow idRow)
    {
        var label = string.IsNullOrWhiteSpace(idRow.Name) ? idRow.Id : idRow.Name;
        _confirmMessage = $"Remove \"{label}\"?";
        _confirmYesText = "Delete";
        _confirmAction = () =>
        {
            RemoveId(idRow.Id);
            return Task.CompletedTask;
        };
    }

    private async Task ConfirmYes()
    {
        var action = _confirmAction;
        _confirmAction = null;
        _confirmMessage = null;
        if (action is not null)
        {
            await action();
        }
    }

    private void ConfirmNo()
    {
        _confirmAction = null;
        _confirmMessage = null;
    }

    private Task<List<Suggestion>> SearchIdsAsync(string query)
    {
        if (_itemsIndex is null)
        {
            return Task.FromResult(new List<Suggestion>());
        }

        var result = _itemsIndex
            .Search(query, _idSet, SearchResultLimit)
            .Select(s => new Suggestion(s.TemplateId, s.Name, _nodeIds.Contains(s.TemplateId) ? "Parent" : "Item", s.InConfig))
            .ToList();

        return Task.FromResult(result);
    }

    private string IdImageSrc(string tplId) => _images.Src(tplId);

    private string ResolveName(string id)
    {
        if (_itemsIndex is not null && _itemsIndex.TryGet(id, out var entry))
        {
            return entry.Name;
        }

        return "";
    }

    private static List<string> ParseIds(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        return text
            .Split(new[] { ',', ';', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static int CountIds(string? text)
    {
        return ParseIds(text).Count;
    }

    private async Task SaveCategoriesAsync()
    {
        if (_pathToMod is null)
        {
            return;
        }

        try
        {
            _isSaving = true;
            await InvokeAsync(StateHasChanged);

            FlushIdEdits();

            var cfg = new CategoriesConfig
            {
                Categories = _rows
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                    .Select(r => new CategoriesConfig.CategoryEntry
                    {
                        Name = r.Name.Trim(),
                        ParentIds = ParseIds(r.ParentIdsText),
                        TemplateIds = ParseIds(r.TemplateIdsText)
                    })
                    .ToList()
            };

            var text = CategoriesConfig.Serialize(cfg);
            var dst = Path.Combine(_pathToMod, CategoriesConfig.FileName);

            await using (var fs = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            await using (var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                await sw.WriteAsync(text);
                await sw.FlushAsync();
                fs.Flush(flushToDisk: true);
            }

            File.SetLastWriteTimeUtc(dst, DateTime.UtcNow);

            _dirty = false;

            await OnChanged.InvokeAsync();

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

    private async Task Close()
    {
        if (_dirty && _confirmMessage is null)
        {
            _confirmMessage = "There are unsaved changes. Close anyway?";
            _confirmYesText = "Close";
            _confirmAction = () => OnClose.InvokeAsync();
            return;
        }

        await OnClose.InvokeAsync();
    }

    private void ShowToast(string message, int durationMs)
    {
        _toastMessage = message;
        _toastVisible = true;

        _ = InvokeAsync(StateHasChanged);

        _toastClearDebouncer.Debounce(durationMs, () => HideToastAsync());
    }

    private async Task HideToastAsync()
    {
        _toastVisible = false;
        await InvokeAsync(StateHasChanged);

        await Task.Delay(300);

        _toastMessage = null;
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _toastClearDebouncer?.Dispose();
    }

    private sealed record IdRow(string Id, string Name, bool IsParent);

    private sealed class CategoryRow
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string ParentIdsText { get; set; } = "";
        public string TemplateIdsText { get; set; } = "";

        public CategoryRow() { }

        public CategoryRow(CategoriesConfig.CategoryEntry? entry)
        {
            Name = entry?.Name ?? "";
            ParentIdsText = string.Join(", ", entry?.ParentIds ?? new List<string>());
            TemplateIdsText = string.Join(", ", entry?.TemplateIds ?? new List<string>());
        }
    }
}
