using UnityEngine;

namespace SleepDebt.Core
{
    public static class SleepDebtSystem
    {
        private static float _restCredit = 100f;
        private static bool _hasActiveSleepResult;
        private static bool _tiredWouldBeApplied;

        private static float _wakeFraction;
        private static int _wakeDay;
        private static float _hoursBeforeTired;

        public static void OnSleepCompleted(double sleptClockHours, int wakeDay, float wakeFraction)
        {
            float idealSleepHours = SleepDebtPlugin.IdealSleepHours.Value;

            if (idealSleepHours <= 0f)
                idealSleepHours = 8f;

            float newRestCredit = (float)(sleptClockHours / idealSleepHours) * 100f;
            newRestCredit = Mathf.Clamp(newRestCredit, 0f, 100f);

            var quality = GetSleepQuality(sleptClockHours);

            _restCredit = newRestCredit;

            if (SleepDebtPlugin.DebugLogging.Value)
            {
                SleepDebtPlugin.Log.LogInfo(
                    $"[SleepDebt] System → hours={sleptClockHours:F2}, " +
                    $"rest={_restCredit:F1}, quality={quality}"
                );
            }

            float fullSleepAwakeHours = SleepDebtPlugin.FullSleepAwakeHours.Value;
            if (fullSleepAwakeHours <= 0f)
                fullSleepAwakeHours = 16f;

            float sleepRatio = Mathf.Clamp01((float)(sleptClockHours / idealSleepHours));

            _hoursBeforeTired = sleepRatio * fullSleepAwakeHours;
            _wakeDay = wakeDay;
            _wakeFraction = wakeFraction;
            _hasActiveSleepResult = true;
            _tiredWouldBeApplied = false;

            float predictedTiredFraction = _wakeFraction + (_hoursBeforeTired / 24f);
            int predictedTiredDay = _wakeDay;

            while (predictedTiredFraction >= 1f)
            {
                predictedTiredFraction -= 1f;
                predictedTiredDay++;
            }

            SleepDebtPlugin.Log.LogWarning(
                $"🟡 [SleepDebt/FATIGUE PLAN] Tired would start after {_hoursBeforeTired:F2} awake hours " +
                $"at day={predictedTiredDay}, fraction={predictedTiredFraction:F3}."
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

        public static void UpdateFatigueCheck()
        {
            if (!_hasActiveSleepResult || _tiredWouldBeApplied)
                return;

            if (Player.m_localPlayer == null || ZNet.instance == null || EnvMan.instance == null)
                return;

            double currentTime = ZNet.instance.GetTimeSeconds();
            int currentDay = EnvMan.instance.GetDay(currentTime);
            float currentFraction = EnvMan.instance.GetDayFraction();

            double awakeHours = currentDay == _wakeDay
                ? (currentFraction - _wakeFraction) * 24.0
                : ((currentDay - _wakeDay) + currentFraction - _wakeFraction) * 24.0;

            if (awakeHours < 0)
                awakeHours = 0;

            if (awakeHours >= _hoursBeforeTired)
            {
                _tiredWouldBeApplied = true;

                SleepDebtPlugin.Log.LogWarning(
                    $"🟡 [SleepDebt/DEBUFF WOULD BE ADDED] Tired reached. " +
                    $"awakeHours={awakeHours:F2}, threshold={_hoursBeforeTired:F2}, " +
                    $"day={currentDay}, fraction={currentFraction:F3}"
                );
            }
        }
    }
}