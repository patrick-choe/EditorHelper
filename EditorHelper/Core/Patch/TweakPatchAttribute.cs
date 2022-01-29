using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace EditorHelper.Core.Patch {
#nullable disable
	/// <summary>
	/// Replaces <see cref="HarmonyPatch"/> and prevents mod crashing from having no class specified in the game's code.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate,
		AllowMultiple = false)]
	public class TweakPatchAttribute : HarmonyPatch {
		private class TweakPatchInfo { }

		private readonly TweakPatchInfo _tweakinfo = new TweakPatchInfo();
		
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
#pragma warning disable 1591
		public TweakPatchAttribute(int minVersion = -1, int maxVersion = -1) {
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, int minVersion = -1, int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, Type[] argumentTypes, int minVersion = -1,
			int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, string methodName, int minVersion = -1,
			int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, string methodName, int minVersion, int maxVersion,
			params Type[] argumentTypes) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, string methodName, params Type[] argumentTypes) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.info.argumentTypes = argumentTypes;
			MinVersion = -1;
			MaxVersion = -1;
		}

		public TweakPatchAttribute(string className, string methodName, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, MethodType methodType, int minVersion = -1,
			int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, MethodType methodType, int minVersion,
			int maxVersion, params Type[] argumentTypes) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, MethodType methodType,
			params Type[] argumentTypes) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = -1;
			MaxVersion = -1;
		}

		public TweakPatchAttribute(string className, MethodType methodType, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodType = new MethodType?(methodType);
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string className, string methodName, MethodType methodType,
			int minVersion = -1,
			int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			var declaringType = Assembly.GetType(className) ??
			                    Assembly.GetTypes().FirstOrDefault(t => t.Name == className);
			this.info.declaringType = declaringType;
			this.info.methodName = methodName;
			this.info.methodType = new MethodType?(methodType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(string methodName, Type[] argumentTypes,
			ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodName = methodName;
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(MethodType methodType, int minVersion = -1, int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(MethodType methodType, int minVersion, int maxVersion,
			params Type[] argumentTypes) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(MethodType methodType, params Type[] argumentTypes) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			this.info.argumentTypes = argumentTypes;
			MinVersion = -1;
			MaxVersion = -1;
		}

		public TweakPatchAttribute(MethodType methodType, Type[] argumentTypes,
			ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.methodType = new MethodType?(methodType);
			this.ParseSpecialArguments(argumentTypes, argumentVariations);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(Type[] argumentTypes, int minVersion = -1, int maxVersion = -1) {
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			this.info.argumentTypes = argumentTypes;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
		}

		public TweakPatchAttribute(Type[] argumentTypes, ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) {
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