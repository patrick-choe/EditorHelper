using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Core.Components {
    public class HitsoundPlayer : MonoBehaviour {
        public Text Label => transform.parent.GetComponentInChildren<Text>();
        public Dropdown dropdown;
        public Button button;

        private void Start() {
            var index = dropdown.options.FindIndex(data => data.text == Label.text);
            if ((HitSound) index == HitSound.None) {
                button.image.enabled = false;
                button.enabled = false;
                return;
            }
            button.image.enabled = true;
            button.enabled = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                AudioManager.Instance._PlayImmediately("snd" + (HitSound) index, 1, true);
                UnityModManager.Logger.Log($"Play {(HitSound) index}");
            });
        }
    }
}