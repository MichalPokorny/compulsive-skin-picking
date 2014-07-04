using System;
using System.Linq;

namespace CompulsiveSkinPicking {
	namespace Tests {
		class SendMoreMoneyMaximize: Test {
			public override void Run() {
				Console.WriteLine("Testing SEND+MORE=MONEY maximization...");
				Variable SEND, MORE, MONEY;
				Variable[] v;
				Problem problem = SendMoreMoneyBuilder.Build(out v, out SEND, out MORE, out MONEY);

				Solver solver = new Solver();
				IVariableAssignment result;

				problem.SetObjective(MONEY, ObjectiveDirection.Maximize);

				Stopwatch.Instrument(() => {
					Assert(solver.SolveParallel(problem, out result));
					Console.WriteLine(string.Join(" ", v.Select(variable => string.Format("{0}={1}", variable.Identifier, result[variable].Value))));
					Console.WriteLine("{0}+{1}={2}", result[SEND].Value, result[MORE].Value, result[MONEY].Value);
				}, (span) => {
					Console.WriteLine("Solved SEND+MORE=MONEY maximization in {0} seconds", span.TotalSeconds);
				});
			}
		}
	}
}
