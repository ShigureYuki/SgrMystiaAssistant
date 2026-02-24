using System;
using BepInEx;
using Il2CppInterop.Runtime;
using NightScene.GuestManagementUtility;
using SgrYuki;
using UnityEngine;

namespace SgrMystiaAssistant;

[AutoLog]
public partial class PluginManager : MonoBehaviour
{
    public static PluginManager Instance { get; private set; }
    public static bool Activated { get; private set; }
    public PluginManager(IntPtr ptr) : base(ptr)
    {
        if (Instance != null)
        {
            Log.LogWarning($"Another instance of PluginManager already exists! Destroying this one.");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    internal static GameObject Create(string name)
    {
        var gameObject = new GameObject(name);
        DontDestroyOnLoad(gameObject);

        gameObject.AddComponent(Il2CppType.Of<PluginManager>());

        return gameObject;
    }

    private void Awake()
    {

    }

    private void OnGUI()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Activated = !Activated;
            Notify.ShowExtern($"小助手已{(Activated ? "启用" : "禁用")}！");
            if (GuestsManager.Instance == null) return;
            if (Activated)
            {
                foreach (var (_, guest) in GuestsManager.Instance.AllGuestsControllersInDesk)
                {
                    var msg = GuestGroupControllerPatch.GetSpecialGuestFoodTags(guest);
                    if (!msg.IsNullOrWhiteSpace())
                    {
                        FloatingTextHelper.ShowFloatingTextOnGuest(guest, msg);
                    }
                }
            }
            else
            {
                FloatingTextHelper.RemoveAllFloatingTextOnGuest();
            }
        }
    }

    private void FixedUpdate()
    {
        CommandScheduler.Tick();
    }

    private void OnDestroy()
    {
    }
}
