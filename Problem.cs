using System.Collections.Generic;
using System.Linq;

namespace CSPS {
	public class Problem {
		public List<Variable> variables;
		public List<IConstrain> constrains;

		public Problem() {
			variables = new List<Variable>();
			constrains = new List<IConstrain>();
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

		public List<IConstrain> AllConstrains() {
			return constrains.ToList(); // XXX: hack
		}

		public IExternalEnumerator<Variable> EnumerateVariables() {
			return variables.GetExternalEnumerator();
		}
	}
}
