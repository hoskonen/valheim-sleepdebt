using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SleepDebt
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class SleepDebtPlugin : BaseUnityPlugin
    {
        // Configuration Manager
        internal static ConfigEntry<bool> EnableMod;
        internal static ConfigEntry<bool> DebugLogging;
        internal static ConfigEntry<float> IdealSleepHours;
        internal static ConfigEntry<float> FullSleepAwakeHours;
        internal static ConfigEntry<float> TiredCheckIntervalSeconds;

        private const string PluginGuid = "petri.valheim.sleepdebt";
        private const string PluginName = "Sleep Debt";
        private const string PluginVersion = "0.1.0";

        private float _nextFatigueCheckTime = 0;

        internal static ManualLogSource Log;

        private readonly Harmony _harmony = new Harmony(PluginGuid);

        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind(
                "General",
                "EnableMod",
                true,
                "Enable or disable the Sleep Debt mod."
            );

            DebugLogging = Config.Bind(
                "General",
                "DebugLogging",
                true,
                "Enable debug logging for sleep tracking and fatigue calculations."
            );

            IdealSleepHours = Config.Bind(
                "Balance",
                "IdealSleepHours",
                8f,
                "Number of in-game clock hours considered a full sleep."
            );

            FullSleepAwakeHours = Config.Bind(
                "Balance",
                "FullSleepAwakeHours",
                16f,
                "How many in-game clock hours the player can stay awake before tiredness after ideal sleep."
            );

            TiredCheckIntervalSeconds = Config.Bind(
                "Debug",
                "TiredCheckIntervalSeconds",
                10f,
                "How often Sleep Debt checks whether tiredness would start."
            );

            _harmony.PatchAll();

            Log.LogInfo("[SleepDebt] Plugin loaded");
        }

        private void Update()
        {
            if (!EnableMod.Value)
                return;

            if (Time.time < _nextFatigueCheckTime)
                return;

            _nextFatigueCheckTime = Time.time + TiredCheckIntervalSeconds.Value;

            SleepDebt.Core.SleepDebtSystem.UpdateFatigueCheck();
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}