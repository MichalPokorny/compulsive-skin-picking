namespace CSPS {
	public interface IVariableManipulator {
		void Restrict(Value v);
		Value Value { get; set; }
		bool Assigned { get; }
		bool CanBe(Value v);
	}

	public interface IVariableAssignment {
		IExternalEnumerator<Value> EnumeratePossibleValues(Variable variable);
		IVariableManipulator this[Variable variable] { get; }
		IVariableAssignment Duplicate();

		void Dump();
	}
}
