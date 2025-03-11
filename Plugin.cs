using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

[BepInPlugin("com.gamerwitchvic.nomissfishing", "No Miss Fishing Mod", "1.0.0")]
public class NoMissFishingMod : BaseUnityPlugin
{
    void Awake()
    {
        UnityEngine.Debug.Log("[NoMissFishing] Mod Loaded! Initializing Harmony patches...");

        Harmony harmony = new Harmony("com.gamerwitchvic.nomissfishing");

        MethodInfo method1 = AccessTools.Method(typeof(FishingUI), "HJBMAKFFJJN");
        MethodInfo method2 = AccessTools.Method(typeof(FishingUI), "FBBIFEBOBHF");

        if (method1 != null)
        {
            harmony.PatchAll();
            UnityEngine.Debug.Log("[NoMissFishing] Patch successfully applied to FishingUI.HJBMAKFFJJN!");
        }
        else
        {
            UnityEngine.Debug.LogError("[NoMissFishing] ERROR: Could not patch FishingUI.HJBMAKFFJJN!");
        }

        if (method2 != null)
        {
            harmony.PatchAll();
            UnityEngine.Debug.Log("[NoMissFishing] Patch successfully applied to FishingUI.FBBIFEBOBHF!");
        }
        else
        {
            UnityEngine.Debug.LogError("[NoMissFishing] ERROR: Could not patch FishingUI.FBBIFEBOBHF!");
        }
    }
}
