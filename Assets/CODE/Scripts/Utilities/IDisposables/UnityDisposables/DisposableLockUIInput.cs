using UnityEngine.UI;

namespace Utilities.IDisposable
{
	public class DisposableLockUIInput : System.IDisposable
	{
		private readonly Graphic _graphic;

		public DisposableLockUIInput(Graphic graphic)
		{
			_graphic = graphic;
			graphic.raycastTarget = false;
		}

		public void Dispose()
		{
			_graphic.raycastTarget = true;
		}
	}
}