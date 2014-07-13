using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CompulsiveSkinPicking.Constrains;

namespace CompulsiveSkinPicking {
	public class Solver {
		private void Log(string fmt, params object[] args) {
			Debug.WriteLine("[Solver] {0}", string.Format(fmt, args));
		}

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
			} while (!cancellationToken.IsCancellationRequested);

			result = null;
			return false; // Cancelled
		}

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
			return SolveParallel(problem, out result);
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

				Debug.WriteLine("Initial solution: {0}", bestKnown[objective].Value);

				if (direction == ObjectiveDirection.Maximize) {
					bounds.Minimum = bestKnown[objective].Value;
				} else {
					bounds.Maximum = bestKnown[objective].Value + 1;
				}

				// Binary search
				while (!bounds.IsSingleton) {
					Debug.WriteLine("Bounds: {0}", bounds);
					// int middle = (bounds.Maximum + bounds.Minimum) / 2;
					int min, max;

					if (direction == ObjectiveDirection.Maximize) {
						min = bestKnown[objective].Value + 1; // bounds.Maximum + 1;
						max = bounds.Maximum;
					} else {
						min = bounds.Minimum;
						max = bestKnown[objective].Value;
					}
					Debug.WriteLine("We will be looking for {0} <= X < {1}", min, max);

					IConstrain lowerConstrain, upperConstrain;
					lowerConstrain = Constrain.GreaterThanOrEqualTo(objective, ((AlgebraicExpression.ConstantNode)(min)).Build(problem));
					upperConstrain = Constrain.GreaterThan(((AlgebraicExpression.ConstantNode)(max)).Build(problem), objective);

					problem.Constrains.Add(lowerConstrain);
					problem.Constrains.Add(upperConstrain);

					IVariableAssignment newSolution = solver(problem);
					if (newSolution != null) {
						Debug.WriteLine("Solution found: {0}", newSolution[objective].Value);
						bestKnown = newSolution;

						if (direction == ObjectiveDirection.Maximize) {
							bounds.Minimum = bestKnown[objective].Value;
						} else {
							bounds.Maximum = bestKnown[objective].Value + 1;
						}
					} else {
						Debug.WriteLine("No solution found");
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
