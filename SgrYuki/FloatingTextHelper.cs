using System;
using System.Collections.Concurrent;
using BepInEx.Unity.IL2CPP.Utils;
using NightScene.GuestManagementUtility;
using SgrMystiaAssistant;
using TMPro;
using UnityEngine;

namespace SgrYuki;

public static class FloatingTextHelper
{
    private static readonly ConcurrentDictionary<IntPtr, GameObject> guestFloatingObjects = new();
    private static GameObject MakeFloatingText(Transform parent, string text)
    {
        var go = new GameObject("FloatingText");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0, 1.6f, 0);

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 2.5f;
        tmp.alignment = TextAlignmentOptions.Center;

        // tmp.fontMaterial.EnableKeyword("OUTLINE_ON");      // 描边
        // tmp.outlineColor = Color.white;
        // tmp.outlineWidth = 0.025f;                         // 描边粗细，范围 0~1

        return go;
    }

    [OnMainThread]
    private static GameObject ShowFloatingText(GameObject oldObject, MonoBehaviour mono, string text, float duration)
    {
        if (mono == null)
        {
            return null;
        }
        if (oldObject != null)
        {
            UnityEngine.Object.Destroy(oldObject);
        }
        oldObject = MakeFloatingText(mono.transform, text);
        mono.StartCoroutine(FadeAndDestroy(oldObject.GetComponent<TextMeshPro>(), duration));
        return oldObject;
    }

    private static System.Collections.IEnumerator FadeAndDestroy(TextMeshPro tmp, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        float fade = 0f;
        while (fade < 1f)
        {
            fade += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fade / 1f);

            var c = tmp.color;
            c.a = alpha;
            tmp.color = c;

            yield return null;
        }

        if (tmp != null && tmp.gameObject != null)
        {
            UnityEngine.Object.Destroy(tmp.gameObject);
        }
    }

    public static void ShowFloatingTextOnGuest(GuestGroupController guest, string Message, float duration = 120f)
    {
        if (guestFloatingObjects.TryGetValue(guest.Pointer, out var oldObj))
        {
            CommandScheduler.EnqueueWithNoCondition(() =>
            {
                var newObj = ShowFloatingText(oldObj, guest?.guestInstances[0], Message, duration);
                guestFloatingObjects.TryUpdate(guest.Pointer, newObj, oldObj);
            });
        }
        else
        {
            CommandScheduler.EnqueueWithNoCondition(() =>
            {
                var newObj = ShowFloatingText(null, guest?.guestInstances[0], Message, duration);
                guestFloatingObjects.TryAdd(guest.Pointer, newObj);
            });
        }
    }

    public static void RemoveFloatingTextOnGuest(GuestGroupController guest)
    {
        if (guestFloatingObjects.TryGetValue(guest.Pointer, out var oldObj))
        {
            guestFloatingObjects.TryRemove(guest.Pointer, out _);
            if (oldObj) UnityEngine.Object.Destroy(oldObj);
        }
    }

    public static void RemoveAllFloatingTextOnGuest()
    {
        foreach (var (_, v) in guestFloatingObjects)
        {
            if (v) UnityEngine.Object.Destroy(v);
        }
        guestFloatingObjects.Clear();
    }
}
