using EditorHelper.Core.TweakFunctions;
using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.ChangeAngleByDragging {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Change Angle By Dragging")]
    [TweakDescription(LangCode.Korean,  "드래그해서 각도 변경")]
    public class ChangeAngleByDraggingTweak : Tweak, IPatchClass<ChangeAngleByDraggingPatch>, ISettingClass<ChangeAngleByDraggingSetting> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }
    }
}