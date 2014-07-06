using System;
using System.Linq;

namespace CompulsiveSkinPicking {
	namespace Tests {
		class NQueens: Test {
			private int N;

			public NQueens(int N) {
				this.N = N;
			}

			public override void Run() {
				Console.WriteLine("Testing {0}-queens...", N);
				var problem = new Problem();
				Variable[] queensX = problem.Variables.AddIntegers(N, 1, N + 1, (x) => string.Format("Queen{0}", x + 1));
				problem.Constrains.Add(Constrain.AllDifferent(queensX));

				Variable[] differenceVariables = new Variable[N];
				{
					for (int i = 0; i < N; i++) {
						differenceVariables[i] = (queensX[i] - i).Build(problem);
					}
					problem.Constrains.Add(Constrain.AllDifferent(differenceVariables));
				}
				{
					for (int i = 0; i < N; i++) {
						differenceVariables[i] = (queensX[i] + i).Build(problem);
					}
					problem.Constrains.Add(Constrain.AllDifferent(differenceVariables));
				}

				Solver solver = new Solver();
				IVariableAssignment result;

				Stopwatch.Instrument(() => {
					Assert(solver.SolveParallel(problem, out result));
					for (int i = 0; i < N; i++) {
						for (int j = 1; j < N + 1; j++) {
							if (result[queensX[i]].Value == j) {
								Console.Write("Q");
							} else {
								Console.Write(".");
							}
						}
						Console.WriteLine();
					}
				}, (span) => {
					Console.WriteLine("Solved {1}-queens in {0} seconds", span.TotalSeconds, N);
				});
			}
		}
	}
}
