using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorHelper.Utils;
using HarmonyLib;

namespace EditorHelper.Core.Patch {
	/// <summary>
	/// Replaces <see cref="HarmonyPatch"/> and prevents mod crashing from having no class specified in the game's code.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate,
		AllowMultiple = false)]
	public class TweakPatchAttribute : HarmonyPatch {
		private static Dictionary<string, bool> _enables = new Dictionary<string, bool>();

		/// <summary>
		/// Id of patch, it should <i>not</i> be identical to other patches' id.
		/// </summary>
		public string PatchId { get; set; }

		/// <summary>
		/// Name of the class to find specific method from.
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// Name of the method in the class to patch.
		/// </summary>
		public string MethodName => info.methodName;

		/// <summary>
		/// Minimum ADOFAI's version of this patch working.
		/// </summary>
		public int MinVersion { get; set; }

		/// <summary>
		/// Maximum ADOFAI's version of this patch working.
		/// </summary>
		public int MaxVersion { get; set; }

		/// <summary>
		/// Assembly to find target method from.
		/// </summary>
		public Assembly Assembly { get; set; }

		/// <summary>
		/// Whether the patch is patched (enabled).
		/// </summary>
		public bool IsEnabled {
			get {
				if (!_enables.ContainsKey(PatchId)) _enables[PatchId] = false;
				return _enables[PatchId];
			}
			set => _enables[PatchId] = value;
		}

#pragma warning disable 1591
		public TweakPatchAttribute(string patchId, string className, string methodName, int minVersion = -1,
			int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);

			info.methodName = methodName;
		}

		public TweakPatchAttribute(string patchId, string className, string methodName, int minVersion = -1,
			int maxVersion = -1, params Type[] argumentTypes) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		public TweakPatchAttribute(string patchId, string className, string methodName, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public TweakPatchAttribute(string patchId, string className, MethodType methodType, int minVersion = -1,
			int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
		}

		public TweakPatchAttribute(string patchId, string className, MethodType methodType, int minVersion = -1,
			int maxVersion = -1, params Type[] argumentTypes) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		public TweakPatchAttribute(string patchId, string className, MethodType methodType, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public TweakPatchAttribute(string patchId, Type assemblyType, string className, string methodName,
			int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
		}

		public TweakPatchAttribute(string patchId, Type assemblyType, string className, string methodName,
			int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		public TweakPatchAttribute(string patchId, Type assemblyType, string className, string methodName,
			Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public TweakPatchAttribute(string patchId, Type assemblyType, string className, MethodType methodType,
			int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
		}

		public TweakPatchAttribute(string patchId, Type assemblyType, string className, MethodType methodType,
			int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		public TweakPatchAttribute(string patchId, Type assemblyType, string className, MethodType methodType,
			Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public TweakPatchAttribute(string patchId, string assemblyName, string className, string methodName,
			int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
		}

		public TweakPatchAttribute(string patchId, string assemblyName, string className, string methodName,
			int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		public TweakPatchAttribute(string patchId, string assemblyName, string className, string methodName,
			Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public TweakPatchAttribute(string patchId, string assemblyName, string className, MethodType methodType,
			int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
		}

		public TweakPatchAttribute(string patchId, string assemblyName, string className, MethodType methodType,
			int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		public TweakPatchAttribute(string patchId, string assemblyName, string className, MethodType methodType,
			Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className) ?? Misc.GetAllTypes(Assembly).FirstOrDefault(t => t.Name == ClassName);
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}
#pragma warning restore 1591
		private void ParseSpecialArguments(Type[] argumentTypes, ArgumentType[] argumentVariations) {
			if (argumentVariations == null || argumentVariations.Length == 0) {
				info.argumentTypes = argumentTypes;
				return;
			}

			if (argumentTypes.Length < argumentVariations.Length) {
				throw new ArgumentException("argumentVariations contains more elements than argumentTypes",
					nameof(argumentVariations));
			}

			List<Type> list = new List<Type>();
			for (int i = 0; i < argumentTypes.Length; i++) {
				Type type = argumentTypes[i];
				switch (argumentVariations[i]) {
					case ArgumentType.Ref:
					case ArgumentType.Out:
						type = type.MakeByRefType();
						break;
					case ArgumentType.Pointer:
						type = type.MakePointerType();
						break;
				}

				list.Add(type);
			}

			info.argumentTypes = list.ToArray();
		}
	}
}