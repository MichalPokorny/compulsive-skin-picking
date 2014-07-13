using System;
using System.Collections.Generic;
using CompulsiveSkinPicking.Constrains;
using System.Linq;

namespace CompulsiveSkinPicking {
	class SolutionState: IBacktrackable<SolutionState> {
		public SolutionState(Problem problem) {
			Assignment = problem.CreateEmptyAssignment();
			Unsolved = problem.AllConstrains();

			// TODO: variable choice heuristic...
			VariableChoice = problem.EnumerateVariables();

			//Supports = new Dictionary<Tuple<IConstrain, Variable, int>, IVariableAssignment>(),
			//ChangedSinceLastArcCheck = new HashSet<Variable>(initial.Assignment.Variables);

			history = new Stack<History>();
		}

		private SolutionState() {
		}

		private void Log(string fmt, params object[] args) {
			Debug.WriteLine("[Asg] {0}", string.Format(fmt, args));
		}

		//public ISet<Variable> ChangedSinceLastArcCheck;
		//public Dictionary<Tuple<IConstrain, Variable, int>, IVariableAssignment> Supports;

		public IVariableAssignment Assignment;
		// TODO: generalize?
		public IExternalEnumerator<int> ValueChoice;
		public IExternalEnumerator<Variable> VariableChoice;

		public SolutionState DeepDuplicate() {
			return new SolutionState() {
				VariableChoice = VariableChoice,
				ValueChoice = ValueChoice,
				Unsolved = Unsolved.ToList(),
				Assignment = Assignment.DeepDuplicate(),
				history = new Stack<History>()
			};
		}

		private struct History {
			public IExternalEnumerator<int> ValueChoice;
			public IExternalEnumerator<Variable> VariableChoice;
			public List<IConstrain> Unsolved;
		}

		private Stack<History> history;

		public bool CanRollbackToSavepoint {
			get {
				return history.Count > 0;
			}
		}

		public void RollbackToSavepoint() {
			VariableChoice = history.Peek().VariableChoice;
			ValueChoice = history.Peek().ValueChoice;
			Unsolved = history.Peek().Unsolved;
			Assignment.RollbackToSavepoint();

			history.Pop();
		}

		public void AddSavepoint() {
			history.Push(new History() {
				ValueChoice = ValueChoice,
				VariableChoice = VariableChoice,
				Unsolved = Unsolved.ToList() // XXX slow
			});
			Assignment.AddSavepoint();
		}

		// Store a list of constrains that remain to be satisfied
		public List<IConstrain> Unsolved;

		public void MarkConstrainSolved(Constrains.IConstrain constrain) {
			Unsolved.Remove(constrain);
		}

		public bool Success { get { return Unsolved.Count == 0; } }

		private bool ResolveFullyInstantiatedConstrains() {
			// TODO: for every *just solved* constrain
			List<IConstrain> solved = new List<IConstrain>();
			foreach (var constrain in Unsolved) {
				if (constrain.Dependencies.All(v => Assignment[v].Ground)) {
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

		public bool PropagateInitial() {
			if (!PropagateTriggers(
				    from v in Assignment.Variables
				where Assignment[v].Ground
				select PropagationTrigger.Assign(v, Assignment[v].Value)
			    )) {
				return false;
			}
			return true;
		}

		public void SetValueChoice() {
			ValueChoice = Assignment[VariableChoice.Value].EnumeratePossibleValues();
		}

		private IEnumerable<IConstrain> AffectedConstrainsOfTriggers(IEnumerable<PropagationTrigger> triggers) {
			var affectedVariables = triggers.Select(trigger => trigger.variable);
			return Unsolved.Where(constrain => constrain.Dependencies.Any(dep => affectedVariables.Contains(dep)));
		}

		private bool PropagateTriggers(IEnumerable<PropagationTrigger> inputTriggers) {
			List<PropagationTrigger> triggers = inputTriggers.ToList(); // XXX HACK
			List<PropagationTrigger> nextTriggers = new List<PropagationTrigger>();

			int round = 0;

			// TODO: kdyz se nejaka promenna ostrihala na jenom 1 prvek, tak to taky chci zpropagovat jako Assign
			while (triggers.Count > 0) {
				if (!ResolveFullyInstantiatedConstrains())
					return false;

				if (round++ > 100) {
					Debug.doDebug = true;
					Log("Round >100!");
					foreach (var trigger in triggers) {
						Log(trigger.ToString());
					}
				}

				List<Constrains.IConstrain> solved = new List<Constrains.IConstrain>();

				foreach (var constrain in AffectedConstrainsOfTriggers(triggers)) {
					Log("Propagate through {0}", constrain);
					// TODO: for the triggers that affect the constrain
					foreach (ConstrainResult result in constrain.Propagate(Assignment, triggers)) {
						Log("==> {0}", result);
						switch (result.type) {
						case ConstrainResult.Type.Failure:
							Log("Constrain {0} failed.", constrain);
							return false;
						case ConstrainResult.Type.Success:
							solved.Add(constrain);
							break;
						// TODO: constrain type "intersect"
						case ConstrainResult.Type.Restrict:
							if (Assignment[result.variable].CanBe(result.value)) {
								nextTriggers.Add(PropagationTrigger.Restrict(result.variable, result.value));
								Restrict(result.variable, result.value);

								if (!Assignment[result.variable].HasPossibleValues) {
									Log("Constrain {0} caused {1} to have empty value set", constrain, result.variable.Identifier);
									return false;
								}

								if (Assignment[result.variable].Ground) {
									nextTriggers.Add(PropagationTrigger.Assign(result.variable, Assignment[result.variable].Value));
								}
							}
							break;
						case ConstrainResult.Type.Assign:
							if (Assignment[result.variable].CanBe(result.value)) {
								nextTriggers.Add(PropagationTrigger.Assign(result.variable, result.value));
								Assign(result.variable, result.value);
								break;
							} else {
								Log("Constrain {0} assigns {1} to {2}, but that's not a possible value.", constrain, result.value, result.variable.Identifier);
								return false;
							}
						default:
							throw new Exception("Unknown constrain result"); // TODO
						}
					}
				}

				foreach (var constrain in solved) {
					Log("Constrain {0} is now solved", constrain);
					MarkConstrainSolved(constrain);
				}

				triggers = nextTriggers;
				nextTriggers = new List<PropagationTrigger>();
			}
			return true;
		}

		public void Assign(Variable variable, int value) {
			Assignment[variable].Value = value;
			/*
			if (ChangedSinceLastArcCheck != null && !ChangedSinceLastArcCheck.Contains(variable)) {
				ChangedSinceLastArcCheck.Add(variable);
			}
			*/
		}

		public void Restrict(Variable variable, int value) {
			Assignment[variable].Restrict(value);
			/*
			if (ChangedSinceLastArcCheck != null && !ChangedSinceLastArcCheck.Contains(variable)) {
				ChangedSinceLastArcCheck.Add(variable);
			}
			*/
		}

		/*
		public void Dump() {
			Console.WriteLine("Step:");
			if (VariableChoice != null && ValueChoice != null) {
				Console.WriteLine("\tWill try to assign {0} to {1}", ValueChoice.Value, VariableChoice.Value);
			}
			Console.WriteLine("\tAssignment:");
			Assignment.Dump();
		}
		*/

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

		public bool Progress() {
			Log("Starting progress");
			Variable variable = VariableChoice.Value;
			Log("Reading value choice");
			int value = ValueChoice.Value;

			Log("Assign: {0} <- {1}", variable, value);

			Assign(variable, value);

			Log("Assigned, propagating");
			if (!PropagateTriggers(new [] { PropagationTrigger.Assign(variable, value) })) {
				Log("Propagating assignment failed.");
				return false;
			}
			if (!ResolveFullyInstantiatedConstrains())
				return false;

			//if (!MakeArcConsistent(next)) {
			//	Log("Failed to make next step arc-consistent.");
			//	return false;
			//}

			if (VariableChoice.TryProgress(out VariableChoice)) {
				ValueChoice = Assignment[VariableChoice.Value].EnumeratePossibleValues();
			} else {
				ValueChoice = null;
			}

			return true;
		}

		public void Dump() {
			Console.WriteLine("SolutionState:");
			Assignment.Dump();
			Console.WriteLine("\tOutstanding constrains:");
			foreach (var c in Unsolved)
				Console.WriteLine("\t\t{0}", c);
		}
	}
}
