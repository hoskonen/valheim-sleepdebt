using BepInEx.Configuration;
using HarmonyLib;
using SleepDebt.Core;

namespace SleepDebt.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.SetSleeping))]
    public static class PlayerSetSleepingPatch
    {
        private static float _sleepStartFraction = -1f;
        private static int _sleepStartDay = -1;
        private static double _sleepStartTime = -1.0;

        private static void Postfix(Player __instance, bool sleep)
        {
            if (!SleepDebtPlugin.EnableMod.Value)
                return;

            if (__instance != Player.m_localPlayer)
                return;

            double currentTime = ZNet.instance.GetTimeSeconds();
            float dayFraction = EnvMan.instance.GetDayFraction();
            int day = EnvMan.instance.GetDay(currentTime);

            if (sleep)
            {
                _sleepStartTime = currentTime;
                _sleepStartFraction = dayFraction;
                _sleepStartDay = day;

                if (SleepDebtPlugin.DebugLogging.Value)
                {
                    SleepDebtPlugin.Log.LogInfo(
                        $"[SleepDebt] Sleep started. " +
                        $"time={currentTime:F1}, day={day}, fraction={dayFraction:F3}"
                    );
                }

                return;
            }

            if (_sleepStartTime == -1)
            {
                if (SleepDebtPlugin.DebugLogging.Value)
                {
                    SleepDebtPlugin.Log.LogWarning(
                        "[SleepDebt] Wake-up detected, but sleep start time was missing."
                    );

                    return;
                }
            }

            double sleptClockHours = day == _sleepStartDay
            ? (dayFraction - _sleepStartFraction) * 24.0
            : ((day - _sleepStartDay) + dayFraction - _sleepStartFraction) * 24.0;

            try
            {
                SleepDebtSystem.OnSleepCompleted(sleptClockHours, day, dayFraction);
            }
            finally
            {
                ResetSleepState();
            }
        }

        private static void ResetSleepState()
        {
            _sleepStartTime = -1.0;
            _sleepStartFraction = -1f;
            _sleepStartDay = -1;
        }
    }
}