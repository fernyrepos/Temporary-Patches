using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace TemporaryPatches
{
    [StaticConstructorOnStartup]
    public static class FactionTerritoriesLabelSuppressor
    {
        private static Harmony harmony;
        private static MethodInfo onGuiMethod;
        private static MethodInfo postfixMethod;

        static FactionTerritoriesLabelSuppressor()
        {
            try
            {
                Type patchType = GenTypes.GetTypeInAnyAssembly("FactionTerritories.WorldLayer_MapMode_OnGUI_FactionTerritoriesLabels");
                postfixMethod = patchType?.GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);
                Type worldLayerType = GenTypes.GetTypeInAnyAssembly("MapModeFramework.WorldLayer_MapMode");
                onGuiMethod = worldLayerType?.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (postfixMethod == null || onGuiMethod == null)
                {
                    Log.Warning("[Temporary Patches] Faction Territories label suppressor: could not resolve target methods. postfixMethod=" + (postfixMethod != null) + " onGuiMethod=" + (onGuiMethod != null));
                    return;
                }
                harmony = new Harmony("ferny.temporarypatches.suppressfactionterritorieslabels");
                LogPatchState("initial");
                TryUnpatch();
                LogPatchState("after initial unpatch");
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to suppress Faction Territories labels: " + ex);
            }
        }

        internal static void Recheck()
        {
            if (harmony == null || onGuiMethod == null || postfixMethod == null)
            {
                return;
            }
            Patches patches = Harmony.GetPatchInfo(onGuiMethod);
            bool stillPresent = patches != null && patches.Postfixes.Any(p => p.PatchMethod == postfixMethod);
            if (stillPresent)
            {
                Log.Warning("[Temporary Patches] Faction Territories vanilla label postfix reappeared; re-unpatching. Current postfix owners: " + DescribePostfixOwners());
                TryUnpatch();
            }
        }

        private static void TryUnpatch()
        {
            try
            {
                harmony.Unpatch(onGuiMethod, postfixMethod);
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Unpatch attempt failed: " + ex);
            }
        }

        private static void LogPatchState(string label)
        {
            Patches patches = Harmony.GetPatchInfo(onGuiMethod);
            Log.Message("[Temporary Patches] WorldLayer_MapMode.OnGUI patch state (" + label + "): " + DescribePostfixOwners() + " | patches null=" + (patches == null));
        }

        private static string DescribePostfixOwners()
        {
            Patches patches = Harmony.GetPatchInfo(onGuiMethod);
            if (patches == null || patches.Postfixes == null || patches.Postfixes.Count == 0)
            {
                return "(none)";
            }
            return string.Join(", ", patches.Postfixes.Select(p => p.owner + ":" + p.PatchMethod.DeclaringType?.FullName + "." + p.PatchMethod.Name));
        }
    }
}
