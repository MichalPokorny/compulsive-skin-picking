using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	namespace Constrains {
		public class Or: AbstractConstrain {
			private IConstrain[] constrains;

			public Or(params IConstrain[] constrains) {
				this.constrains = constrains;
			}

			private class Scratchpad: IScratchpad {
				public List<IConstrain> Unsatisfiable;
				public Dictionary<IConstrain, IScratchpad> Scratchpads;

				public IScratchpad Duplicate() {
					Scratchpad duplicate = new Scratchpad() {
						Unsatisfiable = Unsatisfiable.ToList()
					};
					duplicate.Scratchpads = new Dictionary<IConstrain, IScratchpad>();
					foreach (var pair in Scratchpads) {
						duplicate.Scratchpads.Add(pair.Key, pair.Value == null ? null : pair.Value.Duplicate());
					}
					return duplicate;
				}
			}

			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad _scratchpad) {
				Log("Propagating");
				Scratchpad scratchpad;
				if (_scratchpad == null) {
					scratchpad = new Scratchpad() {
						Scratchpads = new Dictionary<IConstrain, IScratchpad>(),
						Unsatisfiable = constrains.ToList() /* XXX HACK */
					};
					_scratchpad = scratchpad;
				} else {
					scratchpad = (Scratchpad) _scratchpad;
				}

				var results = new List<ConstrainResult>();
				var failed = new List<IConstrain>();
				foreach (var constrain in scratchpad.Unsatisfiable) {
					IScratchpad innerScratchpad;
					if (scratchpad.Scratchpads.ContainsKey(constrain)) {
						innerScratchpad = scratchpad.Scratchpads[constrain];
					} else {
						innerScratchpad = null;
					}
					foreach (var result in constrain.Propagate(assignment, triggers, ref innerScratchpad)) {
						if (result.IsFailure) {
							failed.Add(constrain);
						} else if (result.IsSuccess) {
							results.Add(result);
							break;
						} else {
							results.Add(result);
						}
					}
					scratchpad.Scratchpads[constrain] = innerScratchpad;
				}

				foreach (var constrain in failed) {
					scratchpad.Unsatisfiable.Remove(constrain);
				}

				if (scratchpad.Unsatisfiable.Count == constrains.Length) {
					results.Add(ConstrainResult.Failure);
				}

				return results;
			}

			public override bool Satisfied(IVariableAssignment assignment) {
				return constrains.All(c => c.Satisfied(assignment));
			}

			public override List<Variable> Dependencies {
				get {
					// TODO: slow
					HashSet<Variable> set = new HashSet<Variable>();
					foreach (var constrain in constrains) {
						foreach (var variable in constrain.Dependencies) {
							set.Add(variable);
						}
					}
					return set.ToList();
				}
			}

			public override string Identifier { get { return "<Or>"; } }
		}
	}
}
