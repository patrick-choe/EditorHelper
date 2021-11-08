using EditorHelper.Core.TweakFunctions;
using EditorHelper.Core.Tweaks;
using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Tweaks.RotateScreen {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Rotate screen")]
    [TweakDescription(LangCode.Korean, "화면 회전")]
    public class RotateScreenTweak : Tweak, IPatchClass<RotateScreenPatch>, ISettingClass<RotateScreenSetting> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }
        
        [TweakFunction("RotateScreenCCW")]
        public void RotateScreenCCW() {
            DebugUtils.Log("asdfasdfsadfsadfsdafsdafsafdsdaddfd");
            scrCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, RotateScreenPatch.CurrentRot - (float) this.Setting().DeltaDeg);
            RotateScreenPatch.UpdateRotationPatch.UpdateDirectionButtonsRot(scnEditor.instance);
        }

        [TweakFunction("RotateScreenCW")]
        public void RotateScreenCW() {
            DebugUtils.Log("asdfasdfsadfsadfsdafsdafsafdsdaddfd");
            scrCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, RotateScreenPatch.CurrentRot + (float) this.Setting().DeltaDeg);
            RotateScreenPatch.UpdateRotationPatch.UpdateDirectionButtonsRot(scnEditor.instance);
        }
    }
}