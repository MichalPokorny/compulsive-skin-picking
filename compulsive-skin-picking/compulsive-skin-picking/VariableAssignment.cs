using System;
using System.Linq;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	public class VariableAssignment: IVariableAssignment {
		private Dictionary<Variable, Domain> domains;

		public VariableAssignment(IEnumerable<Variable> variables) {
			domains = new Dictionary<Variable, Domain>();
			foreach (var variable in variables) {
				domains[variable] = new Domain(variable.Range);
			}
		}

		private VariableAssignment() {
		}

		private class VariableManipulator: IVariableManipulator {
			private VariableAssignment _this;
			private Variable variable;

			public VariableManipulator(VariableAssignment _this, Variable variable) {
				// TODO: co kdyz ta promenna neexistuje?
				this._this = _this;
				this.variable = variable;
			}

			private Domain Domain {
				get {
					return _this.domains[variable];
				}
			}

			public int PossibleValueCount {
				get {
					return Domain.Size;
				}
			}

			public void Restrict(int v) {
				Domain.Restrict_WRONG_AND_SLOW(v);
			}

			public int Value {
				get {
					return Domain.Value;
				}
				set {
					Domain.Value = value;
				}
			}

			public bool Ground {
				get {
					return Domain.Ground;
				}
			}

			public bool CanBe(int value) {
				return Domain.Contains(value);
			}

			public bool HasPossibleValues {
				get {
					return !Domain.IsEmpty;
				}
			}

			public IExternalEnumerator<int> EnumeratePossibleValues() {
				// SLOW AND STUPID
				Debug.WriteLine("Enumerating possible values for {0}", variable.Identifier);
				if (HasPossibleValues) {
					return new ValuesEnumerator(Domain.LEGACY_RANGES); // XXX HACK
				} else {
					throw new Exception("No possible values");
				}
			}
		}

		public IVariableManipulator this[Variable variable] {
			get {
				return new VariableManipulator(this, variable);
			}
		}

		public IEnumerable<Variable> Variables {
			get {
				return domains.Keys;
			}
		}

		private class ValuesEnumerator: IExternalEnumerator<int> {
			private int sampleLength;
			private int range;
			private ValueRange[] ranges;

			public ValuesEnumerator(ValueRange[] ranges) {
				this.ranges = ranges;
				this.range = 0;
				this.sampleLength = 1;
			}

			public bool TryProgress(out IExternalEnumerator<int> next) {
				int nextRange = range + 1;
				Debug.WriteLine("TryProgress");
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

			public int Value {
				get {
					int index = sampleLength - 1;
					Debug.WriteLine("range={0} sampleindex={1} ranges.Length={2}", range, index, ranges.Length);
					Debug.WriteLine("Ranges[{0}] (={1}) [{2}]", range, ranges[range], index);
					Debug.Write("All ranges:");
					foreach (var r in ranges) {
						Debug.Write(" {0}", r);
					}
					Debug.WriteLine("");
					Debug.WriteLine("Value returned: {0}", ranges[range][index]);
					return ranges[range][index];
				}
			}
		};

		// SLOW
		public IVariableAssignment DeepDuplicate() {
			VariableAssignment dup = new VariableAssignment();
			dup.domains = new Dictionary<Variable, Domain>();
			foreach (var pair in domains) {
				// XXX HACK
				dup.domains.Add(pair.Key, pair.Value.DeepDuplicate());
			}
			return dup;
		}

		public void Dump() {
			Console.WriteLine("\t{0}", string.Join(" ", from pair in domains
			                                            where pair.Value.Ground
			                                            select string.Format("{0}={1}", pair.Key.Identifier, pair.Value.Value)));
			foreach (var pair in domains) {
				if (!pair.Value.Ground) {
					Console.WriteLine("\t\t{0}: {1}", pair.Key.Identifier, pair.Value);
				}
			}
		}

		public void AddSavepoint() {
			foreach (var domain in domains.Values) {
				domain.AddSavepoint();
			}
		}

		public void RollbackToSavepoint() {
			foreach (var domain in domains.Values) {
				domain.RollbackToSavepoint();
			}
		}

		public bool CanRollbackToSavepoint {
			get {
				throw new NotImplementedException("TODO");
			}
		}
	}
}
