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
            get => _enabled;
            set {
                if (value == _enabled) return;
                if (value) {
                    _enabled = true;
                    OnEnable();
                } else {
                    _enabled = false;
                    OnDisable();
                }
            }
        }
        private bool _enabled;

        public Harmony Harmony { get; internal set; }
    }

    public interface IPatchClass<TPatch> { }
    public interface IPatchClass<TPatch1, TPatch2> { }
    public interface IPatchClass<TPatch1, TPatch2, TPatch3> { }
    public interface IPatchClass<TPatch1, TPatch2, TPatch3, TPatch4> { }
    public interface IPatchClass<TPatch1, TPatch2, TPatch3, TPatch4, TPatch5> { }
}