using System;
using System.Collections.Generic;

namespace RageMission.Core
{
    /// <summary>Delegate that being invoked when a mission finishes.</summary>
    public delegate void OnMissionFinished(Mission mission);

    /// <summary>Manages mission execution.</summary>
    public static class MissionMgr
    {
        /// <summary>Current active mission. Null if there's none.</summary>
        public static Mission ActiveMission { get; private set; }

        /// <summary>Last started mission.</summary>
        public static Mission LastMission { get; private set; }

        /// <summary>Gets a value indicating whether there's mission running or not.</summary>
        public static bool IsMissionActive => ActiveMission != null;

        /// <summary>Event that is invoked when a mission finishes.</summary>
        public static event OnMissionFinished OnMissionFinished;

        private static readonly List<Mission> _missionHistory = new List<Mission>();

        /// <summary>Starts given mission.</summary>
        /// <param name="mission">Mission to start.</param>
        public static void StartMission(Mission mission)
        {
            if (ActiveMission != null)
                throw new Exception("Can't start another mission.");

            ActiveMission = mission;
            ActiveMission.Start();

            LastMission = ActiveMission;
            _missionHistory.Add(LastMission);
        }

        internal static void Update()
        {
            if(ActiveMission == null)
            {
                return;
            }

            if(ActiveMission.CurrentObjective?.Status != ObjectiveStatus.InProgress)
            {
                return;
            }
            ActiveMission.Update();

            if(ActiveMission.IsFinished)
            {
                ActiveMission = null;
                OnMissionFinished(LastMission);
            }
        }

        internal static void Abort()
        {
            _missionHistory.ForEach(m => m.Abort());
        }
    }
}
