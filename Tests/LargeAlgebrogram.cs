using System;
using System.Linq;

namespace CompulsiveSkinPicking {
	namespace Tests {
		class LargeAlgebrogram: Test {
			public override void Run() {
				Console.WriteLine("Testing large algebrogram...");
				Problem problem = new Problem();

				// (W E) (W A N T) + (S O M E) = (M O R E) + (M O N E Y) + (P L E A S E)

				string[] names = "S E N D M O R Y W T A P L".Split();
				Variable[] v = problem.Variables.AddIntegers(13, 0, 10, x => names[x]);

				Variable S = v[0], E = v[1], N = v[2], D = v[3], M = v[4], O = v[5], R = v[6], Y = v[7], W = v[8], T = v[9], A = v[10], P = v[11], L = v[12];

				problem.Constrains.Add(Constrain.AllDifferent(S,E,N,D,M,O,R,Y,W,T));
				problem.Constrains.Add(Constrain.AllDifferent(S,A,P,L));

				// problem.Constrains.Add(Constrain.Plus(N, D, M));
				// problem.Constrains.Add(Constrain.Plus(E, N, D));
				// problem.Constrains.Add(Constrain.Plus(Y, E, N));
				//
				var WE = (W * 10 + E).Build(problem);
				var WANT = (W * 1000 + A * 100 + N * 10 + T).Build(problem);
				var SOME = (S * 1000 + O * 100 + M * 10 + E).Build(problem);
				var MORE = (M * 1000 + O * 100 + R * 10 + E).Build(problem);
				var MONEY = (M * 10000 + O * 1000 + N * 100 + E * 10 + Y).Build(problem);
				var PLEASE = (P * 100000 + L * 10000 + E * 1000 + A * 100 + S * 10 + E).Build(problem);
				problem.Constrains.Add(
					Constrain.Equal(
						((WE * WANT) + SOME).Build(problem),
						(MORE + MONEY + (PLEASE / 3)).Build(problem)
					)
				);
				problem.Constrains.Add(Constrain.NotEqual(W, new AlgebraicExpression.ConstantNode(0).Build(problem)));
				problem.Constrains.Add(Constrain.NotEqual(S, new AlgebraicExpression.ConstantNode(0).Build(problem)));
				problem.Constrains.Add(Constrain.NotEqual(M, new AlgebraicExpression.ConstantNode(0).Build(problem)));
				problem.Constrains.Add(Constrain.NotEqual(P, new AlgebraicExpression.ConstantNode(0).Build(problem)));

				// TODO: dalsi test: maximalizace
				Solver solver = new Solver();
				IVariableAssignment result;

				Stopwatch.Instrument(() => {
					// Assert(solver.SolveParallel(problem, out result));
					Assert(solver.SolveSerial(problem, out result));
					Console.WriteLine(string.Join(" ", v.Select(variable => string.Format("{0}={1}", variable.Identifier, result[variable].Value))));
					Console.WriteLine("({0})({1})+{2}={3}+{4}+{5}/3", result[WE].Value, result[WANT].Value, result[SOME].Value, result[MORE].Value, result[MONEY].Value, result[PLEASE].Value);
				}, (span) => {
					Console.WriteLine("Solved large algebrogram in {0} seconds", span.TotalSeconds);
				});
			}
		}
	}
}
