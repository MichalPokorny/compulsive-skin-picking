using System;
using System.Collections.Generic;

namespace CSPS {
	namespace Constrains {
		public abstract class AbstractConstrain: IConstrain {
			public abstract IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad);
			public abstract List<Variable> Dependencies { get; }
			public abstract bool Satisfied(IReadonlyValueAssignment assignment);
			public abstract string Identifier { get; }

			protected IEnumerable<ConstrainResult> Failure {
				get {
					yield return ConstrainResult.Failure;
					yield break;
				}
			}

			protected IEnumerable<ConstrainResult> Success {
				get {
					yield return ConstrainResult.Success;
					yield break;
				}
			}

			protected IEnumerable<ConstrainResult> Assign(Variable variable, Value value) {
				yield return ConstrainResult.Assign(variable, value);
				yield break;
			}

			protected IEnumerable<ConstrainResult> Restrict(Variable variable, Value value) {
				yield return ConstrainResult.Restrict(variable, value);
				yield break;
			}

			protected IEnumerable<ConstrainResult> Nothing {
				get {
					yield break;
				}
			}

			protected void Log(string str, params object[] args) {
				Debug.WriteLine("{0} {1}", Identifier, string.Format(str, args));
			}
		};
	}
};
