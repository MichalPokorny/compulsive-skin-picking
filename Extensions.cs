using System;
using System.Collections.Generic;

namespace CSPS {
	public class ListTransformationExternalEnumerator<T, C>: IExternalEnumerator<C> {
		// TODO fuj, neefektivni

		private Func<T, C> func;
		private List<T> list;
		private int index;
		public ListTransformationExternalEnumerator(List<T> list, Func<T, C> func) {
			this.list = list;
			this.func = func;
		}

		private ListTransformationExternalEnumerator(List<T> list, Func<T, C> func, int index) {
			this.list = list;
			this.func = func;
			this.index = index;
		}

		public bool TryProgress(out IExternalEnumerator<C> next) {
			if (index == list.Count - 1) {
				next = null;
				return false;
			} else {
				next = new ListTransformationExternalEnumerator<T, C>(list, func, index + 1);
				return true;
			}
		}

		public C Value { get { return func(list[index]); } }
	}
	public static class Extensions {
		public static IExternalEnumerator<T> GetExternalEnumerator<T>(this List<T> list) {
			return new ListTransformationExternalEnumerator<T, T>(list, x => x);
		}

		public static IExternalEnumerator<C> GetTransformedExternalEnumerator<T, C>(this List<T> list, Func<T, C> transform) {
			return new ListTransformationExternalEnumerator<T, C>(list, transform);
		}
	}
}
