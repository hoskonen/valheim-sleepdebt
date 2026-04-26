using UnityEngine;

namespace SleepDebt.Core
{
    public static class SleepDebtSystem
    {
        private static float _restCredit = 100f;
        private const float IdealSleepHours = 8f;

        public static void OnSleepCompleted(double sleptClockHours)
        {
            float newRestCredit = (float)(sleptClockHours / IdealSleepHours) * 100f;
            newRestCredit = Mathf.Clamp(newRestCredit, 0f, 100f);

            var quality = GetSleepQuality(sleptClockHours);

            _restCredit = newRestCredit;

            SleepDebtPlugin.Log.LogInfo(
                $"[SleepDebt] System → hours={sleptClockHours:F2}, " +
                $"rest={_restCredit:F1}, quality={quality}"
            );
        }

        private static SleepQuality GetSleepQuality(double hours)
        {
            if (hours >= 8.0) return SleepQuality.WellRested;
            if (hours >= 6.0) return SleepQuality.Rested;
            if (hours >= 4.0) return SleepQuality.LightSleep;
            if (hours >= 2.0) return SleepQuality.PoorSleep;
            return SleepQuality.BarelySlept;
        }
    }
}