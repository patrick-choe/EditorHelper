using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorHelper.Core.Alerts;
using EditorHelper.Core.Initializer;
using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using EditorHelper.Utils;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper {
    public static class Main {
        private static Dictionary<Type, bool> IsFolded = new();
        private static int? _releaseNum;
        public static bool Unusable;

        public static int ReleaseNum {
            get {
                if (_releaseNum == null) {
                    object value = AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);
                    _releaseNum = value is int i ? i : 0;
                }

                return _releaseNum.Value;
            }
        }
        public static Harmony Harmony { get; private set; } = null!;
        internal static UnityModManager.ModEntry? _mod;
        internal static MainSettings Settings { get; private set; } = null!;
        
        private const int Exact = 0;
        private const int NotLess = 1;
        private const int NotBigger = 2;
        private const int Bigger = 3;
        private const int Less = 4;
        public static int Target = 78;
        
        public static readonly int[] CompletelyIncompatible = {76, 75, 74, 73, 72, 71, 70, 69, 68};
        
        private static bool Load(UnityModManager.ModEntry modEntry) { 
            var editorHelperDir = modEntry.Path;
            UnityModManager.Logger.Log("Dir: " + editorHelperDir);
            var mode = NotBigger;
            bool unsupported = false;
            if (File.Exists(Path.Combine(editorHelperDir, "Version.txt"))) {
                var value = File.ReadAllText(Path.Combine(editorHelperDir, "Version.txt"));
                if (value.StartsWith(">=")) {
                    mode = NotLess;
                    value = value.Substring(2);
                } else if (value.StartsWith("<=")) {
                    mode = NotBigger;
                    value = value.Substring(2);
                } else if (value.StartsWith(">")) {
                    mode = Bigger;
                    value = value.Substring(2);
                } else if (value.StartsWith("<")) {
                    mode = Less;
                    value = value.Substring(2);
                } else if (value.StartsWith("==")) {
                    mode = Exact;
                    value = value.Substring(2);
                }

                if (int.TryParse(value, out var val)) {
                    Target = val;
                    UnityModManager.Logger.Log($"EditorHelper version set to {value}");
                }
            }

            UnityModManager.Logger.Log($"Current version: {ReleaseNum}");

            switch (mode) {
                case Exact:
                    if (ReleaseNum != Target) unsupported = true;
                    break;
                case NotLess:
                    if (ReleaseNum < Target) unsupported = true;
                    break;
                case NotBigger:
                    if (ReleaseNum > Target) unsupported = true;
                    break;
                case Bigger:
                    if (ReleaseNum <= Target) unsupported = true;
                    break;
                case Less:
                    if (ReleaseNum >= Target) unsupported = true;
                    break;
            }

            Settings = UnityModManager.ModSettings.Load<MainSettings>(modEntry);
            
            Settings.EnabledTweaks = new Dictionary<string, bool>();
            foreach ((string tweak, bool enabled) in Settings.EnabledTweaksList) {
                Settings.EnabledTweaks[tweak] = enabled;
            }

            _mod = modEntry;
            _mod.OnToggle = OnToggle;
            _mod.OnGUI = OnGUI;
            _mod.OnSaveGUI = OnSaveGUI;

            Harmony = new Harmony(modEntry.Info.Id);
            Assets.Load();
            Initalizer.Init();
            TweakManager.RegisterAllTweaks();
            Initalizer.LateInit();
            Enabled = modEntry.Enabled;
            
            if (unsupported) {
                if (CompletelyIncompatible.Contains(ReleaseNum)) {
                    Alert.Show<UnusableVersion>();
                    return true;
                } else {
                    Alert.Show<IncompatibleVersion>();
                }
            }
            
            if (!Unusable) {
                if (!Settings.PatchNote_2_0_beta_3) {
                    Alert.Show<PatchNote>();
                }
            }
            
            return true;
        }

        public static bool Enabled = false;
        
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            if (Unusable && value) {
                Main._mod!.Active = false;
            }
            if (value == Enabled) return true;
            Enabled = value;
            if (value) {
                foreach (var tweak in TweakManager.RegisteredTweaks) {
                    var instance = TweakManager.Instance(tweak);
                    if (instance == null) continue;
                    if (instance.Enabled) instance.OnDisable();
                }
            } else {
                foreach (var tweak in TweakManager.RegisteredTweaks) {
                    var instance = TweakManager.Instance(tweak);
                    if (instance == null) continue;
                    if (instance.Enabled) instance.OnEnable();
                }
            }
            return true;
        }

        private static GUIStyle? _mainText;
        private static GUIStyle? _titleText;

        private static void OnGUI(UnityModManager.ModEntry modEntry) {
            var font = GUI.skin.label.font;
            
            GUI.skin.label.font = Assets.SettingFontRegular;
            GUI.skin.toggle.font = Assets.SettingFontRegular;
            if (_mainText == null) {
                _mainText = new GUIStyle();
                _mainText.font = Assets.SettingFontRegular;
                _mainText.normal.textColor = Color.white;
            }
            
            if (_titleText == null) {
                _titleText = new GUIStyle();
                _titleText.font = Assets.SettingFont;
                _titleText.normal.textColor = Color.white;
            }
            
            foreach (var type in TweakManager.RegisteredTweaks) {
                var hasFold = TweakManager.Setting(type) != null;
                if (!IsFolded.TryGetValue(type, out var isFolded)) {
                    IsFolded[type] = true;
                    isFolded = true;
                }

                var tweak = TweakManager.Instance(type)!;
                GUILayout.BeginHorizontal();
                var enabled = tweak.Enabled;
                if (hasFold && enabled) {
                    isFolded = GUILayout.Toggle(isFolded, isFolded ? "  ▶  " : "  ▼  ", _titleText);
                } else {
                    GUILayout.Label("<color=#00000000>  ▶  </color>");
                }

                var prefix = enabled ? "<size=2> </size><size=20>☑ </size>" : "<size=20>☐ </size>";
                enabled = GUILayout.Toggle(enabled, prefix + $"<size=18>{GUIEx.CheckLangCode(TweakManager.GetDescription(type))}</size><size=30></size>", _titleText);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                tweak.Enabled = enabled;
                if (!isFolded && enabled) {
                    GUIEx.BeginIndent();
                    TweakManager.Setting(type)?.OnGUI();
                    GUIEx.EndIndent();
                }

                IsFolded[type] = isFolded;
            }

            GUI.skin.label.font = font;
            GUI.skin.toggle.font = font;
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            Settings.Save(modEntry);
        }
    }
}

namespace System.Runtime.CompilerServices {
    internal class IsExternalInit { }
}