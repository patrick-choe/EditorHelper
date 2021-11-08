using EditorHelper.Core.Tweaks;
using SA.GoogleDoc;

namespace EditorHelper.Tweaks.PreserveTileIndex {
    [RegisterTweak]
    [TweakDescription(LangCode.English, "Preserve Tiles Index")]
    [TweakDescription(LangCode.Korean,  "타일 인덱스 보존")]
    public class PreserveTileIndexTweak : Tweak, IPatchClass<PreserveTileIndexPatch> {
        public override void OnEnable() {
            PatchTweak();
        }

        public override void OnDisable() {
            UnpatchTweak();
        }
    }
}