using System.Collections;
using System.Collections.Generic;

namespace CSPS {
	public interface IConstrain {
		List<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers);
		List<Variable> Dependencies { get; }
		string Identifier { get; }
	};
};
