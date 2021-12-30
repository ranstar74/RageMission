using System;
using System.Collections.Generic;

namespace RageMission.Core
{
    /// <summary>Manages mission execution.</summary>
    public static class MissionMgr
    {
        /// <summary>Current active mission. Null if there's none.</summary>
        public static Mission ActiveMission { get; private set; }

        private static readonly List<Mission> _missionHistory = new List<Mission>();

        /// <summary>Starts given mission.</summary>
        /// <param name="mission">Mission to start.</param>
        public static void StartMission(Mission mission)
        {
            if (ActiveMission != null)
                throw new Exception("Can't start another mission.");

            ActiveMission = mission;
            ActiveMission.Start();

            _missionHistory.Add(ActiveMission);
        }

        internal static void Update()
        {
            if(ActiveMission == null)
            {
                return;
            }

            ActiveMission.Update();
            if(ActiveMission.IsFinished)
            {
                ActiveMission = null;
            }
        }

        internal static void Abort()
        {
            _missionHistory.ForEach(m => m.Abort());
        }
    }
}
