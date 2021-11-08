using System;
using System.Reflection;

namespace EditorHelper.Core.Tweaks {
    public class RegisterTweakAttribute : Attribute {
        public int MinVersion { get; set; } = -1;
        public int MaxVersion { get; set; } = -1;
        
        public bool IsValidPatch => (MinVersion <= Main.ReleaseNum || MinVersion == -1) &&
                                    (MaxVersion >= Main.ReleaseNum || MaxVersion == -1);
    }
}