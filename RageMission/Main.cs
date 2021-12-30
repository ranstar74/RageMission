using GTA;
using RageMission.Core;
using System;

namespace RageMission
{
    /// <summary>Main class of this script.</summary>
    public class Main : Script
    {
        /// <summary>Creates a new script instance.</summary>
        public Main()
        {
            Tick += Main_Tick;
            Aborted += Main_Aborted;
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            ScriptEvents.Update?.Invoke();

            MissionMgr.Update();
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            ScriptEvents.Abort?.Invoke();

            MissionMgr.Abort();
        }
    }
}
