using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.HighlightTargetedTiles {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Highlight Targeted Tiles")]
    [TweakDescription(LangCode.Korean,  "목표 타일 하이라이트")]
    public class HighlightTargetedTilesTweak : Tweak, IPatchClass<HighlightTargetedTilesPatch> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }
    }
}