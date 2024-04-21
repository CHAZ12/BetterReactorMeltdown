
using BetterReactorMeltdown;
using System.Collections;
using UnityEngine;

namespace BetterReactorMeltdownTimer
{
    internal class ReactorTimerManager : MonoBehaviour
    {
        internal static ReactorTimerManager Instance;
        public void Awake()
        {
            if ((UnityEngine.Object)ReactorTimerManager.Instance == (UnityEngine.Object)null)
                ReactorTimerManager.Instance = this;
        }
        public IEnumerator FlashLights(float timeLeftUntilMeltdown)
        {
            for (int i = 0; i < RoundManager.Instance.allPoweredLightsAnimators.Count; i++)
            {
                RoundManager.Instance.allPoweredLightsAnimators[i].SetBool("on", true);
            }
            yield return (object)new WaitForSeconds(2f);
            for (int j = 0; j < RoundManager.Instance.allPoweredLightsAnimators.Count; j++)
            {
                RoundManager.Instance.allPoweredLightsAnimators[j].SetBool("on", false);
            }
            yield return (object)new WaitForSeconds(5f);
        }

        public IEnumerator GararageDoorTrigger(InteractTrigger trigger, float delay)
        {
            ModBase.mls.LogInfo("Closing garage door w/ delay:" + delay);
            yield return new WaitForSeconds(delay);
            trigger.Interact(ModBase.playerControllerB.transform);

        }
    }
}
