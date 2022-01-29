using System;
using System.Collections.Generic;
using System.IO;
using UnityModManagerNet;
using System.Xml.Serialization;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;

namespace EditorHelper.Settings {
    public class MainSettings : UnityModManager.ModSettings {
        [XmlIgnore] public Dictionary<string, bool> EnabledTweaks = new();
        public List<(string tweak, bool enabled)> EnabledTweaksList = new();
        
        public List<(string tweak, (string, XmlObject?)[] value)> TweakSettingsList = new();

        public bool PatchNote_2_1_alpha_1 = false;
        public string EventBundles = string.Empty;

        public override void Save(UnityModManager.ModEntry modEntry) {
            EnabledTweaksList = new List<(string, bool)>();
            TweakSettingsList = new List<(string, (string, XmlObject?)[])>();
            foreach ((var k, bool v) in EnabledTweaks) {
                EnabledTweaksList.Add((k, v));
                var type = TweakManager.GetTweak(k);
                if (type == null) continue;
                var setting = TweakManager.Setting(type);
                if (setting == null) continue;
                TweakSettingsList.Add((k, setting.ToXmlSerialziable()));
            }

            string path = this.GetPath(modEntry);
            try {
                using StreamWriter streamWriter = new StreamWriter(path);
                new XmlSerializer(typeof(MainSettings), default(XmlAttributeOverrides)).Serialize(streamWriter, this);
            } catch (Exception e) {
                modEntry.Logger.Error("Can't save " + path + ".");
                modEntry.Logger.LogException(e);
            }
        }

        public void OnChange() { }
    }
}