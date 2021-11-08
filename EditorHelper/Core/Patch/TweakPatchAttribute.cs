using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorHelper.Utils;
using HarmonyLib;

namespace EditorHelper.Core.Patch {
#nullable disable

	/// <summary>
	/// Replaces <see cref="HarmonyPatch"/> and prevents mod crashing from having no class specified in the game's code.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate,
		AllowMultiple = false)]
	public class TweakPatchAttribute : HarmonyPatch {
		private static Dictionary<string, bool> _enables = new Dictionary<string, bool>();
		private class TweakPatchInfo {
			public string PatchId;
		}

		private readonly TweakPatchInfo _tweakinfo = new TweakPatchInfo();
		
		/// <summary>
		/// Id of patch, it should <i>not</i> be identical to other patches' id.
		/// </summary>
		public string PatchId {
			get => _tweakinfo.PatchId ??= GetHashCode().ToString();
			set => _tweakinfo.PatchId = value;
		}

		/// <summary>
		/// Minimum ADOFAI's version of this patch working.
		/// </summary>
		public int MinVersion { get; init; }

		/// <summary>
		/// Maximum ADOFAI's version of this patch working.
		/// </summary>
		public int MaxVersion { get; init; }

		/// <summary>
		/// Assembly to find target method from.
		/// </summary>
		public Assembly Assembly { get; init;  }

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
		public TweakPatchAttribute(string patchId, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, Type[] argumentTypes, int minVersion = -1,
			int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, string methodName, int minVersion = -1,
			int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, string methodName, int minVersion, int maxVersion,
			params Type[] argumentTypes) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, string methodName, params Type[] argumentTypes) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.info.argumentTypes = argumentTypes;
			MinVersion = -1;
			MaxVersion = -1;
		}

		public TweakPatchAttribute(string patchId, string className, string methodName, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, MethodType methodType, int minVersion = -1,
			int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, MethodType methodType, int minVersion,
			int maxVersion, params Type[] argumentTypes) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, MethodType methodType,
			params Type[] argumentTypes) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = -1;
			MaxVersion = -1;
		}

		public TweakPatchAttribute(string patchId, string className, MethodType methodType, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string className, string methodName, MethodType methodType,
			int minVersion = -1,
			int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.info.methodType = new MethodType?(methodType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, string methodName, Type[] argumentTypes,
			ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodName = methodName;
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, MethodType methodType, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, MethodType methodType, int minVersion, int maxVersion,
			params Type[] argumentTypes) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, MethodType methodType, params Type[] argumentTypes) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = -1;
			MaxVersion = -1;
		}

		public TweakPatchAttribute(string patchId, MethodType methodType, Type[] argumentTypes,
			ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, Type[] argumentTypes, int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string patchId, Type[] argumentTypes, ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) {
			PatchId = patchId;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
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
#nullable restore