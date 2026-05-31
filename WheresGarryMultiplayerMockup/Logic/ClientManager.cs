using Riptide;
using Riptide.Transports;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using WheresGarryMultiplayerMockup.Network;

namespace WheresGarryMultiplayerMockup.Logic
{
    public static class ClientManager
    {
        public static bool isDead = false;
        public static Player localPlayer => Core.controller.player;
        public static Dictionary<ushort, GameObject> players = new Dictionary<ushort, GameObject>();
        public static void Start()
        {
            Core.onSceneCached += OnSceneCached;
            NetworkManager.client.ClientConnected += Client_ClientConnected;
        }

        private static void Client_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            players.Add(e.Id, localPlayer.gameObject);
        }

        private static void OnSceneCached()
        {
            isDead = false;
            players.Clear();
        }

        public static void Update()
        {
            SendPlayerState();
        }

        private static void SendPlayerState()
        {
            if (localPlayer == null) return;
            PlayerStateMessage message = new PlayerStateMessage()
            {
                id = NetworkManager.client.Id,
                position = localPlayer.transform.position,
                rotation = localPlayer.transform.rotation,
            };
            Message outgoing = Message.Create(MessageSendMode.Unreliable, Messages.PlayerState);
            outgoing.Add(message);
            NetworkManager.client.Send(outgoing);
        }

        [MessageHandler((ushort)Messages.PlayerJoin)]
        public static void PlayerJoined(Message incoming)
        {
            var message = incoming.GetSerializable<PlayerJoinMessage>();
            var player = Core.GetNetPlayerPrefab();
            if (players.ContainsKey(message.id))
            {
                players.Remove(message.id);
            }
            players.TryAdd(message.id, player);
        }
        [MessageHandler((ushort)Messages.PlayerLeft)]
        public static void PlayerLeft(Message incoming)
        {
            var message = incoming.GetSerializable<PlayerLeftMessage>();
            if (!players.TryGetValue(message.id, out GameObject player)) return;
            players.Remove(message.id);
            GameObject.Destroy(player);
        }
        [MessageHandler((ushort)Messages.PlayerState)]
        public static void PlayerState(Message incoming)
        {
            var message = incoming.GetSerializable<PlayerStateMessage>();
            if (!players.TryGetValue(message.id, out GameObject player)) return;
            if (player == null)
            {
                players.Remove(message.id);
                return;
            }
            player.transform.position = message.position;
            player.transform.rotation = message.rotation;
        }
        [MessageHandler((ushort)Messages.ControllerState)]
        public static void ControllerState(Message incoming)
        {
            if (Core.controller == null) return;
            if (NetworkManager.isServer) return;
            var message = incoming.GetSerializable<ControllerStateMessage>();
        }
        /*
        [MessageHandler((ushort)Messages.NpcState)]
        public static void NpcState(Message incoming)
        {
            if (NetworkManager.isServer) return;
            var message = incoming.GetSerializable<NpcStateMessage>();
            var npc = Core.enemies[message.id];
            if (npc == null) return;
            npc.GetComponent<NavMeshAgent>().isStopped = true;
            npc.transform.position = message.position;
            npc.transform.rotation = message.rotation;
        }
        */
        [MessageHandler((ushort)Messages.FixError)]
        public static void HandleFixError(Message incoming)
        {
            var message = incoming.GetSerializable<FixMessage>();
            Error error = Core.errors[message.id];
            if (error == null) return;
            error.gameObject.SetActive(false);
        }
        [MessageHandler((ushort)Messages.FixServer)]
        public static void HandleFixServer(Message incoming)
        {
            var message = incoming.GetSerializable<FixMessage>();
            Server server = Core.servers[message.id];
            if (server == null) return;
            Core.controller.Noise(3, server.transform.position);
            server.turnedOn = true;
            server.shake.enabled = true;
            server.spriteRen.sprite = Core.controller.servers[1];
            if (server.loop != null && !server.loop.isPlaying)
            {
                server.loop.Play();
            }
        }

        [MessageHandler((ushort)Messages.AllDied)]
        public static void HandleAllDied(Message incoming)
        {
            SceneManager.LoadScene("Results");
        }
    }
}
