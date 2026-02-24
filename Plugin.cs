using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SgrMystiaAssistant;

[BepInPlugin("000." + MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public static Plugin Instance;
    public Action<Scene, LoadSceneMode> LoadAction;

    public static ConfigEntry<bool> ConfigDebug;

    public static Type[] ToBePatched = [
        typeof(SpecialGuestsControllerPatch),
        typeof(GuestGroupControllerPatch),
        typeof(GuestsManagerPatch)
    ];

    public static bool AllPatched => PatchedException == null;
    public static Exception PatchedException = null;

    public Plugin()
    {
        Instance = this;
    }

    public override void Load()
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch { }
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        try
        {
            ClassInjector.RegisterTypeInIl2Cpp<PluginManager>();
            Log.LogInfo("Registered C# Types in Il2Cpp");
        }
        catch (Exception ex)
        {
            Log.LogError($"FAILED to Register Il2Cpp Type! {ex.Message}");
        }

        try
        {
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            var originalHandle = AccessTools.Method(typeof(CanvasScaler), "Handle");
            var postHandle = AccessTools.Method(typeof(BootstrapPatch), "Handle");
            harmony.Patch(originalHandle, postfix: new HarmonyMethod(postHandle));

            foreach (var patch in ToBePatched)
            {
                harmony.PatchAll(patch);
                Log.LogInfo($"Applied {patch.Name}");
            }
        }
        catch (Exception ex)
        {
            Log.LogFatal($"FAILED to Apply Hooks! {ex.Message}");
            PatchedException = ex;
        }
    }

    class BootstrapPatch
    {
        [HarmonyPostfix]
        static void Handle()
        {
            if (PluginManager.Instance == null)
            {
                Instance.Log.LogMessage("Bootstrapping Trainer...");
                try
                {
                    PluginManager.Create("PluginManager");
                    if (PluginManager.Instance != null)
                    {
                        Instance.Log.LogMessage("Trainer Bootstrapped!");
                    }
                }
                catch (Exception e)
                {
                    Instance.Log.LogMessage($"ERROR Bootstrapping Trainer: {e.Message}");
                }
            }
        }
    }
}
