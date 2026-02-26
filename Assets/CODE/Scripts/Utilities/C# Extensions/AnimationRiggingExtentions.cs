using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Utilities.Extensions
{
    public static class AnimationRiggingExtensions
    {
        public static void SetTowBoneIK(this TwoBoneIKConstraint ik, [MinValue(0), MaxValue(1)] float weight, Vector3 targetPosition = default, Vector3 targetRotation = default, Vector3 hintPosition = default, Vector3 hintRotation = default)
        {
            if (!ik) return;
		
            weight = weight.Clamp01();

            var hint = ik.data.hint;
            var target = ik.data.target;
		
            if (hintPosition != default) hint.localPosition = hintPosition;
            if (hintRotation != default) hint.localRotation = Quaternion.Euler(hintRotation);
		
             if (targetPosition != default) target.localPosition = targetPosition;
             if (targetRotation != default) target.localRotation = Quaternion.Euler(targetRotation);

            ik.weight = weight;
        }
    }
}