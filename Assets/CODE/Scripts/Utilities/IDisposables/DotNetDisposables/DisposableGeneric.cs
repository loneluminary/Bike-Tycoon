using System;

namespace Utilities.IDisposable
{
	public class DisposableGeneric : System.IDisposable
	{
		private readonly Action _onEndAction;

		public DisposableGeneric(Action onStartAction, Action onEndAction)
		{
			onStartAction?.Invoke();
			_onEndAction = onEndAction;
		}

		public void Dispose()
		{
			_onEndAction?.Invoke();
		}
	}
}