using BepInEx;
using BepInEx.Logging;
using BetterReactorMeltdown.Patches;
using BetterReactorMeltdownTimer;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace BetterReactorMeltdown
{
    [BepInPlugin(modGUID, modName, modVersion)] //tell class to call
    public class ModBase : BaseUnityPlugin // ModBase is the name refence for this class 
    {
        private const string modGUID = "Killers.BetterReactorMeltdown";
        private const string modName = "Killers Better Reactor Meldown"; 
        private const string modVersion = "1.0.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static ModBase Instance; // This class
        internal static ManualLogSource mls;

        //Inital Values//
        public static bool _LungRemoved = false; //Aparatus removed?
        public static bool ModFirstMessage = false; // Was Mod Introduced

        //Player controller
        public static bool _PlayerTouchedGround = false;
        public static bool _isFallingFromJump;
        public static float _jumpForce = 20f;
        public static float _fallValue; // the speed at which you fall higher negative = faster fall
        public static float _fallValueUncapped = -1; //Increases when in air, if reach - 20 you die instantly
        public static bool _nightVision = false;
        public static AnimatedObjectTrigger animatedObjectTrigger;
        internal static PlayerControllerB playerControllerB;
        internal ReactorTimerManager reactorTimerManager;

        void Awake() // Mod get loated
        {
            if (Instance == null) { ModBase.Instance = this; }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("Killers.Better.Reactor mod has awaken");
            this.harmony.PatchAll(typeof(SteamValveHazardPatch));
            this.harmony.PatchAll(typeof(LungPropPatch));
            this.harmony.PatchAll(typeof(HUDManagerPatch));
            this.harmony.PatchAll(typeof(StartOfRoundPatch));
            this.harmony.PatchAll(typeof(EntranceTeleportPatch));
            Task.Delay(3000).ContinueWith((Action<Task>)(p => this.CreateMenu()));
        }

        private void CreateMenu()
        {
            GameObject target3 = new GameObject("ReactorTimerManager");
            DontDestroyOnLoad(target3);
            target3.hideFlags = HideFlags.HideAndDontSave;
            target3.AddComponent<ReactorTimerManager>();
            this.reactorTimerManager = (ReactorTimerManager)target3.GetComponent("ReactorTimerManager");
        }
    }
}
