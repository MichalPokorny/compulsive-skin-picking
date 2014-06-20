using System;
using System.Collections.Generic;

namespace CSPS {
	public struct ValueRange {
		public bool MinUnbounded;
		public bool MaxUnbounded;
		public int Minimum;
		public int Maximum;

		public static implicit operator ValueRange(int value) {
			return new ValueRange() {
				Minimum = value,
				Maximum = value
			};
		}

		public IEnumerable<ValueRange> SplitAndRemove(int value) {
			if (value < Minimum || value >= Maximum) {
				throw new Exception("Value not in range");
			}

			if (!MinUnbounded && value == Minimum) {
				if (MaxUnbounded || value + 1 < Maximum) {
					yield return new ValueRange() {
						MinUnbounded = false,
						MaxUnbounded = MaxUnbounded,
						Minimum = value + 1,
						Maximum = Maximum
					};
				}
			} else if (!MaxUnbounded && value == Maximum - 1) {
				if (MinUnbounded || value - 2 > Minimum) {
					yield return new ValueRange() {
						MinUnbounded = MinUnbounded,
						MaxUnbounded = false,
						Minimum = Minimum,
						Maximum = value - 2
					};
				}
			} else {
				yield return new ValueRange() {
					MinUnbounded = MinUnbounded,
					MaxUnbounded = false,
					Minimum = Minimum,
					Maximum = value
				};
				yield return new ValueRange() {
					MinUnbounded = false,
					MaxUnbounded = MaxUnbounded,
					Minimum = value + 1,
					Maximum = Maximum
				};
			}
		}

		public bool Contains(Value v) {
			return (MinUnbounded || Minimum <= v.value) && (MaxUnbounded || Maximum > v.value);
		}

		public bool IsSingleton {
			get {
				return !MinUnbounded && !MaxUnbounded && Minimum == Maximum;
			}
		}

		public int Singleton {
			get {
				if (!IsSingleton) throw new Exception("Not a singleton");
				return Minimum;
			}
		}

		public bool AtLeastElements(int x) {
			return MinUnbounded || MaxUnbounded || Maximum - Minimum >= x;
		}

		public override string ToString() {
			if (IsSingleton) {
				return string.Format("VR<{0}>", Singleton);
			} else {
				return string.Format("VR<{0}...{1}>", MinUnbounded ? "-" : Minimum.ToString(), MaxUnbounded ? "-" : Maximum.ToString());
			}
		}

		public Value this[int x] {
			get {
				int value;
				if (MinUnbounded) {
					if (MaxUnbounded) {
						if (x % 2 == 0) {
							value = x / 2;
						} else {
							value = -(x - 1) / 2;
						}
					} else {
						value = Maximum - 1 - x;
					}
				} else {
					value = Minimum + x;
				}

				Value v = new Value(value);
				if (!Contains(v)) {
					throw new Exception(string.Format("We don't contain {0}, but we're trying to return that!", v));
				}
				return v;
			}
		}
	}
}
