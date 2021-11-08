using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;
using UnityModManagerNet;

namespace EditorHelper.Core.Patch {
    public static class TweakPatcher {
        /// <summary>
        /// Checks whether the patch is valid for current game's version.
        /// </summary>
        /// <param name="patchType">Type of the patching method.</param>
        /// <returns>Patch's current availability in <see cref="bool"/>.</returns>
        public static bool IsValidPatch(Type patchType) {
            var patchAttr = patchType.GetCustomAttribute<TweakPatchAttribute>();
            if (patchAttr == null) {
                return false;
            }

            var classType = patchAttr.Assembly.GetType(patchAttr.ClassName) ?? Misc.GetAllTypes(patchAttr.Assembly).FirstOrDefault(t => t.Name == patchAttr.ClassName);

            if ((patchAttr.MinVersion <= Main.ReleaseNum || patchAttr.MinVersion == -1) &&
                (patchAttr.MaxVersion >= Main.ReleaseNum || patchAttr.MaxVersion == -1) &&
                classType != null) {
                return true;
            }
            
            DebugUtils.Log($"Invalid {patchAttr.MinVersion <= Main.ReleaseNum || patchAttr.MinVersion == -1} && {patchAttr.MaxVersion >= Main.ReleaseNum || patchAttr.MaxVersion == -1} && {classType != null}");

            return false;
        }

        /// <summary>
        /// Patches patch.
        /// <param name="harmony">Harmony class to apply patch.</param>
        /// <param name="patchType">Harmony class to apply patch.</param>
        /// </summary>
        public static void TweakPatch(this HarmonyLib.Harmony harmony, Type patchType) {
            DebugUtils.Log($"Patching patch {patchType.FullName}");
            var metadata = patchType.GetCustomAttribute<TweakPatchAttribute>();
            if (metadata == null) {
                return;
            }
            
            if (metadata.IsEnabled) {
                DebugUtils.Log($"Already patched.");
                return;
            }

            if (!IsValidPatch(patchType)) {
                DebugUtils.Log($"Invalid patch");
                return;
            }

            var declaringType = metadata.Assembly.GetType(metadata.ClassName) ?? Misc.GetAllTypes(metadata.Assembly).FirstOrDefault(t => t.Name == metadata.ClassName);

            if (declaringType == null) {
                DebugUtils.Log($"Cannot find type");
                return;
            }
            DebugUtils.Log($"Type: {declaringType.FullName}");
            DebugUtils.Log($"Method: {metadata.MethodName}");

            try {
                harmony.CreateClassProcessor(patchType).Patch();
            } catch (Exception e) {
                DebugUtils.Log(e);
                return;
            }

            metadata.IsEnabled = true;
            var metadata2 = patchType.GetCustomAttribute<TweakPatchAttribute>();
            DebugUtils.Log($"Patched patch {patchType.FullName}");
        }

        /// <summary>
        /// Unpatches patch.
        /// </summary>
        public static void TweakUnpatch(this HarmonyLib.Harmony harmony, Type patchType) {
            var metadata = patchType.GetCustomAttribute<TweakPatchAttribute>();
            if (metadata == null) {
                return;
            }


            if (!metadata.IsEnabled) {
                return;
            }
            
            var classType = metadata.Assembly.GetType(metadata.ClassName) ?? Misc.GetAllTypes(metadata.Assembly).FirstOrDefault(t => t.Name == metadata.ClassName);

            if (classType == null) {
                return;
            }

            var original = metadata.info.method;
            foreach (var patch in patchType.GetMethods()) {
                harmony.Unpatch(original, patch);
            }

            metadata.IsEnabled = false;
        }

        /// <summary>
        /// Patches patch.
        /// <param name="harmony">Harmony class to apply patch.</param>
        /// <param name="patchId">Harmony class to apply unpatch.</param>
        /// </summary>
        public static void TweakPatch(this HarmonyLib.Harmony harmony, string patchId) {
            var patchType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(Misc.GetAllTypes)
                .FirstOrDefault(t => t.GetCustomAttribute<TweakPatchAttribute>()?.PatchId == patchId);
            if (patchType == null) return;
            harmony.TweakPatch(patchType);
            return;
        }

        /// <summary>
        /// Unpatches patch.
        /// </summary>
        public static void TweakUnpatch(this HarmonyLib.Harmony harmony, string patchId) {
            var patchType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(Misc.GetAllTypes)
                .FirstOrDefault(t => t.GetCustomAttribute<TweakPatchAttribute>().PatchId == patchId);
            if (patchType == null) return;
            harmony.TweakUnpatch(patchType);
            return;
        }

        public static void PatchTweak<T>() where T : Tweak => PatchTweak(typeof(T));
        public static void UnpatchTweak<T>() where T : Tweak => UnpatchTweak(typeof(T));

        public static void PatchTweak(Type tweak) {
            DebugUtils.Log($"Patching tweak {tweak.FullName}");
            if (!tweak.IsSubclassOf(typeof(Tweak))) return;
            
            var interfaces = tweak.GetInterfaces();
            var patchTypes = new HashSet<Type>();

            foreach (var i in interfaces) {
                if (i.GetGenericTypeDefinition() == typeof(IPatchClass<>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                    patchTypes.Add(i.GetGenericArguments()[2]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,,,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                    patchTypes.Add(i.GetGenericArguments()[2]);
                    patchTypes.Add(i.GetGenericArguments()[3]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,,,,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                    patchTypes.Add(i.GetGenericArguments()[2]);
                    patchTypes.Add(i.GetGenericArguments()[3]);
                    patchTypes.Add(i.GetGenericArguments()[4]);
                }
            }

            var harmony = TweakManager.Instance(tweak).Harmony;
            foreach (var patchType in patchTypes.SelectMany(Misc.GetAllTypes)) {
                harmony.TweakPatch(patchType);
            }
            DebugUtils.Log($"Patched tweak {tweak.FullName}");
        }
        
        public static void UnpatchTweak(Type tweak) {
            if (!tweak.IsSubclassOf(typeof(Tweak))) return;
            
            var interfaces = tweak.GetInterfaces();
            var patchTypes = new HashSet<Type>();

            foreach (var i in interfaces) {
                if (i.GetGenericTypeDefinition() == typeof(IPatchClass<>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                    patchTypes.Add(i.GetGenericArguments()[2]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,,,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                    patchTypes.Add(i.GetGenericArguments()[2]);
                    patchTypes.Add(i.GetGenericArguments()[3]);
                } else if (i.GetGenericTypeDefinition() == typeof(IPatchClass<,,,,>)) {
                    patchTypes.Add(i.GetGenericArguments()[0]);
                    patchTypes.Add(i.GetGenericArguments()[1]);
                    patchTypes.Add(i.GetGenericArguments()[2]);
                    patchTypes.Add(i.GetGenericArguments()[3]);
                    patchTypes.Add(i.GetGenericArguments()[4]);
                }
            }

            var harmony = TweakManager.Instance(tweak).Harmony;
            foreach (var patchType in patchTypes.SelectMany(Misc.GetAllTypes)) {
                harmony.TweakUnpatch(patchType);
            }
            UnityModManager.Logger.Log($"Unpatched tweak {tweak.FullName}");
        }
        
        /*

        public static void PatchCategory<T>(this HarmonyLib.Harmony harmony) where T : Category {
            PatchCategory(harmony, typeof(T));
        }

        public static void UnpatchCategory<T>(this HarmonyLib.Harmony harmony) where T : Category {
            UnpatchCategory(harmony, typeof(T));
        }

        public static void PatchCategory(this HarmonyLib.Harmony harmony, Type type) {
            var patchAttr = type.GetCustomAttribute<CategoryAttribute>();
            if (!patchAttr.isValid) {
                return;
            }

            var patchClass = patchAttr.PatchClass;
            if (patchClass == null) {
                return;
            }

            var patches = patchClass.GetNestedTypes(AccessTools.all)
                .Where(t => t.GetCustomAttribute<ConditionalPatchAttribute>() != null);
            foreach (var p in patches) {
                harmony.SafePatch(p);
            }

        }

        public static void UnpatchCategory(this HarmonyLib.Harmony harmony, Type type) {
            var patchAttr = type.GetCustomAttribute<CategoryAttribute>();
            if (!patchAttr.isValid) {
                return;
            }

            var patchClass = patchAttr.PatchClass;
            if (patchClass == null) {
                return;
            }

            var patches = patchClass.GetNestedTypes(AccessTools.all)
                .Where(t => t.GetCustomAttribute<ConditionalPatchAttribute>() != null);
            patches.Do(harmony.SafeUnpatch);
        }
        */
    }
}