using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CompulsiveSkinPicking.Constrains;

// TODO: propagate "assign" u vsech, co jsou assignute v prvnim kroku

namespace CompulsiveSkinPicking {
	public class Solver {
		private void Log(string fmt, params object[] args) {
			Debug.WriteLine("[Solver] {0}", string.Format(fmt, args));
		}

		/*
		// TODO: cache results
		private bool HasArcSupport(Step current, IConstrain constrain, Variable variable, int value) {
			if (current.Supports == null) return true;
			var key = Tuple.Create(constrain, variable, value);
			for (Step step = current; step != null; step = step.Parent) {
				if (step.Supports != null && step.Supports.ContainsKey(key)) {
					IVariableAssignment support = step.Supports[key];
					if (constrain.Dependencies.All(var => current.Assignment[var].CanBe(support[var].Value))) {
						// Support cache used
						current.Supports[key] = support;
						return true;
					}
				}
			}
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
				VariableChoice = constrain.Dependencies.ToList().GetExternalEnumerator(),
				Unsolved = new List<IConstrain>() { constrain },
				Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>(),
				ChangedSinceLastArcCheck = new HashSet<Variable>(constrain.Dependencies),
				Supports = null,
				Parent = current
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
			if (SolveStepSerial(duplicate, out result, CancellationToken.None, false)) {
				current.Supports[key] = result;
				return true;
			} else {
				// Console.WriteLine("AC removed {0}={1} in {2}", variable, value, constrain.Identifier);
				return false; // Unsupported variable - value pair.
			}
		}
		*/

		/*
		private bool Progress(Step current, out Step next, bool doConsistency = true) {
			Variable variable = current.VariableChoice.Value;
			int value = current.ValueChoice.Value;

			Log("Assign: {0} <- {1}", variable, value);

			next = new Step() {
				Assignment = current.Assignment.Duplicate(),
				Unsolved = current.Unsolved.ToList(), // XXX HACK
				Parent = current,
			};

			if (doConsistency) {
				next.ChangedSinceLastArcCheck = new HashSet<Variable>(current.ChangedSinceLastArcCheck);
				next.Supports = new Dictionary<Tuple<IConstrain, Variable, int>, IVariableAssignment>();
			}

			next.Assign(variable, value);

			if (doConsistency) {
				next.Scratchpads = new Dictionary<Constrains.IConstrain, IScratchpad>();
				foreach (var pair in current.Scratchpads) {
					next.Scratchpads.Add(pair.Key, pair.Value == null ? null : pair.Value.Duplicate());
				}
			}

			if (!next.ResolveFullyInstantiatedConstrains()) return false;
			if (doConsistency) {
				if (!next.PropagateTriggers(new [] { PropagationTrigger.Assign(variable, value) })) {
					Log("Propagating assignment failed.");
					return false;
				}

				//if (!MakeArcConsistent(next)) {
				//	Log("Failed to make next step arc-consistent.");
				//	return false;
				//}
			}

			if (current.VariableChoice.TryProgress(out next.VariableChoice)) {
				next.ValueChoice = next.Assignment[next.VariableChoice.Value].EnumeratePossibleValues();
			}

			return true;
		}
		*/

		private Task<IVariableAssignment> SolveAsync(SolutionState step, int depth, CancellationToken cancellationToken) {
			if (depth >= 3) {
				return Task.Factory.StartNew(() => {
					IVariableAssignment result;
					Debug.WriteLine("Solving in serial.");
					if (SearchSerial(step.DeepDuplicate(), out result, cancellationToken)) {
						Debug.WriteLine("Serial search found solution");
						return result;
					} else {
						Debug.WriteLine("Serial search found nothing");
						return null;
					}
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
					SolutionState nextStep = step.DeepDuplicate();
					if (cancellationTokenSource.Token.IsCancellationRequested) {
						Debug.WriteLine("Cancellation requested");
						break;
					}
					if (nextStep.Progress()) {
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
					} else {
						Debug.WriteLine("Progress failed");
					}
				} while (step.NextValueChoice());
				Debug.WriteLine("No more value choices");

				Task.WhenAll(subtasks).ContinueWith((tasks) => {
					completionSource.TrySetResult(null);
				}, cancellationTokenSource.Token);
			}, cancellationToken);
			// TODO: co se tady stane kdyz vevnitr dojde k exceptione?

			return completionSource.Task;
		}

		private bool SearchSerial(SolutionState initial, out IVariableAssignment result, CancellationToken cancellationToken) {
			// initial.Dump();

			SolutionState state = initial.DeepDuplicate(); // TODO: asi neni potreba

			// Stack<Step> stack = new Stack<Step>();

			// stack.Push(initial);
			do {
				//state.Dump();
				// Step step = stack.Peek();

				// Step next = null;
				if (state.Success) {
					Log("Success!");
					state.Dump();
					result = state.Assignment.DeepDuplicate();
					return true;
				} else {
					state.AddSavepoint();

					if (state.Progress()) {
						// OK then.
					} else {
						Log("Cannot progress, tried last value.");
						do {
							if (state.CanRollbackToSavepoint) {
								state.RollbackToSavepoint();

								if (state.NextValueChoice()) {
									// OK
									break;
								} else {
									// We will have to rollback further up
									continue;
								}
							} else {
								Log("And cannot rollback.");
								result = null;
								return false;
							}
						} while (true);
					}
				}

				/*
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
				*/
			} while (!cancellationToken.IsCancellationRequested);

			result = null;
			return false; // Cancelled
		}

		/*
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
			int round = 0;
			List<PropagationTrigger> triggers = new List<PropagationTrigger>();
			var toRemove = new List<int>();

			while (step.ChangedSinceLastArcCheck.Count > 0) {
				triggers.Clear();

				List<IConstrain> changedConstrains = (from c in step.Unsolved where c.Dependencies.Any(dependency => step.ChangedSinceLastArcCheck.Contains(dependency)) select c).ToList();
				// if (changedConstrains.Count > 0) {
				// 	Console.WriteLine("{0}/{1} arcs touched", changedConstrains.Count, step.Unsolved.Count);
				// }
				step.ChangedSinceLastArcCheck.Clear();

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
							foreach (var value in toRemove) {
								step.Restrict(variable, value);
								triggers.Add(PropagationTrigger.Restrict(variable, value));
								if (step.Assignment[variable].Ground) {
									triggers.Add(PropagationTrigger.Assign(variable, step.Assignment[variable].Value));
								}
							}
						}
					}
				}

				if (!step.PropagateTriggers(triggers)) {
					return false;
				}

				round++;
			}
			Log("Problem is now arc consistent.");
			return true;
		}
		*/


		public bool SolveSerial(Problem problem, out IVariableAssignment result) {
			if (problem.IsOptimization) {
				return Optimize(problem, out result, (subproblem) => {
					IVariableAssignment _result;
					if (SolveSerial(subproblem, out _result)) {
						return _result;
					} else {
						return null;
					}
				});
			}

			SolutionState initial = new SolutionState(problem);
			if (!initial.PropagateInitial()) {
				Log("Initial propagation failed, the problem has no solution.");
				result = null;
				return false;
			}
			initial.SetValueChoice();

			return SearchSerial(initial, out result, CancellationToken.None);

			// TODO: backjumping, backmarking?

			// TODO: paralelizace; kazdy by si musel udrzovat vlastni stack...
		}

		public bool SolveParallel(Problem problem, out IVariableAssignment result) {
			if (problem.IsOptimization) {
				return Optimize(problem, out result, (subproblem) => {
					IVariableAssignment _result;
					if (SolveParallel(subproblem, out _result)) {
						return _result;
					} else {
						return null;
					}
				});
			}

			SolutionState initial = new SolutionState(problem);
			if (!initial.PropagateInitial()) {
				Log("Initial propagation failed, the problem has no solution.");
				result = null;
				return false;
			}
			initial.SetValueChoice();

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

		protected bool Optimize(Problem problem, out IVariableAssignment result, Func<Problem, IVariableAssignment> solver) {
			Variable objective = problem.ObjectiveVariable;
			problem.ObjectiveVariable = null;
			try {
				IVariableAssignment bestKnown;
				ValueRange bounds = objective.Range;
				ObjectiveDirection direction = problem.ObjectiveDirection;

				if (direction != ObjectiveDirection.Minimize && direction != ObjectiveDirection.Maximize)
					throw new Exception("Unknown opt direction"); // TODO better exception
					
				bestKnown = solver(problem);
				if (bestKnown == null) {
					result = null;
					Log("Optimization problem is not solvable.");
					return false;
				}

				if (direction == ObjectiveDirection.Maximize) {
					bounds.Minimum = bestKnown[objective].Value;
				} else {
					bounds.Maximum = bestKnown[objective].Value + 1;
				}

				// Binary search
				while (!bounds.IsSingleton) {
					Console.WriteLine("Bounds: {0}", bounds);
					int middle = (bounds.Maximum + bounds.Minimum) / 2;
					int min, max;

					if (direction == ObjectiveDirection.Maximize) {
						min = middle;
						max = bounds.Maximum;
					} else {
						min = bounds.Minimum;
						max = middle;
					}
					Console.WriteLine("We will be looking for {0} <= X < {1}", min, max);

					IConstrain lowerConstrain, upperConstrain;
					lowerConstrain = Constrain.GreaterThanOrEqualTo(objective, ((AlgebraicExpression.ConstantNode)(min)).Build(problem));
					upperConstrain = Constrain.GreaterThan(((AlgebraicExpression.ConstantNode)(max)).Build(problem), objective);

					problem.Constrains.Add(lowerConstrain);
					problem.Constrains.Add(upperConstrain);

					IVariableAssignment newSolution = solver(problem);
					if (newSolution != null) {
						Console.WriteLine("Solution found: {0}", newSolution[objective].Value);
						bestKnown = newSolution;

						if (direction == ObjectiveDirection.Maximize) {
							bounds.Minimum = bestKnown[objective].Value;
						} else {
							bounds.Maximum = bestKnown[objective].Value + 1;
						}
					} else {
						Console.WriteLine("No solution found");
						if (direction == ObjectiveDirection.Maximize) {
							bounds.Maximum = min;
						} else {
							bounds.Minimum = max;
						}
					}

					problem.Constrains.Remove(lowerConstrain);
					problem.Constrains.Remove(upperConstrain);
				}

				result = bestKnown;
				return true;
			} finally {
				problem.ObjectiveVariable = objective;
			}
		}
	}
}
