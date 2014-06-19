namespace CSPS {
	// TODO: generalize?
	public struct Value {
		public int value;

		public Value(int value) {
			this.value = value;
		}

		public static bool Equal(Value a, Value b) {
			return a.value == b.value;
		}

		public override string ToString() {
			return string.Format("V<{0}>", value);
		}
	};
};
