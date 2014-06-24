using CSPS.Constrains;
using System.Collections.Generic;
using System.Linq;

namespace CSPS {
	public static class Constrain {
		// TODO: params [] form
		public static IConstrain NotEqual(Variable a, Variable b) {
			return new Constrains.NotEqual(a, b);
		}
		// TODO: params [] form
		public static IConstrain Equal(Variable a, Variable b) {
			return new Constrains.Equal(a, b);
		}

		public static IConstrain Plus(Variable a, Variable b, Variable sum) {
			return new Constrains.InvertibleBinary(a, b, sum, (A, B) => A + B, (A, Sum) => Sum - A, (B, Sum) => Sum - B) {
				OperatorName = "+"
			};
		}

		public static IConstrain Minus(Variable a, Variable b, Variable diff) {
			return new Constrains.InvertibleBinary(a, b, diff, (A, B) => A - B, (A, Diff) => A - Diff, (B, Diff) => Diff + B) {
				OperatorName = "-"
			};
		}

		public static IConstrain Multiply(Variable a, Variable b, Variable prod) {
			// TODO: better propagation...
			return new Constrains.InvertibleBinary(a, b, prod, (A, B) => A * B, null, null);
		}

		public static IConstrain Divide(Variable a, Variable b, Variable div) {
			// TODO: better propagation...
			return new Constrains.Relational((vars) => {
				int A = vars[0], B = vars[1], Div = vars[2];
				return B != 0 && A / B == Div;
			}, new [] { a, b, div });
		}

		public static IConstrain Modulo(Variable a, Variable b, Variable div) {
			// TODO: better propagation...
			return new Constrains.Relational((vars) => {
				int A = vars[0], B = vars[1], Div = vars[2];
				return B != 0 && A % B == Div;
			}, new [] { a, b, div });
		}

		public static IConstrain And(params IConstrain[] constrains) {
			return new Constrains.And(constrains);
		}

		public static IConstrain And(IEnumerable<IConstrain> constrains) {
			return And(constrains.ToArray());
		}

		public static IConstrain Or(params IConstrain[] constrains) {
			return new Constrains.Or(constrains);
		}

		public static IConstrain VariableNot(Variable a, Variable y) {
			return new Constrains.VariableNot(a, y);
		}

		public static IConstrain VariableAnd(Variable a, Variable b, Variable y) {
			return new Constrains.VariableAnd(a, b, y);
		}

		public static IConstrain VariableOr(Variable a, Variable b, Variable y) {
			return new Constrains.VariableOr(a, b, y);
		}

		public static IConstrain VariableXor(Variable a, Variable b, Variable y) {
			// TODO: better propagation...
			return new Constrains.Relational((vars) => {
				int A = vars[0], B = vars[1], Y = vars[2];
				return (A != 0) ^ (B != 0) == (Y != 0);
			}, new [] { a, b, y });
		}

		public static IConstrain AllDifferent(params Variable[] variables) {
			// TODO: more effective all-different constrain?
			return And(
				from a in variables select And(from b in variables where a != b select NotEqual(a, b))
			);
		}

		public static IConstrain Implies(Variable a, Variable b) {
			// TODO: better propagation...
			return new Constrains.Relational((vars) => {
				int A = vars[0], B = vars[1];
				return !(A != 0 && B == 0);
			}, new [] { a, b });
		}

		public static IConstrain Truth(Variable x) {
			return new Constrains.Truth(x);
		}
	}
}
