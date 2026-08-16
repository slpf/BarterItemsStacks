using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using System.Security.Cryptography;

namespace BarterItemsStacks
{
    [Injectable]
    public sealed class ConfigReload(ISptLogger<ConfigReload> logger) : IDisposable
    {
        private readonly Dictionary<string, FileWatch> _watches = new(StringComparer.Ordinal);

        private sealed class FileWatch : IDisposable
        {
            public FileSystemWatcher Watcher = null!;
            public Timer Debounce = null!;
            public SemaphoreSlim Lock = null!;
            public Func<Task<bool>> Action = null!;
            public string FilePath = null!;
            public byte[]? LastHash;

            public void Dispose()
            {
                Watcher.EnableRaisingEvents = false;
                Watcher.Dispose();
                Debounce.Dispose();
                Lock.Dispose();
            }
        }

        public void Start(string pathToFile, string fileName, Func<Task<bool>> action)
        {
            if (string.IsNullOrWhiteSpace(pathToFile) || string.IsNullOrWhiteSpace(fileName))
            {
                logger.LogWithColor($"[BarterItemsStacks] Config Watcher Error >> Bad path: {Path.Combine(pathToFile, fileName)}", LogTextColor.White, LogBackgroundColor.Red);
                return;
            }

            var filePath = Path.Combine(pathToFile, fileName);

            if (_watches.TryGetValue(filePath, out var existing))
            {
                existing.Dispose();
                _watches.Remove(filePath);
            }

            var watch = new FileWatch
            {
                FilePath = filePath,
                Action = action,
                Lock = new SemaphoreSlim(1, 1),
                LastHash = TryReadHash(filePath)
            };

            watch.Debounce = new Timer(
                async state => await ReloadDebounced((FileWatch)state!).ConfigureAwait(false),
                watch,
                Timeout.Infinite,
                Timeout.Infinite);

            watch.Watcher = new FileSystemWatcher(pathToFile, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            watch.Watcher.Changed += (_, _) => watch.Debounce.Change(500, Timeout.Infinite);

            _watches[filePath] = watch;
        }

        public void Stop()
        {
            foreach (var watch in _watches.Values)
            {
                watch.Dispose();
            }
            _watches.Clear();
        }

        private async Task ReloadDebounced(FileWatch watch)
        {
            var filePath = watch.FilePath;
            var reloadAction = watch.Action;

            await watch.Lock.WaitAsync().ConfigureAwait(false);
            try
            {
                for (var i = 0; i < 10; i++)
                {
                    var hash = TryReadHash(filePath);
                    if (hash != null)
                    {
                        if (watch.LastHash != null && hash.SequenceEqual(watch.LastHash))
                            return;

                        watch.LastHash = hash;
                        break;
                    }

                    await Task.Delay(100).ConfigureAwait(false);
                }

                var success = await reloadAction().ConfigureAwait(false);

                if (success)
                {
                    logger.LogWithColor($"[BarterItemsStacks] Config reloaded: {Path.GetFileName(filePath)}", LogTextColor.Green, LogBackgroundColor.Black);
                }
                else
                {
                    logger.LogWithColor($"[BarterItemsStacks] Config not reloaded: {Path.GetFileName(filePath)}", LogTextColor.Red, LogBackgroundColor.Black);
                }
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[BarterItemsStacks] Config Watcher Error >> {ex}", LogTextColor.White, LogBackgroundColor.Red);
            }
            finally
            {
                watch.Lock.Release();
            }
        }

        private static byte[]? TryReadHash(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                return SHA256.HashData(File.ReadAllBytes(path));
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
