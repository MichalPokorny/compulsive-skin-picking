namespace CompulsiveSkinPicking {
	namespace Tests {
		static class SendMoreMoneyBuilder {
			public static Problem Build(out Variable[] v, out Variable SEND, out Variable MORE, out Variable MONEY) {
				Problem problem = new Problem();

				//   S E N D
				// + M O R E
				// =========
				// M O N E Y
				//
				// Variables: S, E, N, D, M, O, R, Y

				string[] names = "S E N D M O R Y".Split();
				v = problem.Variables.AddIntegers(8, 0, 10, x => names[x]);

				Variable S = v[0], E = v[1], N = v[2], D = v[3], M = v[4], O = v[5], R = v[6], Y = v[7];

				problem.Constrains.Add(Constrain.AllDifferent(v));

				// problem.Constrains.Add(Constrain.Plus(N, D, M));
				// problem.Constrains.Add(Constrain.Plus(E, N, D));
				// problem.Constrains.Add(Constrain.Plus(Y, E, N));
				//
				SEND = (S * 1000 + E * 100 + N * 10 + D).Build(problem);
				MORE = (M * 1000 + O * 100 + R * 10 + E).Build(problem);
				MONEY = (M * 10000 + O * 1000 + N * 100 + E * 10 + Y).Build(problem);
				problem.Constrains.Add(
					Constrain.Equal(
						(SEND + MORE).Build(problem),
						MONEY
					)
				);
				problem.Constrains.Add(Constrain.NotEqual(S, new AlgebraicExpression.ConstantNode(0).Build(problem)));
				problem.Constrains.Add(Constrain.NotEqual(M, new AlgebraicExpression.ConstantNode(0).Build(problem)));

				return problem;
			}
		}
	}
}
