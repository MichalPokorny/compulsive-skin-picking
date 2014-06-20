using System;
using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	namespace Constrains {
		public class InvertibleBinary: AbstractConstrain {
			// TODO: local typedef?
			private Variable a, b, c;
			private Func<Value, Value, Value> ab_to_c, ac_to_b, bc_to_a;

			public InvertibleBinary(Variable a, Variable b, Variable c,
				Func<Value, Value, Value> ab_to_c,
				Func<Value, Value, Value> ac_to_b,
				Func<Value, Value, Value> bc_to_a) {
				this.a = a; this.b = b; this.c = c;
				this.ab_to_c = ab_to_c; this.ac_to_b = ac_to_b; this.bc_to_a = bc_to_a;
			}

			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad) {
				Log("Propagating");
				// TODO: AC with supports
				if (assignment[a].Assigned) {
					if (assignment[b].Assigned && !assignment[c].Assigned) {
						return Assign(c, ab_to_c(assignment[a].Value, assignment[b].Value));
					} else if (assignment[c].Assigned && !assignment[b].Assigned) {
						return Assign(b, ac_to_b(assignment[a].Value, assignment[b].Value));
					} else if (assignment[b].Assigned && assignment[c].Assigned) {
						if (Value.Equal(assignment[c].Value, ab_to_c(assignment[a].Value, assignment[b].Value))) {
							Log("Success.");
							return Success;
						} else {
							Log("Failure!");
							return Failure;
						}
					}
				} else {
					if (assignment[b].Assigned && assignment[c].Assigned) {
						return Assign(a, bc_to_a(assignment[b].Value, assignment[c].Value));
					}
				}
				return Nothing;
				// TODO: AC with supports
			}

			public override string Identifier {
				get {
					return string.Format("<{0}+{1}={2}>", a.Identifier, b.Identifier, c.Identifier);
				}
			}

			public override List<Variable> Dependencies {
				get {
					return new List<Variable>() { a, b, c };
				}
			}
		}
	}
}
