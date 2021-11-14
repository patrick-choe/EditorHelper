using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ADOFAI;
using EditorHelper.Core.Components;
using EditorHelper.Core.Patch;
using EditorHelper.Utils;
using GDMiniJSON;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EditorHelper.Tweaks.MoreEditorSettings {
    public abstract class MoreEditorSettingsPatch {
        [TweakPatchId(nameof(PropertyControl_Export), "Awake")]
        public static class PropertyExportPatch {
            public static void Postfix(PropertyControl_Export __instance) {
                __instance.gameObject.AddComponent<FloorMeshConverter>().Control = __instance;
            }
        }

        [TweakPatchId(nameof(PropertyControl_Toggle), "SelectVar")]
        public static class PropertySetPatch {
            public static void Prefix(PropertyControl_Toggle __instance, ref string var) {
                var selectedEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
                
                if (__instance.propertyInfo?.name == "EH:useLegacyFlash") {
                    CustomLevel.instance.levelData.legacyFlash = var == "Enabled";
                    var = CustomLevel.instance.levelData.legacyFlash ? "Enabled" : "Disabled";
                    DebugUtils.Log($"LegacyFlash: {var}");
                }

                if (__instance.propertyInfo?.name == "EH:useLegacyTiles") {
                    var selected = CustomLevel.instance.levelData.isOldLevel ? "Enabled" : "Disabled";
                    if (selected != var) FloorMeshConverter.Convert();
                    var = CustomLevel.instance.levelData.isOldLevel ? "Enabled" : "Disabled";
                    DebugUtils.Log($"LegacyTiles: {var}");
                }

                if (__instance.propertyInfo?.name == "EH:lockToCameraPos") {
                    if (!selectedEvent.data.TryGetValue("components", out var obj)) {
                        obj = "scrLockToCamera: {}";
                    }
                    var dictComponentToList = Json.Deserialize("{" + obj + "}") as Dictionary<string, object>;
                    dictComponentToList ??= new Dictionary<string, object>();
                    dictComponentToList.TryGetValue("scrLockToCamera", out var cam);
                    if (cam is not Dictionary<string, object> lockToCam) {
                        lockToCam = new Dictionary<string, object>();
                    }
                    lockToCam["lockPos"] = var == "Enabled";
                    dictComponentToList["scrLockToCamera"] = lockToCam;
                    var str = Json.Serialize(dictComponentToList);
                    selectedEvent.data["components"] = str.Substring(1, str.Length - 2);
                }

                if (__instance.propertyInfo?.name == "EH:lockToCameraRot") {
                    if (!selectedEvent.data.TryGetValue("components", out var obj)) {
                        obj = "scrLockToCamera: {}";
                    }
                    var dictComponentToList = Json.Deserialize("{" + obj + "}") as Dictionary<string, object>;
                    dictComponentToList ??= new Dictionary<string, object>();
                    dictComponentToList.TryGetValue("scrLockToCamera", out var cam);
                    if (cam is not Dictionary<string, object> lockToCam) {
                        lockToCam = new Dictionary<string, object>();
                    }
                    lockToCam["lockRot"] = var == "Enabled";
                    dictComponentToList["scrLockToCamera"] = lockToCam;
                    var str = Json.Serialize(dictComponentToList);
                    selectedEvent.data["components"] = str.Substring(1, str.Length - 2);
                }

                if (__instance.propertyInfo?.name == "EH:lockToCameraScale") {
                    if (!selectedEvent.data.TryGetValue("components", out var obj)) {
                        obj = "scrLockToCamera: {}";
                    }
                    var dictComponentToList = Json.Deserialize("{" + obj + "}") as Dictionary<string, object>;
                    dictComponentToList ??= new Dictionary<string, object>();
                    dictComponentToList.TryGetValue("scrLockToCamera", out var cam);
                    if (cam is not Dictionary<string, object> lockToCam) {
                        lockToCam = new Dictionary<string, object>();
                    }
                    lockToCam["lockScale"] = var == "Enabled";
                    dictComponentToList["scrLockToCamera"] = lockToCam;
                    var str = Json.Serialize(dictComponentToList);
                    selectedEvent.data["components"] = str.Substring(1, str.Length - 2);
                }

                if (__instance.propertyInfo?.name == "EH:disableIfMinimumVFX") {
                    if (!selectedEvent.data.TryGetValue("components", out var obj)) {
                        obj = "";
                    }
                    var dictComponentToList = Json.Deserialize("{" + obj + "}") as Dictionary<string, object>;
                    dictComponentToList ??= new Dictionary<string, object>();
                    dictComponentToList["scrDisableIfMinimumVFX"] = new Dictionary<string, object>();
                    if (var != "Enabled") dictComponentToList.Remove("scrDisableIfMinimumVFX");
                    var str = Json.Serialize(dictComponentToList);
                    selectedEvent.data["components"] = str.Substring(1, str.Length - 2);
                }
            }
        }

        [TweakPatchId(nameof(scnEditor), "Awake")]
        public static class MoreEditorSettingsInit {
            public static void Prefix() {
                if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("EH:useLegacyFlash"))
                    GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("EH:useLegacyFlash");

                if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("EH:useLegacyTiles"))
                    GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("EH:useLegacyTiles");
                
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Add("EH:useLegacyTiles",
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:useLegacyTiles"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.settingsInfo["MiscSettings"]));
                
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Add("EH:useLegacyFlash",
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:useLegacyFlash"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.settingsInfo["MiscSettings"]));

                if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:lockToCameraPos"))
                    GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:lockToCameraPos");
                
                if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:lockToCameraRot"))
                    GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:lockToCameraRot");
                
                if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:lockToCameraScale"))
                    GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:lockToCameraScale");
                
                if (GCS.levelEventsInfo["AddDecoration"].propertiesInfo.ContainsKey("EH:disableIfMinimumVFX"))
                    GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Remove("EH:disableIfMinimumVFX");
                
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Add("EH:lockToCameraPos",  
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:lockToCameraPos"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.levelEventsInfo["AddDecoration"]));

                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Add("EH:lockToCameraRot",  
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:lockToCameraRot"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.levelEventsInfo["AddDecoration"]));
                
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Add("EH:lockToCameraScale",  
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:lockToCameraScale"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.levelEventsInfo["AddDecoration"]));
                
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Add("EH:disableIfMinimumVFX",  
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:disableIfMinimumVFX"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.levelEventsInfo["AddDecoration"]));
            }
        }


        [TweakPatchId(nameof(PropertyControl_Toggle), "EnumSetup")]
        public static class EaseIconPatch {
            public static void Postfix(PropertyControl_Toggle __instance, string enumTypeString) {
                var optionData = __instance.dropdown.options.Find(data => data.text == "Linear");

                if (optionData != null) {
                    var options = new List<Dropdown.OptionData>();
                    foreach (var option in __instance.dropdown.options) {
                        var sprite = Assets.Eases[option.text.Replace(" ", "").Replace("-", "")];
                        options.Add(new Dropdown.OptionData(option.text, sprite));
                    }

                    __instance.dropdown.ClearOptions();

                    var itemTemplate = __instance.dropdown.transform
                        .Find("Template")
                        .Find("Viewport")
                        .Find("Content")
                        .Find("Item");
                    var itemImage = new GameObject();
                    itemImage.transform.parent = itemTemplate;
                    itemImage.transform.position = itemTemplate.transform.position;
                    var recttrans = itemImage.GetOrAddComponent<RectTransform>();
                    recttrans.sizeDelta = new Vector2(45, 45);
                    recttrans.anchorMin = new Vector2(0, 0.5f);
                    recttrans.anchorMax = new Vector2(0, 0.5f);
                    recttrans.pivot = new Vector2(0, 0.5f);
                    recttrans.anchoredPosition = new Vector2(30, 0);
                    var rect2 = itemTemplate.GetComponent<RectTransform>();
                    rect2.sizeDelta = new Vector2(rect2.sizeDelta.x, 48);
                    var rect3 = __instance.dropdown.itemText.GetComponent<RectTransform>();
                    rect3.anchoredPosition = new Vector2(75, 0);
                    var rect4 = itemTemplate.parent.GetComponent<RectTransform>();
                    rect4.sizeDelta = new Vector2(rect4.sizeDelta.x, 48);

                    __instance.dropdown.itemText.fontSize = 18;
                    __instance.dropdown.itemImage = itemImage.AddComponent<Image>();
                    __instance.dropdown.AddOptions(options.ToList());
                }

/*
                if (enumTypeString == "TrackStyle") {
                    var options = new List<Dropdown.OptionData>();
                    foreach (var name in Enum.GetNames(typeof(TrackStyle))) {
                        var option = __instance.dropdown.options[(int) Enum.Parse(typeof(TrackStyle), name)];
                        options.Add(
                            new Dropdown.OptionData(option.text, Assets.Bundle.LoadAsset<Sprite>("Tile" + name)));
                    }

                    __instance.dropdown.ClearOptions();

                    var itemTemplate = __instance.dropdown.transform
                        .Find("Template")
                        .Find("Viewport")
                        .Find("Content")
                        .Find("Item");
                    var itemImage = new GameObject();
                    itemImage.transform.parent = itemTemplate;
                    itemImage.transform.position = itemTemplate.transform.position;
                    var recttrans = itemImage.GetOrAddComponent<RectTransform>();
                    recttrans.sizeDelta = new Vector2(114, 30);
                    recttrans.anchorMin = new Vector2(0, 0.5f);
                    recttrans.anchorMax = new Vector2(0, 0.5f);
                    recttrans.pivot = new Vector2(0, 0.5f);
                    recttrans.anchoredPosition = new Vector2(30, 0);
                    var rect2 = itemTemplate.GetComponent<RectTransform>();
                    rect2.sizeDelta = new Vector2(rect2.sizeDelta.x, 48);
                    var rect3 = __instance.dropdown.itemText.GetComponent<RectTransform>();
                    rect3.anchoredPosition = new Vector2(143, 0);
                    var rect4 = itemTemplate.parent.GetComponent<RectTransform>();
                    rect4.sizeDelta = new Vector2(rect4.sizeDelta.x, 48);

                    __instance.dropdown.itemText.fontSize = 18;
                    __instance.dropdown.itemImage = itemImage.AddComponent<Image>();
                    __instance.dropdown.AddOptions(options.ToList());
                }
*/

                if (enumTypeString == "HitSound") {
                    var options = new List<Dropdown.OptionData>();

                    foreach (var name in Enum.GetNames(typeof(TrackStyle))) {
                        var option = __instance.dropdown.options[(int) Enum.Parse(typeof(TrackStyle), name)];
                        options.Add(
                            new Dropdown.OptionData(option.text, Assets.Bundle.LoadAsset<Sprite>("Tile" + name)));
                    }


                    var itemTemplate = __instance.dropdown.transform
                        .Find("Template")
                        .Find("Viewport")
                        .Find("Content")
                        .Find("Item");
                    var itemImage = new GameObject();
                    itemImage.transform.parent = itemTemplate;
                    itemImage.transform.position = itemTemplate.transform.position;

                    var recttransform = itemImage.GetOrAddComponent<RectTransform>();
                    recttransform.anchoredPosition = new Vector2(-30, 0);
                    recttransform.anchorMax = new Vector2(1, 0.5f);
                    recttransform.anchorMin = new Vector2(1, 0.5f);
                    recttransform.pivot = new Vector2(1, 0.5f);

                    var img = itemImage.AddComponent<Image>();
                    img.sprite = GCS.levelEventIcons[LevelEventType.PlaySound];
                    recttransform.sizeDelta = new Vector2(25, 25);

                    var btn = itemImage.AddComponent<Button>();

                    var player = itemImage.AddComponent<HitsoundPlayer>();
                    player.dropdown = __instance.dropdown;
                    player.button = btn;
                }
            }
        }

        [TweakPatchId(nameof(LevelEvent), "Encode")]
        public static class EncodePatch {
	        public static bool Prefix(LevelEvent __instance, bool settings, out string __result) {
		        var eventEncode = new StringBuilder();
		        int count = __instance.data.Keys.Count;
		        if (!settings) {
			        if (__instance.floor != -1) {
				        eventEncode.Append(RDEditorUtils.EncodeInt("floor", __instance.floor, false));
			        }

			        eventEncode.Append(RDEditorUtils.EncodeString("eventType", __instance.eventType.ToString(), count == 0));
			        if (!__instance.enabled) {
				        eventEncode.Append(RDEditorUtils.EncodeBool("enabled", __instance.enabled, count == 0));
			        }
		        }

		        int num = 0;
                foreach (string key in __instance.data.Keys.Where(s => !s.StartsWith("EH:"))) {
                    object obj = __instance.data[key];
                    bool lastValue = num == count - 1;
                    if (__instance.info.propertiesInfo.ContainsKey(key)) {
                        PropertyInfo propertyInfo = __instance.info.propertiesInfo[key];
                        if (propertyInfo.encode) {
                            PropertyType type = propertyInfo.type;
                            switch (type) {
                                case PropertyType.Bool:
                                    eventEncode.Append(RDEditorUtils.EncodeBool(key, (bool) __instance.data[key], lastValue));
                                    break;
                                    
                                case PropertyType.Int:
                                case PropertyType.Rating:
                                    eventEncode.Append(RDEditorUtils.EncodeInt(key, (int) __instance.data[key], lastValue));
                                    break;
                                    
                                case PropertyType.Float:
                                    eventEncode.Append(RDEditorUtils.EncodeFloat(key, Convert.ToSingle(__instance.data[key]), lastValue));
                                    break;
                                    
                                case PropertyType.String:
                                case PropertyType.LongString:
                                case PropertyType.File:
                                    eventEncode.Append(RDEditorUtils.EncodeString(key, LevelEvent.EscapeTextForJSON((string) __instance.data[key]), lastValue));
                                    break;
                                    
                                case PropertyType.Color:
                                    eventEncode.Append(RDEditorUtils.EncodeString(key, (string) __instance.data[key], lastValue));
                                    break;
                                    
                                case PropertyType.Enum:
                                    eventEncode.Append(RDEditorUtils.EncodeString(key, __instance.data[key].ToString(), lastValue));
                                    break;
                                    
                                case PropertyType.Vector2:
                                    eventEncode.Append(RDEditorUtils.EncodeVector2(key, (Vector2) __instance.data[key], lastValue));
                                    break;
                                    
                                case PropertyType.Tile:
                                    eventEncode.Append(RDEditorUtils.EncodeTile(key, __instance.data[key] as Tuple<int, TileRelativeTo>, lastValue));
                                    break;
                                    
                                case PropertyType.Array:
                                    eventEncode.Append(RDEditorUtils.EncodeModsArray(key, (object[]) __instance.data[key], lastValue));
                                    break;
                                    
                                default:
                                    Debug.LogWarning($"{key} not parsed! it is type: {type}");
                                    break;
                            }
                            
                            if (settings) eventEncode.Append("\n");
                            ++num;
                        }
                    }
                }


                if (settings) {
			        eventEncode.Length -= 2;
		        }
		        __instance.set("eventEncode", eventEncode);

		        __result = eventEncode.ToString();
		        return false;
	        }
        }

        [TweakPatchId(nameof(LevelData), "Decode")]
        public static class DecodePatch {
            public static void Postfix(LevelData __instance) {
                DebugUtils.Log("asdfasdfasdfsasdfdassdfaasd");
                __instance.miscSettings.data["EH:useLegacyFlash"] = __instance.legacyFlash ? "Enabled" : "Disabled";
                __instance.miscSettings.data["EH:useLegacyTiles"] = __instance.isOldLevel ? "Enabled" : "Disabled";
                DebugUtils.Log("asdfasdfasdfsasdfdassdfaasd2");
            }
        }
        
        [TweakPatchId(nameof(LevelEvent), "Decode")]
        public static class DecodePatch2 {
            public static void Postfix(LevelEvent __instance) {
                var components = (__instance.data.GetValueSafe("components") as Dictionary<string, object>);
                var lockToCamera = components?["scrLockToCamera"] as Dictionary<string, object>;
                __instance.data["EH:lockToCameraPos"] = lockToCamera?.GetValueSafe("lockPos") as bool? ?? true ? "Enabled" : "Disabled";
                __instance.data["EH:lockToCameraRot"] = lockToCamera?.GetValueSafe("lockRot") as bool? ?? false ? "Enabled" : "Disabled";
                __instance.data["EH:lockToCameraScale"] = lockToCamera?.GetValueSafe("lockScale") as bool? ?? true ? "Enabled" : "Disabled";
                __instance.data["EH:disableIfMinimumVFX"] = components?.ContainsKey("scrDisableIfMinimumVFX") ?? false ? "Enabled" : "Disabled";
            }
        }
    }
}