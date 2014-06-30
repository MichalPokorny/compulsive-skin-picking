using System;

namespace CompulsiveSkinPicking {
	namespace Tests {
		abstract class Test {
			public abstract void Run();
			protected void Assert(bool truth) {
				if (!truth) {
					throw new Exception("Assertion failed");
				}
			}
			/*
			protected void AssertEqual<T>(T x, T y) {
				Assert(x == y);
			}
			*/
		}
	}
}
