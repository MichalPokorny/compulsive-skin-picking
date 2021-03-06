using System;
using System.Linq;

namespace CompulsiveSkinPicking {
	namespace Tests {
		class SendMoreMoney: Test {
			public override void Run() {
				Console.WriteLine("Testing SEND+MORE=MONEY...");
				Variable SEND, MORE, MONEY;
				Variable[] v;
				Problem problem = SendMoreMoneyBuilder.Build(out v, out SEND, out MORE, out MONEY);

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
