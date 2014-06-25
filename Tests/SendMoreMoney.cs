using System;
using System.Linq;

namespace CompulsiveSkinPicking {
	namespace Tests {
		class SendMoreMoney: Test {
			public override void Run() {
				Console.WriteLine("Testing SEND+MORE=MONEY...");
				Problem problem = new Problem();

				//   S E N D
				// + M O R E
				// =========
				// M O N E Y
				//
				// Variables: S, E, N, D, M, O, R, Y

				string[] names = "S E N D M O R Y".Split();
				Variable[] v = problem.Variables.AddIntegers(8, 0, 10, x => names[x]);

				Variable S = v[0], E = v[1], N = v[2], D = v[3], M = v[4], O = v[5], R = v[6], Y = v[7];

				problem.Constrains.Add(Constrain.AllDifferent(v));

				// problem.Constrains.Add(Constrain.Plus(N, D, M));
				// problem.Constrains.Add(Constrain.Plus(E, N, D));
				// problem.Constrains.Add(Constrain.Plus(Y, E, N));
				//
				Variable SEND = (S * 1000 + E * 100 + N * 10 + D).Build(problem);
				Variable MORE = (M * 1000 + O * 100 + R * 10 + E).Build(problem);
				Variable MONEY = (M * 10000 + O * 1000 + N * 100 + E * 10 + Y).Build(problem);
				problem.Constrains.Add(
					Constrain.Equal(
						(SEND + MORE).Build(problem),
						MONEY
					)
				);
				problem.Constrains.Add(Constrain.NotEqual(S, new AlgebraicExpression.ConstantNode(0).Build(problem)));
				problem.Constrains.Add(Constrain.NotEqual(M, new AlgebraicExpression.ConstantNode(0).Build(problem)));

				// TODO: dalsi test: maximalizace
				Solver solver = new Solver();
				IVariableAssignment result;

				// Sequential stupid with much copying: 127.29188 s
				// Parallel stupid with much copying: 150 s
				// Parallel stupid with limited depth: 17.45 s
				Stopwatch.Instrument(() => {
					Assert(solver.SolveParallel(problem, out result));
					// Assert(solver.SolveSerial(problem, out result));
					Console.WriteLine(string.Join(" ", v.Select(variable => string.Format("{0}={1}", variable.Identifier, result[variable].Value))));
					Console.WriteLine("{0}+{1}={2}", result[SEND].Value, result[MORE].Value, result[MONEY].Value);
				}, (span) => {
					Console.WriteLine("Solved SEND+MORE=MONEY in {0} seconds", span.TotalSeconds);
				});
			}
		}
	}
}
