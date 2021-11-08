using System.Collections.Generic;
using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using HarmonyLib;
using UnityModManagerNet;

namespace EditorHelper {
    public static class Main {
        public static readonly int ReleaseNum;
        private static UnityModManager.ModEntry _mod;
        internal static MainSettings Settings { get; private set; }

        static Main() {
            object value = AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);
            ReleaseNum = value is int i ? i : 0;
        }
        
        private static bool Load(UnityModManager.ModEntry modEntry) {
            Settings = UnityModManager.ModSettings.Load<MainSettings>(modEntry);
            
            Settings.EnabledTweaks = new Dictionary<string, bool>();
            foreach ((string tweak, bool enabled) in Settings.EnabledTweaksList) {
                Settings.EnabledTweaks[tweak] = enabled;
            }

            _mod = modEntry;
            _mod.OnToggle = OnToggle;
            _mod.OnGUI = OnGUI;
            _mod.OnSaveGUI = OnSaveGUI;
            
            TweakManager.RegisterAllTweaks();
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            return true;
        }
        private static void OnGUI(UnityModManager.ModEntry modEntry) { }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            Settings.Save(modEntry);
        }
    }
}