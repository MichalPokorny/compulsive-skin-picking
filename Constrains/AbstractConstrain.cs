using System;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		public abstract class AbstractConstrain: IConstrain {
			public abstract IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers, ref IScratchpad scratchpad);
			public abstract bool Satisfied(IVariableAssignment assignment);
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

			protected IEnumerable<ConstrainResult> Assign(Variable variable, int value) {
				yield return ConstrainResult.Assign(variable, value);
				yield break;
			}

			protected IEnumerable<ConstrainResult> Restrict(Variable variable, int value) {
				yield return ConstrainResult.Restrict(variable, value);
				yield break;
			}

			protected IEnumerable<ConstrainResult> Nothing {
				get {
					yield break;
				}
			}

			protected abstract IEnumerable<Variable> GetDependencies();
			private IEnumerable<Variable> _dependencies;
			public IEnumerable<Variable> Dependencies {
				get {
					if (_dependencies == null) _dependencies = GetDependencies();
					return _dependencies;
				}
			}

			protected void Log(string str, params object[] args) {
				Debug.WriteLine("{0} {1}", Identifier, string.Format(str, args));
			}
		};
	}
};
