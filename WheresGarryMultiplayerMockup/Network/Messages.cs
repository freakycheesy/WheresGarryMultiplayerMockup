using Riptide;
using UnityEngine;
using WheresGarryMultiplayerMockup.Logic;

namespace WheresGarryMultiplayerMockup.Network
{
    public enum Messages : ushort
    {
        PlayerJoin,
        PlayerLeft,
        PlayerState,
        FixError,
        FixServer,
        ControllerState,
        NpcState,
        Died,
        AllDied,
    }
    public struct PlayerJoinMessage : IMessageSerializable
    {
        public ushort id;
        public void Deserialize(Message message)
        {
            id = message.GetUShort();
        }
        public void Serialize(Message message)
        {
            message.Add(id);
        }
    }
    public struct PlayerLeftMessage : IMessageSerializable
    {
        public ushort id;
        public void Deserialize(Message message)
        {
            id = message.GetUShort();
        }
        public void Serialize(Message message)
        {
            message.Add(id);
        }
    }
    public struct PlayerStateMessage : IMessageSerializable
    {
        public ushort id;
        public Vector3 position;
        public Quaternion rotation;

        public void Deserialize(Message message)
        {
            id = message.GetUShort();
            position = message.GetVector3();
            rotation = message.GetQuaternion();
        }

        public void Serialize(Message message)
        {
            message.Add(id);
            message.Add(position);
            message.Add(rotation);
        }
    }
    public struct NpcStateMessage : IMessageSerializable
    {
        public int id;
        public Vector3 position;
        public Quaternion rotation;

        public void Deserialize(Message message)
        {
            id = message.GetUShort();
            position = message.GetVector3();
            rotation = message.GetQuaternion();
        }

        public void Serialize(Message message)
        {
            message.Add(id);
            message.Add(position);
            message.Add(rotation);
        }
    }
    public struct FixMessage : IMessageSerializable
    {
        public int id;

        public void Deserialize(Message message)
        {
            id = message.GetInt();
        }

        public void Serialize(Message message)
        {
            message.Add(id);
        }
    }
    public struct ControllerStateMessage : IMessageSerializable
    {
        public Controller controller => Core.controller;
        public void Deserialize(Message message)
        {
            controller.timer = message.GetFloat();
            controller.enableTimer = message.GetBool();
            controller.timerEnded = message.GetBool();
            controller.timers = message.GetFloats();
            controller.freezeTimer = message.GetBool();
            controller.overtimeHearTimer = message.GetFloat();
            controller.shadowTimer = message.GetFloat();
            controller.errorsFixed = message.GetInt();
        }

        public void Serialize(Message message)
        {
            message.Add(controller.timer);
            message.Add(controller.enableTimer);
            message.Add(controller.timerEnded);
            message.Add(controller.timers);
            message.Add(controller.freezeTimer);
            message.Add(controller.overtimeHearTimer);
            message.Add(controller.shadowTimer);
            message.Add(controller.errorsFixed);
        }
    }
}
