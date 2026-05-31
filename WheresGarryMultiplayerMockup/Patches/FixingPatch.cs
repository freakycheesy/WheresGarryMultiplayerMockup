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
        [HarmonyPatch(typeof(Error), "Fixed"), HarmonyPrefix]
        public static void FixedPatch(Error __instance)
        {
            if (!NetworkManager.isRunning) return;
            FixMessage message = new FixMessage()
            {
                id = Array.IndexOf(Core.errors, __instance),
            };
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixError);
            outgoing.Add(message);
            NetworkManager.client.Send(outgoing);
        }

        [HarmonyPatch(typeof(Server), "OnTriggerEnter"), HarmonyPrefix]
        public static void FixedPatch(Server __instance, Collider collision)
        {
            if (!NetworkManager.isRunning) return;
            FixMessage message = new FixMessage()
            {
                id = Array.IndexOf(Core.servers, __instance),
            };
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixServer);
            outgoing.Add(message);
            NetworkManager.client.Send(outgoing);
        }
    }
}
