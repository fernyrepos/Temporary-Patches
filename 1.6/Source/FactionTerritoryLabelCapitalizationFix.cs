using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TemporaryPatches
{
    [StaticConstructorOnStartup]
    public static class FactionTerritoryLabelCapitalizationFix
    {
        static FactionTerritoryLabelCapitalizationFix()
        {
            try
            {
                Type utilityType = GenTypes.GetTypeInAnyAssembly("FactionTerritories.FactionTerritoriesUtility");
                MethodInfo safeFactionLabelMethod = utilityType?.GetMethod("SafeFactionLabel", BindingFlags.Public | BindingFlags.Static);
                if (safeFactionLabelMethod == null)
                {
                    return;
                }
                Harmony harmony = new Harmony("ferny.temporarypatches.fixfactionterritorieslabelcasing");
                harmony.Patch(safeFactionLabelMethod, postfix: new HarmonyMethod(typeof(FactionTerritoryLabelCapitalizationFix), nameof(Postfix)));
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to patch Faction Territories label casing: " + ex);
            }
        }

        private static void Postfix(Faction faction, ref string __result)
        {
            if (faction != null && !faction.HasName && !GenText.NullOrEmpty(__result))
            {
                __result = GenText.CapitalizeAsTitle(__result);
            }
        }
    }
}
