using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CompulsiveSkinPicking.Constrains;

// TODO: propagate "assign" u vsech, co jsou assignute v prvnim kroku

namespace CompulsiveSkinPicking {
	public class Solver {
		private class Step {
			public void Log(string fmt, params object[] args) {
				Debug.WriteLine("[Asg] {0}", string.Format(fmt, args));
			}

			public IVariableAssignment Assignment;
			// TODO: generalize?
			public IExternalEnumerator<int> ValueChoice;
			public IExternalEnumerator<Variable> VariableChoice;

			public Dictionary<IConstrain, IScratchpad> Scratchpads;

			public List<IConstrain> Unsolved;

			public bool Success { get { return Unsolved.Count == 0; } }

			public Dictionary<Tuple<IConstrain, Variable, int>, IVariableAssignment> Supports;

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

			public bool ResolveFullyInstantiatedConstrains() {
				// TODO: for every *just solved* constrain
				List<IConstrain> solved = new List<IConstrain>();
				foreach (var constrain in Unsolved) {
					if (constrain.Dependencies.All(v => Assignment[v].Assigned)) {
						if (constrain.Satisfied(Assignment)) {
							solved.Add(constrain);
						} else {
							return false;
						}
					}
				}
				foreach (var constrain in solved) {
					MarkConstrainSolved(constrain);
				}
				return true;
			}

			public bool PropagateTriggers(IEnumerable<PropagationTrigger> inputTriggers) {
				List<PropagationTrigger> triggers = inputTriggers.ToList(); // XXX HACK
				List<PropagationTrigger> nextTriggers = new List<PropagationTrigger>();

				int round = 0;

				// TODO: kdyz se nejaka promenna ostrihala na jenom 1 prvek, tak to taky chci zpropagovat jako Assign
				while (triggers.Count > 0) {
					if (!ResolveFullyInstantiatedConstrains()) return false;

					if (round++ > 100) {
						Debug.doDebug = true;
						Debug.WriteLine("Round >100!");
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
								// TODO: constrain type "intersect"
								case ConstrainResult.Type.Restrict:
									if (Assignment[result.variable].CanBe(result.value)) {
										nextTriggers.Add(PropagationTrigger.Restrict(result.variable, result.value));
										Assignment[result.variable].Restrict(result.value);

										if (!Assignment[result.variable].HasPossibleValues) {
											Log("Constrain {0} caused {1} to have empty value set", constrain.Identifier, result.variable.Identifier);
											return false;
										}

										if (Assignment[result.variable].Assigned) {
											nextTriggers.Add(PropagationTrigger.Assign(result.variable, Assignment[result.variable].Value));
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
			Debug.WriteLine("[Solver] {0}", string.Format(fmt, args));
		}

		// TODO: cache results
		private bool HasArcSupport(Step current, IConstrain constrain, Variable variable, int value) {
			var key = Tuple.Create(constrain, variable, value);
			if (current.Supports.ContainsKey(key)) {
				IVariableAssignment support = current.Supports[key];
				if (constrain.Dependencies.All(var => current.Assignment[var].CanBe(support[var].Value))) {
					// Support cache used
					return true;
				}
			}

			// Console.WriteLine("Trying AC for constrain {0}, {1}={2}", constrain.Identifier, variable, value);
			Step duplicate = new Step() {
				Assignment = current.Assignment.Duplicate(),
				VariableChoice = constrain.Dependencies.GetExternalEnumerator(),
				Unsolved = new List<IConstrain>() { constrain },
				Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>(),
			};
			foreach (var pair in current.Scratchpads) {
				duplicate.Scratchpads.Add(pair.Key, pair.Value == null ? null : pair.Value.Duplicate());
			}
			duplicate.Assignment[variable].Value = value;
			if (!duplicate.PropagateTriggers(new [] { PropagationTrigger.Assign(variable, value) })) {
				return false;
			}
			duplicate.ValueChoice = duplicate.Assignment[duplicate.VariableChoice.Value].EnumeratePossibleValues();
			IVariableAssignment result;
			// TODO: take cancellation outside
			CancellationTokenSource source = new CancellationTokenSource();
			if (SolveStepSerial(duplicate, out result, source.Token, false)) {
				current.Supports[key] = result;
				return true;
			} else {
				// Console.WriteLine("AC removed {0}={1} in {2}", variable, value, constrain.Identifier);
				return false; // Unsupported variable - value pair.
			}
		}

		private bool Progress(Step current, out Step next, bool doConsistency = true) {
			Variable variable = current.VariableChoice.Value;
			int value = current.ValueChoice.Value;

			Log("Assign: {0} <- {1}", variable, value);

			next = new Step() {
				Assignment = current.Assignment.Duplicate(),
				Unsolved = current.Unsolved.ToList() // XXX HACK
			};

			if (current.Supports != null) {
				next.Supports = new Dictionary<Tuple<IConstrain, Variable, int>, IVariableAssignment>();
				foreach (var pair in current.Supports) {
					next.Supports.Add(pair.Key, pair.Value);
				}
			}

			next.Assignment[variable].Value = value;

			next.Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>();
			foreach (var pair in current.Scratchpads) {
				next.Scratchpads.Add(pair.Key, pair.Value == null ? null : pair.Value.Duplicate());
			}

			if (!next.ResolveFullyInstantiatedConstrains()) return false;
			if (doConsistency) {
				if (!next.PropagateTriggers(new [] { PropagationTrigger.Assign(variable, value) })) {
					Log("Propagating assignment failed.");
					return false;
				}

				if (!MakeArcConsistent(next)) {
					Log("Failed to make next step arc-consistent.");
					return false;
				}
			}

			if (current.VariableChoice.TryProgress(out next.VariableChoice)) {
				next.ValueChoice = next.Assignment[next.VariableChoice.Value].EnumeratePossibleValues();
			}

			return true;
		}

		private Task<IVariableAssignment> SolveAsync(Step step, int depth, CancellationToken cancellationToken) {
			if (depth >= 3) {
				return Task.Factory.StartNew(() => {
					IVariableAssignment result;
					if (SolveStepSerial(step, out result, cancellationToken)) return result;
					return null;
				});
			}

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
					if (cancellationTokenSource.Token.IsCancellationRequested) break;
					if (Progress(step, out nextStep)) {
						subtasks.Add(SolveAsync(nextStep, depth + 1, cancellationTokenSource.Token).ContinueWith((task) => {
							if (task.Result != null) {
								Debug.WriteLine("SOLUTION FOUND");
								if (Debug.doDebug) {
									task.Result.Dump();
								}
								completionSource.TrySetResult(task.Result);
								subtaskCompletedSource.Cancel();
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

		private bool SolveStepSerial(Step initial, out IVariableAssignment result, CancellationToken cancellationToken, bool doConsistency = true) {
			if (initial == null) throw new NullReferenceException("Null initial step passed");

			Stack<Step> stack = new Stack<Step>();

			stack.Push(initial);
			do {
				Step step = stack.Peek();

				Step next = null;
				if (!Progress(step, out next, doConsistency)) {
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
			} while (!cancellationToken.IsCancellationRequested);
			result = null;
			return false; // Cancelled
		}

		private bool ArcConsistencyFeasible(IVariableAssignment assignment, IConstrain constrain) {
			long domainSizeSum = 0;
			long domainSizeProduct = 1;
			foreach (var variable in constrain.Dependencies) {
				int domainSize = assignment[variable].PossibleValueCount;
				domainSizeSum += domainSize;
				domainSizeProduct *= domainSize;
				if (domainSizeSum * domainSizeProduct > 1000000) return false;
			}
			return true;
		}

		// Returns false if any variable gets assigned an empty domain.
		private bool MakeArcConsistent(Step step) {
			List<PropagationTrigger> triggers = new List<PropagationTrigger>();

			bool stable = false;
			List<int> toRemove = new List<int>();
			List<Variable> touchedVariables = step.Assignment.Variables.ToList();
			List<Variable> newlyTouchedVariables = new List<Variable>();
			int round;
			for (round = 0; !stable; round++) {
				stable = true;

				var changedConstrains = from c in step.Unsolved where c.Dependencies.Any(dependency => touchedVariables.Contains(dependency)) select c;

				foreach (var constrain in changedConstrains) {
					if (!ArcConsistencyFeasible(step.Assignment, constrain)) {
						Log("(Arc consistency of {0} is unfeasible, skipping.)", constrain.Identifier);
						continue;
					}

					foreach (var variable in constrain.Dependencies) {
						{
							IExternalEnumerator<int> value = step.Assignment[variable].EnumeratePossibleValues();
							toRemove.Clear();
							do {
								if (!HasArcSupport(step, constrain, variable, value.Value)) {
									Log("Arc consistency: {0}={1} is inconsistent with {2}", variable, value.Value, constrain.Identifier);
									toRemove.Add(value.Value);
								}
							} while (value.TryProgress(out value));
						}

						if (toRemove.Count > 0) {
							stable = false;
							foreach (var value in toRemove) {
								step.Assignment[variable].Restrict(value);
								triggers.Add(PropagationTrigger.Restrict(variable, value));
							}
							if (!newlyTouchedVariables.Contains(variable)) {
								newlyTouchedVariables.Add(variable);
							}
						}
					}
				}
				var tmp = touchedVariables;
				touchedVariables = newlyTouchedVariables;
				newlyTouchedVariables = tmp;
				newlyTouchedVariables.Clear();
			}
			// Console.WriteLine("Did {0} rounds of AC", round);
			Log("Problem is now arc consistent.");

			return step.PropagateTriggers(triggers);
		}


		private bool PropagateInitialTriggers(Step initial) {
			if (!initial.PropagateTriggers(from v in initial.Assignment.Variables where initial.Assignment[v].Assigned select PropagationTrigger.Assign(v, initial.Assignment[v].Value))) {
				return false;
			}
			/*
			if (!MakeArcConsistent(initial)) {
				return false;
			}
			*/
			initial.ValueChoice = initial.Assignment[initial.VariableChoice.Value].EnumeratePossibleValues();
			return true;
		}

		public bool SolveSerial(Problem problem, out IVariableAssignment result) {
			Step initial = BuildInitialStep(problem);
			if (!PropagateInitialTriggers(initial)) {
				Log("Initial propagation failed, the problem has no solution.");
				result = null;
				return false;
			}
			CancellationTokenSource source = new CancellationTokenSource();
			return SolveStepSerial(initial, out result, source.Token);

			// TODO: backjumping, backmarking?

			// TODO: paralelizace; kazdy by si musel udrzovat vlastni stack...
		}

		private Step BuildInitialStep(Problem problem) {
			Step initial = new Step() {
				Assignment = problem.CreateEmptyAssignment(),
				Unsolved = problem.AllConstrains(),
				VariableChoice = problem.EnumerateVariables(),  // TODO: variable choice heuristic...
				Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>(),
				Supports = new Dictionary<Tuple<IConstrain, Variable, int>, IVariableAssignment>()
			};
			return initial;
		}

		public bool SolveParallel(Problem problem, out IVariableAssignment result) {
			Step initial = BuildInitialStep(problem);
			if (!PropagateInitialTriggers(initial)) {
				Log("Initial propagation failed, the problem has no solution.");
				result = null;
				return false;
			}

			// TODO: value choice heuristic
			// TODO: null?

			var source = new CancellationTokenSource();
			result = SolveAsync(initial, 0, source.Token).Result;
			return result != null;

			// TODO: NC, AC with supports
		}

		public bool Solve(Problem problem, out IVariableAssignment result) {
			return SolveSerial(problem, out result);
		}
	}
}
