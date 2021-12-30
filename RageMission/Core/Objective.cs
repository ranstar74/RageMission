using GTA;
using System;

namespace RageMission.Core
{
    /// <summary>An enumeration of all possible objective statuses.</summary>
    public enum ObjectiveStatus
    {
        /// <summary>Objective was not started yet.</summary>
        Pending,
        /// <summary>Objective is currently executing.</summary>
        InProgress,
        /// <summary>Objective was sucessfully finished.</summary>
        Success,
        /// <summary>Objective was failed.</summary>
        Failed
    }

    /// <summary>Defines a mission objective.</summary>
    public abstract class Objective
    {
        /// <summary>Current status of this objective.</summary>
        public ObjectiveStatus Status
        {
            get => _status;
            set
            {
                _status = value;

                if (Status == ObjectiveStatus.Success || Status == ObjectiveStatus.Failed)
                {
                    OnFinished();
                }
            }
        }

        /// <summary>Action that is invoked on objective start.</summary>
        public Action OnStart { get; set; }

        /// <summary>Action that is invoked on objective finish with status of 
        /// <see cref="ObjectiveStatus.Success"/> or <see cref="ObjectiveStatus.Failed"/>.</summary>
        public Action OnFinish { get; set; }

        /// <summary>Key of the localized text that will be displayed after starting this objective.</summary>
        public string StartMessageKey { get; set; }

        /// <summary>A reference to Player <see cref="Ped"/>.</summary>
        protected static Ped GPlayer => Game.Player.Character;

        private ObjectiveStatus _status;

        /// <summary>Creates a new <see cref="Objective"/> instance with <see cref="ObjectiveStatus.Pending"/> status.</summary>
        public Objective()
        {
            Status = ObjectiveStatus.Pending;
        }

        /// <summary>Calls on objective start.</summary>
        public virtual void Start()
        {
            UiHelper.ShowLocalizedSubtitle(StartMessageKey);

            OnStart?.Invoke();
        }

        /// <summary>Calls every tick when <see cref="Status"/> is <see cref="ObjectiveStatus.InProgress"/>.</summary>
        public abstract void Update();

        /// <summary>Frees all unmanaged resources, such as spawned entities.
        /// <para>Calls <see cref="OnFinished"/> automatically.</para></summary>
        public virtual void Abort()
        {
            OnFinished();
        }

        /// <summary>Called when <see cref="Status"/> was changed to 
        /// <see cref="ObjectiveStatus.Success"/> or <see cref="ObjectiveStatus.Failed"/>.</summary>
        protected virtual void OnFinished()
        {
            OnFinish?.Invoke();
        }
    }
}
