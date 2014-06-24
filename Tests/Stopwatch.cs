using System;

namespace CSPS {
	namespace Tests {
		static class Stopwatch {
			public static void Instrument(Action action, Action<TimeSpan> timeSpent) {
				DateTime start = DateTime.Now;
				action();
				DateTime end = DateTime.Now;
				timeSpent(end - start);
			}
		}
	}
}
