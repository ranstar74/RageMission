using System;
using System.Collections.Generic;

namespace RageMission.Core
{
    /// <summary>Defines a mission with objectives.</summary>
    public abstract class Mission
    {
        /// <summary>All the objectives of this mission.</summary>
        public IEnumerable<Objective> Objectives => _objectives;

        /// <summary>Current mission objective. Null if mission is not active.</summary>
        public Objective CurrentObjective { get; protected set; }

        /// <summary>Gets a value indicating whether mission was finished or not.</summary>
        public bool IsFinished { get; private set; }

        /// <summary>Action that invokes when objective starts.</summary>
        protected Action<Objective> OnObjectiveStarted { get; set; }

        private int _currentObjectiveIndex = 0;
        private readonly List<Objective> _objectives;

        /// <summary>Creates a new <see cref="Mission"/> instance.</summary>
        public Mission()
        {
            _objectives = new List<Objective>();
        }

        /// <summary>Starts this mission.</summary>
        public virtual void Start()
        {
            if (_objectives.Count == 0)
                throw new Exception("Mission should have at least one objective.");

            GoToNextObjective();
        }

        /// <summary>Aborts execution of this mission.</summary>
        public virtual void Abort()
        {
            _objectives.ForEach(o => o.Abort());
        }

        /// <summary>Adds a new objective to this mission.</summary>
        protected void AddObjective(Objective objective)
        {
            _objectives.Add(objective);
        }

        /// <summary>Calls every tick when mission is active.</summary>
        public virtual void Update()
        {
            // I really have no idea why that happens
            // but time to time it does
            if (CurrentObjective == null)
            {
                return;
            }

            CurrentObjective.Update();

            if (CurrentObjective.Status == ObjectiveStatus.Success)
            {
                GoToNextObjective();
            }

            if (CurrentObjective.Status == ObjectiveStatus.Failed)
            {
                OnFinish(false);
            }
        }

        /// <summary>Goes to next objective. If there's no objectives left, mission finishes.</summary>
        private void GoToNextObjective()
        {
            if (_objectives.Count == _currentObjectiveIndex)
            {
                OnFinish(true);
                return;
            }
            CurrentObjective = _objectives[_currentObjectiveIndex];
            CurrentObjective.Status = ObjectiveStatus.InProgress;
            CurrentObjective.Start();

            OnObjectiveStarted?.Invoke(CurrentObjective);

            _currentObjectiveIndex++;
        }

        /// <summary>Calls on mission finish.</summary>
        protected virtual void OnFinish(bool success)
        {
            MissionCompleteDialog.ShowFinished(success);

            IsFinished = true;
        }
    }
}
