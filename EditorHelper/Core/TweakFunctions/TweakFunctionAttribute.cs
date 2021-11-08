using System;
using System.Reflection;
using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;

namespace EditorHelper.Core.TweakFunctions {
    [Flags]
    public enum Require {
        SelectionIsEmpty    = 0b001,
        SelectionIsSingle   = 0b010,
        SelectionIsMultiple = 0b100,
        None = 0b111
    }
    
    public class TweakFunctionData {
        public Require Require;
        internal string KeyMapName;
        internal FieldInfo? KeyMapFieldInfo;
        internal ITweakSetting? SettingInstance;
        public TweakFunctionData(Require require, string keyMapName) {
            Require = require;
            KeyMapName = keyMapName;
        }
        public KeyMap? KeyMap => (KeyMap?) KeyMapFieldInfo?.GetValue(SettingInstance);

        public Action? Action;
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class TweakFunctionAttribute : Attribute {
        public Require Require;
        internal string KeyMapName;

        public TweakFunctionAttribute(string keyMapName, Require require = Require.None) {
            Require = require;
            KeyMapName = keyMapName;
        }
    }
}