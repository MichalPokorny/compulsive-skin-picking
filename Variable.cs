namespace CSPS {
	public class Variable {
		public string Identifier;
		public ValueRange Range;

		// TODO: 9999 overriden ops
		public static AlgebraicExpression.Node operator*(Variable variable, int node) {
			return ((AlgebraicExpression.VariableNode) variable) * ((AlgebraicExpression.ConstantNode) node);
		}
		public static AlgebraicExpression.Node operator+(Variable a, Variable b) {
			return ((AlgebraicExpression.VariableNode) a) + ((AlgebraicExpression.VariableNode) b);
		}
	};
};
