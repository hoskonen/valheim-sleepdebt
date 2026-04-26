using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SleepDebt
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class SleepDebtPlugin : BaseUnityPlugin
    {
        private const string PluginGuid = "petri.valheim.sleepdebt";
        private const string PluginName = "Sleep Debt";
        private const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log;

        private readonly Harmony _harmony = new Harmony(PluginGuid);

        private void Awake()
        {
            Log = Logger;

            _harmony.PatchAll();

            Log.LogInfo("[SleepDebt] Plugin loaded");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}