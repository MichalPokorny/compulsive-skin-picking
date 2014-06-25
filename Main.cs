using System;

namespace CompulsiveSkinPicking {
	public class Program {
		public static void Main(string[] args) {
			new Tests.ValueRangeTest().Run();
			new Tests.ThreeColorsInGraph().Run();
			new Tests.TwoTwoFour().Run();
			new Tests.SendMoreMoney().Run();
			new Tests.SAT().Run();
		}
	}
}
