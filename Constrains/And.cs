using System.Linq;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		public class And: AbstractConstrain {
			private IConstrain[] constrains;

			public And(params IConstrain[] constrains) {
				this.constrains = constrains;
			}

			private class Scratchpad: IScratchpad {
				public List<IConstrain> Unsatisfied;
				public Dictionary<IConstrain, IScratchpad> Scratchpads;

				public IScratchpad Duplicate() {
					Scratchpad duplicate = new Scratchpad() {
						Unsatisfied = Unsatisfied.ToList()
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
						Unsatisfied = constrains.ToList() /* XXX HACK */
					};
					_scratchpad = scratchpad;
				} else {
					scratchpad = (Scratchpad) _scratchpad;
				}

				var results = new List<ConstrainResult>();
				var satisfied = new List<IConstrain>();
				foreach (var constrain in scratchpad.Unsatisfied) {
					IScratchpad innerScratchpad;
					if (scratchpad.Scratchpads.ContainsKey(constrain)) {
						innerScratchpad = scratchpad.Scratchpads[constrain];
					} else {
						innerScratchpad = null;
					}
					foreach (var result in constrain.Propagate(assignment, triggers, ref innerScratchpad)) {
						if (result.IsFailure) {
							results.Add(result);
							break;
						} else if (result.IsSuccess) {
							satisfied.Add(constrain);
						} else {
							results.Add(result);
						}
					}
					scratchpad.Scratchpads[constrain] = innerScratchpad;
				}

				foreach (var constrain in satisfied) {
					scratchpad.Unsatisfied.Remove(constrain);
				}

				if (scratchpad.Unsatisfied.Count == 0) {
					results.Add(ConstrainResult.Success);
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

			public override string Identifier { get { return "<And>"; } }
		}
	}
}
