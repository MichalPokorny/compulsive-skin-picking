using System;
using System.Collections.Generic;

namespace CSPS {
	// A simple "not equal" constrain.
	public class NotEqualConstrain: IConstrain {
		private Variable a, b;

		public NotEqualConstrain(Variable a, Variable b) {
			this.a = a; this.b = b;
		}

		private void Log(string str, params object[] args) {
			Console.WriteLine("{0} {1}", Identifier, string.Format(str, args));
		}

		public List<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers) {
			var results = new List<ConstrainResult>();
			foreach (var trigger in triggers) {
				if (trigger.type == PropagationTrigger.Type.Assign) {
					if (trigger.variable == a) {
						if (assignment.IsValueAssigned(b) && Value.Equal(assignment.GetAssignedValue(b), trigger.value)) {
							Log("Assigning {0} to A({1}) made it equal B({2})!", trigger.value, a.Identifier, b.Identifier);
							results.Add(ConstrainResult.Failure);
							break;
						} else {
							Log("Assigning {0} to A({1}).", trigger.value, a.Identifier);
							results.Add(ConstrainResult.Restrict(b, trigger.value));
						}
					}

					if (trigger.variable == b) {
						if (assignment.IsValueAssigned(a) && Value.Equal(assignment.GetAssignedValue(a), trigger.value)) {
							Log("Assigning {0} to B({1}) made it equal A({2})!", trigger.value, b.Identifier, a.Identifier);
							results.Add(ConstrainResult.Failure);
							break;
						} else {
							Log("Assigning {0} to B({1}).", trigger.value, b.Identifier);
							results.Add(ConstrainResult.Restrict(a, trigger.value));
						}
					}
				}
			}
			if (assignment.IsValueAssigned(a) && assignment.IsValueAssigned(b)) {
				if (Value.Equal(assignment.GetAssignedValue(a), assignment.GetAssignedValue(b))) {
					Console.WriteLine("Constrain fail");
					results.Add(ConstrainResult.Failure);
				} else {
					results.Add(ConstrainResult.Success);
				}
			}
			return results;
		}

		public string Identifier {
			get { return string.Format("[{0} != {1}]", a.Identifier, b.Identifier); }
		}
	}
}
