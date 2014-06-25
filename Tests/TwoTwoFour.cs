using System;

namespace CompulsiveSkinPicking {
	namespace Tests {
		class TwoTwoFour: Test {
			public override void Run() {
				Console.WriteLine("Testing TWO+TWO=FOUR...");

				Problem problem = new Problem();

				//   T W O
				// + T W O
				// =======
				// F O U R

				string[] names = "T W O F U R".Split();
				Variable[] v = problem.Variables.AddIntegers(6, 0, 10, x => names[x]);

				Variable T = v[0], W = v[1], O = v[2], F = v[3], U = v[4], R = v[5];

				problem.Constrains.Add(Constrain.AllDifferent(v));
				Variable TWO = (T * 100 + W * 10 + O).Build(problem);
				Variable FOUR = (F * 1000 + O * 100 + U * 10 + R).Build(problem);
				problem.Constrains.Add(
					Constrain.Equal(
						(TWO + TWO).Build(problem),
						FOUR
					)
				);
				problem.Constrains.Add(Constrain.NotEqual(T, new AlgebraicExpression.ConstantNode(0).Build(problem)));
				problem.Constrains.Add(Constrain.NotEqual(F, new AlgebraicExpression.ConstantNode(0).Build(problem)));

				Solver solver = new Solver();
				IVariableAssignment result;

				Assert(solver.Solve(problem, out result));
				Console.WriteLine("TWO+TWO=FOUR: {0}+{0}={1}",
					string.Format("{0}{1}{2}", result[T].Value, result[W].Value, result[O].Value),
					string.Format("{0}{1}{2}{3}", result[F].Value, result[O].Value, result[U].Value, result[R].Value)
				);
			}
		}
	}
}
