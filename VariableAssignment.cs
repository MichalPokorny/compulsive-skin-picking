using System;
using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	public class VariableAssignment: IVariableAssignment {
		public Dictionary<string, List<int>> values;

		private class VariableManipulator: IVariableManipulator {
			private VariableAssignment _this;
			private Variable variable;
			public VariableManipulator(VariableAssignment _this, Variable variable) {
				// TODO: co kdyz ta promenna neexistuje?
				this._this = _this;
				this.variable = variable;
			}

			private List<int> Values {
				get {
					return _this.values[variable.Identifier];
				}
				set {
					_this.values[variable.Identifier] = value;
				}
			}

			public void Restrict(Value v) {
				if (!Values.Remove(v.value)) {
					throw new Exception(string.Format("Cannot remove {0} from domain of {1}", v.value, variable.Identifier));
				}
			}

			public Value Value {
				get {
					if (!Assigned) {
						throw new Exception(string.Format("No value assigned to {0} yet", variable.Identifier));
					}
					return new Value(Values[0]);
				}
				set {
					if (!Values.Contains(value.value)) {
						throw new Exception(string.Format("Cannot assign {0} to variable {1}", value.value, variable.Identifier));
					}
					Values = new List<int> { value.value };
				}
			}
			public bool Assigned {
				get {
					return _this.values[variable.Identifier].Count == 1;
				}
			}
			public bool CanBe(Value v) {
				return _this.values[variable.Identifier].Contains(v.value);
			}
		}

		public IVariableManipulator this[Variable variable] {
			get {
				return new VariableManipulator(this, variable);
			}
		}

		public IExternalEnumerator<Value> EnumeratePossibleValues(Variable variable) {
			// SLOW
			return values[variable.Identifier].GetTransformedExternalEnumerator(x => new Value(x));
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
