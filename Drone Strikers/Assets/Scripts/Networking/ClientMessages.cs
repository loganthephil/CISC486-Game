using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace DroneStrikers.Networking
{
    public enum ClientMessages : byte
    {
        PlayerJoinTeam,
        PlayerMove,
        PlayerAim,
        PlayerShoot,
        PlayerSelectUpgrade
    }

    [Serializable]
    public class PlayerJoinTeamMessage
    {
        public int team;
    }

    [Serializable]
    public class Vector2Message
    {
        public float x;
        public float y;
    }

    [Serializable]
    public class PlayerMoveMessage
    {
        public Vector2Message movement;

        public PlayerMoveMessage(Vector2 movement) => this.movement = new Vector2Message { x = movement.x, y = movement.y };
    }

    [Serializable]
    public class PlayerAimMessage
    {
        public Vector2Message direction;
        public PlayerAimMessage(Vector2 direction) => this.direction = new Vector2Message { x = direction.x, y = direction.y };
    }

    [Serializable]
    public class PlayerSelectUpgradeMessage
    {
        public string upgradeId;

        public PlayerSelectUpgradeMessage(string upgradeID) => upgradeId = upgradeID;
    }
}