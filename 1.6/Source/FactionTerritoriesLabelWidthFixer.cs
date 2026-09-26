using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TemporaryPatches
{
    [StaticConstructorOnStartup]
    public static class FactionTerritoriesLabelWidthFixer
    {
        private const char NonBreakingSpace = ' ';
        private const float WidthMultiplier = 1.8f;
        private const float HeightMultiplier = 1.3f;

        private static FieldInfo labelsField;
        private static FieldInfo sizeField;
        private static FieldInfo contentField;

        static FactionTerritoriesLabelWidthFixer()
        {
            try
            {
                Type cacheType = GenTypes.GetTypeInAnyAssembly("FactionTerritoriesPerformanceFix.TerritoryLabelCache");
                MethodInfo rebuildMethod = cacheType?.GetMethod("Rebuild", BindingFlags.NonPublic | BindingFlags.Static);
                if (rebuildMethod == null)
                {
                    return;
                }
                labelsField = cacheType.GetField("Labels", BindingFlags.NonPublic | BindingFlags.Static);
                Type cachedLabelType = cacheType.GetNestedType("CachedLabel", BindingFlags.NonPublic);
                sizeField = cachedLabelType?.GetField("Size", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                contentField = cachedLabelType?.GetField("Content", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (labelsField == null || sizeField == null || contentField == null)
                {
                    return;
                }
                Harmony harmony = new Harmony("ferny.temporarypatches.widenfactionterritorieslabels");
                harmony.Patch(rebuildMethod, postfix: new HarmonyMethod(typeof(FactionTerritoriesLabelWidthFixer), nameof(Postfix)));
            }
            catch (Exception ex)
            {
                Log.Warning("[Temporary Patches] Failed to patch Faction Territories label width: " + ex);
            }
        }

        private static void Postfix()
        {
            if (!(labelsField.GetValue(null) is IList labels))
            {
                return;
            }
            foreach (object label in labels)
            {
                if (label == null)
                {
                    continue;
                }
                if (contentField.GetValue(label) is GUIContent content && content.text != null)
                {
                    content.text = content.text.Replace(' ', NonBreakingSpace);
                }

                Vector2 size = (Vector2)sizeField.GetValue(label);
                size.x *= WidthMultiplier;
                size.y *= HeightMultiplier;
                sizeField.SetValue(label, size);
            }
        }
    }
}
