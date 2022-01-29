using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.RemoveEditorLimits {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Remove Editor Limits")]
    [TweakDescription(LangCode.Korean,  "에디터 제한 해제")]
    public class RemoveEditorLimitsTweak : Tweak, IPatchClass<RemoveEditorLimitsPatch>, ISettingClass<RemoveEditorLimitsSetting> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
            ADOStartup.SetupLevelEventsInfo();
        }
    }
}