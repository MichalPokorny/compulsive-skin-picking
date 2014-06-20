using System;
using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	public class FunctionalConstrain: IConstrain {
		private Variable[] dependencies;
		private Func<Value[], bool> func;

		public FunctionalConstrain(Func<Value[], bool> func, params Variable[] dependencies) {
			this.func = func;
			this.dependencies = dependencies;
		}

		public List<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers) {
			// TODO: AC with supports
			if (dependencies.All(var => assignment.IsValueAssigned(var))) {
				if (func(dependencies.Select(var => assignment.GetAssignedValue(var)).ToArray())) {
					return new List<ConstrainResult>() { ConstrainResult.Success };
				} else {
					return new List<ConstrainResult>() { ConstrainResult.Failure };
				}
			}
			return new List<ConstrainResult>();
		}

		public string Identifier {
			get {
				return "<Functional Constrain>"; // TODO
			}
		}

		public List<Variable> Dependencies {
			get {
				return dependencies.ToList(); // TODO
			}
		}

		// TODO
	}
}
