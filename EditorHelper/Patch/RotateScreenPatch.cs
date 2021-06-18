// 능지 박살

// using HarmonyLib;
// using SFB;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace EditorHelper.Patch
// {
//     [HarmonyPatch(typeof(scnEditor), "Update")]
//     internal static class UpdateRotationPatch
//     {
//         internal static float CurrentRot;
//         
//         private static void Prefix(scnEditor __instance, ref bool ___refreshBgSprites, ref bool ___refreshDecSprites)
//         {
//             if (!Main.Settings.EnableScreenRot ||
//                 !scrController.instance.paused ||
//                 GCS.standaloneLevelMode ||
//                 StandaloneFileBrowser.lastFrameCount == Time.frameCount)
//             {
//                 return;
//             }
//
//             var selectedObj = __instance.eventSystem.currentSelectedGameObject;
//
//             if (selectedObj != null && selectedObj.GetComponent<InputField>() != null)
//             {
//                 return;
//             }
//
//             if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
//                 Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))
//             {
//                 return;
//             }
//
//             if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
//             {
//                 return;
//             }
//
//             if (Input.GetKeyDown(KeyCode.Comma))
//             {
//                 var camera = scrCamera.instance;
//
//                 if (camera != null)
//                 {
//                     CurrentRot -= 15f;
//                     camera.transform.rotation = Quaternion.Euler(0f, 0f, CurrentRot);
//                 }
//             }
//
//             if (Input.GetKeyDown(KeyCode.Period))
//             {
//                 var camera = scrCamera.instance;
//
//                 if (camera != null)
//                 {
//                     CurrentRot += 15f;
//                     camera.transform.rotation = Quaternion.Euler(0f, 0f, CurrentRot);
//                 }
//             }
//         }
//     }
//     
//     [HarmonyPatch(typeof(scrController), "Awake_Rewind")]
//     internal static class AwakeRewindPatch
//     {
//         private static void Prefix()
//         {
//             if (Main.Settings.EnableScreenRot)
//             {
//                 UpdateRotationPatch.CurrentRot = 0f;
//             }
//         }
//     }
//
//     [HarmonyPatch(typeof(Input), "mousePosition", MethodType.Getter)]
//     internal static class PositionPatch
//     {
//         private static void Postfix(ref Vector3 __result)
//         {
//             if (!Main.Settings.EnableScreenRot || UpdateRotationPatch.CurrentRot == 0f)
//             {
//                 return;
//             }
//
//             __result = Quaternion.AngleAxis(UpdateRotationPatch.CurrentRot, Vector3.forward) * __result;
//         }
//     }
// }