namespace CSPS {
	public interface IVariableManipulator {
		void Restrict(Value v);
		Value Value { get; set; }
		bool Assigned { get; }
		bool HasPossibleValues { get; }
		bool CanBe(Value v);
		IExternalEnumerator<Value> EnumeratePossibleValues();
	}

	public interface IVariableAssignment {
		IVariableManipulator this[Variable variable] { get; }
		IVariableAssignment Duplicate();

		void Dump();
	}
}
