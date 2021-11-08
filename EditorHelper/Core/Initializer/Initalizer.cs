using System;
using System.Linq;
using System.Reflection;
using EditorHelper.Utils;
using HarmonyLib;
using AccessTools = HarmonyLib.AccessTools;

namespace EditorHelper.Core.Initializer {
    public static class Initalizer {
        public static void Init() {

            var methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany(Misc.GetAllTypes)
                .SelectMany(type => type.GetMethods(AccessTools.all))
                .Where(info =>
                    info.IsStatic &&
                    info.GetParameters().Length == 0 &&
                    info.HasMethodBody() &&
                    !info.IsConstructor &&
                    !info.IsGenericMethod &&
                    !info.IsGenericMethodDefinition
                );

            var initalizers = new Action(() => { });
            foreach (var methodInfo in methods) {
                if (methodInfo.GetCustomAttribute<InitAttribute>() != null) {
                    initalizers += () => { methodInfo.Invoke(null, new object[] { }); };
                }
            }

            initalizers();
        }
        
        public static void LateInit() {

            var methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany(Misc.GetAllTypes)
                .SelectMany(type => type.GetMethods(AccessTools.all))
                .Where(info =>
                    info.IsStatic &&
                    info.GetParameters().Length == 0 &&
                    info.HasMethodBody() &&
                    !info.IsConstructor &&
                    !info.IsGenericMethod &&
                    !info.IsGenericMethodDefinition
                );

            var initalizers = new Action(() => { });
            foreach (var methodInfo in methods) {
                if (methodInfo.GetCustomAttribute<LateInitAttribute>() != null) {
                    initalizers += () => { methodInfo.Invoke(null, new object[] { }); };
                }
            }

            initalizers();
        }
    }
}