using System;
using SA.GoogleDoc;

namespace EditorHelper.Core.Tweaks {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TweakDescriptionAttribute : Attribute {
        public readonly LangCode Language;
        public readonly string Description;
        public TweakDescriptionAttribute(LangCode language, string description) {
            Language = language;
            Description = description;
        }
    }
}