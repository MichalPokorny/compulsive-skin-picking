using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	public interface IVariableManipulator {
		void Restrict(int v);
		int Value { get; set; }
		bool Assigned { get; }
		bool HasPossibleValues { get; }
		bool CanBe(int v);
		IExternalEnumerator<int> EnumeratePossibleValues();
		int PossibleValueCount { get; }
	}

	public interface IVariableAssignment {
		List<Variable> Variables { get; }
		IVariableManipulator this[Variable variable] { get; }
		IVariableAssignment Duplicate();

		void Dump();
	}
}
