using System;
using System.Collections.Generic;
using System.IO;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Patch;
using EditorHelper.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper {
    public static class Assets {
        public static AssetBundle Bundle { get; private set; }
        public static Dictionary<string, Sprite> Eases { get; private set; }
        public static Sprite EditorHelperIcon { get; private set; }
        private static Font _settingFont;

        public static Font SettingFont {
            get {
                if (_settingFont == null) _settingFont = RDC.data.prefab_property.transform.GetChild(1).GetComponent<Text>().font;
                return _settingFont;
            }
        }
        
        private static Font _settingFontRegular;

        public static Font SettingFontRegular {
            get {
                if (_settingFontRegular == null) _settingFontRegular = RDC.data.prefab_controlColor.transform.GetChild(1).GetComponent<Text>().font;
                return _settingFontRegular;
            }
        }
        private static Sprite _buttonImage;

        public static Sprite ButtonImage {
            get {
                if (_buttonImage == null) _buttonImage = RDC.data.prefab_controlExport.transform.GetChild(0).GetComponent<Image>().sprite;
                return _buttonImage;
            }
        }
        private static Sprite _inputImage;

        public static Sprite InputImage {
            get {
                if (_inputImage == null) _inputImage = RDC.data.prefab_controlColor.GetComponent<Image>().sprite;
                return _inputImage;
            }
        }
        
        public static string AssetPath => Path.Combine(Directory.GetCurrentDirectory(), "Mods", "EditorHelper", "editorhelper");
        public static void Load() {
            const int size = 540;
            Bundle = AssetBundle.LoadFromFile(AssetPath);
            Eases = new Dictionary<string, Sprite>();
            foreach (var name in Enum.GetNames(typeof(Ease))) {
                var orig = Bundle.LoadAsset<Sprite>(name)?.texture;
                if (orig == null) continue;
                var imageTexture = Misc.DuplicateTexture(orig);
                new TextureScale().Bilinear(imageTexture, size, size);
                Eases[name] = Sprite.Create(imageTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            }

            EditorHelperIcon = Bundle.LoadAsset<Sprite>("EditorHelperIcon");

            UnityModManager.Logger.Log("Loaded assets!");
        }
    }
}