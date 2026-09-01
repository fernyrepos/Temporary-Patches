using System;
using System.Collections;
using System.Reflection;
using RimWorld.Planet;
using Verse;

namespace TemporaryPatches
{
    public class GameComponent_ForceFactionTerritoriesMapMode : GameComponent
    {
        private const int RetryIntervalTicks = 60;
        private const string TargetDefName = "FactionTerritories";

        private static bool resolvedTypes;
        private static FieldInfo instanceField;
        private static FieldInfo mapModesField;
        private static FieldInfo currentMapModeField;
        private static MethodInfo requestSwitchMethod;

        private bool applied;
        private int nextAttemptTick;

        public GameComponent_ForceFactionTerritoriesMapMode(Game game)
        {
        }

        public override void GameComponentUpdate()
        {
            if (applied || !WorldRendererUtility.WorldRendered)
            {
                return;
            }
            int ticksGame = Find.TickManager?.TicksGame ?? 0;
            if (ticksGame < nextAttemptTick)
            {
                return;
            }
            nextAttemptTick = ticksGame + RetryIntervalTicks;
            applied = TrySwitchToFactionTerritories();
        }

        private static bool ResolveTypes()
        {
            if (resolvedTypes)
            {
                return instanceField != null;
            }
            resolvedTypes = true;
            try
            {
                Type mapModeComponentType = GenTypes.GetTypeInAnyAssembly("MapModeFramework.MapModeComponent");
                if (mapModeComponentType == null)
                {
                    return false;
                }
                instanceField = mapModeComponentType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                mapModesField = mapModeComponentType.GetField("mapModes", BindingFlags.Public | BindingFlags.Instance);
                currentMapModeField = mapModeComponentType.GetField("currentMapMode", BindingFlags.Public | BindingFlags.Instance);
                requestSwitchMethod = mapModeComponentType.GetMethod("RequestMapModeSwitch", BindingFlags.Public | BindingFlags.Instance);
                return instanceField != null && mapModesField != null && requestSwitchMethod != null;
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to resolve Map Mode Framework types: " + ex);
                return false;
            }
        }

        private static bool TrySwitchToFactionTerritories()
        {
            if (!ResolveTypes())
            {
                return true;
            }
            try
            {
                object instance = instanceField.GetValue(null);
                if (instance == null)
                {
                    return false;
                }
                if (IsFactionTerritoriesMode(currentMapModeField?.GetValue(instance)))
                {
                    return true;
                }
                if (!(mapModesField.GetValue(instance) is IEnumerable mapModes))
                {
                    return false;
                }
                foreach (object mapMode in mapModes)
                {
                    if (IsFactionTerritoriesMode(mapMode))
                    {
                        requestSwitchMethod.Invoke(instance, new[] { mapMode });
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to force Faction Territories map mode: " + ex);
                return true;
            }
        }

        private static bool IsFactionTerritoriesMode(object mapMode)
        {
            if (mapMode == null)
            {
                return false;
            }
            object def = mapMode.GetType().GetField("def", BindingFlags.Public | BindingFlags.Instance)?.GetValue(mapMode);
            string defName = def?.GetType().GetField("defName", BindingFlags.Public | BindingFlags.Instance)?.GetValue(def) as string;
            return defName == TargetDefName;
        }
    }
}
