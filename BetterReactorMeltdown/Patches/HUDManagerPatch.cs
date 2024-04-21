using HarmonyLib;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using Unity.Netcode;
using System.Runtime.Remoting.Messaging;
using GameNetcodeStuff;

namespace BetterReactorMeltdown.Patches
{ 
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch(typeof(HUDManager))]
        [HarmonyPatch("CanPlayerScan")]
        [HarmonyPostfix]// afterwards
        public static void PlayerCanPing(ref bool __result)
        {
            if (ModBase._LungRemoved)
            {
                __result = false;
            }
        }

        [HarmonyPatch("MeetsScanNodeRequirements")]
        static bool MeetScanNodeRequirementsFix(ScanNodeProperties node, ref bool __result)
        {
            if (node is null) return true;

            __result = false;
            return false;
        }

    }
}
