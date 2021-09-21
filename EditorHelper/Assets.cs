using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using EditorHelper.Utils;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper {
    public static class Assets {
        public static AssetBundle Bundle { get; private set; }
        public static Dictionary<string, Sprite> Eases;
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
            
            UnityModManager.Logger.Log("Loaded assets!");
        }
    }
}