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
				Debug.WriteLine("Remove {0} from the domain of {1}", v.value, variable.Identifier);
				foreach (var range in Values) {
					if (range.Contains(v)) {
						Values.Remove(range);
						Debug.WriteLine("-{0}", range);
						foreach (var newRange in range.SplitAndRemove(v.value)) {
							Values.Add(newRange);
							Debug.WriteLine("+{0}", newRange);
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
					Values.Add(new ValueRange(value.value));
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

			public IExternalEnumerator<Value> EnumeratePossibleValues() {
				// SLOW AND STUPID
				Debug.WriteLine("Enumerating possible values for {0}", variable.Identifier);
				if (_this.values[variable.Identifier].Count > 0) {
					return new ValuesEnumerator(_this.values[variable.Identifier].ToList().ToArray());
				} else {
					return null;
				}
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
				int nextRange = range + 1;
				do {
					if (nextRange < ranges.Length) {
						next = new ValuesEnumerator(ranges) {
							range = nextRange,
							sampleLength = sampleLength
						};

						if (ranges[nextRange].AtLeastElements(sampleLength)) {
							return true;
						} else {
							nextRange++;
						}
					} else {
						int r;
						for (r = 0; r < ranges.Length; r++) {
							if (ranges[r].AtLeastElements(sampleLength + 1)) {
								Debug.WriteLine("Range {0} has as least {1} elements", r, sampleLength + 1);
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
				} while (true);
			}

			public Value Value {
				get {
					int index = sampleLength - 1;
					Debug.WriteLine("range={0} sampleindex={1} ranges.Length={2}", range, index, ranges.Length);
					Debug.WriteLine("Ranges[{0}] (={1}) [{2}]", range, ranges[range], index);
					Debug.Write("All ranges:");
					foreach (var r in ranges) {
						Debug.Write(" {0}", r);
					}
					Debug.WriteLine("");
					return ranges[range][index];
				}
			}
		};

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
