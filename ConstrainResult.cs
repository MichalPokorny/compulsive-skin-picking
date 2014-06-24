using System;

namespace CSPS {
	public struct ConstrainResult {
		public enum Type {
			Failure, Success, Restrict, Assign
		}

		public Type type;
		public int value;
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

		public static ConstrainResult Restrict(Variable variable, int value) {
			// TODO: (vic hodnot?)
			return new ConstrainResult() { type = Type.Restrict, value = value, variable = variable };
		}

		public static ConstrainResult Assign(Variable variable, int value) {
			// TODO: (vic hodnot?)
			return new ConstrainResult() { type = Type.Assign, value = value, variable = variable };
		}

		public override string ToString() {
			string str;
			switch (type) {
				case Type.Failure:
					str = "Failure";
					break;
				case Type.Success:
					str = "Success";
					break;
				case Type.Restrict:
					str = string.Format("Restrict<{0} != {1}>", variable, value);
					break;
				case Type.Assign:
					str = string.Format("Assign<{0} <-- {1}>", variable, value);
					break;
				default:
					throw new Exception("Unknown type");
			}
			return string.Format("ConstrainResult<{0}>", str);
		}
	};
};
