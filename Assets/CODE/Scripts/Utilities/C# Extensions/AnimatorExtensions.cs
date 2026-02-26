using System.Threading.Tasks;
using UnityEngine;

//TODO: Improve Set Animator
namespace Utilities.Extensions
{
	public static class AnimatorExtensions
	{
		// public static Animator SetAnimation(this Animator animator, string animationStateName, bool canPlaySameAnimation = false, int layer = 0, float transitionDuration = 0.2f)
		// {
		// 	animator.CrossFade(animationStateName, transitionDuration, layer);
		// 	// currentAnimationStates[layer] = animationStateName;
		// 	return animator;
		// }
		//
		// public static Animator PlayAnimation(this Animator animator, string animationStateName, int layer = 0)
		// {
		// 	animator.Play(animationStateName, layer);
		// 	return animator;
		// }
		//
		
		public static async Task PlayTask(this Animator animator, string animationStateName, int layer = 0)
		{
			animator.Play(animationStateName, layer);
			
			// Wait until the animation is no longer playing
			while (animator.GetCurrentAnimatorStateInfo(layer).IsName(animationStateName) && animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
			{
				await Task.Yield();
			}
		}
		
		public static async Task CrossFadeTask(this Animator animator, string animationStateName, float transitionDuration = 0.2f, int layer = 0)
		{
			animator.CrossFade(animationStateName, transitionDuration, layer);
			
			// Wait until the animation is no longer playing
			while (animator.GetCurrentAnimatorStateInfo(layer).IsName(animationStateName) && animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
			{
				await Task.Yield();
			}
		}

		public static Animator Pause(this Animator animator)
		{
			animator.speed = 0f;
			return animator;
		}

		public static Animator Resume(this Animator animator)
		{
			animator.speed = 1f;
			return animator;
		}
	}
}