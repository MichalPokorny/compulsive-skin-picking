using System;
using CompulsiveSkinPicking.Constrains;
using System.Collections.Generic;
using System.Linq;

namespace CompulsiveSkinPicking {
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

		/*
		public static IConstrain Or(params IConstrain[] constrains) {
			return new Constrains.Or(constrains);
		}
		*/

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
			return BinaryFunctional((A, B) => {
				return ((A != 0) ^ (B != 0)) ? 1 : 0;
			}, a, b, y);
		}

		public static IConstrain VariableImplies(Variable a, Variable b, Variable y) {
			// TODO: better propagation...
			return BinaryFunctional((A, B) => {
				return (A != 0 && B == 0) ? 0 : 1;
			}, a, b, y);
		}

		public static IConstrain VariableGreaterThan(Variable a, Variable b, Variable y) {
			// TODO: better propagation
			return BinaryFunctional((A, B) => {
				return (A > B) ? 1 : 0;
			}, a, b, y);
		}

		public static IConstrain VariableGreaterThanOrEqualTo(Variable a, Variable b, Variable y) {
			// TODO: better propagation
			return BinaryFunctional((A, B) => {
				return (A >= B) ? 1 : 0;
			}, a, b, y);
		}

		public static IConstrain GreaterThan(Variable a, Variable b) {
			// TODO: better propagation
			return Relational((vars) => vars[0] > vars[1], a, b);
		}

		public static IConstrain GreaterThanOrEqualTo(Variable a, Variable b) {
			// TODO: better propagation
			return Relational((vars) => vars[0] >= vars[1], a, b);
		}

		public static IEnumerable<IConstrain> AllDifferent(params Variable[] variables) {
			// TODO: more effective all-different constrain?
			return (from a in variables
			        select (from b in variables
			                where a != b && a.CompareTo(b) > 0
			                select NotEqual(a, b))).Aggregate((IEnumerable<IConstrain>)new IConstrain[] { }, (a, b) => a.Concat(b));
		}

		public static IConstrain Truth(Variable x) {
			return new Constrains.Truth(x);
		}

		public static IConstrain Relational(Func<int[], bool> func, params Variable[] dependencies) {
			return new Constrains.Relational(func, dependencies);
		}

		public static IConstrain BinaryFunctional(Func<int, int, int> func, Variable a, Variable b, Variable y) {
			return new Constrains.InvertibleBinary(a, b, y, func, null, null);
		}
	}
}
