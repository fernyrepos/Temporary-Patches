using System;
using System.Reflection;
using Verse;

namespace TemporaryPatches
{
    [StaticConstructorOnStartup]
    public static class FactionTerritoriesDefaultMapModeForcer
    {
        static FactionTerritoriesDefaultMapModeForcer()
        {
            try
            {
                Type modType = GenTypes.GetTypeInAnyAssembly("FactionTerritories.FactionTerritoriesMod");
                object instance = modType?.GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                object settings = instance?.GetType().GetField("Settings", BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance);
                settings?.GetType().GetField("defaultToFactionTerritoriesMapMode", BindingFlags.Public | BindingFlags.Instance)?.SetValue(settings, true);
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to force Faction Territories' default map mode setting: " + ex);
            }
        }
    }
}
