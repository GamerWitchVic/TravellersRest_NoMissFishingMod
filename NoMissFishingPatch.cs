using HarmonyLib;
using UnityEngine;
using UnityEngine.UI; 

[HarmonyPatch(typeof(FishingUI))]
public static class NoMissFishingPatch
{
    // Get a reference to the private "progress" field in FishingUI
    private static readonly AccessTools.FieldRef<FishingUI, Slider> progressRef =
        AccessTools.FieldRefAccess<FishingUI, Slider>("progress");

    // Patch to prevent passive progress loss
    [HarmonyPatch("HJBMAKFFJJN")]
    [HarmonyPrefix]
    public static bool PreventProgressLoss(FishingUI __instance)
    {
        Slider progress = progressRef(__instance);

        if (progress != null)
        {
            UnityEngine.Debug.Log("[NoMissFishing] Preventing passive progress loss.");
            progress.value = Mathf.Max(progress.value, progress.value); // Keeps progress unchanged
        }
        else
        {
            UnityEngine.Debug.LogError("[NoMissFishing] ERROR: Could not access 'progress' field!");
        }

        return false; // Skip the original function
    }

    // Patch to ensure progress always increases
    [HarmonyPatch("FBBIFEBOBHF")]
    [HarmonyPrefix]
    public static bool EnsureProgressIncrease(FishingUI __instance)
    {
        Slider progress = progressRef(__instance);

        if (progress != null)
        {
            UnityEngine.Debug.Log("[NoMissFishing] Ensuring fishing progress gain.");
            progress.value += 0.5f; // Adjust this value if necessary
        }
        else
        {
            UnityEngine.Debug.LogError("[NoMissFishing] ERROR: Could not access 'progress' field!");
        }

        return false; // Skip the original function
    }
}
