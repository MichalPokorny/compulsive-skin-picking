namespace CompulsiveSkinPicking {
	public interface IBacktrackable<T> {
		T DeepDuplicate();
		void AddSavepoint();
		void RollbackToSavepoint();

		bool CanRollbackToSavepoint { get; }
	}
}
