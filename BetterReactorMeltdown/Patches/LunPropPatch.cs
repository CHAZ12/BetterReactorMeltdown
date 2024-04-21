using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using System;
using System.Runtime.Remoting.Messaging;
using System.Collections;
using BetterReactorMeltdownTimer;
using System.Xml.Serialization;
namespace BetterReactorMeltdown.Patches
{
    [HarmonyPatch(typeof(LungProp))]
    internal class LungPropPatch
    {
        [HarmonyPatch("EquipItem")]
        [HarmonyPrefix]// afterwards
        internal static void BegnMeldownEvent(LungProp __instance, ref bool ___isLungDocked)
        {

            if (ModBase._LungRemoved || !___isLungDocked) { return; }
            ModBase._jumpForce = 20f;
            ModBase._LungRemoved = true;
            HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
            ModBase.mls.LogInfo((object)string.Format("Lung apparatice was grabbed. Is owner: {0}", (object)__instance.IsOwner));
            ModBase.mls.LogInfo("REACTOR MELDOWN INITIATED");
            HUDManager.Instance.AddTextToChatOnServer("<color=#FF0000>REACTOR MELTDOWN STARTED!Main Exit Has been Locked");
            HUDManager.Instance.DisplayTip("<color=#FF0000>REACTOR MELTDOWN STARTED!", "Gravitational and Electromagnetic Interference Detected \n\nMain Exit Has been Locked", false, false, "LC_Tip1");
            //GetPoweredLights();// Set lights to red
        
        }

        [HarmonyPatch(typeof(PlayerControllerB))]
        [HarmonyPatch("Update")]
        [HarmonyPostfix]// afterwards
        internal static void SetPlayerMeltdownJumpForce(PlayerControllerB __instance, ref bool ___isFallingNoJump, ref bool ___isFallingFromJump)
        {
            if (!ModBase._LungRemoved || __instance.isPlayerDead)
            {
                return;
            }
            __instance.jumpForce = ModBase._jumpForce; //set the jump force to my value

            if (!ModBase._PlayerTouchedGround) // Player is on ground and use default values
            {
                ModBase._fallValue = __instance.fallValue;
            }
            if (___isFallingNoJump)
            {
                if (__instance.sprintMeter >= 5)
                {
                    ModBase._PlayerTouchedGround = true; // Set so Default values aren't triggered
                    __instance.fallValue = -2; // Decrease player fall time
                    __instance.fallValueUncapped = ModBase._fallValueUncapped; //Make sure player does not die
                    return;
                }
            }

            if (___isFallingFromJump && !__instance.isClimbingLadder)
            {
                ModBase._PlayerTouchedGround = true;
                if (JumpTimerComplete)
                {
                    __instance.fallValue = -2;
                    __instance.fallValueUncapped = ModBase._fallValueUncapped;
                }
                JumpTimer();
            }

            if (MeltdownTextTimerComplete)
            {
                //ModBase.mls.LogInfo("MELTDOWN ALARM TEXT and Lights");
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                //HUDManager.Instance.alarmHornEffect.SetTrigger("triggerAlarm"); //Alert text
                HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.radiationWarningAudio, 1f);
                //for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++)
                {
                    RoundManager.Instance.PowerSwitchOffClientRpc();
                    //RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", true);
                }
                MeltdownTextDelayTimer = 8.0f;
                MeltdownTextTimerComplete = false;
            }

            if (MeltdownSoundTimerComplete)
            {
                //ModBase.mls.LogInfo("MELTDOWN ALARM Sound");
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
                StartOfRound.Instance.shipDoorAudioSource.PlayOneShot(StartOfRound.Instance.alarmSFX);
                MeltdownSoundDelayTimer = 30.0f;
                MeltdownSoundTimerComplete = false;
            }
            UpdateMeltdownTimers();
        }

        //Timer Varaibles
        private static float JumpDelayTimer = 0.25f;
        private static bool JumpTimerComplete = false;

        //Aert text popup
        private static float MeltdownTextDelayTimer = 10.0f;
        private static bool MeltdownTextTimerComplete = false;

        private static float MeltdownLightsDelayTimer = 15.0f;

        //Alert text sound
        private static float MeltdownSoundDelayTimer = 30.0f;
        private static bool MeltdownSoundTimerComplete = false;

        [HarmonyPatch(typeof(PlayerControllerB))]
        [HarmonyPatch("PlayerHitGroundEffects")]
        [HarmonyPostfix]// afterwards
        internal static void ResetPlayerJumpForce(PlayerControllerB __instance)
        {
            ModBase._PlayerTouchedGround = false;// when player touches ground when in air or falling from jump
            JumpDelayTimer = 0.25f; //.5 is best value 3 was other
            JumpTimerComplete = false;
        }
        internal static void JumpTimer()
        {
            JumpDelayTimer -= Time.deltaTime;
            // ModBase.mls.LogInfo("DELAYTIMER: " + delayTimer);
            if (JumpDelayTimer <= 0)
            {
                JumpTimerComplete = true;
            }
        }
        internal static void UpdateMeltdownTimers()
        {
            MeltdownTextDelayTimer -= Time.deltaTime;
            if (MeltdownTextDelayTimer <= 0)
            {
                MeltdownTextTimerComplete = true;
            }

            MeltdownSoundDelayTimer -= Time.deltaTime;
            if (MeltdownSoundDelayTimer <= 0)
            {
                MeltdownSoundTimerComplete = true;
            }

            MeltdownLightsDelayTimer -= Time.deltaTime;
            if (MeltdownLightsDelayTimer <= 0)
            {
                PowerOffLights();
            }
        }
        internal static void CloseGarageDoor()
        {
            ModBase.mls.LogInfo("Trying to Close Garage Door");
            if (RoundManager.Instance == null)
            {
                ModBase.mls.LogWarning("RoundManager.Instance is null!");
                return;
            }
            if (RoundManager.Instance.currentLevel.levelID != 0)
            {
                ModBase.mls.LogWarning("You must be in Experimentation to use this command!");
                return;
            }
            InteractTrigger[] Triggers = UnityEngine.Object.FindObjectsOfType<InteractTrigger>();
            float delay = 0;
            foreach (InteractTrigger trigger in Triggers)
            {
                if (trigger.name == "Cube" && trigger.transform.parent.name == "Cutscenes")
                {
                    trigger.randomChancePercentage = 100;
                    for (int i = 0; i < 5; i++)
                    {
                        ReactorTimerManager.Instance.StartCoroutine(ReactorTimerManager.Instance.GararageDoorTrigger(trigger, delay));
                        delay += 1.5f;
                    }
                    return;
                }
            }
        }
        internal static void GetPoweredLights()
        {
            for (int i = 0; i < RoundManager.Instance.allPoweredLights.Count; i++)
            {
                RoundManager.Instance.allPoweredLights[i].color = Color.red;
            }
            //GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("PoweredLight");
            //if (gameObjectsWithTag == null)
            //    return;
            //for (int index = 0; index < gameObjectsWithTag.Length; ++index)
            //{
            //    Light componentInChildren = gameObjectsWithTag[index].GetComponentInChildren<Light>();
            //    if (!(componentInChildren == null))
            //    {
            //        componentInChildren.color = Color.red;
            //    }
            //}

        }
        internal static void PowerOffLights()
        {
            {
                RoundManager.Instance.PowerSwitchOnClientRpc();
                MeltdownLightsDelayTimer = 15.0f;
            }
        }

    }
}
