using System;

namespace CSPS {
	namespace Tests {
		class SAT: Test {
			public override void Run() {
				Problem problem = new Problem();

				Variable[] v = problem.Variables.AddBooleans(7, x => ((char)('A' + x)).ToString());

				Variable A = v[0], B = v[1], C = v[2], D = v[3], E = v[4], F = v[5], G = v[6];

				problem.Constrains.Add(
					Constrain.Truth(
						(
							((!E) | F) & ((!F) | (!A) | (!G)) & (G | C | B) & (D | C) & (F | (!G)) &
							(G | D | (!B)) & ((!A) | (!B) | (!F)) & (E | G) & (E | B | A) &

							!(D&E&F&G&(!A)&(!B)&(!C))
						).Build(problem)
					)
				);
				problem.Constrains.Add(
					Constrain.Truth(F)
				);

				Solver solver = new Solver();
				IVariableAssignment result;

				Assert(solver.Solve(problem, out result));

				int a = result[A].Value, b = result[B].Value, c = result[C].Value, d = result[D].Value, e = result[E].Value, f = result[F].Value, g = result[G].Value;
				Assert(a == 0 && b == 0 && c == 1 && d == 1 && e == 1 && f == 1 && g == 1);
			}
		}
	}
}
