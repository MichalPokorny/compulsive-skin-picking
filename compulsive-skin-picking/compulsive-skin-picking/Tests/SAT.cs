using System;

namespace CompulsiveSkinPicking {
	namespace Tests {
		class SAT: Test {
			public override void Run() {
				Problem problem = new Problem();

				Variable[] v = problem.Variables.AddBooleans(7, x => ((char)('A' + x)).ToString());

				Variable A = v[0], B = v[1], C = v[2], D = v[3], E = v[4], F = v[5], G = v[6];

				problem.Constrains.Add(
					Constrain.Truth(
						(
							((!E) | F) & ((!F) | (!A) | (!G)) &
							(G | C | B) & (D | C) & (F | (!G)) &
							(G | D | (!B)) & ((!A) | (!B) | (!F)) &
							(E | G) & (E | B | A) &

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

				bool a = result[A].Value != 0, b = result[B].Value != 0, c = result[C].Value != 0, d = result[D].Value != 0, e = result[E].Value != 0, f = result[F].Value != 0, g = result[G].Value != 0;
				Assert(!e || f);
				Assert(!f || !a || !g);
				Assert(g || c || b);
				Assert(d || c);
				Assert(f || !g);
				Assert(g || d || !b);
				Assert(!a || !b || !f);
				Assert(e || g);
				Assert(e || b || a);
				Assert(!(d && e && f && g && !a && !b && !c));

				Console.WriteLine("SAT test OK");
			}
		}
	}
}
