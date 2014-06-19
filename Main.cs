using System;

namespace CSPS {
	public class Program {
		public static void Main(string[] args) {
			Problem problem = new Problem();

			Variable a, b, c, d, e, f;
			a = new Variable() {
				Identifier = "A",
				Minimum = 0,
				Maximum = 3
			};
			b = new Variable() {
				Identifier = "B",
				Minimum = 0,
				Maximum = 3
			};
			c = new Variable() {
				Identifier = "C",
				Minimum = 0,
				Maximum = 3
			};
			d = new Variable() {
				Identifier = "D",
				Minimum = 0,
				Maximum = 3
			};
			e = new Variable() {
				Identifier = "E",
				Minimum = 0,
				Maximum = 3
			};
			f = new Variable() {
				Identifier = "F",
				Minimum = 0,
				Maximum = 3
			};
			problem.variables.Add(a);
			problem.variables.Add(b);
			problem.variables.Add(c);
			problem.variables.Add(d);
			problem.variables.Add(e);
			problem.variables.Add(f);

			// A--B--C
			//  \ |\ |\
			//   \| \| \
			//    D--E--F

			problem.constrains.Add(new NotEqualConstrain(a, b));
			problem.constrains.Add(new NotEqualConstrain(b, c));
			problem.constrains.Add(new NotEqualConstrain(a, d));
			problem.constrains.Add(new NotEqualConstrain(b, d));
			problem.constrains.Add(new NotEqualConstrain(b, e));
			problem.constrains.Add(new NotEqualConstrain(c, e));
			problem.constrains.Add(new NotEqualConstrain(d, e));
			problem.constrains.Add(new NotEqualConstrain(f, e));
			problem.constrains.Add(new NotEqualConstrain(f, c));

			Solver solver = new Solver();
			IVariableAssignment result;

			if (!solver.Solve(problem, out result)) {
				Console.WriteLine("Unsolvable problem");
			} else {
				Console.WriteLine("Solvable problem");
				foreach (var v in new []{a, b, c, d, e, f}) {
					Console.WriteLine("{0}: {1}", v.Identifier, result.GetAssignedValue(v));
				}
			}
		}
	}
}
