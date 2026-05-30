using MelonLoader;
using Riptide;
using UnityEngine;

namespace WheresGarryMultiplayerMockup.Network
{
    public static class NetworkManager
    {
        public static bool isRunning => isServer || isClient;
        public static bool isServer => server != null && server.IsRunning;
        public static bool isClient => client != null && client.IsConnected;
        public static Riptide.Server server = new();
        public static Client client = new();
        public static NetworkSettings settings;
        public static void CreateServer()
        {
            server.Start(settings.port, settings.maxConnections);
        }
        public static void JoinServer(string address, bool usePort)
        {
            if (usePort)
                client.Connect($"{address}:{settings.port}");
            else
                client.Disconnect();
        }
        public static void Shutdown()
        {
            server?.Stop();
            client?.Disconnect();
        }
        public static LemonAction serverUpdate;
        public static LemonAction clientUpdate;
        public static void Update()
        {
            if (server != null)
            {
                server.Update();
                if (isServer)
                {
                    serverUpdate.Invoke();
                }
            }
            if (client != null)
            {
                client.Update();
                if (isClient) {
                    clientUpdate.Invoke();
                }
            }
            if (isRunning)
            {
                Application.runInBackground = true;
                Time.timeScale = 1;
            }
        }
    }
}
