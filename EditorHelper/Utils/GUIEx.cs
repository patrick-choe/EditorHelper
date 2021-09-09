using System;
using System.Globalization;
using System.Linq;
using EditorHelper.Settings;
using HarmonyLib;
using SA.GoogleDoc;
using UnityEngine;
using UnityModManagerNet;

namespace EditorHelper.Utils {
    public class GUIEx {
        public static void BeginIndent(int indent = 30) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            GUILayout.BeginVertical();
        }
        
        public static void EndIndent() {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        
        public static void Label(params (LangCode, string)[] labels) {
            GUILayout.Label(CheckLangCode(labels));
        }
        
        public static void Label(int width, Action gui, params (LangCode, string)[] labels) {
            GUILayout.BeginHorizontal();
            gui?.Invoke();
            GUILayout.Label(CheckLangCode(labels), GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }
        
        public static void Toggle(ref bool value, params (LangCode, string)[] labels) {
            value = GUILayout.Toggle(value, labels == null ? "" : " " + CheckLangCode(labels));
            GUILayout.Space(5);
        }
        public static void IntField(ref int value, params (LangCode, string)[] labels) {
            var toCheck = value.ToString();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            toCheck = GUILayout.TextField(toCheck, GUILayout.Width(25));
            if (labels != null) GUILayout.Label(" " + CheckLangCode(labels));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (toCheck == "-") toCheck = "0";
            if (toCheck == "") toCheck = "0";
            if (int.TryParse(toCheck, out int val)) {
                value = val;
            }
        }
        
        public static void IntField(ref int value, int min = int.MinValue, int max = int.MaxValue, int width = 30) {
            var toCheck = value.ToString();
            toCheck = GUILayout.TextField(toCheck, GUILayout.Width(width));
            if (toCheck == "-") toCheck = "0";
            if (toCheck == "") toCheck = "0";
            if (int.TryParse(toCheck, out int val)) {
                if (val < min || val > max) return;
                value = val;
            }
        }
        
        public static void FloatField(ref float value, float min = float.NegativeInfinity, float max = float.PositiveInfinity, int width = 30) {
            var toCheck = value.ToString(CultureInfo.InvariantCulture);
            toCheck = GUILayout.TextField(toCheck, GUILayout.Width(width));
            if (toCheck == "-") toCheck = "0";
            if (toCheck == "") toCheck = "0";
            if (toCheck.EndsWith(".")) toCheck = toCheck + "0";
            if (int.TryParse(toCheck, out int val)) {
                if (val < min || val > max) return;
                value = val;
            }
        }
        
        public static void TextField(ref string value, int width = 100) {
            value = GUILayout.TextField(value, GUILayout.Width(width));
        }

        internal static KeyMap currentKeymap;
        public static void KeyMap(ref KeyMap keyMap, params (LangCode, string)[] labels) {
            GUILayout.Label(CheckLangCode(labels));
            GUILayout.BeginHorizontal();
            keyMap.NeedsCtrl = GUILayout.Toggle(keyMap.NeedsCtrl, "Ctrl", GUILayout.Width(60));
            keyMap.NeedsShift = GUILayout.Toggle(keyMap.NeedsShift, "Shift", GUILayout.Width(60));
            keyMap.NeedsAlt = GUILayout.Toggle(keyMap.NeedsAlt, "Alt", GUILayout.Width(60));
            var keycode = keyMap.GetKeycodeValue();
            if (keyMap.KeyCode != KeyCode.None) {
                GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
                GUI.skin.textField.richText = true;
                GUILayout.TextField(keycode, GUI.skin.textField, GUILayout.Width(Math.Max(keycode.RemoveRichTags().Length * 12, 30)), GUILayout.Height(30));
                GUI.skin.textField.alignment = TextAnchor.UpperLeft;
                GUI.skin.textField.richText = false;
                
                GUILayout.Space(10);
                if (currentKeymap == keyMap) {
                    if (Event.current.isKey && Event.current.type == EventType.KeyDown) {
                        var pressed = Event.current.keyCode;
                        if (pressed != KeyCode.None) {
                            keyMap.KeyCode = pressed;
                            if (pressed == KeyCode.Escape) 
                                currentKeymap = null;
                        }
                    }

                    if (GUILayout.Button("End Edit", GUILayout.Width(80))) {
                        currentKeymap = null;
                    }
                } else {
                    if (GUILayout.Button("Edit", GUILayout.Width(80))) {
                        currentKeymap = keyMap;
                    }
                }
                
            }

            GUILayout.Space(10);
            var reset = GUILayout.Button(keyMap == keyMap.inital ? "" : "Reset", keyMap == keyMap.inital ? GUIStyle.none : GUI.skin.button, GUILayout.Width(60), GUILayout.Height(16));
            if (reset && keyMap != keyMap.inital) {
                keyMap.Reset();
                currentKeymap = null;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        public static string CheckLangCode(params (LangCode, string)[] codes) {
            var found = codes.Where(c => c.Item1 == Localization.CurrentLanguage).ToArray();
            if (found.Any()) {
                return found[0].Item2;
            }
            found = codes.Where(c => c.Item1 == LangCode.English).ToArray();
            if (found.Any()) {
                return found[0].Item2;
            }

            return "TRANSLATION_FAILED";
        }
    }
}