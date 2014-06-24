using System;
using System.Collections.Generic;

namespace CSPS {
	public struct ValueRange {
		public int Minimum;
		public int Maximum;

		public ValueRange(int value) {
			Minimum = value;
			Maximum = value + 1;
		}
		public ValueRange(int min, int max) {
			Minimum = min; Maximum = max;
			if (Minimum >= Maximum) {
				throw new Exception("Invalid range settings");
			}
		}

		public IEnumerable<ValueRange> SplitAndRemove(int value) {
			if (value < Minimum || value >= Maximum) {
				throw new Exception(string.Format("Value {0} not in range {1}", value, this));
			}

			Debug.WriteLine("Remove {0} from {1}", value, this);
			if (value == Minimum) {
				if (Minimum + 1 < Maximum) {
					yield return new ValueRange() {
						Minimum = value + 1,
						Maximum = Maximum
					};
				} else {
					Debug.WriteLine("(Empty new set A)");
				}
			} else if (value == Maximum - 1) {
				if (Maximum - 1 > Minimum) {
					yield return new ValueRange() {
						Minimum = Minimum,
						Maximum = Maximum - 1
					};
				} else {
					Debug.WriteLine("(Empty new set B)");
				}
			} else {
				yield return new ValueRange() {
					Minimum = Minimum,
					Maximum = value
				};
				yield return new ValueRange() {
					Minimum = value + 1,
					Maximum = Maximum
				};
			}
		}

		public bool Contains(int v) {
			return Minimum <= v && Maximum > v;
		}

		public bool IsSingleton {
			get {
				return Minimum + 1 == Maximum;
			}
		}

		public int Singleton {
			get {
				if (!IsSingleton) throw new Exception("Not a singleton");
				return Minimum;
			}
		}

		public bool AtLeastElements(int x) {
			return Maximum - Minimum >= x;
		}

		public override string ToString() {
			if (IsSingleton) {
				return string.Format("VR<{0}>", Singleton);
			} else {
				return string.Format("VR<{0}...{1}>", Minimum, Maximum);
			}
		}

		public int this[int x] {
			get {
				int value = Minimum + x;
				if (!Contains(value)) {
					throw new Exception(string.Format("We don't contain {0}, but we're trying to return that!", value));
				}
				return value;
			}
		}

		public static ValueRange Boolean {
			get {
				return new ValueRange(0, 2);
			}
		}
	}
}
