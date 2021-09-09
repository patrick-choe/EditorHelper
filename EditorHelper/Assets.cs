using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper {
    public static class Assets {
        public static AssetBundle bundle { get; private set; }
        public static GameObject ControlTogglePrefab { get; private set; }
        public static string AssetPath => Path.Combine(Directory.GetCurrentDirectory(), "Mods", "EditorHelper", "editorhelper");
        public static void Load() {
            return;
        }
    }
}