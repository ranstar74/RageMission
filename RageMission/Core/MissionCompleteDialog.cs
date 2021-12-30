using GTA;
using GTA.Native;

namespace RageMission.Core
{
    /// <summary>Shows big message on screen.</summary>
    public static class MissionCompleteDialog
    {
        // https://forums.gta5-mods.com/topic/8501/c-mission-passed-scaleform-movie/7

        private static Scaleform _scBigMessage;
        private static int _timer = -1;
        private static int _stage = -1;

        static MissionCompleteDialog()
        {
            ScriptEvents.Update += Update;
            ScriptEvents.Abort += MarkAsNoLongerNeeded;
        }

        private static void MarkAsNoLongerNeeded()
        {
            _scBigMessage.Dispose();
        }

        /// <summary>Shows mission finished dialog.</summary>
        public static void ShowFinished(bool success)
        {
            if (_stage != -1)
                return;

            _scBigMessage = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
            while (!_scBigMessage.IsLoaded)
            {
                Script.Yield();
            }

            string message = Game.GetLocalizedString(success ? "M_FB4P3_P" : "REPLAY_T");
            _scBigMessage.CallFunction("SHOW_MISSION_PASSED_MESSAGE", message, string.Empty, 100, true, 0, true);
            _stage = 0;

            Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "MICHAEL_SMALL_01");
        }

        private static void Update()
        {
            if (!_scBigMessage.IsLoaded || _stage == -1)
            {
                return;
            }

            switch (_stage)
            {
                // Wait 2 seconds
                case 0:
                    {
                        _timer = Game.GameTime + 2000;
                        _stage++;
                        break;
                    }
                case 1:
                    {
                        if(_timer < Game.GameTime)
                        {
                            // Start rendering for 3 seconds
                            _timer = Game.GameTime + 3000;
                            _stage++;
                        }
                        break;
                    }
                // Render for 3 seconds and start transition out
                case 2:
                    {
                        if (_timer > Game.GameTime)
                        {
                            _scBigMessage.Render2D();
                            break;
                        }
                        _scBigMessage.CallFunction("TRANSITION_OUT");
                        _timer = Game.GameTime + 2000;
                        _stage++;

                        _scBigMessage.Render2D();
                        break;
                    }
                // Render transition out
                case 3:
                    {
                        if(_timer > Game.GameTime)
                        {
                            _scBigMessage.Render2D();
                            break;
                        }
                        _stage = -1;
                        MarkAsNoLongerNeeded();
                        break;
                    }
            }
        }
    }
}
