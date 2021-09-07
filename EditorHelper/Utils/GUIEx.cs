using System;
using System.Globalization;
using System.Linq;
using SA.GoogleDoc;
using UnityEngine;

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
            GUILayout.Label(CheckLangCode(labels), GUILayout.Width(width));
            gui();
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

        public static string CheckLangCode((LangCode, string)[] codes) {
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