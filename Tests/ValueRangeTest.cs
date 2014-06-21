namespace CSPS {
	namespace Tests {
		class ValueRangeTest: Test {
			public override void Run() {
				ValueRange singleton = new ValueRange() {
					MinUnbounded = false,
					MaxUnbounded = false,
					Minimum = 5,
					Maximum = 6
				};
				Assert(singleton.IsSingleton);
				Assert(5 == singleton.Singleton);

				Assert(!singleton.Contains(new Value(4)));
				Assert(singleton.Contains(new Value(5)));
				Assert(!singleton.Contains(new Value(6)));

				singleton = new ValueRange(5);
				Assert(singleton.IsSingleton);
				Assert(5 == singleton.Singleton);

				Assert(!singleton.Contains(new Value(4)));
				Assert(singleton.Contains(new Value(5)));
				Assert(!singleton.Contains(new Value(6)));
			}
		}
	}
}
