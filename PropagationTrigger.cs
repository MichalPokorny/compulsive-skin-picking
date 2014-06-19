using System.Collections.Generic;

namespace CSPS {
	public struct PropagationTrigger {
		public enum Type {
			Restrict, Assign
		};

		public Type type;
		public Variable variable;
		public Value value;

		public static PropagationTrigger Restrict(Variable variable, Value value) {
			return new PropagationTrigger() { type = Type.Restrict, variable = variable, value = value };
		}

		public static PropagationTrigger Assign(Variable variable, Value value) {
			return new PropagationTrigger() { type = Type.Assign, variable = variable, value = value };
		}
	}
}
