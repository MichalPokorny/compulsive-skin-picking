namespace CSPS {
	public interface IVariableAssignment {
		IVariableAssignment Restrict(Variable variable, Value removedValue);
		IVariableAssignment Assign(Variable variable, Value assignedValue);

		IExternalEnumerator<Value> EnumeratePossibleValues(Variable variable);
		bool ValuePossible(Variable variable, Value value);

		bool IsValueAssigned(Variable variable);
		Value GetAssignedValue(Variable variable);

		IVariableAssignment Duplicate();

		void Dump();
	}
}
