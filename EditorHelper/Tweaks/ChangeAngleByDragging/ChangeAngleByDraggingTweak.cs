using EditorHelper.Core.TweakFunctions;
using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.ChangeAngleByDragging {
    [TweakDescription(LangCode.English, "Better Free Angle")]
    [TweakDescription(LangCode.Korean,  "개선된 자유 각도")]
    public class ChangeAngleByDraggingTweak : Tweak, IPatchClass<ChangeAngleByDraggingPatch>, ISettingClass<ChangeAngleByDraggingSetting> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }
    }
}