using System;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		class VariableOr: AbstractConstrain {
			private Variable a, b, y;
			public VariableOr(Variable a, Variable b, Variable y) {
				this.a = a; this.b = b; this.y = y;
			}
			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers) {
				if (assignment[a].Ground && assignment[b].Ground && !assignment[y].Ground) {
					if (assignment[a].Value != 0 || assignment[b].Value != 0) {
						if (assignment[y].CanBe(0)) {
							return Restrict(y, 0);
						}
					} else {
						return Assign(y, 0);
					}
				}

				if (assignment[a].Ground && !assignment[b].Ground && assignment[y].Ground) {
					if (assignment[a].Value == 0 && assignment[y].Value != 0) {
						if (assignment[b].CanBe(0)) {
							return Restrict(b, 0);
						}
					}

					if (assignment[y].Value == 0) {
						return Assign(b, 0);
					}
				}

				if (assignment[b].Ground && !assignment[a].Ground && assignment[y].Ground) {
					if (assignment[b].Value == 0 && assignment[y].Value != 0) {
						if (assignment[a].CanBe(0)) {
							return Restrict(a, 0);
						}
					}

					if (assignment[y].Value == 0) {
						return Assign(a, 0);
					}
				}

				return Nothing;
			}
			protected override IEnumerable<Variable> GetDependencies() {
				yield return a;
				yield return b;
				yield return y;
			}
			public override bool Satisfied(IVariableAssignment assignment) {
				return (assignment[y].Value != 0) == (assignment[a].Value != 0 || assignment[b].Value != 0);
			}
			public override string ToString() { return string.Format("<{0} || {1} == {2}>", a, b, y); }
		}
	}
}
