using GTA;
using GTA.Math;
using RageMission.Core;

namespace RageMission.Objectives
{
    /// <summary>Objective that is considered as successfully complete when player reaches given position.</summary>
    public class WaypointObjective : Objective
    {
        /// <summary>Gets position that player should reach.</summary>
        public Vector3 TargetPosition => _position;

        /// <summary>Squared distance to <see cref="TargetPosition"/> that is required to complete this objective.</summary>
        /// <remarks>Default value is 1024 (32 meters).</remarks>
        public int Distance { get; set; } = 1024;

        private readonly Vector3 _position;
        private readonly string _blipKey;
        private Blip _blip;
        private float _distanceToWaypoint = float.MaxValue;

        /// <summary>Creates a new <see cref="WaypointObjective"/> with a <see cref="Blip"/> on given position.</summary>
        /// <param name="position">Position where to place <see cref="Blip"/>.</param>
        /// <param name="blipNameKey">Localized <see cref="Blip"/> name text key.</param>
        public WaypointObjective(Vector3 position, string blipNameKey)
        {
            _position = position;
            _blipKey = blipNameKey;
        }

        /// <summary>Starts execution of this mission.</summary>
        public override void Start()
        {
            base.Start();

            _blip = World.CreateBlip(_position);
            _blip.Name = Game.GetLocalizedString(_blipKey);
            _blip.Color = BlipColor.Yellow;
            _blip.ShowRoute = true;
        }

        /// <summary>Updates execution of this mission. 
        /// Needs to be called every tick when mission is active.</summary>
        public override void Update()
        {
            _distanceToWaypoint = Vector3.DistanceSquared(_position, GPlayer.Position);

            if (_distanceToWaypoint < 1000)
            {
                Status = ObjectiveStatus.Success;
            }
        }

        /// <inheritdoc/>
        protected override void OnFinished()
        {
            base.OnFinished();

            _blip.Delete();
        }
    }
}
