using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	public interface IVariableManipulator {
		void Restrict(int v);
		int Value { get; set; }
		bool Ground { get; }
		bool HasPossibleValues { get; }
		bool CanBe(int v);
		IExternalEnumerator<int> EnumeratePossibleValues();
		int PossibleValueCount { get; }
	}

	public interface IVariableAssignment: IBacktrackable<IVariableAssignment> {
		IVariableManipulator this[Variable variable] { get; }
		// IVariableAssignment Duplicate();

		void Dump();
		IEnumerable<Variable> Variables { get; }
	}
}
