using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BetterReactorMeltdown.Patches
{
    [HarmonyPatch(typeof(SteamValveHazard))]
    internal class SteamValveHazardPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]// afterwards

        public static void SetAllValvesCrackTime(SteamValveHazard __instance, ref float ___valveCrackTime, ref bool ___valveHasBurst, ref float ___valveBurstTime)
        {

            if (StartOfRound.Instance.allPlayersDead || !GameNetworkManager.Instance.gameHasStarted && !ModBase._LungRemoved)
            {

                SteamValveHazard[] valveObjects = UnityEngine.Object.FindObjectsOfType<SteamValveHazard>();

                try
                {
                    SteamValveHazard[] valveObjectsWithTag = valveObjects.Where(valve => valve.gameObject.tag == "MOD").ToArray();

                    foreach (SteamValveHazard Valve in valveObjectsWithTag)
                    {
                        ___valveCrackTime = -999999;
                    }
                }

                catch (Exception e) { ModBase.mls.LogInfo("NOT FOUND" + e.Message); }

            }
        }
    }
   
}