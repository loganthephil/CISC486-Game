using System;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;

namespace DroneStrikers.Game.Drone
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