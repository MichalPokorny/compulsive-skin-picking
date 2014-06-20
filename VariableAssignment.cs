using System;
using System.Linq;
using System.Collections.Generic;

namespace CSPS {
	public class VariableAssignment: IVariableAssignment {
		private Dictionary<string, List<ValueRange>> values;

		public VariableAssignment(IEnumerable<Variable> variables) {
			values = new Dictionary<string, List<ValueRange>>();
			foreach (var variable in variables) {
				values[variable.Identifier] = new List<ValueRange>() { variable.Range };
			}
		}

		private VariableAssignment() {}

		private class VariableManipulator: IVariableManipulator {
			private VariableAssignment _this;
			private Variable variable;
			public VariableManipulator(VariableAssignment _this, Variable variable) {
				// TODO: co kdyz ta promenna neexistuje?
				this._this = _this;
				this.variable = variable;
			}

			private List<ValueRange> Values {
				get {
					return _this.values[variable.Identifier];
				}
			}

			public void Restrict(Value v) {
				foreach (var range in Values) {
					if (range.Contains(v)) {
						Values.Remove(range);
						foreach (var newRange in range.SplitAndRemove(v.value)) {
							Values.Add(newRange);
						}
						return;
					}
				}
				throw new Exception(string.Format("Cannot remove {0} from domain of {1}, it's not in the domain", v.value, variable.Identifier));
			}

			public Value Value {
				get {
					if (!Assigned) {
						throw new Exception(string.Format("No value assigned to {0} yet", variable.Identifier));
					}
					return new Value(Values[0].Singleton);
				}
				set {
					if (!CanBe(value)) {
						throw new Exception(string.Format("Cannot assign {0} to variable {1}", value.value, variable.Identifier));
					}
					Values.Clear();
					Values.Add(value.value);
				}
			}
			public bool Assigned {
				get {
					return Values.Count == 1 && Values[0].IsSingleton;
				}
			}
			public bool CanBe(Value value) {
				foreach (var range in Values) {
					if (range.Contains(value)) {
						return true;
					}
				}
				return false;
			}
		}

		public IVariableManipulator this[Variable variable] {
			get {
				return new VariableManipulator(this, variable);
			}
		}

		private class ValuesEnumerator: IExternalEnumerator<Value> {
			private int sampleLength;
			private int range;
			private ValueRange[] ranges;

			public ValuesEnumerator(ValueRange[] ranges) {
				this.ranges = ranges;
				this.range = 0;
				this.sampleLength = 1;
			}

			public bool TryProgress(out IExternalEnumerator<Value> next) {
				if (range + 1 < ranges.Length) {
					next = new ValuesEnumerator(ranges) {
						range = range + 1,
						sampleLength = sampleLength
					};
					return true;
				} else {
					int r;
					for (r = 0; r < ranges.Length; r++) {
						if (ranges[r].AtLeastElements(sampleLength + 1)) {
							Console.WriteLine("Range {0} has as least {1} elements", r, sampleLength + 1);
							break;
						}
					}

					if (r == ranges.Length) {
						next = null;
						return false;
					} else {
						next = new ValuesEnumerator(ranges) {
							range = r,
							sampleLength = sampleLength + 1
						};
						return true;
					}
				}
			}

			public Value Value {
				get {
					Console.WriteLine("Ranges[{0}] (={1}) [{2}]", range, ranges[range], sampleLength - 1);
					return ranges[range][sampleLength - 1];
				}
			}
		};

		public IExternalEnumerator<Value> EnumeratePossibleValues(Variable variable) {
			// SLOW AND STUPID
			Console.WriteLine("Enumerating possible values for {0}", variable.Identifier);
			return new ValuesEnumerator(values[variable.Identifier].ToList().ToArray());
		}

		// SLOW
		public IVariableAssignment Duplicate() {
			VariableAssignment dup = new VariableAssignment();
			dup.values = new Dictionary<string, List<ValueRange>>();
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
