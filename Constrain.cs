using CSPS.Constrains;
using System.Collections.Generic;
using System.Linq;

namespace CSPS {
	public static class Constrain {
		// TODO: params [] form
		public static IConstrain NotEqual(Variable a, Variable b) {
			return new Constrains.NotEqual(a, b);
		}

		public static IConstrain Plus(Variable a, Variable b, Variable sum) {
			return new Constrains.InvertibleBinary(a, b, sum, (A, B) => new Value(A.value + B.value), (A, Sum) => new Value(Sum.value - A.value), (B, Sum) => new Value(Sum.value - B.value));
		}

		public static IConstrain And(params IConstrain[] constrains) {
			return new Constrains.And(constrains);
		}

		public static IConstrain And(IEnumerable<IConstrain> constrains) {
			return And(constrains.ToArray());
		}

		public static IConstrain AllDifferent(params Variable[] variables) {
			// TODO: more effective all-different constrain?
			return And(
				from a in variables select And(from b in variables where a != b select NotEqual(a, b))
			);
		}
	}
}
