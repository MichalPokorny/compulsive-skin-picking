using System;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		class VariableNot: AbstractConstrain {
			private Variable a, y;
			public VariableNot(Variable a, Variable y) {
				this.a = a; this.y = y;
			}
			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad) {
				if (assignment[a].Assigned) {
					if (assignment[a].Value == 0) {
						if (assignment[y].CanBe(0)) {
							return Restrict(y, 0);
						}
					} else {
						return Assign(y, 0);
					}
				}

				if (!assignment[a].CanBe(0)) {
					if (!assignment[y].Assigned) {
						return Assign(y, 0);
					}
				}

				if (assignment[y].Assigned) {
					if (assignment[y].Value == 0) {
						if (assignment[a].CanBe(0)) {
							return Restrict(a, 0);
						}
					} else {
						return Assign(a, 0);
					}
				}

				if (!assignment[y].CanBe(0)) {
					if (!assignment[a].Assigned) {
						return Assign(a, 0);
					}
				}

				return Nothing;
			}
			protected override IEnumerable<Variable> GetDependencies() {
				yield return a;
				yield return y;
			}
			public override bool Satisfied(IVariableAssignment assignment) {
				return (assignment[a].Value != 0) == (assignment[y].Value == 0);
			}
			public override string Identifier { get { return string.Format("<{0} == ! {1}>", a, y); } }
		}
	}
}
