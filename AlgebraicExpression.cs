using System;

namespace CSPS {
	namespace AlgebraicExpression {
		public interface NodeVisitor<T> {
			T VisitVariableNode(Variable variable);
			T VisitConstantNode(Value value);
			T VisitBinaryNode(BinaryNode.Type type, Node left, Node right);
		}

		public abstract class Node {
			public static Node operator+(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Plus, left, right);
			}
			public static Node operator+(Node left, Variable right) {
				return left + (VariableNode) right;
			}
			public static Node operator-(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Minus, left, right);
			}
			// TODO: unary minus?
			public static Node operator*(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Multiply, left, right);
			}
			public static Node operator/(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Divide, left, right);
			}
			public static Node operator%(Node left, Node right) {
				return new BinaryNode(BinaryNode.Type.Modulo, left, right);
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
			private Value value;
			public ConstantNode(int value) {
				this.value = new Value(value);
			}
			public ConstantNode(Value value) {
				this.value = value;
			}
			public override T AcceptVisitor<T>(NodeVisitor<T> visitor) {
				return visitor.VisitConstantNode(value);
			}
			public static implicit operator ConstantNode(int value) {
				return new ConstantNode(value);
			}
		}

		public class BinaryNode: Node {
			public enum Type {
				Plus, Minus, Multiply, Divide, Modulo // TODO: and, or, xor
			};
			private Type type;
			private Node left, right;
			public BinaryNode(Type type, Node left, Node right) {
				this.type = type; this.left = left; this.right = right;
			}
			public override T AcceptVisitor<T>(NodeVisitor<T> visitor) {
				return visitor.VisitBinaryNode(type, left, right);
			}
			public static Constrains.IConstrain CreateConstrain(Type type, Variable a, Variable b, Variable c) {
				switch (type) {
					case Type.Plus:
						return Constrain.Plus(a, b, c);
					case Type.Minus:
						return Constrain.Minus(a, b, c);
					case Type.Multiply:
						return Constrain.Multiply(a, b, c);
					default:
						throw new Exception("TODO");
						/*
					case Type.Divide:
					case Type.Modulo:
						*/
						// TODO
				}
			}
		}

		class ExpressorVisitor: NodeVisitor<Variable> {
			private Problem problem;
			public ExpressorVisitor(Problem problem) {
				this.problem = problem;
			}

			// TODO: cache by value
			public Variable VisitConstantNode(Value value) {
				return problem.Variables.AddInteger(value.value, value.value + 1);
			}

			public Variable VisitVariableNode(Variable variable) {
				return variable;
			}

			public Variable VisitBinaryNode(BinaryNode.Type type, Node left, Node right) {
				Variable a = left.AcceptVisitor(this), b = right.AcceptVisitor(this);
				Variable c = problem.Variables.AddInteger();
				problem.Constrains.Add(BinaryNode.CreateConstrain(type, a, b, c));
				return c;
			}
		}
	}
}
