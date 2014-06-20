namespace CSPS {
	public struct ConstrainResult {
		public enum Type {
			Failure, Success, Restrict, Assign
		}

		public Type type;
		public Value value;
		public Variable variable;

		public static ConstrainResult Failure {
			get {
				return new ConstrainResult() { type = Type.Failure };
			}
		}

		public static ConstrainResult Success {
			get {
				return new ConstrainResult() { type = Type.Success };
			}
		}

		public bool IsFailure { get { return type == Type.Failure; } }
		public bool IsSuccess { get { return type == Type.Success; } }

		public static ConstrainResult Restrict(Variable variable, Value value) {
			// TODO: (vic hodnot?)
			return new ConstrainResult() { type = Type.Restrict, value = value, variable = variable };
		}

		public static ConstrainResult Assign(Variable variable, Value value) {
			// TODO: (vic hodnot?)
			return new ConstrainResult() { type = Type.Assign, value = value, variable = variable };
		}
	};
};
