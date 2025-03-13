using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

[BepInPlugin("com.yourname.nomissfishing", "No Miss Fishing Mod", "1.1.0")]
public class NoMissFishingMod : BaseUnityPlugin
{
    private Harmony harmony;

    void Awake()
    {
        UnityEngine.Debug.Log("[NoMissFishing] Mod Loaded! Patching FishingUI for instant catches...");
        harmony = new Harmony("com.yourname.nomissfishing");

        // Patch progress-gaining methods to force 100% success
        PatchFishingProgress();
    }

    /// <summary>
    /// Hooks only the functions that modify progress and forces instant success.
    /// </summary>
    private void PatchFishingProgress()
    {

        var fishingMethods = typeof(FishingUI)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => !m.IsConstructor && !m.IsAbstract && m.DeclaringType == typeof(FishingUI));

        // find the correct fishing method
        foreach (var method in fishingMethods)
        {
            var methodBody = method.GetMethodBody();
            if (methodBody == null || methodBody.LocalVariables == null) continue;

            if (method.IsAbstract || method.IsConstructor)
                continue; // Skip abstract and constructors

            if (MethodReferencesFields(method, "progress", "fishIcon", "box", "hitProgression", "hitReduction"))
            {
                UnityEngine.Debug.Log($"[NoMissFishing] 🎯 Patching Function: {method.Name}");
                harmony.Patch(method, postfix: new HarmonyMethod(typeof(NoMissFishingMod), nameof(ForceMaxProgress)));

            }
        }

    }

    /// <summary>
    /// Forces fishing progress to 100% immediately.
    /// </summary>
    static void ForceMaxProgress(FishingUI __instance)
    {
        Slider progressBar = Traverse.Create(__instance).Field("progress").GetValue<Slider>();
        if (progressBar != null)
        {
            progressBar.value = 1.0f; // Instantly max out progress
            UnityEngine.Debug.Log($"[NoMissFishing] 🎣 Forced Progress to 100%! Instant Catch Activated.");
        }
        else
        {
            UnityEngine.Debug.LogError("[NoMissFishing] ❌ ERROR: Could not find progress bar!");
        }
    }

    /// <summary>
    /// Ensures we only patch the correct method by checking references to key variables.
    /// </summary>
    private bool MethodReferencesFields(MethodInfo method, params string[] fieldNames)
    {
        try
        {
            // Load the assembly and type definition
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(method.Module.FullyQualifiedName);
            var typeDef = assembly.MainModule.Types.FirstOrDefault(t => t.Name == method.DeclaringType.Name);
            var methodDef = typeDef?.Methods.FirstOrDefault(m => m.Name == method.Name);

            if (methodDef == null || !methodDef.HasBody)
            {
                UnityEngine.Debug.LogError($"[NoMissFishing] ❌ ERROR extracting IL from {method.Name}: Method has no body.");
                return false;
            }

            var instructions = methodDef.Body.Instructions;

            // Get all referenced fields in the method
            var referencedFields = new HashSet<string>(
                instructions
                    .Where(instr => instr.OpCode == OpCodes.Ldfld || instr.OpCode == OpCodes.Stfld) // Load/store field
                    .Select(instr => (instr.Operand as FieldReference)?.Name)
                    .Where(name => name != null) // Remove nulls
            );

            return fieldNames.All(field => referencedFields.Contains(field));
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[NoMissFishing] ❌ ERROR analyzing method {method.Name}: {ex.Message}");
            return false;
        }
    }


}