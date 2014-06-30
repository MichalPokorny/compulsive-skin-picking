using System;
using System.Linq;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		public class Relational: AbstractConstrain {
			private Variable[] dependencies;
			private Func<int[], bool> func;

			public Relational(Func<int[], bool> func, params Variable[] dependencies) {
				this.func = func;
				this.dependencies = dependencies;
			}

			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers) {
				// TODO: AC with supports
				if (dependencies.All(var => assignment[var].Ground)) {
					if (func(dependencies.Select(var => assignment[var].Value).ToArray())) {
						return Success;
					} else {
						return Failure;
					}
				}
				// TODO: else AC with supports
				return new List<ConstrainResult>();
			}

			public override bool Satisfied(IVariableAssignment assignment) {
				return func(dependencies.Select(var => assignment[var].Value).ToArray());
			}

			protected override IEnumerable<Variable> GetDependencies() {
				return dependencies;
			}

			// TODO
		}
	}
}
