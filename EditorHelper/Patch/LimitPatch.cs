using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using GDMiniJSON;
using HarmonyLib;
using UnityEngine;

namespace EditorHelper.Patch
{
    [HarmonyPatch(typeof(RDEditorUtils), "IsValidHexColor", typeof(string), typeof(bool))]
    internal static class IsValidHexColorPatch
    {
        private static bool Prefix(ref bool __result, string s)
        {
            if (!Main.IsEnabled || !Main.Settings.RemoveLimits)
            {
                return true;
            }

            __result = s.IsValidHexColor();

            return false;
        }
    }

    [HarmonyPatch(typeof(PropertyControl_Color), "Validate")]
    internal static class ValidatePatch
    {
        private static bool Prefix(ref string __result, PropertyControl_Color __instance)
        {
            if (!Main.IsEnabled || !Main.Settings.RemoveLimits || __instance.propertyInfo?.type != PropertyType.Color)
            {
                return true;
            }

            var text = __instance.inputField.text;
            __result = text.IsValidHexColor() ? text : (string) __instance.propertyInfo.value_default;
            return false;
        }
    }

    [HarmonyPatch(typeof(PropertyControl_Text), "Setup")]
    internal static class SetupPatch
    {
        private static bool Prefix(PropertyControl_Text __instance, bool addListener)
        {
            if (!Main.IsEnabled || !Main.Settings.RemoveLimits || !addListener)
            {
                return true;
            }

            if (__instance.propertyInfo.name == "artist")
            {
                __instance.inputField.onValueChanged.AddListener(value =>
                {
                    __instance.editor.settingsPanel.ToggleArtistPopup(value, __instance.rectTransform.position.y, __instance);
                    __instance.ToggleOthersEnabled();
                });
            }
            else
            {
                __instance.inputField.onEndEdit.AddListener(value =>
                {
                    __instance.editor.SaveState();
                    ++__instance.editor.changingState;
                    __instance.ValidateInput();
                    var selectedEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
                    var text = __instance.inputField.text;
                    object result = null;
                    switch (__instance.propertyInfo.type)
                    {
                        case PropertyType.Int:
                            result = int.Parse(text);
                            break;
                        case PropertyType.Float:
                            result = float.Parse(text);
                            break;
                        case PropertyType.String:
                            result = text;
                            break;
                        case PropertyType.Tile:
                            if (selectedEvent[__instance.propertyInfo.name] is Tuple<int, TileRelativeTo> tuple)
                            {
                                result = new Tuple<int, TileRelativeTo>(int.Parse(text), tuple.Item2);
                            }
                            break;
                    }
                    selectedEvent[__instance.propertyInfo.name] = result;
                    if (__instance.propertyInfo.name == "angleOffset")
                    {
                        __instance.editor.levelEventsPanel.ShowPanelOfEvent(selectedEvent);
                    }
                    __instance.ToggleOthersEnabled();
                    switch (selectedEvent.eventType)
                    {
                        case LevelEventType.BackgroundSettings:
                            __instance.customLevel.SetBackground();
                            break;
                        case LevelEventType.AddDecoration:
                            __instance.editor.UpdateDecorationSprites();
                            break;
                    }
                    __instance.editor.ApplyEventsToFloors();
                    __instance.editor.ShowEventIndicators(__instance.editor.selectedFloor);
                    --__instance.editor.changingState;
                });
            }

            if (string.IsNullOrEmpty(__instance.propertyInfo.unit))
            {
                return false;
            }
			__instance.unit.gameObject.SetActive(true);
			__instance.unit.text = RDString.Get("editor.unit." + __instance.propertyInfo.unit);
            return false;
        }
    }
    
    [HarmonyPatch(typeof(scnEditor), "Start")]
    internal static class StartPatch
    {
        private static void Prefix()
        {
            if (!Main.IsEnabled || !Main.Settings.RemoveLimits || Main.FirstLoaded)
            {
                return;
            }

            Main.FirstLoaded = true;

            if (Main.Settings.RemoveLimits)
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

                foreach (var eventPair in GCS.levelEventsInfo)
                {
                    var levelEventInfo = levelEventsInfo[eventPair.Key];

                    foreach (var propertiesPair in eventPair.Value.propertiesInfo)
                    {
                        var originalPropertyInfo = levelEventInfo.propertiesInfo[propertiesPair.Key];
                        
                        var propertyInfo = propertiesPair.Value;

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

                foreach (var eventPair in GCS.settingsInfo)
                {
                    var levelEventInfo = settingsInfo[eventPair.Key];

                    foreach (var propertiesPair in eventPair.Value.propertiesInfo)
                    {
                        var originalPropertyInfo = levelEventInfo.propertiesInfo[propertiesPair.Key];
                        
                        var propertyInfo = propertiesPair.Value;

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