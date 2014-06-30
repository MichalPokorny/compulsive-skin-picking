using System;

namespace CompulsiveSkinPicking {
	static class Debug {
		public static bool doDebug = false;

		public static void Write(string fmt, params object[] args) {
			if (doDebug) {
				Console.Write(string.Format(fmt, args));
			}
		}

		public static void WriteLine(string fmt, params object[] args) {
			if (doDebug) {
				Console.WriteLine(string.Format(fmt, args));
			}
		}
	}
}
