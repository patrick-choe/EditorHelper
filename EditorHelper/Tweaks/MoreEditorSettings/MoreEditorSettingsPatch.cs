using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using EditorHelper.Core.Components;
using EditorHelper.Core.Patch;
using EditorHelper.Utils;
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
                if (__instance.propertyInfo?.name == "useLegacyFlash") {
                    CustomLevel.instance.levelData.legacyFlash = var == "Enabled";
                    var = CustomLevel.instance.levelData.legacyFlash ? "Enabled" : "Disabled";
                    DebugUtils.Log($"LegacyFlash: {var}");
                }

                if (__instance.propertyInfo?.name == "useLegacyTiles") {
                    var selected = CustomLevel.instance.levelData.isOldLevel ? "Enabled" : "Disabled";
                    if (selected != var) FloorMeshConverter.Convert();
                    var = CustomLevel.instance.levelData.isOldLevel ? "Enabled" : "Disabled";
                    DebugUtils.Log($"LegacyTiles: {var}");
                }
            }
        }

        [TweakPatchId(nameof(scnEditor), "Awake")]
        public static class MoreEditorSettingsInit {
            public static void Prefix() {
                if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyFlash"))
                    GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyFlash");

                if (GCS.settingsInfo["MiscSettings"].propertiesInfo.ContainsKey("useLegacyTiles"))
                    GCS.settingsInfo["MiscSettings"].propertiesInfo.Remove("useLegacyTiles");

                GCS.settingsInfo["MiscSettings"].propertiesInfo.Add("useLegacyFlash",
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "useLegacyFlash"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.settingsInfo["MiscSettings"]));
                GCS.settingsInfo["MiscSettings"].propertiesInfo.Add("useLegacyTiles",
                    new PropertyInfo(new Dictionary<string, object> {
                        {"name", "useLegacyTiles"},
                        {"type", "Enum:Toggle"},
                        {"default", "Disabled"}
                    }, GCS.settingsInfo["MiscSettings"]));
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
    }
}