using EditorHelper.Core.Patch;
using HarmonyLib;

namespace EditorHelper.Core.Tweaks {
    public abstract class Tweak {
        public virtual void OnRegister(bool enabled) {
            if (enabled) OnEnable();
            else OnDisable();
        }

        public abstract void OnEnable();
        public abstract void OnDisable();

        public void PatchTweak() {
            TweakPatcher.PatchTweak(GetType());
        }
        
        public void UnpatchTweak() {
            TweakPatcher.UnpatchTweak(GetType());
        }

        public bool Enabled {
            get => Main.Settings!.EnabledTweaks[Id];
            set {
                if (value == Main.Settings!.EnabledTweaks[Id]) return;
                if (value) {
                    Main.Settings.EnabledTweaks[Id] = true;
                    OnEnable();
                } else {
                    Main.Settings.EnabledTweaks[Id] = false;
                    OnDisable();
                }
            }
        }

        public string Id => _id ??= TweakManager.GetID(GetType())!;
        private string? _id;

        public Harmony Harmony { get; internal set; } = null!;
    }

    public interface IPatchClass<TPatch> { }
    public interface IPatchClass<TPatch1, TPatch2> { }
    public interface IPatchClass<TPatch1, TPatch2, TPatch3> { }
    public interface IPatchClass<TPatch1, TPatch2, TPatch3, TPatch4> { }
    public interface IPatchClass<TPatch1, TPatch2, TPatch3, TPatch4, TPatch5> { }
    public interface ISettingClass<TSetting> where TSetting : ITweakSetting, new() { }
}