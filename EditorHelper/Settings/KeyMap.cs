using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorHelper.Settings {
    [Flags]
    public enum AdditionalKey {
        None        = 0b00000,
        Ctrl        = 0b00001,
        Shift       = 0b00010,
        Alt         = 0b00100,
        BackQuote   = 0b01000,
    }
    
    public sealed class KeyMap {
        public override int GetHashCode() {
            unchecked {
                int hashCode = NeedsCtrl.GetHashCode();
                hashCode = (hashCode * 397) ^ NeedsShift.GetHashCode();
                hashCode = (hashCode * 397) ^ NeedsAlt.GetHashCode();
                hashCode = (hashCode * 397) ^ NeedsBackQuote.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) KeyCode;
                hashCode = (hashCode * 397) ^ (Inital != null ? Inital.GetHashCode() : 0);
                return hashCode;
            }
        }

        private bool Equals(KeyMap other) {
            return this == other;
        }

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyMap) obj);
        }

        public bool NeedsCtrl;
        public bool NeedsShift;
        public bool NeedsAlt;
        public bool NeedsBackQuote;
        public KeyCode KeyCode;
        public KeyMap? Inital;
        
        public KeyMap(KeyCode keyCode, AdditionalKey additionalKey = AdditionalKey.None) {
            KeyCode = keyCode;
            NeedsCtrl = additionalKey.HasFlag(AdditionalKey.Ctrl);
            NeedsShift = additionalKey.HasFlag(AdditionalKey.Shift);
            NeedsAlt = additionalKey.HasFlag(AdditionalKey.Alt);
            NeedsBackQuote = additionalKey.HasFlag(AdditionalKey.BackQuote);
            Inital = new KeyMap {
                KeyCode = keyCode,
                NeedsCtrl = NeedsCtrl,
                NeedsShift = NeedsShift,
                NeedsAlt = NeedsAlt,
                Inital = null
            };
        }

        public void Reset() {
            if (Inital == null) return;
            KeyCode = Inital.KeyCode;
            NeedsCtrl = Inital.NeedsCtrl;
            NeedsShift = Inital.NeedsShift;
            NeedsAlt = Inital.NeedsAlt;
            NeedsBackQuote = Inital.NeedsBackQuote;
        }

        public KeyMap() { }
        
        public static bool operator==(KeyMap? orig, KeyMap? diff) {
            return orig?.GetHashCode() == diff?.GetHashCode();
        }

        public static bool operator !=(KeyMap? orig, KeyMap? diff) {
            return !(orig == diff);
        }

        public bool Check {
            get {
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) != NeedsCtrl) return false;
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) != NeedsShift) return false;
                if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) != NeedsAlt) return false;
                if (Input.GetKey(KeyCode.BackQuote) != NeedsBackQuote) return false;
                return KeyCode == KeyCode.None || Input.GetKeyDown(KeyCode);
            }
        }

        public override string ToString() {
            var keys = new List<string>();
            if (NeedsCtrl) keys.Add("Ctrl");
            if (NeedsShift) keys.Add("Shift");
            if (NeedsAlt) keys.Add("Alt");
            if (NeedsBackQuote) keys.Add("~");
            keys.Add(GetKeycodeValue());
            
            return string.Join(" + ", keys);
        }

        public string GetKeycodeValue() {
            return KeyCode switch {
                KeyCode.None => "",
                KeyCode.Minus => "-",
                KeyCode.Equals => "=",
                KeyCode.Backslash => "\\",
                KeyCode.LeftBracket => "[",
                KeyCode.RightBracket => "]",
                KeyCode.BackQuote => "~",
                KeyCode.CapsLock => "Caps Lock",
                KeyCode.Slash => "/",
                KeyCode.Asterisk => "*",
                KeyCode.Comma => "<size=20>,</size>",
                KeyCode.Period => "<size=20>.</size>",
                KeyCode.UpArrow => "↑",
                KeyCode.DownArrow => "↓",
                KeyCode.RightArrow => "→",
                KeyCode.LeftArrow => "←",
                _ => Enum.GetName(typeof(KeyCode), KeyCode) ?? string.Empty
            };
        }
        
        public string GetRawKeycodeValue() {
            return KeyCode switch {
                KeyCode.None => "",
                KeyCode.Minus => "-",
                KeyCode.Equals => "=",
                KeyCode.Backslash => "\\",
                KeyCode.LeftBracket => "[",
                KeyCode.RightBracket => "]",
                KeyCode.BackQuote => "~",
                KeyCode.CapsLock => "Caps Lock",
                KeyCode.Slash => "/",
                KeyCode.Asterisk => "*",
                KeyCode.Comma => ",",
                KeyCode.Period => ".",
                KeyCode.UpArrow => "↑",
                KeyCode.DownArrow => "↓",
                KeyCode.RightArrow => "→",
                KeyCode.LeftArrow => "←",
                _ => Enum.GetName(typeof(KeyCode), KeyCode) ?? string.Empty
            };
        }
    }
}