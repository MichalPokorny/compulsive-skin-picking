using System;
using System.Linq;
using System.Collections.Generic;

// TODO: propagate "assign" u vsech, co jsou assignute v prvnim kroku

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

			public Dictionary<Constrains.IConstrain, IScratchpad> Scratchpads;

			public List<Constrains.IConstrain> Unsolved;
			public List<Constrains.IConstrain> Solved;

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

			public void MarkConstrainSolved(Constrains.IConstrain constrain) {
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

				while (Assignment[VariableChoice.Value].Assigned) {
					Log("Skipping already assigned variable.");
					if (!VariableChoice.TryProgress(out VariableChoice)) {
						throw new Exception("Cannot progress to next variable");
					}
					ValueChoice = Assignment.EnumeratePossibleValues(VariableChoice.Value);
				}
				Variable variable = VariableChoice.Value;
				Value value = ValueChoice.Value;

				Log("Assign: {0} <- {1}", variable.Identifier, value.value);

				List<PropagationTrigger> triggers = new List<PropagationTrigger>();
				triggers.Add(PropagationTrigger.Assign(variable, value));

				List<PropagationTrigger> nextTriggers = new List<PropagationTrigger>();

				Step next = new Step();

				next.Assignment = Assignment.Duplicate(); // TODO: suboptimal
				next.Assignment[variable].Value = value;
				if (VariableChoice.TryProgress(out next.VariableChoice)) {
					next.ValueChoice = next.Assignment.EnumeratePossibleValues(next.VariableChoice.Value);
				}
				//	throw new Exception("Cannot progress further :(");
				//}

				// Tohle bude bod paralelizace.
				next.Unsolved = Unsolved.ToList(); // HACK
				next.Solved = Solved.ToList(); // HACK

				next.Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>();
				foreach (var pair in Scratchpads) {
					next.Scratchpads.Add(pair.Key, pair.Value == null ? null : pair.Value.Duplicate());
				}

				// TODO: kdyz se nejaka promenna ostrihala na jenom 1 prvek, tak to taky chci zpropagovat jako Assign
				while (triggers.Count > 0) {
					List<Constrains.IConstrain> solved = new List<Constrains.IConstrain>();

					// TODO: for every *affected* constrain
					foreach (var constrain in next.Unsolved) {
						// TODO: for the triggers that affect the constrain
						IScratchpad scratchpad;
						if (next.Scratchpads.ContainsKey(constrain)) {
							scratchpad = next.Scratchpads[constrain];
						} else {
							scratchpad = null;
						}
						foreach (ConstrainResult result in constrain.Propagate(next.Assignment, triggers, ref scratchpad)) {
							switch (result.type) {
								case ConstrainResult.Type.Failure:
									Log("Constrain {0} failed.", constrain.Identifier);
									next.MarkFailure();
									return next;
								case ConstrainResult.Type.Success:
									solved.Add(constrain);
									break;
								case ConstrainResult.Type.Restrict:
									if (next.Assignment[result.variable].CanBe(result.value)) {
										nextTriggers.Add(PropagationTrigger.Restrict(result.variable, result.value));
										next.Assignment[result.variable].Restrict(result.value);
									}
									break;
								case ConstrainResult.Type.Assign:
									if (next.Assignment[result.variable].CanBe(result.value)) {
										nextTriggers.Add(PropagationTrigger.Assign(result.variable, result.value));
										next.Assignment[result.variable].Value = result.value;
										break;
									} else {
										Log("Constrain {0} assigns {1} to {2}, but that's not a possible value.", constrain.Identifier, result.value, result.variable.Identifier);
										next.MarkFailure();
										return next;
									}
								// TODO: assign trigger
								default:
									throw new Exception("Unknown constrain result"); // TODO
							}
						}
						next.Scratchpads[constrain] = scratchpad;
					}

					foreach (var constrain in solved) {
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
				Solved = new List<Constrains.IConstrain>(), // Empty set: no constrain satisfied so far.
				VariableChoice = problem.EnumerateVariables(),  // TODO: variable choice heuristic...
				Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>()
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
