using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TemporaryPatches
{
    [StaticConstructorOnStartup]
    public static class FactionTerritorySettleWarning
    {
        private const string WarningText = "This tile is in another faction's territory. They will frequent the tile and might even attack. Consider settling on an unaffiliated tile.";

        private static MethodInfo tryGetClaimingFactionsMethod;
        private static MethodInfo doNextMethod;
        private static bool bypassNextCheck;

        static FactionTerritorySettleWarning()
        {
            try
            {
                Type cacheType = GenTypes.GetTypeInAnyAssembly("FactionTerritories.TerritoryOwnershipCache");
                tryGetClaimingFactionsMethod = cacheType?.GetMethod("TryGetClaimingFactions", BindingFlags.Public | BindingFlags.Static);
                doNextMethod = AccessTools.Method(typeof(Page_SelectStartingSite), "DoNext");
                if (tryGetClaimingFactionsMethod == null || doNextMethod == null)
                {
                    return;
                }
                Harmony harmony = new Harmony("ferny.temporarypatches.factionterritorysettlewarning");
                harmony.Patch(doNextMethod, prefix: new HarmonyMethod(typeof(FactionTerritorySettleWarning), nameof(Prefix)));
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to patch starting site territory warning: " + ex);
            }
        }

        private static bool Prefix(Page_SelectStartingSite __instance)
        {
            if (bypassNextCheck)
            {
                bypassNextCheck = false;
                return true;
            }
            try
            {
                PlanetTile selectedTile = Find.WorldInterface.SelectedTile;
                if (!selectedTile.Valid || !IsInForeignTerritory(selectedTile))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to check tile territory ownership: " + ex);
                return true;
            }
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(WarningText, delegate
            {
                bypassNextCheck = true;
                doNextMethod.Invoke(__instance, null);
            }));
            return false;
        }

        private static bool IsInForeignTerritory(int tile)
        {
            List<int> factionIds = new List<int>();
            bool claimed = (bool)tryGetClaimingFactionsMethod.Invoke(null, new object[] { tile, factionIds });
            return claimed && factionIds.Count > 0;
        }
    }
}
