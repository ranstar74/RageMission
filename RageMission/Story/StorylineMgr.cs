using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

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

        /// <summary>Name of the file name. Leave empty if save's are not required.</summary>
        protected abstract string SaveFileName { get; }

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

        private readonly JavaScriptSerializer _jsonSerializer = new JavaScriptSerializer();

        /// <summary>Creates a new <see cref="StorylineMgr"/> instance.</summary>
        public StorylineMgr()
        {
            LoadProgress();

            RefreshAvailableMissions();

            MissionMgr.OnMissionFinished += MissionMgr_OnMissionFinished;
        }

        /// <summary>Saves progress to file.</summary>
        public void SaveProgress()
        {
            if (string.IsNullOrEmpty(SaveFileName))
                return;

            GTA.UI.LoadingPrompt.Show();

            try
            {
                if (File.Exists(SaveFileName))
                {
                    new FileInfo(SaveFileName).MoveTo(SaveFileName + ".bak");
                }
                using (StreamWriter sw = new StreamWriter(File.Create(SaveFileName)))
                {
                    sw.Write(_jsonSerializer.Serialize(MissionFlags));
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show(
                    icon: GTA.UI.NotificationIcon.SocialClub,
                    sender: "Rage Mission",
                    subject: "An Error Occured While Saving",
                    message: ex.ToString());
            }
            finally
            {
                GTA.UI.LoadingPrompt.Hide();
            }
        }

        /// <summary>Loads progress from file. In case if there's no file nothing is loaded.</summary>
        public void LoadProgress()
        {
            if (string.IsNullOrEmpty(SaveFileName))
                return;

            if (!File.Exists(SaveFileName))
            {
                return;
            }

            GTA.UI.LoadingPrompt.Show();

            try
            {
                Dictionary<string, bool> flags;

                using (StreamReader sr = new StreamReader(File.OpenRead(SaveFileName)))
                {
                    flags = _jsonSerializer.Deserialize<Dictionary<string, bool>>(sr.ReadToEnd());
                }

                foreach (KeyValuePair<string, bool> keyPair in flags)
                {
                    if (!MissionFlags.ContainsKey(keyPair.Key))
                        continue;

                    MissionFlags[keyPair.Key] = keyPair.Value;
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show(
                    icon: GTA.UI.NotificationIcon.SocialClub,
                    sender: "Rage Mission",
                    subject: "Save Data Corruped",
                    message: ex.ToString(), true, true);
            }
            finally
            {
                GTA.UI.LoadingPrompt.Hide();
            }
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

            if (ClosestMistanceDistance < 2)
            {
                Type missionType = MissionMap[ClosestMission.MissionName];
                Mission mission = (Mission)Activator.CreateInstance(missionType);

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
            foreach (Blip blip in _missionBlips.Values)
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

            SaveProgress();
        }

        private void DrawMissionMarkers()
        {
            foreach (StoryMission mission in AvailableMissions)
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
            if (MissionMgr.IsMissionActive)
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
            foreach (StoryMission mission in Missions)
            {
                // Remove not available mission blips
                if (!availableMissions.Contains(mission))
                {
                    if (_missionBlips.ContainsKey(mission))
                    {
                        Blip blip = _missionBlips[mission];
                        blip.Delete();

                        _missionBlips.Remove(mission);
                    }
                    continue;
                }
                // Add new blips
                if (!_missionBlips.ContainsKey(mission))
                {
                    Blip blip = World.CreateBlip(mission.Position);
                    blip.Sprite = mission.BlipSprite;

                    _missionBlips.Add(mission, blip);
                }
            }
        }

        private IEnumerable<StoryMission> GetAvailableMissions()
        {
            if (!MissionMgr.IsMissionActive)
            {
                foreach (StoryMission sMission in Missions)
                {
                    string required = sMission.RequiredFlag;
                    string finished = sMission.FinishedFlag;

                    // Check if mission was finished
                    if (string.IsNullOrEmpty(finished))
                        throw new Exception("Finished flag cannot be empty.");

                    if (!MissionFlags.ContainsKey(finished))
                        throw new Exception("Finished flag does not present in mission flags dictionary.");

                    if (MissionFlags[finished])
                        continue;

                    // Check if mission is unlocked
                    if (!string.IsNullOrEmpty(required))
                    {
                        if (!MissionMap.ContainsKey(required))
                            continue;

                        if (!MissionFlags[required])
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

        /// <summary>A flag that is required to unlock this mission. Leave empty if not required.</summary>
        public string RequiredFlag { get; set; }

        /// <summary>A flag that marks this mission as finished.</summary>
        public string FinishedFlag { get; set; }

        /// <summary>Position of this mission <see cref="Blip"/> on Map.</summary>
        public Vector3 Position { get; set; }

        /// <summary>Sprite of this mission <see cref="Blip"/> on Map.</summary>
        public BlipSprite BlipSprite { get; set; }
    }
}
