using System;
using System.ComponentModel;
using EditorHelper.Core.Tweaks;
using HarmonyLib;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.Miscellaneous {
    [TweakDescription(LangCode.English, "Miscellaneous")]
    [TweakDescription(LangCode.Korean, "기타 설정")]
    [RegisterTweak(int.MaxValue)]
    public class MiscellaneousTweak : Tweak, IPatchClass<MiscellaneousPatch>, ISettingClass<MiscellaneousSetting> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }
    }
}