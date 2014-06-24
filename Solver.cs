using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// TODO: propagate "assign" u vsech, co jsou assignute v prvnim kroku

namespace CSPS {
	public class Solver {
		private class Step {
			public void Log(string fmt, params object[] args) {
				Debug.WriteLine("[Asg] {1}", string.Format(fmt, args));
			}

			public IVariableAssignment Assignment;
			// TODO: generalize?
			public IExternalEnumerator<Value> ValueChoice;
			public IExternalEnumerator<Variable> VariableChoice;

			public Dictionary<Constrains.IConstrain, IScratchpad> Scratchpads;

			public List<Constrains.IConstrain> Unsolved;

			public bool Success { get { return Unsolved.Count == 0; } }

			public void Dump() {
				Console.WriteLine("Step:");
				if (VariableChoice != null && ValueChoice != null) {
					Console.WriteLine("\tWill try to assign {0} to {1}", ValueChoice.Value, VariableChoice.Value);
				}
				Console.WriteLine("\tAssignment:");
				Assignment.Dump();
			}

			public void MarkConstrainSolved(Constrains.IConstrain constrain) {
				Unsolved.Remove(constrain);
			}

			public bool NextValueChoice() {
				// Skip values that are already pruned.
				do {
					bool result = ValueChoice.TryProgress(out ValueChoice);
					if (result) {
						Log("Progressed to value {0} into variable {1}.", ValueChoice.Value, VariableChoice.Value.Identifier);
					} else {
						Log("Nothing else to assign into {0}.", VariableChoice.Value.Identifier);
						return false;
					}
				} while (!Assignment[VariableChoice.Value].CanBe(ValueChoice.Value));
				return true;
			}

			public bool Next(ref Step next) {
				// Dump();

				Variable variable = VariableChoice.Value;
				Value value = ValueChoice.Value;

				Log("Assign: {0} <- {1}", variable.Identifier, value.value);

				next = new Step();

				next.Assignment = Assignment.Duplicate(); // TODO: suboptimal
				next.Assignment[variable].Value = value;

				// Tohle bude bod paralelizace.
				next.Unsolved = Unsolved.ToList(); // XXX HACK

				next.Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>();
				foreach (var pair in Scratchpads) {
					next.Scratchpads.Add(pair.Key, pair.Value == null ? null : pair.Value.Duplicate());
				}

				if (!next.PropagateTriggers(new [] { PropagationTrigger.Assign(variable, value) })) {
					Log("Propagating assignment failed.");
					return false;
				}

				if (VariableChoice.TryProgress(out next.VariableChoice)) {
					next.ValueChoice = next.Assignment[next.VariableChoice.Value].EnumeratePossibleValues();
				}

				return true;
			}

			public bool PropagateTriggers(IEnumerable<PropagationTrigger> inputTriggers) {
				List<PropagationTrigger> triggers = inputTriggers.ToList(); // XXX HACK
				List<PropagationTrigger> nextTriggers = new List<PropagationTrigger>();

				int round = 0;

				// TODO: kdyz se nejaka promenna ostrihala na jenom 1 prvek, tak to taky chci zpropagovat jako Assign
				while (triggers.Count > 0) {
					if (round++ > 10) {
						Debug.doDebug = true;
						Debug.WriteLine("Round >10!");
						foreach (var trigger in triggers) {
							Debug.WriteLine(trigger.ToString());
						}
					}

					List<Constrains.IConstrain> solved = new List<Constrains.IConstrain>();

					// TODO: for every *affected* constrain
					foreach (var constrain in Unsolved) {
						Debug.WriteLine("Propagate through {0}", constrain.Identifier);
						// TODO: for the triggers that affect the constrain
						IScratchpad scratchpad;
						if (Scratchpads.ContainsKey(constrain)) {
							scratchpad = Scratchpads[constrain];
						} else {
							scratchpad = null;
						}
						foreach (ConstrainResult result in constrain.Propagate(Assignment, triggers, ref scratchpad)) {
							Debug.WriteLine("==> {0}", result);
							switch (result.type) {
								case ConstrainResult.Type.Failure:
									Log("Constrain {0} failed.", constrain.Identifier);
									return false;
								case ConstrainResult.Type.Success:
									solved.Add(constrain);
									break;
								case ConstrainResult.Type.Restrict:
									if (Assignment[result.variable].CanBe(result.value)) {
										nextTriggers.Add(PropagationTrigger.Restrict(result.variable, result.value));
										Assignment[result.variable].Restrict(result.value);

										if (!Assignment[result.variable].HasPossibleValues) {
											Log("Constrain {0} caused {1} to have empty value set", constrain.Identifier, result.variable.Identifier);
											return false;
										}
									}
									break;
								case ConstrainResult.Type.Assign:
									if (Assignment[result.variable].CanBe(result.value)) {
										nextTriggers.Add(PropagationTrigger.Assign(result.variable, result.value));
										Assignment[result.variable].Value = result.value;
										break;
									} else {
										Log("Constrain {0} assigns {1} to {2}, but that's not a possible value.", constrain.Identifier, result.value, result.variable.Identifier);
										return false;
									}
								// TODO: assign trigger
								default:
									throw new Exception("Unknown constrain result"); // TODO
							}
						}
						Scratchpads[constrain] = scratchpad;
					}

					foreach (var constrain in solved) {
						Log("Constrain {0} is now solved", constrain.Identifier);
						MarkConstrainSolved(constrain);
					}

					triggers = nextTriggers;
					nextTriggers = new List<PropagationTrigger>();
				}
				return true;
			}
		};

		private void Log(string fmt, params object[] args) {
			Debug.WriteLine("[Solver} {0}", string.Format(fmt, args));
		}

		private Task<IVariableAssignment> SolveAsync(Step step, CancellationToken cancellationToken) {
			if (step.Success) {
				return Task.FromResult(step.Assignment);
			}

			var completionSource = new TaskCompletionSource<IVariableAssignment>();

			Task.Factory.StartNew(() => {
				List<Task> subtasks = new List<Task>();
				var subtaskCompletedSource = new CancellationTokenSource();
				var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(subtaskCompletedSource.Token, cancellationToken);
				do {
					Step nextStep = null;
					if (step.Next(ref nextStep)) {
						subtasks.Add(SolveAsync(nextStep, cancellationTokenSource.Token).ContinueWith((task) => {
							if (task.Result != null) {
								Console.WriteLine("SOLUTION FOUND");
								task.Result.Dump();
								completionSource.TrySetResult(task.Result);
								subtaskCompletedSource.Cancel();
								// TODO: cancel all other tasks
							}
						}, cancellationTokenSource.Token));
					}
				} while (step.NextValueChoice());

				Task.WhenAll(subtasks).ContinueWith((tasks) => {
					completionSource.TrySetResult(null);
				}, cancellationTokenSource.Token);
			}, cancellationToken);

			return completionSource.Task;
		}

		public bool SolveSerial(Problem problem, out IVariableAssignment result) {
			Stack<Step> stack = new Stack<Step>();

			Step initial = BuildInitialStep(problem);
			/*
			 * TODO
			if (!initial.PropagateTriggers(
				from v in problem.Variables where initial.Assignment[v].Assigned select PropagationTrigger.Assign(v, initial.Assignment[v].Value)
			)) {
				Log("Initial propagation failed, the problem has no solution.");
				result = null;
				return false;
			}
			*/

			stack.Push(initial);

			// TODO: backjumping, backmarking?

			// TODO: paralelizace; kazdy by si musel udrzovat vlastni stack...
			do {
				Step step = stack.Peek();

				Step next = null;
				if (!step.Next(ref next)) {
					do {
						if (stack.Peek().NextValueChoice()) {
							// OK
							break;
						} else {
							if (stack.Count == 1) {
								// Unsolvable, trying to backtrack up.
								Log("Fail, unsolvable.");
								result = null;
								return false;
							} else {
								Log("No next value choice. Turning back.");
								stack.Pop(); // No next value choice. Return back.
							}
						}
					} while (true);
				} else {
					if (next.Success) {
						Log("Success!");
						result = next.Assignment;
						return true;
					} else {
						stack.Push(next);
					}
				}
			} while (true);
		}

		private Step BuildInitialStep(Problem problem) {
			Step initial = new Step() {
				Assignment = problem.CreateEmptyAssignment(),
				Unsolved = problem.AllConstrains(),
				VariableChoice = problem.EnumerateVariables(),  // TODO: variable choice heuristic...
				Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>()
			};
			initial.ValueChoice = initial.Assignment[initial.VariableChoice.Value].EnumeratePossibleValues();
			return initial;
		}

		public bool SolveParallel(Problem problem, out IVariableAssignment result) {
			Step initial = BuildInitialStep(problem);

			// TODO: value choice heuristic
			// TODO: null?

			/*
			 * TODO
			if (!initial.PropagateTriggers(
				from v in problem.Variables where initial.Assignment[v].Assigned select PropagationTrigger.Assign(v, initial.Assignment[v].Value)
			)) {
				Log("Initial propagation failed, the problem has no solution.");
				result = null;
				return false;
			}
			*/

			var source = new CancellationTokenSource();
			result = SolveAsync(initial, source.Token).Result;
			return result != null;

			// TODO: NC, AC with supports
		}

		public bool Solve(Problem problem, out IVariableAssignment result) {
			return SolveSerial(problem, out result);
		}
	}
}
