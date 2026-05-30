using HarmonyLib;
using Riptide;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WheresGarryMultiplayerMockup.Network;

namespace WheresGarryMultiplayerMockup.Patches
{
    internal class FixingPatch
    {
        public static List<Error> errors = new List<Error>();
        [HarmonyPatch(typeof(Error), "Start"), HarmonyPostfix]
        public static void StartPatch(Error __instance)
        {
            errors.Add(__instance);
        }
        [HarmonyPatch(typeof(Error), "Fixed"), HarmonyPostfix]
        public static void FixedPatch(Error __instance)
        {
            if (!NetworkManager.isRunning) return;
            FixMessage message = new FixMessage()
            {
                id = errors.IndexOf(__instance),
            };
            Message outgoing = Message.Create(MessageSendMode.Reliable, Messages.FixError);
            outgoing.Add(message);
            NetworkManager.client.Send(outgoing);
        }
    }
}
