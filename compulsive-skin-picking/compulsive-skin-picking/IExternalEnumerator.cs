namespace CompulsiveSkinPicking {
	public interface IExternalEnumerator<T> {
		bool TryProgress(out IExternalEnumerator<T> next);
		T Value { get; }
	}
}
