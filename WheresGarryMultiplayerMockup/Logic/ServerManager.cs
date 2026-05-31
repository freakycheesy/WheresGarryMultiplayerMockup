using MelonLoader;
using Riptide;
using Riptide.Transports;
using UnityEngine;
using WheresGarryMultiplayerMockup.Network;

namespace WheresGarryMultiplayerMockup.Logic
{
    public static class ServerManager
    {
        public static List<int> fixedErrors = new();
        public static List<int> fixedServers = new();
        public static List<ushort> deadPlayers = new();
        public static void Start()
        {
            Core.onSceneCached += OnSceneCached;
            NetworkManager.server.ClientConnected += HandlePlayerJoin;
            NetworkManager.server.ClientDisconnected += HandlePlayerLeaving;
        }

        private static void OnSceneCached()
        {
            deadPlayers.Clear();
            fixedErrors.Clear();
            fixedServers.Clear();
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
                outgoing = Message.Create(MessageSendMode.Reliable, Messages.PlayerJoin);
                PlayerJoinMessage message2 = new()
                {
                    id = client.Id
                };
                outgoing.Add(message2);
                NetworkManager.server.Send(outgoing, e.Client.Id);
            }

            foreach(var error in fixedErrors)
            {
                FixMessage message2 = new()
                {
                    id = error
                };
                outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixError);
                outgoing.Add(message);
                NetworkManager.server.Send(outgoing, e.Client.Id);
            }
            foreach (var server in fixedServers)
            {
                FixMessage message2 = new()
                {
                    id = server
                };
                outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixServer);
                outgoing.Add(message);
                NetworkManager.server.Send(outgoing, e.Client.Id);
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
            if (deadPlayers.Count >= NetworkManager.server.ClientCount)
            {
                outgoing = Message.Create(MessageSendMode.Reliable, Messages.AllDied);
                NetworkManager.server.SendToAll(outgoing);
            }
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
            FixCode(sender, incoming, Messages.FixError);
        }
        [MessageHandler((ushort)Messages.FixServer)]
        public static void HandleFixServers(ushort sender, Message incoming)
        {
            FixCode(sender, incoming, Messages.FixServer);
        }

        private static void FixCode(ushort sender, Message incoming, Messages messageId)
        {
            Core.controller.Fix();
            var message = incoming.GetSerializable<FixMessage>();
            Message outgoing = Message.Create(MessageSendMode.Reliable, messageId);
            outgoing.Add(message);
            switch (messageId)
            {
                case Messages.FixError:
                    fixedErrors.Add(message.id);
                    break;
                case Messages.FixServer:
                    fixedServers.Add(message.id);
                    break;
            }
            NetworkManager.server.SendToAll(outgoing);
        }

        [MessageHandler((ushort)Messages.Died)]
        public static void HandleDeath(ushort sender, Message incoming)
        {
            MelonLogger.Msg("Received Dead Message");
            if(!deadPlayers.Contains(sender)) deadPlayers.Add(sender);
            if (sender == NetworkManager.client.Id)
            {
                ClientManager.localPlayer.transform.position = Vector3.up * 500;
                ClientManager.localPlayer.gameObject.SetActive(false);
            }
            else
            {
                ClientManager.players.Remove(sender);
            }        
            if (deadPlayers.Count >= NetworkManager.server.ClientCount)
            {
                Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.AllDied);
                NetworkManager.server.SendToAll(outgoing);
            }
        }


        public static void Update()
        {
            SendControllerState();
            /*
            if (Core.controller)
            {
                for (int i = 0; i < Core.enemies.Length; i++)
                {
                    SendNpcState(i);
                }
            }
            */
        }
        /*
        private static void SendNpcState(int id)
        {
            var npc = Core.enemies[id];
            if (npc == null) return;
            Transform closest = ClientManager.players[0].transform;
            foreach (var player in ClientManager.players.Values)
            {
                if (Vector3.Distance(player.transform.position, npc.transform.position) < Vector3.Distance(closest.transform.position, npc.transform.position))
                {
                    closest = player.transform;
                }
            }
            if (npc.TryGetComponent(out Enemy enemy))
            {
                typeof(Enemy).GetField("player").SetValue(enemy, closest.GetComponent<Player>());
            }
            else if (npc.TryGetComponent(out PuppetNPC puppet))
            {
                typeof(PuppetNPC).GetField("player").SetValue(puppet, closest.GetComponent<Player>());
            }
            NpcStateMessage message = new()
            {
                id = id,
                position = npc.transform.position,
                rotation = npc.transform.rotation,
            };
            Message outgoing = Message.Create(MessageSendMode.Unreliable, Messages.NpcState);
            outgoing.Add(message);
            NetworkManager.server.SendToAll(outgoing, NetworkManager.client.Id);
        }*/

        private static void SendControllerState()
        {
            if (Core.controller == null) return;
            Message outgoing = Message.Create(MessageSendMode.Unreliable, Messages.ControllerState);
            outgoing.Add(new ControllerStateMessage());
            NetworkManager.server.SendToAll(outgoing, NetworkManager.client.Id);
        }
    }
}
