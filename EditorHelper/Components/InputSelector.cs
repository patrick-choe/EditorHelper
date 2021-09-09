
/*using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Components {
    public class InputSelector : InputField {
        public Dropdown Dropdown;
        public ScrollRect ScrollRect;

        public override void OnSelect(BaseEventData eventData) {
            if (Main.Settings.EnumInputField) {
                base.OnSelect(eventData);
            } else {
                Dropdown.Show();
            }
        }

        protected override void Start() {
            base.Start();
            ScrollRect = Dropdown.transform.Find("Template").GetComponent<ScrollRect>();
            this.onEndEdit.AddListener((value) => {
                var index = 0;
                foreach (var option in Dropdown.options) {
                    if (option.text == value) {
                        Dropdown.value = index;
                        break;
                    }

                    index++;
                }
            });
        }

        private void Update() {
            ScrollRect.scrollSensitivity = 60;
            if (!this.isFocused) {
                this.text = Dropdown.options[Dropdown.value].text;
            }
        }
    }
}*/