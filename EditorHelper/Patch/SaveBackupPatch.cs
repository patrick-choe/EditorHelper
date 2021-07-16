using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace EditorHelper.Patch
{
    [HarmonyPatch(typeof(scnEditor), "SaveBackup")]
    internal static class SaveBackupPatch
    {
        public static bool Prefix(scnEditor __instance)
        {
			// do nothing if this feature is not enabled
			if (!Main.Settings.EnableBetterBackup)
            {
				return true;
            }

			// original source code paste
			if (!__instance.controller.paused || string.IsNullOrEmpty(__instance.levelPath))
			{
				return false;
			}

			if (AccessTools.Field(typeof(scnEditor), "saveBackupLastFrame").GetValue(__instance).Equals(__instance.saveStateLastFrame))
			{
				__instance.printe("not going to saveBackup because state has not changed (EditorHelper Mod Patched)");
				return false;
			}

			Debug.Log("Saving Backup (EditorHelper Mod Patched)");
			string data = __instance.levelData.Encode(),
				// create unique path for the backup file
				path = $"/backups/backup-{DateTime.Now:yyyy-MM-dd HH-mm-ss}.adofai";

			// create FileInfo and make directory without any complicated nested directory shit
			FileInfo backupInfo = new FileInfo(path);
			backupInfo.Create();

			// write that data to the file
			RDFile.WriteAllText(Path.GetDirectoryName(__instance.levelPath) + path, data, null);

			// save the backup also on backup.adofai if user prefers to
			if (Main.Settings.SaveLatestBackup)
            {
				RDFile.WriteAllText(Path.GetDirectoryName(__instance.levelPath) + "backup.adofai", data, null);
			}

			// if backup amount is limited
			if (Main.Settings.MaximumBackups > 0)
            {
				// get backups from that directory
				FileInfo[] backups = backupInfo.Directory.GetFiles();

				// sort by creation datetime
				Array.Sort(
					backups,
					delegate (FileInfo fi1, FileInfo fi2)
					{
						return fi1.CreationTime.CompareTo(fi2.CreationTime);
					});

				// amount of backups - maximum allowed backups = amount to delete
				for (int i = 0; i < backups.Length - Main.Settings.MaximumBackups; i++)
                {
					// wrap in trycatch just in case user has opened up some kind of notepad or text editor app to edit that file
					try
                    {
						backups[i].Delete();
					}
					catch (Exception e)
                    {
						Main.Logger.Log($"An exception occurred while trying to delete backups: {e}");
                    }
                }
			}

			// original source code paste
			__instance.ShowNotification(RDString.Get("editor.notification.backupSaved"));
			AccessTools.Property(typeof(scnEditor), "unsavedChanges").SetValue(__instance, false);

			// return false because we do not want to use the original method again (it will save twice stupidly)
			return false;
        }
    }
}