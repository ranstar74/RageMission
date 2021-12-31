using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RageMission.Core
{
    /// <summary>A story line manager.</summary>
    public abstract class StorylineMgr
    {
        /// <summary>A List of all missions of this story line.</summary>
        protected abstract List<StoryMission> Missions { get; }

        /// <summary>A Map that links a name to a mission.</summary>
        protected abstract Dictionary<string, Type> MissionMap { get; }

        /// <summary>This is a set of various flags that is used to toggle
        /// missions. For example it could be IS_TUTORIAL_DONE: TRUE, and
        /// some other mission will became available with this flag set to True.</summary>
        protected abstract Dictionary<string, bool> MissionFlags { get; }

        /// <summary>Gets a Collection of all available missions.</summary>
        protected IEnumerable<StoryMission> AvailableMissions => _availableMissions;

        /// <summary>Gets a closest available story mission. 
        /// Could be null if there's none of some mission is currently running.</summary>
        public StoryMission ClosestMission { get; private set; }

        /// <summary>Squared distance to <see cref="ClosestMission"/>.</summary>
        public float ClosestMistanceDistance { get; private set; }

        private readonly Dictionary<StoryMission, Blip> _missionBlips = new Dictionary<StoryMission, Blip>();
        private IEnumerable<StoryMission> _availableMissions;
        private int _refreshTime = -1;

        /// <summary>Creates a new <see cref="StorylineMgr"/> instance.</summary>
        public StorylineMgr()
        {
            RefreshAvailableMissions();

            MissionMgr.OnMissionFinished += MissionMgr_OnMissionFinished;
        }

        /// <summary>Gets state of given flag.</summary>
        /// <param name="flag">Name of the flag.</param>
        /// <returns>A Boolean value, indicating whether flag is done or not.</returns>
        public bool GetFlagState(string flag)
        {
            if (!MissionFlags.ContainsKey(flag))
                throw new ArgumentException($"Flag: {flag} was not found.");

            return MissionFlags[flag];
        }

        /// <summary>Sets state of given flag.</summary>
        /// <param name="flag">Name of the flag.</param>
        /// <param name="value">Value to set.</param>
        public void SetFlagState(string flag, bool value)
        {
            if (!MissionFlags.ContainsKey(flag))
                throw new ArgumentException($"Flag: {flag} was not found.");

            MissionFlags[flag] = value;
        }

        /// <summary>Updates all blips of available missions on map.
        /// Needs to be called every tick.</summary>
        public void Update()
        {
            RefreshClosestMission();
            DrawMissionMarkers();

            if (ClosestMission == null)
                return;

            if(ClosestMistanceDistance < 2)
            {
                Type missionType = MissionMap[ClosestMission.MissionName];
                Mission mission = (Mission) Activator.CreateInstance(missionType);

                int fadeTime = 250;
                GTA.UI.Screen.FadeOut(fadeTime);

                Script.Wait(fadeTime);
                MissionMgr.StartMission(mission);

                GTA.UI.Screen.FadeIn(fadeTime);

                RefreshAvailableMissions();
            }
        }

        /// <summary>Removes all blips for map.</summary>
        public void Abort()
        {
            foreach(Blip blip in _missionBlips.Values)
            {
                blip.Delete();
            }
            _missionBlips.Clear();
        }

        private void MissionMgr_OnMissionFinished(Mission mission)
        {
            // Check if mission is from this storyline
            if (!MissionMap.Values.Contains(mission.GetType()))
                return;

            RefreshAvailableMissions();
        }

        private void DrawMissionMarkers()
        {
            foreach(StoryMission mission in AvailableMissions)
            {
                World.DrawMarker(
                    type: MarkerType.VerticalCylinder,
                    pos: mission.Position,
                    dir: default,
                    rot: default,
                    scale: new Vector3(1f, 1f, 1f),
                    color: Color.Red);
            }
        }

        private void RefreshClosestMission()
        {
            if(MissionMgr.IsMissionActive)
            {
                ClosestMission = null;
                ClosestMistanceDistance = -1;
                return;
            }

            if (_refreshTime > Game.GameTime)
            {
                return;
            }

            float closestDistance = float.MaxValue;
            StoryMission closestMission = null;
            Vector3 pPos = Game.Player.Character.Position;
            foreach (StoryMission mission in AvailableMissions)
            {
                float distance = pPos.DistanceToSquared(mission.Position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMission = mission;
                }
            }
            ClosestMission = closestMission;
            ClosestMistanceDistance = closestDistance;

            _refreshTime = Game.GameTime + 500;
        }

        private void RefreshAvailableMissions()
        {
            _availableMissions = GetAvailableMissions();

            List<StoryMission> availableMissions = AvailableMissions.ToList();
            foreach(StoryMission mission in Missions)
            {
                // Remove not available mission blips
                if(!availableMissions.Contains(mission))
                {
                    if(_missionBlips.ContainsKey(mission))
                    {
                        Blip blip = _missionBlips[mission];
                        blip.Delete();

                        _missionBlips.Remove(mission);
                    }
                    continue;
                }
                // Add new blips
                if(!_missionBlips.ContainsKey(mission))
                {
                    Blip blip = World.CreateBlip(mission.Position);
                    blip.Sprite = mission.BlipSprite;

                    _missionBlips.Add(mission, blip);
                }
            }
        }

        private IEnumerable<StoryMission> GetAvailableMissions()
        {
            if(!MissionMgr.IsMissionActive)
            {
                foreach (StoryMission sMission in Missions)
                {
                    string flag = sMission.RequiredFlag;

                    if (!string.IsNullOrEmpty(flag))
                    {
                        if (!MissionMap.ContainsKey(flag))
                            continue;

                        if (!MissionFlags[flag])
                            continue;
                    }

                    yield return sMission;
                }
            }
        }
    }

    /// <summary>A story mission with a marker on map.</summary>
    public sealed class StoryMission
    {
        /// <summary>Name of this mission.</summary>
        public string MissionName { get; set; }

        /// <summary><see cref="StorylineMgr.MissionFlags"/> that is required 
        /// to unlock this mission.</summary>
        public string RequiredFlag { get; set; }

        /// <summary>Position of this mission <see cref="Blip"/> on Map.</summary>
        public Vector3 Position { get; set; }

        /// <summary>Sprite of this mission <see cref="Blip"/> on Map.</summary>
        public BlipSprite BlipSprite { get; set; }
    }
}
