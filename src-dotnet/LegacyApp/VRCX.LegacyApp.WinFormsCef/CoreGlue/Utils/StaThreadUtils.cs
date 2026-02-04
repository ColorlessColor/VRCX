namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Utils;

internal static class StaThreadUtils
{
    public static Task RunOnStaThread(Action action)
    {
        var tcs = new TaskCompletionSource<object?>();

        var thread = new Thread(() =>
        {
            try
            {
                action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return tcs.Task;
    }

    public static Task<T> RunOnStaThread<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();

        var thread = new Thread(() =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return tcs.Task;
    }
}