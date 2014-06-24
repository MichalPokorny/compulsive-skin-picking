using System;
using System.Collections.Generic;

namespace CSPS {
	public struct PropagationTrigger {
		public enum Type {
			Restrict, Assign
		};

		public Type type;
		public Variable variable;
		public int value;

		public static PropagationTrigger Restrict(Variable variable, int value) {
			return new PropagationTrigger() { type = Type.Restrict, variable = variable, value = value };
		}

		public static PropagationTrigger Assign(Variable variable, int value) {
			return new PropagationTrigger() { type = Type.Assign, variable = variable, value = value };
		}

		public override string ToString() {
			switch (type) {
				case Type.Restrict:
					return string.Format("Restrict({0} -= {1})", variable, value);
				case Type.Assign:
					return string.Format("Assign({0} <== {1})", variable, value);
				default:
					throw new Exception("TODO");
			}
		}
	}
}
