using System;
using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	public class Solver {
		private class Step {
			public int ID;

			public void Log(string fmt, params object[] args) {
				Console.WriteLine("[Asg {0}] {1}", ID, string.Format(fmt, args));
			}

			public IVariableAssignment Assignment;
			// TODO: generalize?
			public IExternalEnumerator<Value> ValueChoice;
			public IExternalEnumerator<Variable> VariableChoice;

			public List<IConstrain> Unsolved;
			public List<IConstrain> Solved;

			public bool Failure { get; private set; }
			public bool Success { get { return Unsolved.Count == 0; } }

			public void Dump() {
				Console.WriteLine("Step {0}:", ID);
				Console.WriteLine("\tAssignment:");
				Assignment.Dump();
			}

			public void MarkFailure() {
				Failure = true;
			}

			public void MarkConstrainSolved(IConstrain constrain) {
				Unsolved.Remove(constrain);
				Solved.Add(constrain);
			}

			public Step() {
				Failure = false;
			}

			public bool NextValueChoice() {
				bool result = ValueChoice.TryProgress(out ValueChoice);
				if (result) {
					Log("Progressed to value {0} into variable {1}.", ValueChoice.Value, VariableChoice.Value.Identifier);
				} else {
					Log("Nothing else to assign into {0}.", VariableChoice.Value.Identifier);
				}
				return result;
			}

			public Step Next() {
				Dump();

				Variable variable = VariableChoice.Value;
				Value value = ValueChoice.Value;

				Log("Assign: {0} <- {1}", variable.Identifier, value.value);

				List<PropagationTrigger> triggers = new List<PropagationTrigger>();
				triggers.Add(PropagationTrigger.Assign(variable, value));

				List<PropagationTrigger> nextTriggers = new List<PropagationTrigger>();

				Step next = new Step();

				next.Assignment = Assignment.Assign(variable, value); // TODO: suboptimal
				if (VariableChoice.TryProgress(out next.VariableChoice)) {
					next.ValueChoice = next.Assignment.EnumeratePossibleValues(next.VariableChoice.Value);
				}
				//	throw new Exception("Cannot progress further :(");
				//}

				// Tohle bude bod paralelizace.
				next.Unsolved = Unsolved.ToList(); // HACK
				next.Solved = Solved.ToList(); // HACK

				while (triggers.Count > 0) {
					List<IConstrain> solved = new List<IConstrain>();

					// TODO: for every *affected* constrain
					foreach (IConstrain constrain in next.Unsolved) {
						foreach (ConstrainResult result in constrain.Propagate(next.Assignment, triggers)) {
							switch (result.type) {
								case ConstrainResult.Type.Failure:
									next.MarkFailure();
									return next;
								case ConstrainResult.Type.Success:
									solved.Add(constrain);
									break;
								case ConstrainResult.Type.Restrict:
									if (next.Assignment.ValuePossible(result.variable, result.value)) {
										nextTriggers.Add(PropagationTrigger.Restrict(result.variable, result.value));
										next.Assignment.Restrict(result.variable, result.value);
									}
									break;
								// TODO: assign trigger
								default:
									throw new Exception("Unknown constrain result"); // TODO
							}
						}
					}

					foreach (IConstrain constrain in solved) {
						Log("Constrain {0} is now solved", constrain.Identifier);
						next.MarkConstrainSolved(constrain);
					}

					triggers = nextTriggers;
					nextTriggers = new List<PropagationTrigger>();
				}

				return next;
			}
		};

		public bool Solve(Problem problem, out IVariableAssignment result) {
			Stack<Step> stack = new Stack<Step>();

			int ID = 0;
			Step initial = new Step() {
				ID = ID,
				Assignment = problem.CreateEmptyAssignment(),
				Unsolved = problem.AllConstrains(),
				Solved = new List<IConstrain>(), // Empty set: no constrain satisfied so far.
				VariableChoice = problem.EnumerateVariables()  // TODO: variable choice heuristic...
			};

			initial.ValueChoice = initial.Assignment.EnumeratePossibleValues(initial.VariableChoice.Value); // TODO: value choice heuristic

			// TODO: NC, AC with supports
			stack.Push(initial);


			// TODO: backjumping, backmarking?

			// TODO: paralelizace; kazdy by si musel udrzovat vlastni stack...
			do {
				Step step = stack.Peek();

				Step next = step.Next();
				next.ID = ++ID;

				if (next.Success) {
					Console.WriteLine("Success!");
					result = next.Assignment;
					return true;
				} else if (next.Failure) {
					do {
						if (stack.Peek().NextValueChoice()) {
							// OK
							break;
						} else {
							if (stack.Count == 1) {
								// Unsolvable, trying to backtrack up.
								Console.WriteLine("Fail, unsolvable.");
								result = null;
								return false;
							} else {
								Console.WriteLine("No next value choice. Turning back.");
								stack.Pop(); // No next value choice. Return back.
							}
						}
					} while (true);
				} else {
					stack.Push(next);
				}
			} while (true);
		}
	}
}
