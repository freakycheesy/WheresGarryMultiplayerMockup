using MelonLoader;
using MelonLoader.Utils;
using Riptide;
using Riptide.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using WheresGarryMultiplayerMockup.Logic;
using WheresGarryMultiplayerMockup.Network;

[assembly: MelonInfo(typeof(WheresGarryMultiplayerMockup.Core), "WheresGarryMultiplayerMockup", "1.0.0", "cheesy", null)]
[assembly: MelonGame("Realest Billy", "Where's Garry")]

namespace WheresGarryMultiplayerMockup
{
    public class Core : MelonMod
    {
        public static LemonAction onSceneCached;
        public static Controller controller;
        public static Error[] errors;
        public static Server[] servers;
        public override void OnInitializeMelon()
        {
            Message.MaxPayloadSize = ushort.MaxValue;
            onSceneCached += OnSceneCached;
            LoggerInstance.Msg("Initialized.");
            HarmonyInstance.PatchAll();
            string path = Path.Combine(MelonEnvironment.UserDataDirectory, "networkSettings.json");
            if (!File.Exists(path))
            {
                var settings = new NetworkSettings() {
                    port = 7777,
                    maxConnections = 10
                };
                File.WriteAllText(path, JsonUtility.ToJson(settings));
            }
            RiptideLogger.Initialize(MelonLogger.Msg, MelonLogger.Msg, MelonLogger.Warning, MelonLogger.Error, true);
            NetworkManager.settings = JsonUtility.FromJson<NetworkSettings>(File.ReadAllText(path));
            MelonEvents.OnGUI.Subscribe(NetworkGUI.RenderGUI);
            MelonEvents.OnUpdate.Subscribe(Update);
            NetworkManager.serverUpdate += ServerManager.Update;
            NetworkManager.clientUpdate += ClientManager.Update;
            ServerManager.Start();
            ClientManager.Start();
        }

        private void OnSceneCached()
        {
            errors = GameObject.FindObjectsByType<Error>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            servers = GameObject.FindObjectsByType<Server>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
        }

        private void Update()
        {
            NetworkManager.Update();
        }

        public static GameObject GetNetPlayerPrefab()
        {
            var instance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            instance.AddComponent<Player>().enabled = false;
            instance.AddComponent<Rigidbody>().isKinematic = true;
            instance.tag = "Player";
            return instance;
        }
    }
}