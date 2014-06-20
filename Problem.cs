using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPS {
	public class Problem {
		public interface IVariables {
			// TODO implement and test
			// public Variables[] AddInteger(int minimum, int maximum); // minimum: inclusive, maximum: exclusive
			Variable[] AddIntegers(int count, int minimum, int maximum, Func<int, string> namingConvention = null); // minimum: inclusive, maximum: exclusive
		}

		private VariablesType _Variables;
		public IVariables Variables {
			get {
				return _Variables;
			}
		}

		private class VariablesType: IVariables {
			Problem problem;
			public VariablesType(Problem problem) {
				this.problem = problem;
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
						name = string.Format("Var{0:5}", problem.variables.Count + 1);
					} else {
						name = namingConvention(i);
					}
					// TODO: check uniqueness
					result[i] = new Variable() {
						Identifier = name,
						Minimum = minimum,
						Maximum = maximum
					};
					problem.variables.Add(result[i]);
					// TODO: unique variable names
				}
				return result;
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
			VariableAssignment assignment = new VariableAssignment();
			assignment.values = new Dictionary<string, List<int>>();
			foreach (Variable v in variables) {
				List<int> list = new List<int>();
				for (int i = v.Minimum; i < v.Maximum; i++) {
					list.Add(i);
				}
				assignment.values[v.Identifier] = list;
			}
			return assignment;
		}

		public List<Constrains.IConstrain> AllConstrains() {
			return constrains.ToList(); // XXX: hack
		}

		public IExternalEnumerator<Variable> EnumerateVariables() {
			return variables.GetExternalEnumerator();
		}
	}
}
