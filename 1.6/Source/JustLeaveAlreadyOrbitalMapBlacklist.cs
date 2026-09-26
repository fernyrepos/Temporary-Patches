using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TemporaryPatches
{
    [StaticConstructorOnStartup]
    public static class JustLeaveAlreadyOrbitalMapBlacklist
    {
        static JustLeaveAlreadyOrbitalMapBlacklist()
        {
            try
            {
                MethodInfo getter = AccessTools.PropertyGetter(typeof(ExitMapGrid), nameof(ExitMapGrid.MapUsesExitGrid));
                if (getter == null)
                {
                    return;
                }
                HarmonyMethod postfix = new HarmonyMethod(typeof(JustLeaveAlreadyOrbitalMapBlacklist), nameof(Postfix))
                {
                    priority = Priority.Last
                };
                Harmony harmony = new Harmony("ferny.temporarypatches.blacklistorbitalexitgrid");
                harmony.Patch(getter, postfix: postfix);
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to blacklist orbital maps from exit grid: " + ex);
            }
        }

        private static void Postfix(Map ___map, ref bool __result)
        {
            if (__result && ___map != null && ___map.Tile.LayerDef == PlanetLayerDefOf.Orbit)
            {
                __result = false;
            }
        }
    }
}
