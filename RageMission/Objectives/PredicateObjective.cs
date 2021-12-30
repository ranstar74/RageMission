using System;
using RageMission.Core;

namespace RageMission.Objectives
{
    /// <summary>Objective with <see cref="Success"/> and <see cref="Fail"/> predicates.</summary>
    public class PredicateObjective<T> : Objective
    {
        /// <summary>Target of this objective. For example could be vehicle.</summary>
        public T Target { get; }

        /// <summary>Predicate that defines success status of this objective. Can't be null.</summary>
        public Predicate<T> Success { get; }

        /// <summary>Predicate that defines failed status of this objective. Could be null if not required.</summary>
        public Predicate<T> Fail { get; }

        /// <summary>Creates a new <see cref="PredicateObjective{T}"/> with <see cref="Target"/> and <see cref="Success"/> predicate. 
        /// <see cref="Fail"/> is optinal.</summary>
        public PredicateObjective(T target, Predicate<T> success, Predicate<T> fail = null)
        {
            Target = target;
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
            if(Success(Target))
            {
                Status = ObjectiveStatus.Success;
                return;
            }

            if(Fail?.Invoke(Target) == true)
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
