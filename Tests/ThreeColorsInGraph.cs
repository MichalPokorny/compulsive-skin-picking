namespace CSPS {
	namespace Tests {
		public class ThreeColorsInGraph: Test {
			public override void Run() {
				Problem problem = new Problem();

				// 6 integers, 0 <= variables[i] < 3
				Variable[] v = problem.Variables.AddIntegers(6, 0, 3, x => ((char)('A' + x)).ToString());

				// 0--1--2
				//  \ |\ |\
				//   \| \| \
				//    3--4--5

				var edges = new int[][] {
					new [] { 0, 1 },
					new [] { 1, 2 },
					new [] { 0, 3 },
					new [] { 1, 3 },
					new [] { 1, 4 },
					new [] { 2, 4 },
					new [] { 2, 5 },
					new [] { 3, 4 },
					new [] { 4, 5 }
				};
				foreach (var pair in edges) {
					problem.Constrains.Add(new NotEqualConstrain(v[pair[0]], v[pair[1]]));
				}

				Solver solver = new Solver();
				IVariableAssignment result;

				Assert(solver.Solve(problem, out result));
				foreach (var pair in edges) {
					Assert(!Value.Equal(result.GetAssignedValue(v[pair[0]]), result.GetAssignedValue(v[pair[1]])));
				}
			}
		}
	}
}
