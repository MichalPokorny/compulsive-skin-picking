using System;
using System.Collections.Generic;
using System.Linq;

namespace CompulsiveSkinPicking {
	public class Problem {
		public interface IVariables {
			// TODO implement and test
			Variable[] AddIntegers(int count, int minimum, int maximum, Func<int, string> namingConvention = null); // minimum: inclusive, maximum: exclusive
			Variable AddInteger(ValueRange range, string name = null);
			Variable AddInteger(int minimum, int maximum, string name = null);

			Variable AddBoolean(string name = null);
			Variable[] AddBooleans(int count, Func<int, string> namingConvention = null);
		}

		private VariablesType _Variables;
		public IVariables Variables {
			get {
				return _Variables;
			}
		}

		private string GenerateVariableName() {
			Debug.WriteLine("Creating variable number {0}", variables.Count + 1);
			return string.Format("_{0}", variables.Count + 1);
		}

		private class VariablesType: IVariables {
			Problem problem;
			public VariablesType(Problem problem) {
				this.problem = problem;
			}
			private void AddVariableInternal(Variable variable) {
				if (problem.variables.Any(v => v.Identifier == variable.Identifier)) {
					throw new Exception(string.Format("Variable name {0} is already used.", variable.Identifier));
				}
				problem.variables.Add(variable);
			}
			public Variable AddInteger(ValueRange range, string name = null) {
				Variable variable = new Variable() {
					Problem = problem,
					Range = range,
					Identifier = name ?? problem.GenerateVariableName()
				};
				AddVariableInternal(variable);
				return variable;
			}
			public Variable AddInteger(int minimum, int maximum, string name = null) {
				return AddInteger(new ValueRange(minimum, maximum), name);
			}
			public Variable AddBoolean(string name = null) {
				return AddInteger(0, 2, name);
			}
			public Variable[] AddIntegers(int count, int minimum, int maximum, Func<int, string> namingConvention = null) {
				if (count < 0) {
					throw new ArgumentException("Negative variable count passed to AddIntegers");
				}

				if (minimum > maximum) {
					throw new ArgumentException("Minimum > maximum for AddIntegers");
				}

				Variable[] result = new Variable[count];
				for (int i = 0; i < count; i++) {
					string name;
					if (namingConvention == null) {
						name = problem.GenerateVariableName();
					} else {
						name = namingConvention(i);
					}
					result[i] = new Variable() {
						Problem = problem,
						Identifier = name,
						Range = {
							Minimum = minimum,
							Maximum = maximum
						}
					};
					AddVariableInternal(result[i]);
					// TODO: not exception-safe
				}
				return result;
			}
			public Variable[] AddBooleans(int count, Func<int, string> namingConvention = null) {
				return AddIntegers(count, 0, 2, namingConvention);
			}
		}

		// TODO: unique variable names
		private List<Variable> variables;
		private List<Constrains.IConstrain> constrains;

		public interface IConstrains {
			void Add(Constrains.IConstrain constrain);
		};

		private ConstrainsType _Constrains;
		private class ConstrainsType: IConstrains {
			Problem problem;
			public ConstrainsType(Problem problem) {
				this.problem = problem;
			}

			public void Add(Constrains.IConstrain constrain) {
				// TODO check that the variable is from this problem
				// TODO check uniqueness
				problem.constrains.Add(constrain);
			}
		};

		public IConstrains Constrains {
			get {
				return _Constrains;
			}
		}

		// TODO: what about free variables? (not constrained enough)

		public Problem() {
			variables = new List<Variable>();
			constrains = new List<Constrains.IConstrain>();

			_Variables = new VariablesType(this);
			_Constrains = new ConstrainsType(this);
		}

		public IVariableAssignment CreateEmptyAssignment() {
			return new VariableAssignment(variables);
		}

		public List<Constrains.IConstrain> AllConstrains() {
			return constrains.ToList(); // XXX: hack
		}

		public IExternalEnumerator<Variable> EnumerateVariables() {
			return variables.GetExternalEnumerator();
		}
	}
}
