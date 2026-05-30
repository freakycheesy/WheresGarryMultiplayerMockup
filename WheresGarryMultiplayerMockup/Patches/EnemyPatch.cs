using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using WheresGarryMultiplayerMockup.Network;

namespace WheresGarryMultiplayerMockup.Patches
{
    [HarmonyPatch()]
    public static class EnemyPatch
    {
        [HarmonyPatch(typeof(Enemy), "TryAttackPlayer"), HarmonyPrefix]
        public static bool TryAttackPlayer(Enemy __instance, Collider collision)
        {
            if (!NetworkManager.isRunning) return true;
            if (collision == Core.controller.player.GetComponent<Collider>()) return true;
            return false;
        }
    }
}
