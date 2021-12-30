using System;

namespace RageMission
{
    internal static class ScriptEvents
    {
        public static Action Update { get; set; }
        public static Action Abort { get; set; }
    }
}
