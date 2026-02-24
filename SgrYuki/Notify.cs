using Common.UI;
using SgrMystiaAssistant;

namespace SgrYuki;

[AutoLog]
public static partial class Notify
{
    public static ReceivedObjectDisplayerController Instance => DEYU.Singletons.MonoSingleton<ReceivedObjectDisplayerController>.Instance;

    [OnMainThread]
    public static void Show(string text)
    {
        try
        {
            ReceivedObjectDisplayerController.Instance?.NotifyTextMessage(text);
            Log.InfoCaller(text);
        }
        catch
        {
            Log.WarningCaller($"notify {text} failed");
            ShowExtern(text);
        }
    }

    [OnMainThread]
    public static void ShowExtern(string text)
    {
        Log.InfoCaller(text);
        NotifyOverlay.Show(text);
    }

    public static void ShowOnMainThread(string text)
    {
        CommandScheduler.EnqueueWithNoCondition(() => Show(text));
    }

    public static void ShowExternOnMainThread(string text)
    {
        CommandScheduler.EnqueueWithNoCondition(() => ShowExtern(text));
    }
}
