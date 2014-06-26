using System;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		class Truth: AbstractConstrain {
			private Variable variable;
			public Truth(Variable variable) {
				this.variable = variable;
			}
			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad) {
				if (assignment[variable].Assigned) {
					if (assignment[variable].Value == 0) {
						return Failure;
					} else {
						return Success;
					}
				}
				if (assignment[variable].CanBe(0)) {
					return Restrict(variable, 0);
				}
				return Nothing;
			}
			protected override IEnumerable<Variable> GetDependencies() { yield return variable; }
			public override bool Satisfied(IVariableAssignment assignment) {
				return assignment[variable].Value != 0;
			}
			public override string Identifier { get { return string.Format("<Truth {0}>", variable.Identifier); } }
		}
	}
}
