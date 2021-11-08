using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorHelper.Core.TweakFunctions;
using EditorHelper.Settings;
using EditorHelper.Utils;
using HarmonyLib;
using SA.GoogleDoc;
using UnityModManagerNet;

namespace EditorHelper.Core.Tweaks {
    public static class TweakManager {
        private struct TweakData {
            public Tweak TweakInstance;
            public string Id;
            public (LangCode, string)[] Desc;
            public ITweakSetting? SettingInstance;
            public TweakData(Tweak instance, string id, (LangCode, string)[] desc, ITweakSetting? settingInstance) {
                TweakInstance = instance;
                Id = id;
                Desc = desc;
                SettingInstance = settingInstance;
            }
        }
        
        public static List<Type> RegisteredTweaks = new();
        private static readonly Dictionary<Type, TweakData> TweakDatas = new();

        public static void RegisterAllTweaks() {
            var allTweaks = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(Misc.GetAllTypes)
                .Where(t => t.IsSubclassOf(typeof(Tweak)));

            foreach (var type in allTweaks) {
                UnityModManager.Logger.Log($"Registering {type.FullName}");
                RegisterTweakInternal(type, false);
            }

            RegisteredTweaks = RegisteredTweaks
                .OrderBy(t => t.GetCustomAttribute<RegisterTweakAttribute>().Priority)
                .ThenBy(t => t.Assembly == Assembly.GetExecutingAssembly() ? 0 : 1)
                .ThenBy(GetID)
                .ToList();
        }

        public static bool RegisterTweak<T>() where T : Tweak, new() {
            return RegisterTweakInternal(typeof(T));
        }

        private static bool RegisterTweakInternal(Type type, bool resort = true) {
            if (Registered(type)) return true;
            
            var attr = type.GetCustomAttribute<RegisterTweakAttribute>();
            if (attr == null) {
                UnityModManager.Logger.Log("RegisterTweak attr not found");
                return false;
            }

            if (!attr.IsValidTweak) {
                UnityModManager.Logger.Log("Invalid patch");
                return false;
            }
            var dependency = type.GetCustomAttributes<TweakDependencyAttribute>();
            foreach (var d in dependency) {
                var dependencies = new Queue<(Type type, TweakDependencyAttribute attr)>();
                dependencies.Enqueue((type, d));
                while (dependencies.Count > 0) {
                    var currtype = dependencies.Peek().type;
                    var currdep = dependencies.Dequeue().attr;
                    
                    if (currtype == type) {
                        UnityModManager.Logger.Log($"Circular reference detected from {type}");
                        return false;
                    }
                    
                    if (!Registered(currtype) && !RegisterTweakInternal(currtype)) return false;

                    var newdep = currdep.Dependency.GetCustomAttributes<TweakDependencyAttribute>();
                    foreach (var dep in newdep) {
                        dependencies.Enqueue((currdep.Dependency, dep));
                    }
                }
            }

            string? id = attr.Id;
            if (id == null) {
                id = type.Name;
                if (id.EndsWith("Tweak")) id = id.Substring(0, id.Length - 5);
                DebugUtils.Log($"Id: {id}");
            }
            var descriptions = type.GetCustomAttributes<TweakDescriptionAttribute>()
                .Select(a => (a.Language, a.Description)).ToArray();
            if (descriptions.Length == 0) descriptions = new[] {(LangCode.English, id)};

            var settingType = type.GetInterfaces().FirstOrDefault(t => t.GetGenericTypeDefinition() == typeof(ISettingClass<>))?.GenericTypeArguments[0];
            
            var instance = (Tweak) Activator.CreateInstance(type);
            instance.Harmony = new Harmony(id);
            
            var settingInstance = settingType == null ? null : (ITweakSetting) Activator.CreateInstance(settingType);
            if (settingType != null) {
                try {
                    var data = Main.Settings!.TweakSettingsList.FirstOrDefault(tuple => tuple.tweak == id).value;
                    if (data != null) {
                        foreach (var (field, value) in data) {
                            var info = settingType.GetField(field, AccessTools.all);
                            if (info == null) continue;

                            try {
                                if (info.FieldType == typeof(KeyMap) && value?.KeyMap != null) {
                                    info.SetValue(settingInstance, value?.KeyMap);
                                } else if (info.FieldType == typeof(Fraction) && value?.Fraction != null) {
                                    info.SetValue(settingInstance, value?.Fraction);
                                } else if (info.FieldType == typeof(bool) && value?.Boolean != null) {
                                    info.SetValue(settingInstance, value?.Boolean);
                                } else if (info.FieldType == typeof(int) && value?.Int32 != null) {
                                    info.SetValue(settingInstance, value?.Int32);
                                } else if (info.FieldType == typeof(float) && value?.Single != null) {
                                    info.SetValue(settingInstance, value?.Single);
                                } else if (info.FieldType == typeof(string) && value?.String != null) {
                                    info.SetValue(settingInstance, value?.String);
                                }
                            } catch { }
                        }
                    }
                } catch (Exception e) {
                    DebugUtils.Log($"Cannot load setting of {id}");
                    DebugUtils.Log(e);
                }
            }

            TweakDatas[type] = new TweakData(instance, id, descriptions, settingInstance);
            RegisteredTweaks.Add(type);

            var methods = 
                type.GetMethods(AccessTools.all)
                .Where(info =>
                    info.GetParameters().Length == 0 &&
                    info.HasMethodBody() &&
                    !info.IsConstructor &&
                    !info.IsGenericMethod &&
                    !info.IsGenericMethodDefinition
                );

            if (settingType != null) {
                foreach (var method in methods) {
                    var funcAttr = method.GetCustomAttribute<TweakFunctionAttribute>();
                    if (funcAttr == null) continue;
                    DebugUtils.Log($"Registering TweakFunction {method.Name}: {funcAttr.KeyMapName}");
                    var data = new TweakFunctionData(funcAttr.Require, funcAttr.KeyMapName);
                    data.KeyMapFieldInfo = settingType.GetField(data.KeyMapName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (data.KeyMapFieldInfo == null) {
                        DebugUtils.Log("Field not found");
                        continue;
                    }

                    DebugUtils.Log($"Info: {data.KeyMapFieldInfo.Name}");
                    if (data.KeyMapFieldInfo.FieldType != typeof(KeyMap)) {
                        DebugUtils.Log("Field type not correct");
                        data.KeyMapFieldInfo = null;
                        continue;
                    }

                    data.SettingInstance = settingInstance;
                    if (method.IsStatic) {
                        data.Action = () => {
                            if (instance.Enabled) method.Invoke(null, new object[] { });
                        };
                    } else {
                        data.Action = () => {
                            if (instance.Enabled) method.Invoke(instance, new object[] { });
                        };
                    }

                    var keymap = data.KeyMap;
                    if (keymap == null) continue;
                    TweakFunction.Funcs.Add(data);
                    DebugUtils.Log($"Registered TweakFunction {method.Name}");
                }
            } else {
                DebugUtils.Log($"Setting type of {type} not found");
            }

            if (Main.Settings!.EnabledTweaks.TryGetValue(id, out bool enabled)) {
                instance.OnRegister(enabled);
            } else {
                instance.OnRegister(true);
                Main.Settings.EnabledTweaks[id] = true;
            }

            if (resort) {
                RegisteredTweaks = RegisteredTweaks.OrderBy(GetID).ThenBy(t => t.GetCustomAttribute<RegisterTweakAttribute>().Priority).ToList();
            }
            UnityModManager.Logger.Log($"Registered tweak {type.FullName}");
            return true;
        }

        public static T Instance<T>() where T : Tweak, new() => (Instance(typeof(T)) as T)!;
        public static Tweak? Instance(Type type) {
            if (Registered(type)) return TweakDatas[type].TweakInstance;
            return !RegisterTweakInternal(type) ? null : TweakDatas[type].TweakInstance;
        }
        
        public static ITweakSetting? Setting<TTweak>() where TTweak : Tweak, new() => Setting(typeof(TTweak));
        public static TSetting? Setting<TTweak, TSetting>() where TTweak : Tweak, ISettingClass<TSetting>, new() where TSetting : ITweakSetting, new() => (TSetting?) Setting(typeof(TTweak));
        public static ITweakSetting? Setting(Type type) {
            if (Registered(type)) return TweakDatas[type].SettingInstance;
            return !RegisterTweakInternal(type) ? null : TweakDatas[type].SettingInstance;
        }
        
        public static bool Registered<T>() where T : Tweak, new() => Registered(typeof(T));
        public static bool Registered(Type type) {
            return RegisteredTweaks.Contains(type);
        }

        public static string GetID<T>() where T : Tweak, new() => GetID(typeof(T))!;
        public static string? GetID(Type type) {
            return TweakDatas.TryGetValue(type, out var data) ? data.Id : null;
        }
        
        public static (LangCode, string)[]? GetDescription<T>() => GetDescription(typeof(T));
        public static (LangCode, string)[]? GetDescription(Type type) {
            return TweakDatas.TryGetValue(type, out var data) ? data.Desc : null;
        }

        public static Type? GetTweak(string id) => RegisteredTweaks.FirstOrDefault(t => GetID(t) == id);
    }
}