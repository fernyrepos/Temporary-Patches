using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace TemporaryPatches
{
    [StaticConstructorOnStartup]
    public static class FactionTerritoriesExpansionSilencer
    {
        private const string ConstructionStartedPrefix = "Settlement construction: ";
        private const string ConstructionCompletedPrefix = "Settlement completed: ";

        static FactionTerritoriesExpansionSilencer()
        {
            Harmony harmony = new Harmony("ferny.temporarypatches.silentfactionexpansion");
            HarmonyMethod prefix = new HarmonyMethod(typeof(FactionTerritoriesExpansionSilencer), nameof(Prefix));

            foreach (MethodInfo method in typeof(Messages).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(Messages.Message) && m.GetParameters().FirstOrDefault()?.ParameterType == typeof(string)))
            {
                harmony.Patch(method, prefix);
            }
        }

        private static bool Prefix(string text)
        {
            if (text != null && (text.StartsWith(ConstructionStartedPrefix) || text.StartsWith(ConstructionCompletedPrefix)))
            {
                return false;
            }
            return true;
        }
    }
}
