namespace CSPS {
	public interface IVariableManipulator {
		void Restrict(int v);
		int Value { get; set; }
		bool Assigned { get; }
		bool HasPossibleValues { get; }
		bool CanBe(int v);
		IExternalEnumerator<int> EnumeratePossibleValues();
	}

	public interface IVariableAssignment {
		IVariableManipulator this[Variable variable] { get; }
		IVariableAssignment Duplicate();

		void Dump();
	}
}
