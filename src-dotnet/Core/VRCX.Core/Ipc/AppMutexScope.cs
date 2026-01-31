using System;
using System.Security.Cryptography;
using System.Threading;
using VRCX.Core.Utils;

namespace VRCX.Core.Ipc;

public sealed class AppMutexScope : IDisposable
{
    private const string MutexPrefix = "Global\\VRCXAppMutex-fa7fc8b8-";

    private readonly Mutex _mutex;
    private bool _isMutexOwned;

    private AppMutexScope(string mutexName)
    {
        _mutex = new Mutex(true, mutexName);
    }

    private void OwnMutex()
    {
        try
        {
            _isMutexOwned = _mutex.WaitOne(0, true);
        }
        catch (AbandonedMutexException)
        {
            // The previous owner of the mutex exited without releasing it.
            // We can still take ownership of the mutex.
            _isMutexOwned = true;
        }

        if (!_isMutexOwned)
            throw new MutexOwnedByAnotherInstanceException();
    }

    public static AppMutexScope? TryEnter(
        AppMutexScopeType appType,
        string? pathToDataDirectory
    )
    {
        var mutexName = GetMutexName(appType, pathToDataDirectory);

        try
        {
            var mutexScope = new AppMutexScope(mutexName);
            mutexScope.OwnMutex();

            return mutexScope;
        }
        catch (MutexOwnedByAnotherInstanceException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_isMutexOwned)
            _mutex.ReleaseMutex();

        _mutex.Dispose();
    }

    #region Mutex Name

    private static string GetMutexName(AppMutexScopeType appType, string? pathToDataDirectory)
    {
        var normalizeDataPath = pathToDataDirectory != null ? PathUtils.NormalizePath(pathToDataDirectory) : "default";
        var normalizeDataPathHash = GetMd5Hash(normalizeDataPath);
        var appTypeName = appType switch
        {
            AppMutexScopeType.App => "App",
            _ => throw new ArgumentOutOfRangeException(nameof(appType), appType, null)
        };

        return $"{MutexPrefix}{appTypeName}-{normalizeDataPathHash}";
    }

    private static string GetMd5Hash(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexStringLower(hashBytes);
    }

    #endregion

    public enum AppMutexScopeType
    {
        App
    }

    private class MutexOwnedByAnotherInstanceException()
        : Exception("The application mutex is owned by another instance.");
}