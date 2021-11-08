using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SA.GoogleDoc;
using UnityModManagerNet;
using System.Reflection;
using System.Xml.Serialization;
using EditorHelper.Utils;
using UnityEngine;

namespace EditorHelper.Settings {
    public class MainSettings : UnityModManager.ModSettings, IDrawable {
        [XmlIgnore] public Dictionary<string, bool> EnabledTweaks;
        public List<(string tweak, bool enabled)> EnabledTweaksList;

        public override void Save(UnityModManager.ModEntry modEntry) {
            EnabledTweaksList = new List<(string tweak, bool enabled)>();
            EnabledTweaks["asdf"] = true;
            foreach ((var k, bool v) in EnabledTweaks) {
                EnabledTweaksList.Add((k, v));
            }

            Save(this, modEntry);
        }

        public void OnChange() { }
    }
}