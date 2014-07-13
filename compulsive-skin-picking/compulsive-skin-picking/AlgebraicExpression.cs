using System;
using System.Linq;

namespace CompulsiveSkinPicking {
	namespace AlgebraicExpression {
		public interface NodeVisitor<T> {
			T VisitVariableNode(Variable variable);

			T VisitConstantNode(int value);

			T VisitUnaryNode(UnaryNode.Type type, Node x);

			T VisitBinaryNode(BinaryNode.Type type, Node left, Node right);
		}

		public static class IntegerExtensions {
			public static Variable Build(this int x, Problem problem) {
				return new ConstantNode(x).Build(problem);
			}
		}

		public abstract class Node {
			public static Node operator+(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Plus, left, right);
			}

			public static Node operator+(Node left, Variable right) {
				return left + (VariableNode)right;
			}

			public static Node operator+(Variable left, Node right) {
				return (VariableNode)left + right;
			}

			public static Node operator-(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Minus, left, right);
			}

			public static Node operator-(Node left) {
				return new BinaryNode(BinaryNode.Type.Multiply, (ConstantNode)(-1), left);
			}

			public static Node operator*(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Multiply, left, right);
			}

			public static Node operator*(Node left, Variable right) {
				return left * (VariableNode)right;
			}

			public static Node operator*(Variable left, Node right) {
				return (VariableNode)left * right;
			}

			public static Node operator/(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Divide, left, right);
			}

			public static Node operator%(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Modulo, left, right);
			}

			public static Node operator&(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.And, left, right);
			}

			public static Node operator|(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Or, left, right);
			}

			public static Node operator|(Node left, Variable right) {
				return left | ((VariableNode)right);
			}

			public static Node operator|(Variable left, Node right) {
				return ((VariableNode)left) | right;
			}

			public static Node operator&(Variable left, Node right) {
				return ((VariableNode)left) & right;
			}

			public static Node operator&(Node left, Variable right) {
				return left & ((VariableNode)right);
			}

			public static Node operator^(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Xor, left, right);
			}

			public static Node operator!(Node x) {
				return new UnaryNode(UnaryNode.Type.Not, x);
			}

			public static Node Implies(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Implies, left, right);
			}

			public static Node GreaterThan(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.GreaterThan, left, right);
			}

			public static Node GreaterThanOrEqualTo(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.GreaterThanOrEqualTo, left, right);
			}

			public static Node LessThan(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.GreaterThan, right, left);
			}

			public static Node LessThanOrEqualTo(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.GreaterThanOrEqualTo, right, left);
			}

			public abstract T AcceptVisitor<T>(NodeVisitor<T> visitor);

			public Variable Build(Problem problem) {
				return AcceptVisitor(new ExpressorVisitor(problem));
			}
		}

		public class VariableNode: Node {
			private Variable variable;

			public VariableNode(Variable variable) {
				this.variable = variable;
			}

			public override T AcceptVisitor<T>(NodeVisitor<T> visitor) {
				return visitor.VisitVariableNode(variable);
			}

			public static implicit operator VariableNode(Variable variable) {
				return new VariableNode(variable);
			}
		}

		public class ConstantNode: Node {
			private int value;

			public ConstantNode(int value) {
				this.value = value;
			}

			public override T AcceptVisitor<T>(NodeVisitor<T> visitor) {
				return visitor.VisitConstantNode(value);
			}

			public static implicit operator ConstantNode(int value) {
				return new ConstantNode(value);
			}
		}

		public class UnaryNode: Node {
			public enum Type {
				Not}

			;

			private Type type;
			private Node x;

			public UnaryNode(Type type, Node x) {
				this.type = type;
				this.x = x;
			}

			public override T AcceptVisitor<T>(NodeVisitor<T> visitor) {
				return visitor.VisitUnaryNode(type, x);
			}

			public static Constrains.IConstrain CreateConstrain(Type type, Variable a, Variable y) {
				switch (type) {
				case Type.Not:
					return Constrain.VariableNot(a, y);
				default:
					throw new NotImplementedException(string.Format("Unary node type {0} not implemented", type));
				}
			}

			public static ValueRange GetBoundsOnY(Type type, Variable a) {
				switch (type) {
				case Type.Not:
					return ValueRange.Boolean;
				default:
					throw new NotImplementedException(string.Format("Unary node type {0} not implemented", type));
				}
			}
		}

		public class BinaryNode: Node {
			public enum Type {
				Plus,
				Minus,
				Multiply,
				Divide,
				Modulo,
				And,
				Or,
				Xor,
				Implies,

				GreaterThan,
				GreaterThanOrEqualTo}

			;

			private Type type;
			private Node left, right;

			public BinaryNode(Type type, Node left, Node right) {
				this.type = type;
				this.left = left;
				this.right = right;
			}

			public override T AcceptVisitor<T>(NodeVisitor<T> visitor) {
				return visitor.VisitBinaryNode(type, left, right);
			}

			public static ValueRange GetBoundsOnY(Type type, Variable a, Variable b) {
				switch (type) {
				case Type.Plus:
					return new ValueRange(a.Range.Minimum + b.Range.Minimum, a.Range.Maximum + b.Range.Maximum);
				case Type.Minus:
					return new ValueRange(a.Range.Minimum - b.Range.Maximum, a.Range.Maximum - b.Range.Minimum);
				case Type.Divide:
					{
						// TODO: don't be lazy and actually get tight bounds?
						int magnitude = new [] { Math.Abs(a.Range.Minimum), Math.Abs(a.Range.Maximum) }.Max();
						return new ValueRange(-magnitude, magnitude);
					}
				case Type.Modulo:
					{
						// TODO: don't be lazy and actually get tight bounds?
						int magnitude = new [] { Math.Abs(b.Range.Minimum), Math.Abs(b.Range.Maximum) }.Max();
						return new ValueRange(-magnitude + 1, magnitude);
					}
				case Type.Multiply:
					{
						// TODO: don't be lazy and actually get tight bounds?
						int magnitude1 = new [] { Math.Abs(a.Range.Minimum), Math.Abs(a.Range.Maximum) }.Max();
						int magnitude2 = new [] { Math.Abs(b.Range.Minimum), Math.Abs(b.Range.Maximum) }.Max();
						int mult = magnitude1 * magnitude2;
						return new ValueRange(-mult, mult);
					}
				case Type.And:
					return ValueRange.Boolean;
				case Type.Or:
					return ValueRange.Boolean;
				case Type.Xor:
					return ValueRange.Boolean;
				case Type.Implies:
					return ValueRange.Boolean;
				case Type.GreaterThan:
					return ValueRange.Boolean;
				case Type.GreaterThanOrEqualTo:
					return ValueRange.Boolean;
				default:
					throw new NotImplementedException(string.Format("Binary node type {0} not implemented", type));
				}
			}

			public static Constrains.IConstrain CreateConstrain(Type type, Variable a, Variable b, Variable c) {
				switch (type) {
				case Type.Plus:
					return Constrain.Plus(a, b, c);
				case Type.Minus:
					return Constrain.Minus(a, b, c);
				case Type.Multiply:
					return Constrain.Multiply(a, b, c);
				case Type.Divide:
					return Constrain.Divide(a, b, c);
				case Type.Modulo:
					return Constrain.Modulo(a, b, c);
				case Type.And:
					return Constrain.VariableAnd(a, b, c);
				case Type.Or:
					return Constrain.VariableOr(a, b, c);
				case Type.Xor:
					return Constrain.VariableXor(a, b, c);
				case Type.Implies:
					return Constrain.VariableImplies(a, b, c);
				case Type.GreaterThan:
					return Constrain.VariableGreaterThan(a, b, c);
				case Type.GreaterThanOrEqualTo:
					return Constrain.VariableGreaterThanOrEqualTo(a, b, c);
				default:
					throw new NotImplementedException(string.Format("Binary node type {0} not implemented", type));
				}
			}
		}

		class ExpressorVisitor: NodeVisitor<Variable> {
			private Problem problem;

			public ExpressorVisitor(Problem problem) {
				this.problem = problem;
			}

			// TODO: cache by value?
			public Variable VisitConstantNode(int value) {
				return problem.Variables.AddInteger(value, value + 1);
			}

			public Variable VisitVariableNode(Variable variable) {
				return variable;
			}

			public Variable VisitUnaryNode(UnaryNode.Type type, Node x) {
				Variable a = x.AcceptVisitor(this);
				Variable y = problem.Variables.AddInteger(UnaryNode.GetBoundsOnY(type, a));
				problem.Constrains.Add(UnaryNode.CreateConstrain(type, a, y));
				return y;
			}

			public Variable VisitBinaryNode(BinaryNode.Type type, Node left, Node right) {
				Variable a = left.AcceptVisitor(this), b = right.AcceptVisitor(this);
				Variable c = problem.Variables.AddInteger(BinaryNode.GetBoundsOnY(type, a, b));
				problem.Constrains.Add(BinaryNode.CreateConstrain(type, a, b, c));
				return c;
			}
		}
	}
}
