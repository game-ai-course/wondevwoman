using System.Diagnostics;

namespace CG
{
    public class Countdown
    {
        private readonly Stopwatch stopwatch;
        private readonly int timeAvailableMs;

        public Countdown(int ms)
        {
            stopwatch = Stopwatch.StartNew();
            timeAvailableMs = ms;
        }

        public bool IsFinished => TimeLeftMs <= 0;

        public long TimeLeftMs => timeAvailableMs - stopwatch.ElapsedMilliseconds;
        public long ElapsedMs => stopwatch.ElapsedMilliseconds;

        public static implicit operator Countdown(int milliseconds)
        {
            return new Countdown(milliseconds);
        }
    }
}