using System;
using System.Collections.Generic;
using ADOFAI;
using DG.Tweening;
using EditorHelper.Utils;
using HarmonyLib;
using MoreEditorOptions.Util;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace EditorHelper.Patch {
	/*
	[HarmonyPatch(typeof(scnEditor), "FloorPointsBackwards", typeof(float))]
	public static class FPBfloatPatch {
		public static bool Prefix(scnEditor __instance, float floorAngle) {
			var previousFloor = __instance.invoke<scrFloor>("PreviousFloor");
			previousFloor(__instance.selectedFloors[0]);
			float b = floorAngle.NormalizeAngle();
			double num = __instance.selectedFloors[0].entryangle * 57.295780181884766;
			return Mathf.Approximately(Mathf.Abs(450f - (float)num).NormalizeAngle(), b);
		}
	}
	*/

}