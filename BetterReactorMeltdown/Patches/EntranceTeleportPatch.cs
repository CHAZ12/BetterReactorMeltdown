using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace BetterReactorMeltdown.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatch
    {
        internal static Transform exitPOS = null;
        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPrefix]
        public static void TeleportPlayerPrefix(bool ___isEntranceToBuilding, int ___entranceId, bool ___gotExitPoint, EntranceTeleport __instance)
        {

            //if (ModBase._LungRemoved) //&& !NetworkManager.Singleton.IsHost
            //{

            //    if (!___gotExitPoint)
            //    {
            //        ModBase.mls.LogInfo("NO EXIT POINT");
            //    }

            //    else if (___isEntranceToBuilding && ___entranceId == 0)
            //    {
            //        ForceTeleportPosition(__instance);
            //        HUDManager.Instance.DisplayTip("<color=#FF0000>REACTOR MELTDOWN STARTED 1!", "Access Denied: Facility Undergoing Meltdown Protocol.", false, false, "LC_Tip1");

            //       int playerObj = (int)GameNetworkManager.Instance.localPlayerController.playerClientId;
            //        __instance.playersManager.allPlayerScripts[playerObj].isInsideFactory = false;
            //        return false;
            //    }

            //    if (ModBase._LungRemoved && ___isEntranceToBuilding && ___entranceId != 0) //fire exit enter
            //    {
            //        ForceTeleportPosition(__instance);
            //        HUDManager.Instance.DisplayTip("<color=#FF0000>REACTOR MELTDOWN STARTED 2!", "Access Denied: Facility Undergoing Meltdown Protocol.", false, false, "LC_Tip1");
            //        int playerObj = (int)GameNetworkManager.Instance.localPlayerController.playerClientId;
            //        __instance.playersManager.allPlayerScripts[playerObj].isInsideFactory = true;
            //        return false;
            //    }
            //}
            //return true;
        }
        [HarmonyPatch("FindExitPoint")]
        [HarmonyPrefix]
        public static void FindExitPointPostfix(bool ___isEntranceToBuilding, int ___entranceId, ref bool __result, bool ___gotExitPoint, EntranceTeleport __instance)
        {
            //if (ModBase._LungRemoved)
            //{

            //    if (!___gotExitPoint)
            //    {
            //        ModBase.mls.LogInfo("NO EXIT POINT");
            //    }
            //    else if (___entranceId != 0 && ___isEntranceToBuilding)
            //    {
            //        //ForceTeleportPosition(__instance);
            //        HUDManager.Instance.DisplayTip("Access Denied", "Facility Undergoing Meltdown Protocol 1. Entry Prohibited.", false, false, "LC_Tip1");
            //        //int playerObj = (int)GameNetworkManager.Instance.localPlayerController.playerClientId;
            //        //__instance.playersManager.allPlayerScripts[playerObj].isInsideFactory = false;
            //        //__result = false;
            //        return false;
            //    }

            //    if (___entranceId == 0 && !___isEntranceToBuilding)
            //    {
            //       // ForceTeleportPosition(__instance);
            //        HUDManager.Instance.DisplayTip("Access Denied", "Facility Undergoing Meltdown Protocol 2. Entry Prohibited.", false, false, "LC_Tip1");
            //        //int playerObj = (int)GameNetworkManager.Instance.localPlayerController.playerClientId;
            //        //__instance.playersManager.allPlayerScripts[playerObj].isInsideFactory = true;
            //        //__result = false;
            //       // return false;
            //    }
            //}
            //return true;
        }

        [HarmonyPatch("TeleportPlayerServerRpc")]
        [HarmonyPrefix]
        public static bool TeleportPlayerServerRpc(int playerObj, ref Transform ___exitPoint)
        {
            Vector3 entrancePosition = RoundManager.FindMainEntrancePosition(true);
            for (int index1 = 0; index1 < StartOfRound.Instance.allPlayerScripts.Length; ++index1)
            {
                PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[index1];
                if (playerScript.isPlayerControlled && (int)playerScript.playerClientId == playerObj)
                {
                    //___exitPoint.position = entrancePosition;
                    if(exitPOS != null)
                        ___exitPoint = exitPOS;
                    //__instance.TeleportPlayerServerRpc((int)playerScript.playerClientId);
                    //playerScript.TeleportPlayer(entrancePosition, true);
                    return false;
                }
                else
                {
                    ModBase.mls.LogWarning("Player ID: " + playerScript.playerClientId + "Does not match: " + playerObj);
                }
            }
            return false;
        }


        [HarmonyPatch(typeof(PlayerControllerB))]
        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPrefix]
        public static void TeleportPlayerPatch(Vector3 pos, bool withRotation = false, float rot = 0f, bool allowInteractTrigger = false, bool enableController = true)
        {
            Vector3 entrancePosition = RoundManager.FindMainEntrancePosition(false);
            if (entrancePosition == null) { return; }
            for (int index1 = 0; index1 < StartOfRound.Instance.allPlayerScripts.Length; ++index1)
            {
                PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[index1];
                if (playerScript.isPlayerControlled)
                {
                    FindEntranceDoor(playerScript, entrancePosition);
                }
                
            }
        }

        public static void FindEntranceDoor(PlayerControllerB player, Vector3 entrancePos)
        {
            if (!ModBase._LungRemoved || player.isPlayerDead)
            {
                return;
            }

            float destroyDistance = 6f;
            foreach (EntranceTeleport teleport in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>())
            {
                ModBase.mls.LogInfo("Teleport found in range: " + teleport.name);
                if (Vector3.Distance(teleport.transform.position, player.transform.position) <= destroyDistance)
                {
                    if (teleport.name == "EntranceTeleportA(Clone)") //A: Outside Entrance
                    {
                        ModBase.mls.LogInfo(string.Format("Found player: {0} to keep inside", player.playerUsername.ToString()));

                        if (player.isPlayerControlled)
                        {
                            player.serverPlayerPosition = entrancePos;
                            ModBase.mls.LogWarning("Entrance POS: " + entrancePos);
                            ModBase.mls.LogWarning("Player POS: " + player.transform.position);
                            ModBase.mls.LogWarning("Exit POS: " + exitPOS.transform.position);
                            exitPOS = teleport.transform;
                            return;
                        }

                        else
                        {
                            ModBase.mls.LogWarning("Entrance Teleport not found: " + teleport.name);
                        }
                    }
                }
            }
            return;
        }
    }
}
