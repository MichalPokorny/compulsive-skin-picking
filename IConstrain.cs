using System.Collections;
using System.Collections.Generic;

namespace CSPS {
	public interface IConstrain {
		List<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers);
		// public List<IVariable> Dependencies();
		string Identifier { get; }
	};
};
