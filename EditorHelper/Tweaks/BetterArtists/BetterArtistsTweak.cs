using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.BetterArtists {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Better Artists Tweak")]
    [TweakDescription(LangCode.Korean, "더 나은 아티스트 설정")]
    public class BetterArtistsTweak : Tweak, IPatchClass<BetterArtistsPatch>, ISettingClass<BetterArtistsSetting> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }
    }
}