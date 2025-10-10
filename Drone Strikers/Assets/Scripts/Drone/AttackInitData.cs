using System;
using DroneStrikers.Combat;

namespace DroneStrikers.Drone
{
    [Serializable]
    public struct AttackInitData
    {
        public float Velocity;
        public float Damage;
        public float Pierce;

        public Team Team;
        public IDestructionContextReceiver InstigatorContextReceiver;
    }
}