namespace CSPS {
	public interface IReadonlyValueAssignment {
		Value this[Variable variable] { get; }
	}
}
