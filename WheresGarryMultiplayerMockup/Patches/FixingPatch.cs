using HarmonyLib;
using Riptide;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WheresGarryMultiplayerMockup.Network;

namespace WheresGarryMultiplayerMockup.Patches
{
    [HarmonyPatch]
    public static class FixingPatch
    {
        [HarmonyPatch(typeof(Error), "Start"), HarmonyPostfix]
        public static void StartPatch(Error __instance)
        {
            Core.errors.Add(__instance);
        }
        [HarmonyPatch(typeof(Error), "Fixed"), HarmonyPostfix]
        public static void FixedPatch(Error __instance)
        {
            if (!NetworkManager.isRunning) return;
            FixMessage message = new FixMessage()
            {
                id = Core.errors.IndexOf(__instance),
            };
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixError);
            outgoing.Add(message);
            NetworkManager.client.Send(outgoing);
        }

        [HarmonyPatch(typeof(Server), "Start"), HarmonyPostfix]
        public static void StartPatch(Server __instance)
        {
            Core.servers.Add(__instance);
        }
        [HarmonyPatch(typeof(Server), "OnTriggerEnter"), HarmonyPostfix]
        public static void FixedPatch(Server __instance, Collider collision)
        {
            if (!NetworkManager.isRunning) return;
            FixMessage message = new FixMessage()
            {
                id = Core.servers.IndexOf(__instance),
            };
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixServer);
            outgoing.Add(message);
            NetworkManager.client.Send(outgoing);
        }
    }
}
