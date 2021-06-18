using HarmonyLib;

namespace EditorHelper.Patch
{
    [HarmonyPatch(typeof(ArtistUIDisclaimer), "Confirm")]
    internal static class ConfirmPatch
    {
        private static void Postfix(ADOBase __instance, bool ___onlyChecking, ArtistData ___currentArtistData)
        {
            if (!Main.Settings.AutoArtistURL || ___onlyChecking || ___currentArtistData == null)
            {
                return;
            }

            var link1 = ___currentArtistData.link1;
            var link2 = ___currentArtistData.link2;

            var link = link1 != null ? link2 != null ? link1 + ", " + link2 : link1 : string.Empty;
            __instance.editor.levelData.levelSettings[nameof(__instance.editor.levelData.artistLinks)] = link;
            __instance.editor.settingsPanel.panelsList[1].properties["artistLinks"].control.text = link;
        }
    }
}