using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using EditorHelper.Components;
using EditorHelper.Utils;
using HarmonyLib;
using MoreEditorOptions.Util;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Patch {
    [HarmonyPatch(typeof(PropertyControl_Toggle), "EnumSetup")]
    public static class EaseIconPatch {
        public static void Postfix(PropertyControl_Toggle __instance, string enumTypeString) {
            var staticScroll = __instance.dropdown.template.gameObject.AddComponent<StaticScrollPosition>();
            staticScroll.id = __instance.enumValList[0];
            var optionData = __instance.dropdown.options.Find(data => data.text == "Linear");
            if (optionData != null) {

                var options = new List<Dropdown.OptionData>();
                foreach (var option in __instance.dropdown.options) {
                    var sprite = Assets.Eases[option.text.Replace(" ", "").Replace("-", "")];
                    options.Add(new Dropdown.OptionData(option.text, sprite));
                }

                __instance.dropdown.ClearOptions();
/*
                var imageobj = new GameObject();
                imageobj.transform.parent = __instance.dropdown.transform;
                var image = imageobj.AddComponent<Image>();
                __instance.dropdown.captionImage = image;*/

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
            
            if (enumTypeString == "TrackStyle") {
                var options = new List<Dropdown.OptionData>();
                foreach (var name in Enum.GetNames(typeof(TrackStyle))) {
                    var option = __instance.dropdown.options[(int) Enum.Parse(typeof(TrackStyle), name)];
                    options.Add(new Dropdown.OptionData(option.text, Assets.Bundle.LoadAsset<Sprite>("Tile" + name)));
                    UnityModManager.Logger.Log("Tile" + name);
                }

                __instance.dropdown.ClearOptions();
/*
                var imageobj = new GameObject();
                imageobj.transform.parent = __instance.dropdown.transform;
                var image = imageobj.AddComponent<Image>();
                __instance.dropdown.captionImage = image;*/

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
            
                        
            if (enumTypeString == "HitSound") {
                var options = new List<Dropdown.OptionData>();
                foreach (var name in Enum.GetNames(typeof(TrackStyle))) {
                    var option = __instance.dropdown.options[(int) Enum.Parse(typeof(TrackStyle), name)];
                    options.Add(new Dropdown.OptionData(option.text, Assets.Bundle.LoadAsset<Sprite>("Tile" + name)));
                    UnityModManager.Logger.Log("Tile" + name);
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
                img.sprite = GCS.levelEventIcons[LevelEventType.SetHitsound];
                recttransform.sizeDelta = new Vector2(25, 25);
                var btn = itemImage.AddComponent<Button>();
                var player = itemImage.AddComponent<HitsoundPlayer>();
                player.dropdown = __instance.dropdown;
                player.button = btn;
                /*
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
                rect4.sizeDelta = new Vector2(rect4.sizeDelta.x, 48);*/
            }
        }
    }
}