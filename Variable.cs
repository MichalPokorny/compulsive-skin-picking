namespace CompulsiveSkinPicking {
	public class Variable {
		public Problem Problem;
		public string Identifier;
		public ValueRange Range;

		// TODO: 9999 overriden ops
		public static AlgebraicExpression.Node operator*(Variable variable, int node) {
			return ((AlgebraicExpression.VariableNode) variable) * ((AlgebraicExpression.ConstantNode) node);
		}
		public static AlgebraicExpression.Node operator*(Variable a, Variable b) {
			return ((AlgebraicExpression.VariableNode) a) * ((AlgebraicExpression.VariableNode) b);
		}
		public static AlgebraicExpression.Node operator+(Variable a, Variable b) {
			return ((AlgebraicExpression.VariableNode) a) + ((AlgebraicExpression.VariableNode) b);
		}
		public static AlgebraicExpression.Node operator/(Variable variable, int node) {
			return ((AlgebraicExpression.VariableNode) variable) / ((AlgebraicExpression.ConstantNode) node);
		}
		public static AlgebraicExpression.Node operator!(Variable x) {
			return ! ((AlgebraicExpression.VariableNode) x);
		}
		public static AlgebraicExpression.Node operator|(Variable a, Variable b) {
			return ((AlgebraicExpression.VariableNode) a) | ((AlgebraicExpression.VariableNode) b);
		}
		public static AlgebraicExpression.Node operator&(Variable a, Variable b) {
			return ((AlgebraicExpression.VariableNode) a) & ((AlgebraicExpression.VariableNode) b);
		}

		public override string ToString() {
			return string.Format("Var<{0}>", Identifier);
		}
	};
};
