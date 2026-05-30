using HarmonyLib;
using Riptide;
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
            NetworkManager.client.Send(Message.Create(MessageSendMode.Reliable, Messages.Died));
            return false;
        }
    }
}
