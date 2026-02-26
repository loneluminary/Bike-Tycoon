using System;

namespace Utilities.IDisposable
{
	public class DisposableSetBoolean : System.IDisposable
	{
		private readonly Action<bool> _action;
		private readonly bool _startingValue;

		public DisposableSetBoolean(Action<bool> action, bool startingValue)
		{
			_action = action;
			_startingValue = startingValue;
			_action?.Invoke(startingValue);
		}

		public void Dispose()
		{
			_action?.Invoke(!_startingValue);
		}
	}
}