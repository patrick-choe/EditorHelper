using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ADOFAI;
using EditorHelper.Core.Patch;
using EditorHelper.Core.Tweaks;
using EditorHelper.Tweaks.RemoveEditorLimits;
using EditorHelper.Utils;

namespace EditorHelper.Tweaks.BetterArtists {
    public abstract class BetterArtistsPatch {
        private static BetterArtistsSetting Setting => TweakManager.Setting<BetterArtistsTweak, BetterArtistsSetting>()!;
        
        [TweakPatch(nameof(InspectorPanel), "ToggleArtistPopup")]
        public static class RemoveRichTagFromNamePatch {
            private static readonly string[] Separators = {
                "&",
                "vs",
                "Vs",
                "VS",
                "+",
            };

            private static readonly string[] SpacedSeparators = Separators.Select(s => $" {s} ").ToArray();
            public static bool Prefix(ref string search, float yPos, PropertyControl_Text artistPropertyControl) {
                if (!Setting.BetterArtistCheck) return true;
                if (search == SetDataPatch.GetAllArtists) return false;
                if (SetDataPatch.isChanging || !artistPropertyControl.inputField.isFocused) return true;
                var artistsToCheck = new List<string>();
                var artistsToCheckWithRichTags = new List<string>();
                var allSeparators = new List<string>();
                var builder = new StringBuilder();
                if (search.IsNullOrEmpty()) {
                    return true;
                }
                foreach (var token in search.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)) {
                    if (Separators.Contains(token)) {
                        artistsToCheckWithRichTags.Add(builder.ToString().Trim());
                        artistsToCheck.Add(builder.ToString().RemoveRichTags().Trim());
                        allSeparators.Add(token);
                        builder = new StringBuilder();
                    } else {
                        builder.Append(token + " ");
                    }
                }

                if (builder.ToString() != "") {
                    artistsToCheckWithRichTags.Add(builder.ToString().Trim());
                    artistsToCheck.Add(builder.ToString().RemoveRichTags().Trim());
                }

                SetDataPatch.ArtistsToCheckWithRichTags = artistsToCheckWithRichTags;
                SetDataPatch.ArtistsToCheck = artistsToCheck;
                SetDataPatch.AllSeparators = allSeparators;
                search = SetDataPatch.ArtistsToCheck[0];
                SetDataPatch.CurrentArtistIndex = 0;
                return true;
            }
        }


        [TweakPatch(nameof(ArtistUIDisclaimer), "SetData")]
        public static class SetDataPatch {
            public static int CurrentArtistIndex;
            public static List<string> ArtistsToCheckWithRichTags = new();
            public static List<string> ArtistsToCheck = new();
            public static List<string> URLs = new();
            public static bool isChanging;
            public static bool ForceNotOnlyCheck;
            public static List<string> AllSeparators = new();

            public static string GetAllArtists {
                get {
                    var toJoin = new string[ArtistsToCheckWithRichTags.Count + AllSeparators.Count];
                    for (int i = 0; i < ArtistsToCheckWithRichTags.Count; i++) {
                        toJoin[2 * i] = ArtistsToCheckWithRichTags[i];
                    }

                    for (int i = 0; i < AllSeparators.Count; i++) {
                        toJoin[2 * i + 1] = AllSeparators[i];
                    }

                    return string.Join(" ", toJoin);
                }
            }

            public static void Prefix(ArtistData data, PropertyControl_Text artistPC, ref bool onlyCheckingMode) {
                if (!Setting.BetterArtistCheck) return;
                if (data.name != ArtistsToCheckWithRichTags[CurrentArtistIndex].RemoveRichTags())
                    ArtistsToCheckWithRichTags[CurrentArtistIndex] = data.name;
                isChanging = true;
                onlyCheckingMode = !(ForceNotOnlyCheck || !onlyCheckingMode);
                ForceNotOnlyCheck = false;
            }

            public static void Postfix(ArtistUIDisclaimer __instance) {
                if (!Setting.BetterArtistCheck) return;
                scnEditor.instance.levelData.artist = GetAllArtists;
                __instance.get<PropertyControl_Text>("artistPropertyControl")!.inputField.text = GetAllArtists;
            }
        }

        [TweakPatch(nameof(ArtistUIDisclaimer), "Cancel")]
        public static class InitUrlPatch {
            public static void Postfix() {
                SetDataPatch.ArtistsToCheck = new List<string>();
                SetDataPatch.ArtistsToCheckWithRichTags = new List<string>();
                SetDataPatch.CurrentArtistIndex = -1;
                SetDataPatch.URLs = new List<string>();
                SetDataPatch.isChanging = false;
            }
        }

        [TweakPatch(nameof(ArtistUIDisclaimer), "Confirm")]
        internal static class ConfirmPatch {
            private static void Postfix(ADOBase __instance, bool ___onlyChecking, ArtistData? ___currentArtistData) {
                if (___onlyChecking) return;
                SetDataPatch.isChanging = false;
                if (Setting.BetterArtistCheck)
                    scnEditor.instance.levelData.artist =
                        __instance.get<PropertyControl_Text>("artistPropertyControl")!.inputField.text =
                            SetDataPatch.GetAllArtists;
                if (___currentArtistData != null) {
                    var link1 = ___currentArtistData.link1;
                    var link2 = ___currentArtistData.link2;
                    SetDataPatch.URLs.Add(link1);
                    SetDataPatch.URLs.Add(link2);
                }

                if (SetDataPatch.ArtistsToCheck.Any() && Setting.BetterArtistCheck) {
                    SetDataPatch.CurrentArtistIndex += 1;
                    var artist = SetDataPatch.ArtistsToCheck[SetDataPatch.CurrentArtistIndex];
                    var settingsPanel = scnEditor.instance.settingsPanel;
                    var ctrl = __instance.get<PropertyControl_Text>("artistPropertyControl")!;
                    SetDataPatch.ForceNotOnlyCheck = true;
                    settingsPanel.ToggleArtistPopup(artist, ctrl.rectTransform.position.y, ctrl);
                    ctrl.ToggleOthersEnabled();
                } else if (Setting.AutoArtistURL && ___currentArtistData != null) {
                    var link = string.Join(" ", SetDataPatch.URLs);
                    __instance.editor.levelData.levelSettings[nameof(__instance.editor.levelData.artistLinks)] = link;
                    __instance.editor.settingsPanel.panelsList[1].properties["artistLinks"].control.text = link;
                    SetDataPatch.ArtistsToCheck = new List<string>();
                    SetDataPatch.URLs = new List<string>();
                }
            }
        }
    }
}