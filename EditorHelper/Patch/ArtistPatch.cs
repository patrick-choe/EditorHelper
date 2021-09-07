using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using ADOFAI;
using HarmonyLib;
using MoreEditorOptions.Util;
using UnityModManagerNet;

namespace EditorHelper.Patch {
    [HarmonyPatch(typeof(InspectorPanel), "ToggleArtistPopup")]
    public static class RemoveRichTagFromNamePatch {
        public static bool Prefix(ref string search, float yPos, PropertyControl_Text artistPropertyControl) {
            if (!Main.Settings.EnableBetterArtistCheck) return true;
            if (search == SetDataPatch.GetAllArtists) return false;
            if (SetDataPatch.isChanging || !artistPropertyControl.inputField.isFocused) return true;
            SetDataPatch.ArtistsToCheckWithRichTags =
                search.Split(new string[] {"&", "vs", "Vs", "VS"}, StringSplitOptions.None).Select(s => s.Trim()).ToList();

            search = search.RemoveRichTags();
            SetDataPatch.ArtistsToCheck =
                search.Split(new string[] {"&", "vs", "Vs", "VS"}, StringSplitOptions.None).Select(s => s.Trim()).ToList();
            search = SetDataPatch.ArtistsToCheck[0];
            SetDataPatch.CurrentArtistIndex = 0;
            return true;
        }
    }
    

    [HarmonyPatch(typeof(ArtistUIDisclaimer), "SetData")]
    public static class SetDataPatch {
        public static int CurrentArtistIndex;
        public static List<string> ArtistsToCheckWithRichTags = new List<string>();
        public static List<string> ArtistsToCheck = new List<string>();
        public static List<string> CheckedArtist;
        public static List<string> URLs = new List<string>();
        public static bool isChanging = false;
        public static bool ForceNotOnlyCheck = false;
        public static string GetAllArtists => string.Join(" & ", ArtistsToCheckWithRichTags);

        public static void Prefix(ArtistData data, PropertyControl_Text artistPC, ref bool onlyCheckingMode) {
            if (!Main.Settings.EnableBetterArtistCheck) return;
            if (data.name != ArtistsToCheckWithRichTags[CurrentArtistIndex].RemoveRichTags())
                ArtistsToCheckWithRichTags[CurrentArtistIndex] = data.name;
            isChanging = true;
            onlyCheckingMode = !(ForceNotOnlyCheck || !onlyCheckingMode);
            ForceNotOnlyCheck = false;
        }
        public static void Postfix(ArtistUIDisclaimer __instance) {
            if (!Main.Settings.EnableBetterArtistCheck) return;
            scnEditor.instance.levelData.artist = GetAllArtists;
            __instance.get<PropertyControl_Text>("artistPropertyControl").inputField.text = GetAllArtists;
        }
    }

    [HarmonyPatch(typeof(ArtistUIDisclaimer), "Cancel")]
    public static class InitUrlPatch {
        public static void Postfix() {
            SetDataPatch.ArtistsToCheck = new List<string>();
            SetDataPatch.ArtistsToCheckWithRichTags = new List<string>();
            SetDataPatch.CurrentArtistIndex = -1;
            SetDataPatch.URLs = new List<string>();
            SetDataPatch.isChanging = false;
        }
    }

    [HarmonyPatch(typeof(ArtistUIDisclaimer), "Confirm")]
    internal static class ConfirmPatch {
        private static void Postfix(ADOBase __instance, bool ___onlyChecking, ArtistData ___currentArtistData) {
            if (___onlyChecking) return;
            SetDataPatch.isChanging = false;
            if (Main.Settings.EnableBetterArtistCheck)
                scnEditor.instance.levelData.artist = 
                    __instance.get<PropertyControl_Text>("artistPropertyControl").inputField.text = SetDataPatch.GetAllArtists;
            if (___currentArtistData != null) {
                var link1 = ___currentArtistData.link1;
                var link2 = ___currentArtistData.link2;
                SetDataPatch.URLs.Add(link1);
                SetDataPatch.URLs.Add(link2);
            }
            if (SetDataPatch.ArtistsToCheck.Any() && Main.Settings.EnableBetterArtistCheck) {
                SetDataPatch.CurrentArtistIndex += 1;
                var artist = SetDataPatch.ArtistsToCheck[SetDataPatch.CurrentArtistIndex];
                var settingsPanel = scnEditor.instance.settingsPanel;
                var ctrl = __instance.get<PropertyControl_Text>("artistPropertyControl");
                SetDataPatch.ForceNotOnlyCheck = true;
                settingsPanel.ToggleArtistPopup(artist, ctrl.rectTransform.position.y, ctrl);
                ctrl.ToggleOthersEnabled();
                return;
            } else if (Main.Settings.AutoArtistURL && ___currentArtistData != null) {
                var link = string.Join(" ", SetDataPatch.URLs);
                __instance.editor.levelData.levelSettings[nameof(__instance.editor.levelData.artistLinks)] = link;
                __instance.editor.settingsPanel.panelsList[1].properties["artistLinks"].control.text = link;
                SetDataPatch.ArtistsToCheck = new List<string>();
                SetDataPatch.URLs = new List<string>();
            }
        }
    }
}