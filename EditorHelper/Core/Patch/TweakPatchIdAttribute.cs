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
	public class TweakPatchIdAttribute : TweakPatchAttribute {

#pragma warning disable 1591
		public TweakPatchIdAttribute(int minVersion = -1, int maxVersion = -1) : base(null, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string className, int minVersion = -1, int maxVersion = -1) : base(null, className,
			minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string className, Type[] argumentTypes, int minVersion = -1, int maxVersion = -1) :
			base(null, className, argumentTypes, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string className, string methodName, int minVersion = -1, int maxVersion = -1) :
			base(null, className, methodName, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string className, string methodName, int minVersion, int maxVersion,
			params Type[] argumentTypes) : base(null, className, methodName, minVersion, maxVersion, argumentTypes) { }

		public TweakPatchIdAttribute(string className, string methodName, params Type[] argumentTypes) : base(null, className,
			methodName, argumentTypes) { }

		public TweakPatchIdAttribute(string className, string methodName, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) : base(null, className, methodName,
			argumentTypes, argumentVariations, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string className, MethodType methodType, int minVersion = -1, int maxVersion = -1)
			: base(null, className, methodType, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string className, MethodType methodType, int minVersion, int maxVersion,
			params Type[] argumentTypes) : base(null, className, methodType, minVersion, maxVersion, argumentTypes) { }

		public TweakPatchIdAttribute(string className, MethodType methodType, params Type[] argumentTypes) : base(null, 
			className, methodType, argumentTypes) { }

		public TweakPatchIdAttribute(string className, MethodType methodType, Type[] argumentTypes,
			ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1) : base(null, className, methodType,
			argumentTypes, argumentVariations, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string className, string methodName, MethodType methodType, int minVersion = -1,
			int maxVersion = -1) : base(null, className, methodName, methodType, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) : base(null, methodName, argumentTypes, argumentVariations, minVersion,
			maxVersion) { }

		public TweakPatchIdAttribute(MethodType methodType, int minVersion = -1, int maxVersion = -1) : base(null, methodType,
			minVersion, maxVersion) { }

		public TweakPatchIdAttribute(MethodType methodType, int minVersion, int maxVersion, params Type[] argumentTypes)
			: base(null, methodType, minVersion, maxVersion, argumentTypes) { }

		public TweakPatchIdAttribute(MethodType methodType, params Type[] argumentTypes) : base(null, methodType,
			argumentTypes) { }

		public TweakPatchIdAttribute(MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations,
			int minVersion = -1, int maxVersion = -1) : base(null, methodType, argumentTypes, argumentVariations, minVersion,
			maxVersion) { }

		public TweakPatchIdAttribute(Type[] argumentTypes, int minVersion = -1, int maxVersion = -1) : base(null, 
			argumentTypes, minVersion, maxVersion) { }

		public TweakPatchIdAttribute(Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1,
			int maxVersion = -1) : base(null, argumentTypes, argumentVariations, minVersion, maxVersion) { }
#pragma warning restore 1591
	}
}
#nullable restore