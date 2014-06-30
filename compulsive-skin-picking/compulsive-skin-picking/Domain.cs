using System;
using System.Linq;
using System.Collections.Generic;

namespace CompulsiveSkinPicking {
	public class Domain: IBacktrackable<Domain> {
		// TODO: make more efficent
		private struct History {
			public List<ValueRange> _values;
		};
		private int changesSinceLastSavepoint;

		private Stack<History> history;

		private List<ValueRange> values;

		public Domain() {
			history = new Stack<History>();
		}

		public Domain(ValueRange range) {
			history = new Stack<History>();
			values = new List<ValueRange>() { range };
		}

		public int Size {
			get {
				return (from range in values select range.Size).Sum();
			}
		}

		public Domain DeepDuplicate() {
			return new Domain() {
				values = values.ToList() // XXX hack
			};
		}

		public bool Ground {
			get {
				return values.Count == 1 && values[0].IsSingleton;
			}
		}

		public bool Contains(int value) {
			// TODO: can be optimized -- ranges are ascending, no?
			foreach (var range in values) {
				if (range.Contains(value)) {
					return true;
				}
			}
			return false;
		}

		public bool IsEmpty {
			get {
				return values.Count == 0;
			}
		}

		public void Restrict_WRONG_AND_SLOW(int v) {
			Debug.WriteLine("Remove {0} from a domain", v);
			changesSinceLastSavepoint++;
			foreach (var range in values) {
				if (range.Contains(v)) {
					values.Remove(range);
					Debug.WriteLine("-{0}", range);
					foreach (var newRange in range.SplitAndRemove(v)) {
						values.Add(newRange);
						Debug.WriteLine("+{0}", newRange);
					}
					return;
				}
			}
			throw new Exception(string.Format("Cannot remove {0} from a domain, it's not in the domain", v));
		}

		public int Value {
			get {
				if (!Ground) {
					throw new Exception(string.Format("No value assigned to domain yet"));
				}
				return values[0].Singleton;
			}
			set {
				if (!Contains(value)) {
					throw new Exception(string.Format("Cannot assign {0} to domain", value));
				}
				values = new List<ValueRange> { new ValueRange(value) };
			}
		}

		public ValueRange[] LEGACY_RANGES {
			get {
				return values.ToArray();
			}
		}

		public void RollbackToSavepoint() {
			values = history.Peek()._values;
			history.Pop();
		}

		public void AddSavepoint() {
			history.Push(new History() {
				_values = values.ToList()
			});
		}

		public bool CanRollbackToSavepoint {
			get {
				return history.Count > 0;
			}
		}

		public override string ToString() {
			return string.Join(" | ", values);
		}
	}
}
