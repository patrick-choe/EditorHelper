using System;

namespace EditorHelper.Core.Tweaks {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TweakDependencyAttribute : Attribute {
        internal Type Dependency { get; }

        public TweakDependencyAttribute(Type dependency) {
            Dependency = dependency;
        }
    }
}