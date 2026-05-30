using MelonLoader;
using Riptide;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using WheresGarryMultiplayerMockup.Network;

namespace WheresGarryMultiplayerMockup.Logic
{
    public static class ServerManager
    {
        public static List<int> fixedErrors = new();
        public static List<int> fixedServers = new();
        public static void Start()
        {
            NetworkManager.server.ClientConnected += HandlePlayerJoin;
            NetworkManager.server.ClientDisconnected += HandlePlayerLeaving;
        }

        private static void HandlePlayerJoin(object sender, Riptide.ServerConnectedEventArgs e)
        {
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.PlayerJoin);
            PlayerJoinMessage message = new()
            {
                id = e.Client.Id
            };
            outgoing.Add(message);
            NetworkManager.server.SendToAll(outgoing, e.Client.Id);

            foreach (var client in NetworkManager.server.Clients)
            {
                if (client.Id == e.Client.Id)
                    continue;
                Message outgoing2 = Message.Create(MessageSendMode.Reliable, Messages.PlayerJoin);
                PlayerJoinMessage message2 = new()
                {
                    id = client.Id
                };
                outgoing2.Add(message2);
                NetworkManager.server.Send(outgoing2, e.Client.Id);
            }
        }
        private static void HandlePlayerLeaving(object sender, ServerDisconnectedEventArgs e)
        {
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.PlayerLeft);
            PlayerLeftMessage message = new()
            {
                id = e.Client.Id
            };
            outgoing.Add(message);
            NetworkManager.server.SendToAll(outgoing);
        }


        [MessageHandler((ushort)Messages.PlayerState)]
        public static void HandlePlayerState(ushort sender, Message incoming)
        {
            var message = incoming.GetSerializable<PlayerStateMessage>();
            message.id = sender;
            Message outgoing = Message.Create(MessageSendMode.Unreliable, Messages.PlayerState);
            outgoing.Add(message);
            NetworkManager.server.SendToAll(outgoing, sender);
        }

        [MessageHandler((ushort)Messages.FixError)]
        public static void HandleFixErrors(ushort sender, Message incoming)
        {
            if (sender != NetworkManager.client.Id) Core.controller.Fix();
            var message = incoming.GetSerializable<FixMessage>();
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixError);
            outgoing.Add(message);
            NetworkManager.server.SendToAll(outgoing, sender);
        }

        public static void Update()
        {
            SendControllerState();/*
            for (int i = 0; i < Core.agents.Length; i++)
            {
                SendNpcState(i, Core.agents[i]);
            }*/
        }

        private static void SendNpcState(int id, NavMeshAgent agent)
        {
            if (agent == null) return;
            Message outgoing = Message.Create(MessageSendMode.Unreliable, Messages.NpcState);
            ushort closest = 0;
            foreach(var player in ClientManager.players)
            {
                if (Vector3.Distance(player.Value.transform.position, agent.transform.position) < Vector3.Distance(ClientManager.players[closest].transform.position, agent.transform.position))
                {
                    closest = player.Key;
                }
            }
            agent.SetDestination(ClientManager.players[closest].transform.position);
            NpcStateMessage message = new()
            {
                id = id,
                position = agent.transform.position,
                rotation = agent.transform.rotation,
            };
            outgoing.Add(message);
            NetworkManager.server.SendToAll(outgoing, NetworkManager.client.Id);
        }

        private static void SendControllerState()
        {
            if (Core.controller == null) return;
            Message outgoing = Message.Create(MessageSendMode.Unreliable, Messages.ControllerState);
            outgoing.Add(new ControllerStateMessage());
            NetworkManager.server.SendToAll(outgoing, NetworkManager.client.Id);
        }
    }
}
