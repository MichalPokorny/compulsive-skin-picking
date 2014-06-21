using System;

namespace CSPS {
	public class Program {
		public static void Main(string[] args) {
			new Tests.ValueRangeTest().Run();
			new Tests.ThreeColorsInGraph().Run();
			new Tests.SendMoreMoney().Run();
		}
	}
}
