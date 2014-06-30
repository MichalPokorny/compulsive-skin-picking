namespace CompulsiveSkinPicking {
	namespace Tests {
		class ValueRangeTest: Test {
			public override void Run() {
				ValueRange singleton = new ValueRange() {
					Minimum = 5,
					Maximum = 6
				};
				Assert(singleton.IsSingleton);
				Assert(5 == singleton.Singleton);

				Assert(!singleton.Contains(4));
				Assert(singleton.Contains(5));
				Assert(!singleton.Contains(6));

				singleton = new ValueRange(5);
				Assert(singleton.IsSingleton);
				Assert(5 == singleton.Singleton);

				Assert(!singleton.Contains(4));
				Assert(singleton.Contains(5));
				Assert(!singleton.Contains(6));
			}
		}
	}
}
