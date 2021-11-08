using System.Runtime.CompilerServices;
using UnityModManagerNet;

namespace EditorHelper.Utils {
    public abstract class DebugUtils {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(object log) {
#if  DEBUG
            UnityModManager.Logger.Log($"{log}", "[DEBUG] ");
#endif
        }
    }
}