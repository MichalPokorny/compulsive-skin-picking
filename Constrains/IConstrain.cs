using System.Collections;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	namespace Constrains {
		public interface IConstrain {
			IEnumerable<ConstrainResult> Propagate(IVariableAssignment assignment, IEnumerable<PropagationTrigger> triggers);
			bool Satisfied(IVariableAssignment assignment);
			IEnumerable<Variable> Dependencies { get; } // Constant over constrain lifetime.
		};
	}
};
