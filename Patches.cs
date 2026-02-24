using System.Linq;
using GameData.CoreLanguage.Collections;
using HarmonyLib;
using NightScene.GuestManagementUtility;
using SgrYuki;

namespace SgrMystiaAssistant;

[HarmonyPatch]
[AutoLog]
public partial class SpecialGuestsControllerPatch
{
    [HarmonyPatch(typeof(SpecialGuestsController), nameof(SpecialGuestsController.GetOrderFoodText))]
    [HarmonyPostfix]
    public static void GetOrderFoodTextPostfix(SpecialGuestsController __instance, GuestsManager.SpecialOrder specialOrder, ref string __result)
    {
        if (PluginManager.Activated)
        {
            var ret = $"{__result} ({specialOrder.foodRequest.GetFoodTag()}) ({GuestGroupControllerPatch.GetSpecialGuestFoodTags(__instance, ",", "ff0000")})";
            __result = ret;
        }
    }

    [HarmonyPatch(typeof(SpecialGuestsController), nameof(SpecialGuestsController.GetOrderBevText))]
    [HarmonyPostfix]
    public static void GetOrderBevTextPostfix(SpecialGuestsController __instance, GuestsManager.SpecialOrder specialOrder, ref string __result)
    {
        if (PluginManager.Activated)
        {
            var ret = $"{__result} ({GuestGroupControllerPatch.GetSpecialGuestBevLikedTags(__instance, "ff0000")})";
            __result = ret;
        }
    }
}

[AutoLog]
public partial class GuestGroupControllerPatch
{
    [HarmonyPatch(typeof(GuestGroupController), nameof(GuestGroupController.MoveToDesk))]
    [HarmonyPostfix]
    public static void MoveToDesk_Postfix(GuestGroupController __instance)
    {
        if (PluginManager.Activated)
        {
            FloatingTextHelper.ShowFloatingTextOnGuest(__instance, GetSpecialGuestFoodTags(__instance));
        }
    }

    public static string GetSpecialGuestBevLikedTags(GuestGroupController guest, string color = "E6B4A6")
    {
        if (guest.TryCast<SpecialGuestsController>() is SpecialGuestsController sgc)
        {
            var tags = sgc.LikeBevTags.Distinct().Select(t => t.GetBeverageTag());
            return $"<color=#{color}>{string.Join(",", tags)}";
        }
        return "";
    }

    public static string GetSpecialGuestFoodLikedTags(GuestGroupController guest, string color = "E6B4A6")
    {
        if (guest.TryCast<SpecialGuestsController>() is SpecialGuestsController sgc)
        {
            var tags = sgc.LikeFoodTags.Distinct().Select(t => t.GetFoodTag());
            return $"<color=#{color}>{string.Join(",", tags)}";
        }
        return "";
    }
    public static string GetSpecialGuestFoodHatedTags(GuestGroupController guest, string color = "000000")
    {
        if (guest.TryCast<SpecialGuestsController>() is SpecialGuestsController sgc)
        {
            var tags = sgc.HateFoodTags.Distinct().Select(t => t.GetFoodTag());
            return $"<color=#{color}>{string.Join(",", tags)}";
        }
        return "";
    }

    public static string GetSpecialGuestFoodTags(GuestGroupController guest, string delimiter = "\n", string colorLike = "E6B4A6", string colorHate = "000000")
    {
        if (guest.TryCast<SpecialGuestsController>() is SpecialGuestsController sgc)
        {
            return $"{GetSpecialGuestFoodLikedTags(guest, colorLike)}{delimiter}{GetSpecialGuestFoodHatedTags(guest, colorHate)}";
        }
        return "";
    }
}



[AutoLog]
public partial class GuestsManagerPatch
{
    [HarmonyPatch(typeof(GuestsManager), nameof(GuestsManager.LeaveFromDesk))]
    [HarmonyPostfix]
    public static void LeaveFromDesk_Postfix(GuestsManager __instance, GuestGroupController toLeave)
    {
        if (PluginManager.Activated)
        {
            FloatingTextHelper.RemoveFloatingTextOnGuest(toLeave);
        }
    }
}
