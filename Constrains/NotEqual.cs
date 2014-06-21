using System;
using System.Collections.Generic;

namespace CSPS {
	namespace Constrains {
		// A simple "not equal" constrain.
		public class NotEqual: AbstractConstrain {
			private Variable a, b;

			public NotEqual(Variable a, Variable b) {
				this.a = a; this.b = b;
			}

			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad) {
				var results = new List<ConstrainResult>();

				if (a == b) {
					return Failure;
				}

				foreach (var trigger in triggers) {
					if (trigger.type == PropagationTrigger.Type.Assign) {
						if (trigger.variable == a) {
							if (assignment[b].Assigned && Value.Equal(assignment[b].Value, trigger.value)) {
								Log("Assigning {0} to A({1}) made it equal B({2})!", trigger.value, a.Identifier, b.Identifier);
								return Failure;
							} else {
								Log("Assigning {0} to A({1}).", trigger.value, a.Identifier);
								results.Add(ConstrainResult.Restrict(b, trigger.value));
							}
						}

						if (trigger.variable == b) {
							if (assignment[a].Assigned && Value.Equal(assignment[a].Value, trigger.value)) {
								Log("Assigning {0} to B({1}) made it equal A({2})!", trigger.value, b.Identifier, a.Identifier);
								return Failure;
							} else {
								Log("Assigning {0} to B({1}).", trigger.value, b.Identifier);
								results.Add(ConstrainResult.Restrict(a, trigger.value));
							}
						}
					}
				}
				if (assignment[a].Assigned && assignment[b].Assigned) {
					if (Value.Equal(assignment[a].Value, assignment[b].Value)) {
						Debug.WriteLine("Constrain fail");
						return Failure;
					} else {
						results.Add(ConstrainResult.Success);
					}
				}
				return results;
			}

			public override string Identifier {
				get { return string.Format("[{0} != {1}]", a.Identifier, b.Identifier); }
			}

			public override List<Variable> Dependencies {
				get { return new List<Variable>() { a, b }; }
			}
		}
	}
}