using System.Collections.Generic;

namespace CSPS {
	struct CanceledVariableValue {
		public Variable Variable;
		public Value Value;
	}

	class ArcConsistency {
		public IEnumerable<CanceledVariableValue> Run(VariableAssignment assignment, Constrains.IConstrain arc, ref IScratchpad scratchpad) {
		}
	}
}
