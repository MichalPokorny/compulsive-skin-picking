using System;
using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	public class VariableAssignment: IVariableAssignment {
		public Dictionary<string, List<int>> values;

		public IVariableAssignment Restrict(Variable variable, Value removedValue) {
			// SLOW
			VariableAssignment dup = (VariableAssignment) Duplicate();
			if (!dup.values[variable.Identifier].Remove(removedValue.value)) {
				throw new Exception(string.Format("Cannot remove {0} from domain of {1}", removedValue.value, variable.Identifier));
			}
			return dup;
		}

		public bool ValuePossible(Variable variable, Value value) {
			return values[variable.Identifier].Contains(value.value);
		}

		public IVariableAssignment Assign(Variable variable, Value removedValue) {
			// SLOW
			VariableAssignment dup = (VariableAssignment) Duplicate();
			if (!dup.values[variable.Identifier].Contains(removedValue.value)) {
				throw new Exception(string.Format("Cannot assign {0} to variable {1}", removedValue.value, variable.Identifier));
			}
			dup.values[variable.Identifier] = new List<int> { removedValue.value };
			return dup;
		}

		public IExternalEnumerator<Value> EnumeratePossibleValues(Variable variable) {
			// SLOW
			return values[variable.Identifier].GetTransformedExternalEnumerator(x => new Value(x));
		}

		public bool IsValueAssigned(Variable variable) {
			return values[variable.Identifier].Count == 1;
		}

		public Value GetAssignedValue(Variable variable) {
			if (!IsValueAssigned(variable)) {
				throw new Exception(string.Format("No value assigned to {0} yet", variable.Identifier));
			}
			return new Value(values[variable.Identifier][0]);
		}

		// SLOW
		public IVariableAssignment Duplicate() {
			VariableAssignment dup = new VariableAssignment();
			dup.values = new Dictionary<string, List<int>>();
			foreach (var pair in values) {
				// XXX HACK
				dup.values.Add(pair.Key, pair.Value.ToList());
			}
			return dup;
		}

		public void Dump() {
			foreach (var pair in values) {
				Console.Write("\t\t{0}: ", pair.Key);
				foreach (var val in pair.Value) {
					Console.Write("{0} ", val);
				}
				Console.WriteLine();
			}
		}
	}
}
