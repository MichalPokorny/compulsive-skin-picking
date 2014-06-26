using System;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		class VariableAnd: AbstractConstrain {
			private Variable a, b, y;
			public VariableAnd(Variable a, Variable b, Variable y) {
				this.a = a; this.b = b; this.y = y;
			}
			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad) {
				if (!assignment[y].Assigned) {
					if ((assignment[a].Assigned && assignment[a].Value == 0) || (assignment[b].Assigned && assignment[b].Value == 0)) {
						return Assign(y, 0);
					}
				}

				if (!assignment[a].CanBe(0) && !assignment[b].CanBe(0) && assignment[y].CanBe(0)) {
					return Restrict(y, 0);
				}

				if (assignment[a].Assigned && assignment[b].Assigned) {
					if (assignment[a].Value != 0 || assignment[b].Value != 0) {
						return Assign(y, 0);
					} else if (assignment[y].CanBe(0)) {
						return Restrict(y, 0);
					}
				}

				if (assignment[y].Assigned && assignment[y].Value == 0) {
					if (assignment[a].Assigned && assignment[a].Value != 0) {
						return Assign(b, 0);
					}
					if (assignment[b].Assigned && assignment[b].Value != 0) {
						return Assign(a, 0);
					}
				}

				if (assignment[y].Assigned && assignment[y].Value != 0) {
					if (assignment[a].CanBe(0)) return Restrict(a, 0);
					if (assignment[b].CanBe(0)) return Restrict(b, 0);
				}

				return Nothing;
			}
			protected override IEnumerable<Variable> GetDependencies() {
				yield return a;
				yield return b;
				yield return y;
			}
			public override bool Satisfied(IVariableAssignment assignment) {
				return (assignment[y].Value != 0) == (assignment[a].Value != 0 && assignment[b].Value != 0);
			}
			public override string Identifier { get { return string.Format("<{0} && {1} == {2}>", a, b, y); } }
		}
	}
}