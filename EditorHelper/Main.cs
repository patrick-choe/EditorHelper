using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using EditorHelper.Settings;
using GDMiniJSON;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper
{
    internal static class Main
    {
        private static Harmony _harmony;
        private static UnityModManager.ModEntry _mod;
        internal static MainSettings Settings { get; private set; }
        internal static bool FirstLoaded;
        internal static bool IsEnabled;
        internal static bool highlightEnabled;
        // internal static UnityModManager.ModEntry.ModLogger Logger => _mod?.Logger;

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            var version = AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);

            if (version == null || (int) version != 75)
            {
                return false;
            }
            
            Settings = UnityModManager.ModSettings.Load<MainSettings>(modEntry);

            _mod = modEntry;
            _mod.OnToggle = OnToggle;
            _mod.OnGUI = OnGUI;
            _mod.OnSaveGUI = OnSaveGUI;

            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            _mod = modEntry;
            IsEnabled = value;

            if (value)
            {
                StartTweaks();
                ApplyConfig();
            }
            else
            {
                StopTweaks();
                ApplyConfig(true);
            }

            return true;
        }

        private static void StartTweaks()
        {
            _harmony = new Harmony(_mod.Info.Id);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            MethodInfo original = AccessTools.Method(typeof(scnEditor), "CreateFloorWithCharOrAngle");
            MethodInfo prefix = AccessTools.Method(typeof(FloorPatch), nameof(FloorPatch.SetFloors));
            MethodInfo postfix = AccessTools.Method(typeof(FloorPatch), nameof(FloorPatch.AfterCreateFloor));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
            original = AccessTools.Method(typeof(scnEditor), "DeleteFloor");
            postfix = AccessTools.Method(typeof(FloorPatch), nameof(FloorPatch.AfterDeleteFloor));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
            original = AccessTools.Method(typeof(scnEditor), "PasteFloors");
            postfix = AccessTools.Method(typeof(FloorPatch), nameof(FloorPatch.AfterPasteFloors));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));

            original = AccessTools.Method(typeof(scnEditor), "SelectFloor");
            prefix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.BeforeSelectFloor));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix));
            original = AccessTools.Method(typeof(scnEditor), "MultiSelectFloors");
            prefix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.UntargetFloor));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix));
            original = AccessTools.Method(typeof(scnEditor), "RemoveEventAtSelected");
            postfix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.AfterRemoveEventAtSelected));
            _harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            original = AccessTools.Method(typeof(PropertyControl_Text), nameof(PropertyControl_Text.Setup));
            postfix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.AfterTextSetup));
            _harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            original = AccessTools.Method(typeof(PropertyControl_Tile), nameof(PropertyControl_Tile.Setup));
            postfix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.AfterTileSetup));
            _harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            original = AccessTools.Method(typeof(CustomLevel), "RemakePath");
            prefix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.BeforeRemakePath));
            postfix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.AfterRemakePath));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
            original = AccessTools.Method(typeof(InspectorPanel), nameof(InspectorPanel.ShowPanel));
            prefix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.UntargetFloor));
            postfix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.AfterShowPanel));
            _harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
            original = AccessTools.Method(typeof(scnEditor), "QuitToMenu");
            postfix = AccessTools.Method(typeof(TargetPatch), nameof(TargetPatch.AfterQuitToMenu));
            _harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }

        private static void StopTweaks()
        {
            TargetPatch.UntargetFloor();
            _harmony.UnpatchAll(_harmony.Id);

            MethodInfo original = AccessTools.Method(typeof(scnEditor), "CreateFloorWithCharOrAngle");
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(scnEditor), "DeleteFloor");
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(scnEditor), "PasteFloors");
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(scnEditor), "SelectFloor");
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(scnEditor), "MultiSelectFloors");
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(scnEditor), "RemoveEventAtSelected");
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(PropertyControl_Text), nameof(PropertyControl_Text.Setup));
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(PropertyControl_Tile), nameof(PropertyControl_Tile.Setup));
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(CustomLevel), "RemakePath");
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(InspectorPanel), nameof(InspectorPanel.ShowPanel));
            _harmony.Unpatch(original, HarmonyPatchType.All);
            original = AccessTools.Method(typeof(scnEditor), "QuitToMenu");
            _harmony.Unpatch(original, HarmonyPatchType.All);

            _harmony = null;
        }
        
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Draw(modEntry);
            if (Settings.highlightTargetedTiles && !highlightEnabled)
            {
                highlightEnabled = true;
                if (scnEditor.instance?.selectedFloors.Count == 1)
                    if (scnEditor.instance.levelEventsPanel.selectedEventType == LevelEventType.MoveTrack || scnEditor.instance.levelEventsPanel.selectedEventType == LevelEventType.RecolorTrack)
                        TargetPatch.TargetFloor();
            }
            if (!Settings.highlightTargetedTiles && highlightEnabled)
            {
                highlightEnabled = false;
                if (TargetPatch.targets.Count != 0)
                    TargetPatch.UntargetFloor();
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
            
            ApplyConfig();
        }

        private static void ApplyConfig(bool forceRemove = false)
        {
            if (GCS.settingsInfo == null)
            {
                return;
            }

            if (!FirstLoaded)
            {
                FirstLoaded = true;
            }

            if (Settings.RemoveLimits && !forceRemove)
            {

                foreach (var propertyInfo in GCS.levelEventsInfo.SelectMany(eventPair =>
                    eventPair.Value.propertiesInfo.Select(propertiesPair => propertiesPair.Value)))
                {
                    switch (propertyInfo.type)
                    {
                        case PropertyType.Color:
                            propertyInfo.color_usesAlpha = true;
                            break;
                        case PropertyType.Int:
                            propertyInfo.int_min = int.MinValue;
                            propertyInfo.int_max = int.MaxValue;
                            break;
                        case PropertyType.Float:
                            propertyInfo.float_min = float.NegativeInfinity;
                            propertyInfo.float_max = float.PositiveInfinity;
                            break;
                        case PropertyType.Vector2:
                            propertyInfo.maxVec = Vector2.positiveInfinity;
                            propertyInfo.minVec = Vector2.negativeInfinity;
                            break;
                    }
                }

                foreach (var propertyInfo in GCS.settingsInfo.SelectMany(eventPair =>
                    eventPair.Value.propertiesInfo.Select(propertiesPair => propertiesPair.Value)))
                {
                    switch (propertyInfo.type)
                    {
                        case PropertyType.Color:
                            propertyInfo.color_usesAlpha = true;
                            break;
                        case PropertyType.Int:
                            propertyInfo.int_min = int.MinValue;
                            propertyInfo.int_max = int.MaxValue;
                            break;
                        case PropertyType.Float:
                            propertyInfo.float_min = float.NegativeInfinity;
                            propertyInfo.float_max = float.PositiveInfinity;
                            break;
                        case PropertyType.Vector2:
                            propertyInfo.maxVec = Vector2.positiveInfinity;
                            propertyInfo.minVec = Vector2.negativeInfinity;
                            break;
                    }
                }
            }
            else
            {
                if (!(Json.Deserialize(Resources.Load<TextAsset>("LevelEditorProperties").text) is
                    Dictionary<string, object> dictionary))
                {
                    return;
                }

                var levelEventsInfo = Utils.Decode(dictionary["levelEvents"] as IEnumerable<object>);
                var settingsInfo = Utils.Decode(dictionary["settings"] as IEnumerable<object>);

                foreach (var (key, value) in GCS.levelEventsInfo)
                {
                    var levelEventInfo = levelEventsInfo[key];

                    foreach (var (property, propertyInfo) in value.propertiesInfo)
                    {
                        var originalPropertyInfo = levelEventInfo.propertiesInfo[property];

                        switch (propertyInfo.type)
                        {
                            case PropertyType.Color:
                                propertyInfo.color_usesAlpha = originalPropertyInfo.color_usesAlpha;
                                break;
                            case PropertyType.Int:
                                propertyInfo.int_min = originalPropertyInfo.int_min;
                                propertyInfo.int_max = originalPropertyInfo.int_max;
                                break;
                            case PropertyType.Float:
                                propertyInfo.float_min = originalPropertyInfo.float_min;
                                propertyInfo.float_max = originalPropertyInfo.float_max;
                                break;
                            case PropertyType.Vector2:
                                propertyInfo.maxVec = originalPropertyInfo.maxVec;
                                propertyInfo.minVec = originalPropertyInfo.minVec;
                                break;
                        }
                    }
                }

                foreach (var (key, value) in GCS.settingsInfo)
                {
                    var levelEventInfo = settingsInfo[key];

                    foreach (var (property, propertyInfo) in value.propertiesInfo)
                    {
                        var originalPropertyInfo = levelEventInfo.propertiesInfo[property];

                        switch (propertyInfo.type)
                        {
                            case PropertyType.Color:
                                propertyInfo.color_usesAlpha = originalPropertyInfo.color_usesAlpha;
                                break;
                            case PropertyType.Int:
                                propertyInfo.int_min = originalPropertyInfo.int_min;
                                propertyInfo.int_max = originalPropertyInfo.int_max;
                                break;
                            case PropertyType.Float:
                                propertyInfo.float_min = originalPropertyInfo.float_min;
                                propertyInfo.float_max = originalPropertyInfo.float_max;
                                break;
                            case PropertyType.Vector2:
                                propertyInfo.maxVec = originalPropertyInfo.maxVec;
                                propertyInfo.minVec = originalPropertyInfo.minVec;
                                break;
                        }
                    }
                }
            }
        }
    }
}