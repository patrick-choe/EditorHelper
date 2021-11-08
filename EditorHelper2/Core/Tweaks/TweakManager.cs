using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorHelper.Utils;
using HarmonyLib;
using UnityModManagerNet;

namespace EditorHelper.Core.Tweaks {
    public static class TweakManager {
        private static readonly HashSet<Type> RegisteredTweaks = new();
        private static readonly Dictionary<Type, Tweak> TweakInstances = new();

        public static void RegisterAllTweaks() {
            var allTweaks = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(Misc.GetAllTypes)
                .Where(t => t.IsSubclassOf(typeof(Tweak)));

            foreach (var type in allTweaks) {
                UnityModManager.Logger.Log($"Registering {type.FullName}");
                RegisterTweakInternal(type);
            }
        }

        public static bool RegisterTweak<T>() where T : Tweak, new() {
            return RegisterTweakInternal(typeof(T));
        }

        private static bool RegisterTweakInternal(Type type) {
            if (Registered(type)) return true;
            
            var attr = type.GetCustomAttribute<RegisterTweakAttribute>();
            if (attr == null) {
                UnityModManager.Logger.Log("RegisterTweak attr not found");
                return false;
            }

            if (!attr.IsValidPatch) {
                UnityModManager.Logger.Log("Invalid patch");
                return false;
            }
            var dependency = type.GetCustomAttributes<TweakDependencyAttrubute>();
            foreach (var d in dependency) {
                var dependencies = new Queue<(Type type, TweakDependencyAttrubute attr)>();
                dependencies.Enqueue((type, d));
                while (dependencies.Count > 0) {
                    var currtype = dependencies.Peek().type;
                    var currdep = dependencies.Dequeue().attr;
                    
                    if (currtype == type) {
                        UnityModManager.Logger.Log($"Circular reference detected from {type}");
                        return false;
                    }
                    
                    if (!Registered(currtype) && !RegisterTweakInternal(currtype)) return false;

                    var newdep = currdep.Dependency.GetCustomAttributes<TweakDependencyAttrubute>();
                    foreach (var dep in newdep) {
                        dependencies.Enqueue((currdep.Dependency, dep));
                    }
                }
            }
            
            var instance = (Tweak) Activator.CreateInstance(type);
            instance.Harmony = new Harmony(type.FullName);
            TweakInstances[type] = instance;
            RegisteredTweaks.Add(type);
            var enabled = true;
            instance.OnRegister(enabled);
            UnityModManager.Logger.Log($"Registered tweak {type.FullName}");
            return true;
        }

        public static T Instance<T>() where T : Tweak, new() => (T) Instance(typeof(T));
        public static bool Registered<T>() where T : Tweak, new() => Registered(typeof(T));
        
        public static Tweak Instance(Type type) {
            if (Registered(type)) return TweakInstances[type];
            return !RegisterTweakInternal(type) ? null : TweakInstances[type];
        }
        
        public static bool Registered(Type type) {
            return RegisteredTweaks.Contains(type);
        }
    }
}