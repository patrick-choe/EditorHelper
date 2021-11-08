using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorHelper.Settings;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Tweaks {
    public interface ITweakSetting {
        public void OnGUI();
    }

    public static class TweakSetting {
        // ReSharper disable UseSwitchCasePatternVariable
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public static void Draw(this ITweakSetting setting) {
            if (setting == null) throw new NullReferenceException();
            var type = setting.GetType();
            var fields = type.GetFields().Concat<MemberInfo>(type.GetProperties().Where(i => i.CanRead && i.CanWrite)).Where(i => i.GetCustomAttribute<DrawAttribute>() != null).ToArray();
            var count = 0;
            var total = fields.Length;
            foreach (var info in fields) {
                count++;
                var attr = info.GetCustomAttribute<DrawAttribute>();
                
                var label = info.GetCustomAttributes<LabelAttribute>().Select(a => (a.Language, a.Label)).ToArray();
                object value = info.GetValue(setting)!;
                switch (attr.DrawType) {
                    case DrawType.AUTO:
                        switch (value) {
                            case bool boolValue:
                                GUIEx.Label(240, () => boolValue = GUILayout.Toggle(boolValue, ""), 15, label);
                                if (boolValue != (bool) value) info.SetValue(setting, boolValue);
                                break;

                            case int intValue:
                                attr.MinValue ??= int.MinValue;
                                attr.MaxValue ??= int.MaxValue;
                                GUIEx.Label(250,
                                    () => GUIEx.IntField(ref intValue, Convert.ToInt32(attr.MinValue), Convert.ToInt32(attr.MaxValue), 80),
                                    15, label);
                                if (intValue != (int) value) info.SetValue(setting, intValue);
                                break;

                            case float floatValue:
                                attr.MinValue ??= float.PositiveInfinity;
                                attr.MaxValue ??= float.NegativeInfinity;
                                GUIEx.Label(250,
                                    () => GUIEx.FloatField(ref floatValue, Convert.ToSingle(attr.MinValue), Convert.ToSingle(attr.MaxValue),
                                        80), 15, label);
                                if (floatValue != (float) value) info.SetValue(setting, floatValue);
                                break;

                            case string stringValue:
                                GUIEx.Label(250, () => GUIEx.TextField(ref stringValue, 200), 15, label);
                                if (stringValue != (string) value) info.SetValue(setting, stringValue);
                                break;

                            case KeyMap keyMapValue:
                                GUILayout.Space(1.5f);
                                GUIEx.KeyMap(ref keyMapValue, label);
                                if (keyMapValue != (KeyMap) value) info.SetValue(setting, keyMapValue);
                                GUILayout.Space(-5);
                                break;
                            
                            case Fraction fractionValue:
                                attr.MinValue ??= Fraction.MinValue;
                                attr.MaxValue ??= Fraction.MaxValue;
                                if (attr.MinValue is not Fraction) attr.MinValue = (Fraction) Convert.ToDouble(attr.MinValue);
                                if (attr.MaxValue is not Fraction) attr.MaxValue = (Fraction) Convert.ToDouble(attr.MaxValue);
                                GUIEx.Label(250,
                                    () => GUIEx.FractionField(ref fractionValue, (Fraction) attr.MinValue, (Fraction) attr.MaxValue), 15, label);
                                if (fractionValue != (Fraction) value) info.SetValue(setting, fractionValue);
                                break;
                        }
                        break;
                    case DrawType.INT:
                        value = Convert.ToInt32(value);
                        goto case DrawType.AUTO;
                        
                    case DrawType.FLOAT:
                        value = Convert.ToSingle(value);
                        goto case DrawType.AUTO;
                        
                    case DrawType.STRING:
                        value = Convert.ToString(value);
                        goto case DrawType.AUTO;
                        
                    case DrawType.FRACTION:
                        var f = (Fraction) Convert.ToSingle(value);
                        var fvalue = f;
                        attr.MinValue ??= Fraction.MinValue;
                        attr.MaxValue ??= Fraction.MaxValue;
                        if (attr.MinValue is not Fraction) attr.MinValue = (Fraction) Convert.ToDouble(attr.MinValue);
                        if (attr.MaxValue is not Fraction) attr.MaxValue = (Fraction) Convert.ToDouble(attr.MaxValue);
                        GUIEx.Label(250,
                            () => GUIEx.FractionField(ref fvalue, (Fraction) attr.MinValue, (Fraction) attr.MaxValue), 15, label);
                        if (fvalue != (Fraction) value) info.SetValue(setting, fvalue);
                        break;
                }
            }
            GUILayout.Space(20);
        }

        public static T Setting<T>(this ISettingClass<T> settingClass) where T : ITweakSetting, new() {
            return (T) TweakManager.Setting(settingClass.GetType())!;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class LabelAttribute : Attribute {
        public LangCode Language;
        public string Label;
        public LabelAttribute(LangCode language, string label) {
            Language = language;
            Label = label;
        }
    }

    public enum DrawType {
        AUTO, 
        INT,
        FLOAT,
        STRING,
        FRACTION
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DrawAttribute : Attribute {
        private class DrawInfo {
            public object? MinValue;
            public object? MaxValue;
            public DrawType DrawType;
        }

        private DrawInfo _drawInfo;

        public object? MinValue {
            get => _drawInfo.MinValue;
            set => _drawInfo.MinValue = value;
        }

        public object? MaxValue {
            get => _drawInfo.MaxValue;
            set => _drawInfo.MaxValue = value;
        }

        public DrawType DrawType {
            get => _drawInfo.DrawType;
            set => _drawInfo.DrawType = value;
        }

        public DrawAttribute(double minValue = double.MinValue, double maxValue = double.MaxValue, DrawType drawType = DrawType.AUTO) {
            _drawInfo = new DrawInfo();
            MinValue = minValue;
            MaxValue = maxValue;
            DrawType = drawType;
        }
        
        public DrawAttribute(Fraction minValue, Fraction maxValue, DrawType drawType) {
            _drawInfo = new DrawInfo();
            MinValue = minValue;
            MaxValue = maxValue;
            DrawType = drawType;
        }
    }
}