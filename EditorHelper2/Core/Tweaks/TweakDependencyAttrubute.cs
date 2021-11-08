using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EditorHelper.Core.Tweaks {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TweakDependencyAttrubute : Attribute {
        internal Type Dependency { get; }

        public TweakDependencyAttrubute(Type dependency) {
            Dependency = dependency;
        }
    }
}