using EditorHelper.Utils;
using SA.GoogleDoc;
using UnityEngine;

namespace EditorHelper.Core.Alerts {
    public class HelperUpdate : Alert {
        protected override (int, int) WindowSize => (960, 540);
        protected override (int, int) TargetRes => (1920, 1080);

        private GUIStyle _updateList = null!;

        public override void Init() {
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Download New Version"), (LangCode.Korean, "새 버전 다운로드")),
                () => {
                    //TODO
                    Close();
                },
                false
            );
            AddButton(
                () => GUIEx.CheckLangCode((LangCode.English, "Close"), (LangCode.Korean, "닫기")),
                () => {
                    //TODO
                    Close();
                }
            );

            _updateList = new GUIStyle(); 
            _updateList.font = Assets.SettingFontRegular;
        }

        public override void Content() {
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(65)}>EditorHelper신버전출시뿌슝빠슝???.</size></color>", SettingTitle);
            GUIEx.BeginIndent(20);
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(22)}>" + @" - 타일 각도 표시
 - 이벤트 번들 (여러 개의 이벤트 저장)
 - 개선된 아티스트 확인 ('&'로 여러 명의 아티스트 지정, color/size 태그 무시)
 - `키를 눌러 15도 단위 표시
 - WAV, MP3 확장자 지원 (MP3는 기본적으로 비활성화)
 - Alt + 화살표 키로 이벤트 사이 움직이기 / Alt + Delete로 현재 이벤트 삭제
 - 곡 로딩할때 BPM 측정 (오프셋도 지원하지만 mp3 확장자로만 작동)
 - 커스텀 키매핑
" + "</size></color>", _updateList);
            GUIEx.EndIndent();
            GUILayout.Label($"<color=#FFFFFF><size={MatchRes(35)}>호환 버전: r3145 (현재 버전: r{Main.ReleaseNum})</size></color>", SettingTitle);
        }
    }
}