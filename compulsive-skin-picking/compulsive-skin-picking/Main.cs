using System;
using System.Collections.Generic;
using CompulsiveSkinPicking.Tests;

namespace CompulsiveSkinPicking {
	public class Program {
		public static void Main(string[] args) {
			var tests = new KeyValuePair<string, Test>[] {
				new KeyValuePair<string, Test>("ValueRange", new ValueRangeTest()),
				new KeyValuePair<string, Test>("ThreeColorsInGraph", new ThreeColorsInGraph()),
				new KeyValuePair<string, Test>("TwoTwoFour", new TwoTwoFour()),
				new KeyValuePair<string, Test>("SAT", new SAT()),
				new KeyValuePair<string, Test>("SendMoreMoney", new SendMoreMoney()),
				new KeyValuePair<string, Test>("SendMoreMoneyMaximize", new SendMoreMoneyMaximize()),
				new KeyValuePair<string, Test>("LargeAlgebrogram", new LargeAlgebrogram()),
				new KeyValuePair<string, Test>("NQueens[8]", new NQueens(8)),
				new KeyValuePair<string, Test>("NQueens[15]", new NQueens(15)),
				new KeyValuePair<string, Test>("NQueens[30]", new NQueens(30))
			};
			do {
				for (int j = 0; j < tests.Length; j++) {
					Console.WriteLine("[{0}]: {1}", j, tests[j].Key);
				}
				Console.WriteLine("Which test should I run? (enter -1 to quit)");
				int i = 0;
				if (!int.TryParse(Console.ReadLine(), out i) || i < -1 || i >= tests.Length) {
					Console.WriteLine("Sorry, I don't understand.");
					continue;
				}
				if (i == -1) {
					Console.WriteLine("Goodbye.");
					break;
				}
				Console.WriteLine("Running {0}", tests[i].Key);
				tests[i].Value.Run();
			} while (true);
		}
	}
}
