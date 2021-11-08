using EditorHelper.Core.Tweaks;
using EditorHelper.Settings;
using SA.GoogleDoc;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace EditorHelper.Tweaks.MoveEventAltArrow {
    public class MoveEventAltArrowSetting : ITweakSetting {
        [Label(LangCode.English, "Move to the event above")]
        [Label(LangCode.Korean, "위의 이벤트로 이동")]
        [Draw] public KeyMap MoveEventUp = new KeyMap(KeyCode.UpArrow, AdditionalKey.BackQuote);

        [Label(LangCode.English, "Move to the event below")]
        [Label(LangCode.Korean, "아래의 이벤트로 이동")]
        [Draw] public KeyMap MoveEventDown = new KeyMap(KeyCode.DownArrow, AdditionalKey.BackQuote);

        [Label(LangCode.English, "Move to the previous event")]
        [Label(LangCode.Korean, "이전 이벤트로 이동")]
        [Draw] public KeyMap MoveEventLeft = new KeyMap(KeyCode.LeftArrow, AdditionalKey.BackQuote);

        [Label(LangCode.English, "Move to the next event")]
        [Label(LangCode.Korean, "다음 이벤트로 이동")]
        [Draw] public KeyMap MoveEventRight = new KeyMap(KeyCode.RightArrow, AdditionalKey.BackQuote);

        [Label(LangCode.English, "Delete the selected event")]
        [Label(LangCode.Korean, "선택된 이벤트 삭제")]
        [Draw] public KeyMap DeleteEvent = new KeyMap(KeyCode.Delete, AdditionalKey.BackQuote);

        public void OnGUI() {
            this.Draw();
        }
    }
}