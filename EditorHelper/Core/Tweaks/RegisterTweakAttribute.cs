using System;

namespace EditorHelper.Core.Tweaks {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterTweakAttribute : Attribute {
        public int MinVersion { get; init; } = -1;
        public int MaxVersion { get; init; } = -1;
        public int Priority { get; init; } = 0;
        public string? Id;

        public bool IsValidTweak => (MinVersion <= Main.ReleaseNum || MinVersion == -1) &&
                                    (MaxVersion >= Main.ReleaseNum || MaxVersion == -1);

        public RegisterTweakAttribute(string id, int priority = 0) {
            Id = id;
            Priority = priority;
        }

        public RegisterTweakAttribute(int priority = 0) : this(null!, priority) { }
    }
}