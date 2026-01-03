using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using System.Security.Cryptography;

namespace BarterItemsStacks
{
    [Injectable]
    public sealed class ConfigReload(ISptLogger<ConfigReload> logger) : IDisposable
    {
        private FileSystemWatcher? _watcher;
        private Timer? _debounceTimer;
        private readonly SemaphoreSlim _reloadLock = new(1, 1);

        private string? _filePath;
        private Func<Task<bool>>? _action;

        private byte[]? _lastHash;

        public void Start(string pathToFile, string fileName, Func<Task<bool>> action)
        {
            Stop();

            _filePath = Path.Combine(pathToFile, fileName);
            _action = action;

            if (string.IsNullOrWhiteSpace(pathToFile) || string.IsNullOrWhiteSpace(fileName))
            {
                logger.LogWithColor($"[BarterItemsStacks] Config Watcher Error >> Bad path: {_filePath}", LogTextColor.White, LogBackgroundColor.Red);
                return;
            }

            _debounceTimer = new Timer(async _ => await ReloadDebounced().ConfigureAwait(false), null, Timeout.Infinite, Timeout.Infinite);

            _watcher = new FileSystemWatcher(pathToFile, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };

            _watcher.Changed += OnChangevent;

            _lastHash = TryReadHash(_filePath);
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnChangevent;
                _watcher.Dispose();
                _watcher = null;
            }

            _debounceTimer?.Dispose();
            _debounceTimer = null;

            _filePath = null;
            _action = null;
            _lastHash = null;
        }

        private void OnChangevent(object sender, FileSystemEventArgs e)
        {
            _debounceTimer?.Change(500, Timeout.Infinite);
        }

        private async Task ReloadDebounced()
        {
            var filePath = _filePath;
            var reloadAction = _action;

            if (filePath == null || reloadAction == null)
                return;

            await _reloadLock.WaitAsync().ConfigureAwait(false);
            try
            {
                for (var i = 0; i < 10; i++)
                {
                    var hash = TryReadHash(filePath);
                    if (hash != null)
                    {
                        if (_lastHash != null && hash.SequenceEqual(_lastHash))
                            return;

                        _lastHash = hash;
                        break;
                    }

                    await Task.Delay(100).ConfigureAwait(false);
                }

                var success = await reloadAction().ConfigureAwait(false);

                if (success)
                {
                    logger.LogWithColor("[BarterItemsStacks] Config reloaded.", LogTextColor.Green, LogBackgroundColor.Black);
                } else
                {
                    logger.LogWithColor("[BarterItemsStacks] Config not reloaded.", LogTextColor.Red, LogBackgroundColor.Black);
                }
                
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[BarterItemsStacks] Config Watcher Error >> {ex}", LogTextColor.White, LogBackgroundColor.Red);
            }
            finally
            {
                _reloadLock.Release();
            }
        }

        private static byte[]? TryReadHash(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                var bytes = File.ReadAllBytes(path);
                return SHA256.HashData(bytes);
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            Stop();
            _reloadLock.Dispose();
        }
    }
}
