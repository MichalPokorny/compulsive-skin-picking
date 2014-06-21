using System;
using System.Collections.Generic;

namespace CSPS {
	namespace Constrains {
		// A simple "equal" constrain.
		public class Equal: AbstractConstrain {
			private Variable a, b;

			public Equal(Variable a, Variable b) {
				this.a = a; this.b = b;
			}

			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad) {
				if (a == b) {
					return Success;
				}

				if (assignment[a].Assigned && assignment[b].Assigned) {
					if (Value.Equal(assignment[a].Value, assignment[b].Value)) {
						return Success;
					} else {
						return Failure;
					}
				}

				foreach (var trigger in triggers) {
					switch (trigger.type) {
						case PropagationTrigger.Type.Assign:
							if (trigger.variable == a) {
								return Assign(b, trigger.value);
							}

							if (trigger.variable == b) {
								return Assign(a, trigger.value);
							}

							break;
						case PropagationTrigger.Type.Restrict:
							if (trigger.variable == a) {
								return Restrict(b, trigger.value);
							}

							if (trigger.variable == b) {
								return Restrict(a, trigger.value);
							}

							break;
					}
				}

				// Nothing to do.
				Log("I don't know what to do.");
				return Nothing;
			}

			public override string Identifier {
				get { return string.Format("[{0} == {1}]", a.Identifier, b.Identifier); }
			}

			public override List<Variable> Dependencies {
				get { return new List<Variable>() { a, b }; }
			}
		}
	}
}
