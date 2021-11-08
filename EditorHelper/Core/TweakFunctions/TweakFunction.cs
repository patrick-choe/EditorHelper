using System.Collections.Generic;
using EditorHelper.Core.Initializer;
using EditorHelper.Core.Patch;
using EditorHelper.Utils;
using SFB;
using UnityEngine;

namespace EditorHelper.Core.TweakFunctions {
    public static class TweakFunction {
        internal static readonly List<TweakFunctionData> Funcs = new();

        // ReSharper disable once SuggestVarOrType_DeconstructionDeclarations
        public static bool RunFuncs() {
            if (!scrController.instance.paused ||
                GCS.standaloneLevelMode ||
                scnEditor.instance.get<bool>("userIsEditingAnInputField") ||
                scnEditor.instance.get<bool>("showingPopup") ||
                StandaloneFileBrowser.lastFrameCount == Time.frameCount
            ) return true;

            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            bool bqPressed = Input.GetKey(KeyCode.BackQuote);

            bool isEmpty = scnEditor.instance.SelectionIsEmpty();
            bool isSingle = scnEditor.instance.SelectionIsSingle();
            bool isMultiple = !isEmpty && !isSingle;

            var result = true;
            foreach (var data in Funcs) {
                var keymap = data!.KeyMap;
                if (keymap == null) {
                    DebugUtils.Log("Keymap is null");
                    continue;
                }
                
                if (!Input.GetKeyDown(keymap.KeyCode)) continue;

                var checkShift = keymap.NeedsShift;
                var checkCtrl = keymap.NeedsCtrl;
                var checkAlt = keymap.NeedsAlt;
                var checkBq = keymap.NeedsBackQuote;

                var checkEmpty = data.Require.HasFlag(Require.SelectionIsEmpty);
                var checkSingle = data.Require.HasFlag(Require.SelectionIsSingle);
                var checkMultiple = data.Require.HasFlag(Require.SelectionIsMultiple);
                if (checkShift != shiftPressed || checkCtrl != ctrlPressed || checkAlt != altPressed || checkBq != bqPressed ||
                    (!isEmpty || !checkEmpty) && (!isSingle || !checkSingle) && (!isMultiple || !checkMultiple)) continue;
                data.Action!();
                result = false;
            }

            return result;
        }

        [Init] public static void InitRunFuncs() {
            Main.Harmony.TweakPatch(typeof(RunFuncsPatch));
        }
        
        [TweakPatchId(nameof(scnEditor), "Update")]
        private static class RunFuncsPatch {
            public static bool Prefix() => RunFuncs();
        }
    }
}