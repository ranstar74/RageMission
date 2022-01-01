using System;
using RageMission.Core;

namespace RageMission.Objectives
{
    /// <summary>Objective with <see cref="Success"/> and <see cref="Fail"/> functions.</summary>
    public class FuncObjective : Objective
    {
        /// <summary>Predicate that defines success status of this objective. Can't be null.</summary>
        public Func<bool> Success { get; }

        /// <summary>Predicate that defines failed status of this objective. Could be null if not required.</summary>
        public Func<bool> Fail { get; }

        /// <summary>Creates a new <see cref="FuncObjective"/> with and <see cref="Success"/> function. <see cref="Fail"/> is optinal.</summary>
        public FuncObjective(Func<bool> success, Func<bool> fail = null)
        {
            Success = success;
            Fail = fail;
        }

        /// <summary>Starts execution of this objective.</summary>
        public override void Start()
        {
            base.Start();   
        }

        /// <summary>Updates execution of this objective.</summary>
        public override void Update()
        {
            if(Success.Invoke() == true)
            {
                Status = ObjectiveStatus.Success;
                return;
            }

            if(Fail?.Invoke() == true)
            {
                Status = ObjectiveStatus.Failed;
                return;
            }
        }

        /// <inheritdoc/>
        protected override void OnFinished()
        {
            base.OnFinished();   
        }
    }
}
