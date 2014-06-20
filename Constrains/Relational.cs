using System;
using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	namespace Constrains {
		public class Relational: AbstractConstrain {
			private Variable[] dependencies;
			private Func<Value[], bool> func;

			public Relational(Func<Value[], bool> func, params Variable[] dependencies) {
				this.func = func;
				this.dependencies = dependencies;
			}

			public override IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad) {
				// TODO: AC with supports
				if (dependencies.All(var => assignment[var].Assigned)) {
					if (func(dependencies.Select(var => assignment[var].Value).ToArray())) {
						return Success;
					} else {
						return Failure;
					}
				}
				// TODO: else AC with supports
				return new List<ConstrainResult>();
			}

			public override string Identifier {
				get {
					return "<Relational>"; // TODO
				}
			}

			public override List<Variable> Dependencies {
				get {
					return dependencies.ToList(); // TODO
				}
			}

			// TODO
		}
	}
}
