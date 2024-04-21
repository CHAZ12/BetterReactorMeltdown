using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace BetterReactorMeltdown.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [ClientRpc]
        [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
        [HarmonyPostfix]// afterwards
        public static void UserDisconnected(StartOfRound __instance)
        {
            if (__instance.IsServer)
            {
                Resetall();
                return;
            }
            ModBase.mls.LogInfo("Client Has Disconntected");
            Resetall();
            ModBase.mls.LogInfo("FIRST MESSAGE RESET, NO LONGER IN A GAME");
        }

        [HarmonyPatch(typeof(StartMatchLever))]
        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]// afterwards
        [ServerRpc(RequireOwnership = false)]
        public static void ResetModVariables(StartMatchLever __instance)
        {
            if (__instance.playersManager.travellingToNewLevel || __instance.playersManager.inShipPhase || StartOfRound.Instance.currentLevelID == 3)
                return;
            Resetall();
            SetSpawnText();
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        private static void GameEnded()
        {
            Resetall();
        }


        [HarmonyPatch(typeof(AnimatedObjectTrigger), "Start")]
        [HarmonyPostfix]
        private static void AnimatedObjectTriggerPatch(AnimatedObjectTrigger __instance)
        {
            ModBase.animatedObjectTrigger = __instance;
        }

        [ServerRpc(RequireOwnership = false)]
        private static void SetSpawnText()
        {
            if (!ModBase.ModFirstMessage && RoundManager.Instance.currentLevel.levelID != 3)
            {
                ModBase.mls.LogInfo("ADDED MOD TEXT ON SPAWN");
                string nameOfUserWhoTyped = "BETTER REACTOR";
                string chatMessage = "BETTER REACTOR HAS LOADED";
                string str;
                str = "<color=#FF0000>" + nameOfUserWhoTyped + "</color>: <color=#FFFF00>'" + chatMessage + "'</color>";
                HUDManager.Instance.ChatMessageHistory.Add(str);
                HUDManager.Instance.chatText.text = "";
                for (int index = 0; index < HUDManager.Instance.ChatMessageHistory.Count; ++index)
                {
                    TextMeshProUGUI chatText = HUDManager.Instance.chatText;
                    chatText.text = chatText.text + "\n" + HUDManager.Instance.ChatMessageHistory[index];
                }
            }

        }

        [ServerRpc(RequireOwnership = false)]
        private static void Resetall()
        {
            ModBase.mls.LogInfo("RESETTTING VARIABLES");
            StartOfRound.Instance.shipAnimatorObject.gameObject.GetComponent<Animator>().SetBool("AlarmRinging", false);
            ModBase._LungRemoved = false;
            ModBase.ModFirstMessage = false;
            ModBase._nightVision = false;
            ModBase._jumpForce = 13;
            GameNetworkManager.Instance.localPlayerController.jumpForce = 13;

        }

        [HarmonyPatch(typeof(RoundManager))]
        [HarmonyPatch("BeginEnemySpawning")]
        [HarmonyPostfix]// afterwards
        [ServerRpc(RequireOwnership = false)]
        public static void CheckForLung(StartMatchLever __instance)
        {
            GameObject lungApparatus = GameObject.Find("LungApparatus(Clone)");

            if (lungApparatus == null)
            {
                ModBase.mls.LogWarning("NO LUNG FOUND FOUND IN LEVEL ");
                ModBase._LungRemoved = false;
            }
            ModBase.ModFirstMessage = true;
        }
        [HarmonyPatch(typeof(PlayerControllerB))]
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPatch(PlayerControllerB __instance)
        {
            if (!(__instance == null))
                return;
            ModBase.playerControllerB = __instance;

        }
    }
}
