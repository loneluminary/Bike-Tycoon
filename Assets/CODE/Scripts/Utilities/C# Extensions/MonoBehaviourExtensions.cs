using System;
using System.Collections;
using UnityEngine;

namespace Utilities.Extensions
{
	public static class MonoBehaviourExtensions
	{
		public static Coroutine DelayedExecution(this MonoBehaviour monoBehaviour, float delay, Action callback)
		{
			return monoBehaviour.StartCoroutine(Execute());
			
			IEnumerator Execute()
			{
				yield return new WaitForSeconds(delay);
				callback?.Invoke();
			}
		}

		public static Coroutine DelayedExecutionUntilNextFrame(this MonoBehaviour monoBehaviour, Action callback)
		{
			return monoBehaviour.StartCoroutine(ExecuteAfterFrame());
			IEnumerator ExecuteAfterFrame()
			{
				yield return null;
				callback?.Invoke();
			}
		}

		public static Coroutine DelayedExecutionUntil(this MonoBehaviour monoBehaviour, Func<bool> condition, Action callback, bool expectedResult = true)
		{
			return monoBehaviour.StartCoroutine(WaitForCondition());
			
			IEnumerator WaitForCondition()
			{
				yield return new WaitUntil(() => condition() == expectedResult);
				callback?.Invoke();
			}
		}

		public static Coroutine RepeatExecutionWhile(this MonoBehaviour behaviour, Func<bool> condition, float startDelay, float interval, Action callback)
		{
			return behaviour.StartCoroutine(RepeatWhileCoroutine());
			
			IEnumerator RepeatWhileCoroutine()
			{
				if (startDelay > 0) yield return new WaitForSeconds(startDelay);
				while (condition())
				{
					yield return new WaitForSeconds(interval);
					callback?.Invoke();
				}
			}
		}

		public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component
		{
			T component = behaviour.GetComponent<T>();
			return component ? component : behaviour.gameObject.AddComponent<T>();
		}

		public static void AddComponentIfMissing<T>(this MonoBehaviour behaviour) where T : Component
		{
			if (!behaviour.GetComponent<T>()) behaviour.gameObject.AddComponent<T>();
		}
	}
}