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
                        {"type", Main.EnumToggle},
                        {"default", "Disabled"}
                    }, GCS.settingsInfo["MiscSettings"]));
                
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Add("EH:useLegacyFlash",
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:useLegacyFlash"},
                        {"type", Main.EnumToggle},
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
                        {"type", Main.EnumToggle},
                        {"default", "Disabled"}
                    }, GCS.levelEventsInfo["AddDecoration"]));

                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Add("EH:lockToCameraRot",  
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:lockToCameraRot"},
                        {"type", Main.EnumToggle},
                        {"default", "Disabled"}
                    }, GCS.levelEventsInfo["AddDecoration"]));
                
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Add("EH:lockToCameraScale",  
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:lockToCameraScale"},
                        {"type", Main.EnumToggle},
                        {"default", "Disabled"}
                    }, GCS.levelEventsInfo["AddDecoration"]));
                
                GCS.levelEventsInfo["AddDecoration"].propertiesInfo.Add("EH:disableIfMinimumVFX",  
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "EH:disableIfMinimumVFX"},
                        {"type", Main.EnumToggle},
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
            private static Dictionary<string, object> _data = null!;
	        public static void Prefix(LevelEvent __instance) {
                _data = new Dictionary<string, object>();
                foreach (var (key, value) in __instance.data) {
                    _data[key] = value;
                }

                foreach (var key in __instance.data.Keys.Where(s => s.StartsWith("EH:")).ToArray()) {
                    __instance.data.Remove(key);
                }
	        }

            public static void Postfix(LevelEvent __instance) {
                __instance.data = _data;
            }
        }

        [TweakPatchId(nameof(LevelData), "Decode")]
        public static class DecodePatch {
            public static void Postfix(LevelData __instance) {
                __instance.miscSettings.data["EH:useLegacyFlash"] = __instance.legacyFlash ? "Enabled" : "Disabled";
                __instance.miscSettings.data["EH:useLegacyTiles"] = __instance.isOldLevel ? "Enabled" : "Disabled";
            }
        }
        
        [TweakPatchId(nameof(LevelEvent), "Decode")]
        public static class DecodePatch2 {
            public static void Postfix(LevelEvent __instance) {
                if (__instance.eventType is not LevelEventType.AddDecoration) return;
                string? compText = (__instance.data.GetValueSafe("components") as string)?.Apply(comp => "{" + comp + "}");
                var components = Json.Deserialize(compText) as Dictionary<string, object>;
                if (components?.GetValueSafe("scrLockToCamera") is Dictionary<string, object> lockToCamera) {
                    __instance.data["EH:lockToCameraPos"] = lockToCamera.GetValueSafe("lockPos") as bool? ?? true ? "Enabled" : "Disabled";
                    __instance.data["EH:lockToCameraRot"] = lockToCamera.GetValueSafe("lockRot") as bool? ?? false ? "Enabled" : "Disabled";
                    __instance.data["EH:lockToCameraScale"] = lockToCamera.GetValueSafe("lockScale") as bool? ?? true ? "Enabled" : "Disabled";
                } else {
                    __instance.data["EH:lockToCameraPos"] = "Disabled";
                    __instance.data["EH:lockToCameraRot"] = "Disabled";
                    __instance.data["EH:lockToCameraScale"] = "Disabled";
                }

                __instance.data["EH:disableIfMinimumVFX"] = components?.ContainsKey("scrDisableIfMinimumVFX") ?? false ? "Enabled" : "Disabled";
                
                __instance.disabled["EH:lockToCameraPos"] = false;
                __instance.disabled["EH:lockToCameraRot"] = false;
                __instance.disabled["EH:lockToCameraScale"] = false;
                __instance.disabled["EH:disableIfMinimumVFX"] = false;
            }
        }
    }
}