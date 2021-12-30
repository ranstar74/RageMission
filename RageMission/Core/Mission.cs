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

        private int _currentObjectiveIndex = -1;
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
            CurrentObjective.Update();

            if (CurrentObjective.Status == ObjectiveStatus.Success)
            {
                GoToNextObjective();
            }

            if (CurrentObjective.Status == ObjectiveStatus.Failed)
            {
                Finish(false);
            }
        }

        /// <summary>Goes to next objective. If there's no objectives left, mission finishes.</summary>
        private void GoToNextObjective()
        {
            if (_objectives.Count == _currentObjectiveIndex + 1)
            {
                Finish(true);
                return;
            }

            CurrentObjective = _objectives[++_currentObjectiveIndex];
            CurrentObjective.Start();
        }

        private void Finish(bool success)
        {
            MissionCompleteDialog.ShowFinished(success);

            IsFinished = true;
        }
    }
}
