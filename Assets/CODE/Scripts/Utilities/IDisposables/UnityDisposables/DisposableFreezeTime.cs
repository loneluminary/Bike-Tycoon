using UnityEngine;

namespace Utilities.IDisposable
{
	public class DisposableFreezeTime : System.IDisposable
	{
		private readonly float _previousTimeScale;

		public DisposableFreezeTime()
		{
			_previousTimeScale = Time.timeScale;
			Time.timeScale = 0f;
		}

		public void Dispose()
		{
			Time.timeScale = _previousTimeScale;
		}
	}
}