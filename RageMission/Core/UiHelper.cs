using GTA.Native;

namespace RageMission.Core
{
    /// <summary>UI Methods.</summary>
    public static class UiHelper
    {
        /// <summary>Shows a mission subtitle with localized text by given key.</summary>
        public static void ShowLocalizedSubtitle(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Function.Call(Hash.BEGIN_TEXT_COMMAND_PRINT, key);
            Function.Call(Hash.END_TEXT_COMMAND_PRINT, 7500, true);
        }
    }
}
