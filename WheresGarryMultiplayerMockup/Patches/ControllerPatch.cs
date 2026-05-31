using HarmonyLib;
using MelonLoader;
using Riptide;
using UnityEngine;
using WheresGarryMultiplayerMockup.Logic;
using WheresGarryMultiplayerMockup.Network;

namespace WheresGarryMultiplayerMockup.Patches
{
    [HarmonyPatch]
    public static class ControllerPatch
    {
        [HarmonyPatch(typeof(Controller), "Start"), HarmonyPostfix]
        public static void StartPostfix(Controller __instance)
        {
            Core.controller = __instance;
            Core.onSceneCached?.Invoke();
        }
        [HarmonyPatch(typeof(Controller), "CacheSceneObjects"), HarmonyPostfix]
        public static void CacheSceneObjectsPostfix(Controller __instance)
        {
            Core.onSceneCached?.Invoke();
        }

        [HarmonyPatch(typeof(Controller), nameof(Controller.SendToResults)), HarmonyPrefix]
        public static bool Prefix(Controller __instance, int reason, int killer)
        {
            if (!NetworkManager.isRunning) return true;
            if (!ClientManager.isDead)
            {
                MelonLogger.Msg("Sending Dead Message");
                ClientManager.isDead = true;
                ClientManager.localPlayer.transform.position = Vector3.up * 500;
                ClientManager.localPlayer.gameObject.SetActive(false);
                NetworkManager.client.Send(Message.Create(MessageSendMode.Reliable, Messages.Died));
            }
            return false;
        }
    }
}
