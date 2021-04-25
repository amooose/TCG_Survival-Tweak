using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Reflection;
using System.Collections;
using HutongGames.PlayMaker;

namespace TCG_SurvivalTweak
{
    [BepInPlugin("an0nymooose_TCG_SurvivalTweak", "TCG_SurvivalTweak", "1.0.0.0")]
    public class Main : BaseUnityPlugin
    {
        public const string MODNAME = "TCG_SurvivalTweak";
        public const string AUTHOR = "an0nymooose";
        public const string GUID = "an0nymooose_TCG_SurvivalTweak";
        public const string VERSION = "1.0.0.0";
        public const float DEGRADE_NORMAL = .06f;
        public const float TIME_NORMAL = .05f;
        internal readonly ManualLogSource log;
        internal readonly Harmony harmony;
        internal readonly Assembly assembly;
        private static ConfigEntry<bool> configNoCurfew;
        private static ConfigEntry<float> configHealthDegrade;
        private static ConfigEntry<float> configEnergyDegrade;
        private static ConfigEntry<float> configTimeRate;
        public Main()
        {
            this.log = base.Logger;
            this.harmony = new Harmony("an0nymooose_TCG_SurvivalTweak");
            this.assembly = Assembly.GetExecutingAssembly();
        }

        public void Start()
        {
            this.harmony.PatchAll(this.assembly);
            Debug.Log("TCG_SurvivalTweak Loaded!");
            configNoCurfew = Config.Bind("General.Toggles",
                                            "No Curfew",
                                            true,
                                            "Disables the curfew, return home whenever you want. (Time caps at midnight due to game code)");
            configHealthDegrade = Config.Bind("General",
                                            "Health Degradation",
                                            DEGRADE_NORMAL,
                                            "The rate at which your health decreases, 0.06 is the default (Lower = slower)");
            configEnergyDegrade = Config.Bind("General",
                                "Energy Degradation Max",
                                DEGRADE_NORMAL,
                                "The max rate at which your health decreases, 0.06 is the default max. Energy degrades from 0.005 to 0.06 depending on what you consume. (Lower = slower)");
            configTimeRate = Config.Bind("General",
                    "Time Passing Rate",
                    TIME_NORMAL,
                    "The rate at which time passes, 0.05 is the default (Lower = faster)");

        }

        // Hook onto Sky loading 
        // (I'm sure there's a better way, but lots of good hook 
        // potential methods are instead done within FSMs, this'll do fine.)
        [HarmonyPatch(typeof(EnviroSky), "Start")]
        private class InjectFMSEdits
        {
            private static bool Prefix(EnviroSky __instance)
            {
                __instance.StartCoroutine(injectTimer(__instance, 5));
                return true;
            }

            private static IEnumerator injectTimer(EnviroSky __instance, int secs)
            {
                yield return new WaitForSeconds((float)secs);
                if (GameObject.Find("TIMER CNTRL") != null)
                {
                    Debug.Log("[Tweak] Timer load detected, patching..");
                    PlayMakerFSM CurfewTimer = GameObject.Find("TIMER CNTRL").GetComponent<PlayMakerFSM>();
                    if (configNoCurfew.Value)
                    {
                        // Removes finite-state machine transition of "isPlayerHome"
                        CurfewTimer.Fsm.States[4].Transitions = new HutongGames.PlayMaker.FsmTransition[0];
                        GameObject.Find("Panel CURFEW DATA").SetActive(false);
                        GameObject.Find("CIRCLE_CURFEW_BLUE (1)").SetActive(false);
                    }
                    //Health
                    if (configHealthDegrade.Value != DEGRADE_NORMAL)
                    {
                        PlayMakerFSM HealthTimer = GameObject.Find("HEALTH CNTRL").GetComponent<PlayMakerFSM>();
                        HealthTimer.Fsm.Variables.FloatVariables[1].Name = "disableHealthUpdates";
                        HealthTimer.Fsm.Variables.FloatVariables[1].Value = configHealthDegrade.Value;
                    }
                    //Energy
                    if (configEnergyDegrade.Value != DEGRADE_NORMAL)
                    {
                        PlayMakerFSM EnergyTimer = GameObject.Find("ENERGY CNTRL").GetComponent<PlayMakerFSM>();
                        //Access float clamp via reflection
                        FsmStateAction clampState = EnergyTimer.Fsm.States[1].Actions[10];
                        //Force the float to stop changing by making the
                        FsmFloat clampMax = (FsmFloat)clampState.GetType().GetField("maxValue").GetValue(clampState);
                        //FsmFloat clampMin = (FsmFloat)clampState.GetType().GetField("minValue").GetValue(clampState);
                        clampMax.SafeAssign(configEnergyDegrade.Value);
                    }
                    if (configTimeRate.Value != TIME_NORMAL)
                    {
                        //Curfew Timer is also tied to ingame time.
                        CurfewTimer.Fsm.Variables.FloatVariables[0].Value = configTimeRate.Value;
                    }
                }
                //Timer not loaded yet, wait another 5 seconds.
                else
                {
                    __instance.StartCoroutine(injectTimer(__instance, 5));
                }
                yield break;
            }
        }
    }
}
