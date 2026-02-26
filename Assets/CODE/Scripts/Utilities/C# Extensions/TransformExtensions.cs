using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Utilities.Extensions
{
    public static class TransformExtensions
    {
        public static Transform ResetValues(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            return transform;
        }

        #region LocalEulerAngle

        public static Transform SetLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var eulerAngles = transform.localEulerAngles;

            if (x.HasValue)
            {
                eulerAngles.x = x.Value;
            }

            if (y.HasValue)
            {
                eulerAngles.y = y.Value;
            }

            if (z.HasValue)
            {
                eulerAngles.z = z.Value;
            }

            transform.localEulerAngles = eulerAngles;
            return transform;
        }

        public static Transform AddLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var eulerAngles = transform.localEulerAngles;

            if (x.HasValue)
            {
                eulerAngles.x += x.Value;
            }

            if (y.HasValue)
            {
                eulerAngles.y += y.Value;
            }

            if (z.HasValue)
            {
                eulerAngles.z += z.Value;
            }

            transform.localEulerAngles = eulerAngles;
            return transform;
        }

        public static Transform SubtractLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var eulerAngles = transform.localEulerAngles;

            if (x.HasValue)
            {
                eulerAngles.x -= x.Value;
            }

            if (y.HasValue)
            {
                eulerAngles.y -= y.Value;
            }

            if (z.HasValue)
            {
                eulerAngles.z -= z.Value;
            }

            transform.localEulerAngles = eulerAngles;
            return transform;
        }

        public static Transform SetLocalEulerAngleX(this Transform transform, float x)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithX(x);
            return transform;
        }

        public static Transform SetLocalEulerAngleY(this Transform transform, float y)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithY(y);
            return transform;
        }

        public static Transform SetLocalEulerAngleZ(this Transform transform, float z)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithZ(z);
            return transform;
        }

        public static Transform AddLocalEulerAngleX(this Transform transform, float x)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithAddX(x);
            return transform;
        }

        public static Transform AddLocalEulerAngleY(this Transform transform, float y)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithAddY(y);
            return transform;
        }

        public static Transform AddLocalEulerAngleZ(this Transform transform, float z)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithAddZ(z);
            return transform;
        }

        public static Transform SubtractLocalEulerAngleX(this Transform transform, float x)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithSubtractX(x);
            return transform;
        }

        public static Transform SubtractLocalEulerAngleY(this Transform transform, float y)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithSubtractY(y);
            return transform;
        }

        public static Transform SubtractLocalEulerAngleZ(this Transform transform, float z)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithSubtractZ(z);
            return transform;
        }

        #endregion

        #region EulerAngle

        public static Transform SetEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var eulerAngles = transform.eulerAngles;

            if (x.HasValue)
            {
                eulerAngles.x = x.Value;
            }

            if (y.HasValue)
            {
                eulerAngles.y = y.Value;
            }

            if (z.HasValue)
            {
                eulerAngles.z = z.Value;
            }

            transform.eulerAngles = eulerAngles;
            return transform;
        }

        public static Transform AddEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var eulerAngles = transform.eulerAngles;

            if (x.HasValue)
            {
                eulerAngles.x += x.Value;
            }

            if (y.HasValue)
            {
                eulerAngles.y += y.Value;
            }

            if (z.HasValue)
            {
                eulerAngles.z += z.Value;
            }

            transform.eulerAngles = eulerAngles;
            return transform;
        }

        public static Transform SubtractEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var eulerAngles = transform.eulerAngles;

            if (x.HasValue)
            {
                eulerAngles.x -= x.Value;
            }

            if (y.HasValue)
            {
                eulerAngles.y -= y.Value;
            }

            if (z.HasValue)
            {
                eulerAngles.z -= z.Value;
            }

            transform.eulerAngles = eulerAngles;
            return transform;
        }

        public static Transform SetEulerAngleX(this Transform transform, float x)
        {
            transform.eulerAngles = transform.eulerAngles.WithX(x);
            return transform;
        }

        public static Transform SetEulerAngleY(this Transform transform, float y)
        {
            transform.eulerAngles = transform.eulerAngles.WithY(y);
            return transform;
        }

        public static Transform SetEulerAngleZ(this Transform transform, float z)
        {
            transform.eulerAngles = transform.eulerAngles.WithZ(z);
            return transform;
        }

        public static Transform AddEulerAngleX(this Transform transform, float x)
        {
            transform.eulerAngles = transform.eulerAngles.WithAddX(x);
            return transform;
        }

        public static Transform AddEulerAngleY(this Transform transform, float y)
        {
            transform.eulerAngles = transform.eulerAngles.WithAddY(y);
            return transform;
        }

        public static Transform AddEulerAngleZ(this Transform transform, float z)
        {
            transform.eulerAngles = transform.eulerAngles.WithAddZ(z);
            return transform;
        }

        public static Transform SubtractEulerAngleX(this Transform transform, float x)
        {
            transform.eulerAngles = transform.eulerAngles.WithSubtractX(x);
            return transform;
        }

        public static Transform SubtractEulerAngleY(this Transform transform, float y)
        {
            transform.eulerAngles = transform.eulerAngles.WithSubtractY(y);
            return transform;
        }

        public static Transform SubtractEulerAngleZ(this Transform transform, float z)
        {
            transform.eulerAngles = transform.eulerAngles.WithSubtractZ(z);
            return transform;
        }

        #endregion

        #region Position

        public static Transform SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var position = transform.position;

            if (x.HasValue)
            {
                position.x = x.Value;
            }

            if (y.HasValue)
            {
                position.y = y.Value;
            }

            if (z.HasValue)
            {
                position.z = z.Value;
            }

            transform.position = position;
            return transform;
        }

        public static Transform AddPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var position = transform.position;

            if (x.HasValue)
            {
                position.x += x.Value;
            }

            if (y.HasValue)
            {
                position.y += y.Value;
            }

            if (z.HasValue)
            {
                position.z += z.Value;
            }

            transform.position = position;
            return transform;
        }

        public static Transform SubtractPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var position = transform.position;

            if (x.HasValue)
            {
                position.x -= x.Value;
            }

            if (y.HasValue)
            {
                position.y -= y.Value;
            }

            if (z.HasValue)
            {
                position.z -= z.Value;
            }

            transform.position = position;
            return transform;
        }

        #region Set Position

        public static Transform SetPositionX(this Transform transform, float x)
        {
            transform.position = transform.position.WithX(x);
            return transform;
        }

        public static Transform SetPositionY(this Transform transform, float y)
        {
            transform.position = transform.position.WithY(y);
            return transform;
        }

        public static Transform SetPositionZ(this Transform transform, float z)
        {
            transform.position = transform.position.WithZ(z);
            return transform;
        }

        public static Transform SetPositionX(this Transform transform, Vector3 x)
        {
            transform.position = transform.position.WithX(x.x);
            return transform;
        }

        public static Transform SetPositionY(this Transform transform, Vector3 y)
        {
            transform.position = transform.position.WithY(y.y);
            return transform;
        }

        public static Transform SetPositionZ(this Transform transform, Vector3 z)
        {
            transform.position = transform.position.WithZ(z.z);
            return transform;
        }

        public static Transform SetPositionX(this Transform transform, Transform x)
        {
            transform.position = transform.position.WithX(x.position.x);
            return transform;
        }

        public static Transform SetPositionY(this Transform transform, Transform y)
        {
            transform.position = transform.position.WithY(y.position.y);
            return transform;
        }

        public static Transform SetPositionZ(this Transform transform, Transform z)
        {
            transform.position = transform.position.WithZ(z.position.z);
            return transform;
        }

        #endregion

        #region Set Local Position

        public static Transform SetLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = transform.localPosition.WithX(x);
            return transform;
        }

        public static Transform SetLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = transform.localPosition.WithY(y);
            return transform;
        }

        public static Transform SetLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = transform.localPosition.WithZ(z);
            return transform;
        }

        public static Transform SetLocalPositionX(this Transform transform, Vector3 x)
        {
            transform.localPosition = transform.localPosition.WithX(x.x);
            return transform;
        }

        public static Transform SetLocalPositionY(this Transform transform, Vector3 y)
        {
            transform.localPosition = transform.localPosition.WithY(y.y);
            return transform;
        }

        public static Transform SetLocalPositionZ(this Transform transform, Vector3 z)
        {
            transform.localPosition = transform.localPosition.WithZ(z.z);
            return transform;
        }

        public static Transform SetLocalPositionX(this Transform transform, Transform x)
        {
            transform.localPosition = transform.localPosition.WithX(x.localPosition.x);
            return transform;
        }

        public static Transform SetLocalPositionY(this Transform transform, Transform y)
        {
            transform.localPosition = transform.localPosition.WithY(y.localPosition.y);
            return transform;
        }

        public static Transform SetLocalPositionZ(this Transform transform, Transform z)
        {
            transform.localPosition = transform.localPosition.WithZ(z.localPosition.z);
            return transform;
        }

        #endregion

        #region Set Euler Angles

        public static Transform SetEulerAnglesX(this Transform transform, float x)
        {
            transform.eulerAngles = transform.eulerAngles.WithX(x);
            return transform;
        }

        public static Transform SetEulerAnglesY(this Transform transform, float y)
        {
            transform.eulerAngles = transform.eulerAngles.WithY(y);
            return transform;
        }

        public static Transform SetEulerAnglesZ(this Transform transform, float z)
        {
            transform.eulerAngles = transform.eulerAngles.WithZ(z);
            return transform;
        }

        public static Transform SetEulerAnglesX(this Transform transform, Vector3 x)
        {
            transform.eulerAngles = transform.eulerAngles.WithX(x.x);
            return transform;
        }

        public static Transform SetEulerAnglesY(this Transform transform, Vector3 y)
        {
            transform.eulerAngles = transform.eulerAngles.WithY(y.y);
            return transform;
        }

        public static Transform SetEulerAnglesZ(this Transform transform, Vector3 z)
        {
            transform.eulerAngles = transform.eulerAngles.WithZ(z.z);
            return transform;
        }

        public static Transform SetEulerAnglesX(this Transform transform, Transform x)
        {
            transform.eulerAngles = transform.eulerAngles.WithX(x.eulerAngles.x);
            return transform;
        }

        public static Transform SetEulerAnglesY(this Transform transform, Transform y)
        {
            transform.eulerAngles = transform.eulerAngles.WithY(y.eulerAngles.y);
            return transform;
        }

        public static Transform SetEulerAnglesZ(this Transform transform, Transform z)
        {
            transform.eulerAngles = transform.eulerAngles.WithZ(z.eulerAngles.z);
            return transform;
        }

        #endregion

        #region Set Local Euler Angles

        public static Transform SetLocalEulerAnglesX(this Transform transform, float x)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithX(x);
            return transform;
        }

        public static Transform SetLocalEulerAnglesY(this Transform transform, float y)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithY(y);
            return transform;
        }

        public static Transform SetLocalEulerAnglesZ(this Transform transform, float z)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithZ(z);
            return transform;
        }

        public static Transform SetLocalEulerAnglesX(this Transform transform, Vector3 x)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithX(x.x);
            return transform;
        }

        public static Transform SetLocalEulerAnglesY(this Transform transform, Vector3 y)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithY(y.y);
            return transform;
        }

        public static Transform SetLocalEulerAnglesZ(this Transform transform, Vector3 z)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithZ(z.z);
            return transform;
        }

        public static Transform SetLocalEulerAnglesX(this Transform transform, Transform x)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithX(x.localEulerAngles.x);
            return transform;
        }

        public static Transform SetLocalEulerAnglesY(this Transform transform, Transform y)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithY(y.localEulerAngles.y);
            return transform;
        }

        public static Transform SetLocalEulerAnglesZ(this Transform transform, Transform z)
        {
            transform.localEulerAngles = transform.localEulerAngles.WithZ(z.localEulerAngles.z);
            return transform;
        }

        #endregion

        #region Set Local Scale

        public static Transform SetLocalScale(this Transform transform, float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
            return transform;
        }

        public static Transform SetLocalScaleX(this Transform transform, float x)
        {
            transform.localScale = transform.localScale.WithX(x);
            return transform;
        }

        public static Transform SetLocalScaleY(this Transform transform, float y)
        {
            transform.localScale = transform.localScale.WithY(y);
            return transform;
        }

        public static Transform SetLocalScaleZ(this Transform transform, float z)
        {
            transform.localScale = transform.localScale.WithZ(z);
            return transform;
        }

        public static Transform SetLocalScaleX(this Transform transform, Vector3 x)
        {
            transform.localScale = transform.localScale.WithX(x.x);
            return transform;
        }

        public static Transform SetLocalScaleY(this Transform transform, Vector3 y)
        {
            transform.localScale = transform.localScale.WithY(y.y);
            return transform;
        }

        public static Transform SetLocalScaleZ(this Transform transform, Vector3 z)
        {
            transform.localScale = transform.localScale.WithZ(z.z);
            return transform;
        }

        public static Transform SetLocalScaleX(this Transform transform, Transform x)
        {
            transform.localScale = transform.localScale.WithX(x.localScale.x);
            return transform;
        }

        public static Transform SetLocalScaleY(this Transform transform, Transform y)
        {
            transform.localScale = transform.localScale.WithY(y.localScale.y);
            return transform;
        }

        public static Transform SetLocalScaleZ(this Transform transform, Transform z)
        {
            transform.localScale = transform.localScale.WithZ(z.localScale.z);
            return transform;
        }

        #endregion

        public static Transform AddPositionX(this Transform transform, float x)
        {
            transform.position = transform.position.WithAddX(x);
            return transform;
        }

        public static Transform AddPositionY(this Transform transform, float y)
        {
            transform.position = transform.position.WithAddY(y);
            return transform;
        }

        public static Transform AddPositionZ(this Transform transform, float z)
        {
            transform.position = transform.position.WithAddZ(z);
            return transform;
        }

        public static Transform SubtractPositionX(this Transform transform, float x)
        {
            transform.position = transform.position.WithSubtractX(x);
            return transform;
        }

        public static Transform SubtractPositionY(this Transform transform, float y)
        {
            transform.position = transform.position.WithSubtractY(y);
            return transform;
        }

        public static Transform SubtractPositionZ(this Transform transform, float z)
        {
            transform.position = transform.position.WithSubtractZ(z);
            return transform;
        }

        #endregion

        #region LocalPosition

        public static Transform SetLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var localPosition = transform.localPosition;

            if (x.HasValue)
            {
                localPosition.x = x.Value;
            }

            if (y.HasValue)
            {
                localPosition.y = y.Value;
            }

            if (z.HasValue)
            {
                localPosition.z = z.Value;
            }

            transform.localPosition = localPosition;
            return transform;
        }

        public static Transform AddLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var localPosition = transform.localPosition;

            if (x.HasValue)
            {
                localPosition.x += x.Value;
            }

            if (y.HasValue)
            {
                localPosition.y += y.Value;
            }

            if (z.HasValue)
            {
                localPosition.z += z.Value;
            }

            transform.localPosition = localPosition;
            return transform;
        }

        public static Transform SubtractLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var localPosition = transform.localPosition;

            if (x.HasValue)
            {
                localPosition.x -= x.Value;
            }

            if (y.HasValue)
            {
                localPosition.y -= y.Value;
            }

            if (z.HasValue)
            {
                localPosition.z -= z.Value;
            }

            transform.localPosition = localPosition;
            return transform;
        }

        public static Transform AddLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = transform.localPosition.WithAddX(x);
            return transform;
        }

        public static Transform AddLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = transform.localPosition.WithAddY(y);
            return transform;
        }

        public static Transform AddLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = transform.localPosition.WithAddZ(z);
            return transform;
        }

        public static Transform SubtractLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = transform.localPosition.WithSubtractX(x);
            return transform;
        }

        public static Transform SubtractLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = transform.localPosition.WithSubtractY(y);
            return transform;
        }

        public static Transform SubtractLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = transform.localPosition.WithSubtractZ(z);
            return transform;
        }

        #endregion

        #region Scale

        public static Transform SetScale(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var scale = transform.localScale;

            if (x.HasValue) scale.x = x.Value;
            if (y.HasValue) scale.y = y.Value;
            if (z.HasValue) scale.z = z.Value;

            transform.localScale = scale;
            return transform;
        }

        public static Transform AddScale(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var scale = transform.localScale;

            if (x.HasValue)
            {
                scale.x += x.Value;
            }

            if (y.HasValue)
            {
                scale.y += y.Value;
            }

            if (z.HasValue)
            {
                scale.z += z.Value;
            }

            transform.localScale = scale;
            return transform;
        }

        public static Transform SubtractScale(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var scale = transform.localScale;

            if (x.HasValue)
            {
                scale.x -= x.Value;
            }

            if (y.HasValue)
            {
                scale.y -= y.Value;
            }

            if (z.HasValue)
            {
                scale.z -= z.Value;
            }

            transform.localScale = scale;
            return transform;
        }

        public static Transform SetScale(this Transform transform, float UniformScaleValue) => transform.SetScale(UniformScaleValue, UniformScaleValue, UniformScaleValue);

        public static Transform AddScale(this Transform transform, float UniformScaleValue) => transform.AddScale(UniformScaleValue, UniformScaleValue, UniformScaleValue);

        public static Transform SubtractScale(this Transform transform, float UniformScaleValue) => transform.SubtractScale(UniformScaleValue, UniformScaleValue, UniformScaleValue);

        public static Transform SetScaleX(this Transform transform, float x)
        {
            transform.localScale = transform.localScale.WithX(x);
            return transform;
        }

        public static Transform SetScaleY(this Transform transform, float y)
        {
            transform.localScale = transform.localScale.WithY(y);
            return transform;
        }

        public static Transform SetScaleZ(this Transform transform, float z)
        {
            transform.localScale = transform.localScale.WithZ(z);
            return transform;
        }

        public static Transform AddScaleX(this Transform transform, float x)
        {
            transform.localScale = transform.localScale.WithAddX(x);
            return transform;
        }

        public static Transform AddScaleY(this Transform transform, float y)
        {
            transform.localScale = transform.localScale.WithAddY(y);
            return transform;
        }

        public static Transform AddScaleZ(this Transform transform, float z)
        {
            transform.localScale = transform.localScale.WithAddZ(z);
            return transform;
        }

        public static Transform SubtractScaleX(this Transform transform, float x)
        {
            transform.localScale = transform.localScale.WithSubtractX(x);
            return transform;
        }

        public static Transform SubtractScaleY(this Transform transform, float y)
        {
            transform.localScale = transform.localScale.WithSubtractY(y);
            return transform;
        }

        public static Transform SubtractScaleZ(this Transform transform, float z)
        {
            transform.localScale = transform.localScale.WithSubtractZ(z);
            return transform;
        }

        #endregion

        #region Children

        public static Transform DestroyChildrens(this Transform transform)
        {
            foreach (Transform child in transform) Object.Destroy(child.gameObject);

            return transform;
        }

        public static Transform DetachChildren(this Transform transform)
        {
            foreach (Transform child in transform) child.SetParent(null);

            return transform;
        }

        public static Transform GetRandomChild(this Transform transform) => transform.GetChild(Random.Range(0, transform.childCount));

        #endregion

        #region Distance

        public static float Distance(this Transform transform, Transform target) => Vector3.Distance(transform.position, target.position);

        public static float Distance(this Transform transform, Vector3 target) => Vector3.Distance(transform.position, target);

        public static float DistanceWithoutHeight(this Transform transform, Transform target) => Vector3.Distance(transform.position.WithY(0), target.position.WithY(0));

        public static float DistanceWithoutHeight(this Transform transform, Vector3 target) => Vector3.Distance(transform.position.WithY(0), target.WithY(0));

        #endregion

        #region Direction

        public static Vector3 DirectionTo(this Transform source, Transform target) => (target.position - source.position).normalized;

        public static Vector3 DirectionFrom(this Transform target, Transform source) => (source.position - target.position).normalized;

        public static Vector3 DirectionToIgnoringHeight(this Transform source, Transform target) => (target.position.WithY(0) - source.position.WithY(0)).normalized;

        public static Vector3 DirectionFromIgnoringHeight(this Transform target, Transform source) => (source.position.WithY(0) - target.position.WithY(0)).normalized;

        public static Vector3 DirectionTo(this Transform source, Vector3 target) => (target - source.position).normalized;

        public static Vector3 DirectionFrom(this Transform target, Vector3 source) => (source - target.position).normalized;

        public static Vector3 DirectionToIgnoringHeight(this Transform source, Vector3 target) => (target.WithY(0) - source.position.WithY(0)).normalized;

        public static Vector3 DirectionFromIgnoringHeight(this Transform target, Vector3 source) => (source.WithY(0) - target.position.WithY(0)).normalized;

        public static Vector3 back(this Transform v) => -v.forward;

        public static Vector3 left(this Transform v) => -v.right;

        public static Vector3 down(this Transform v) => -v.up;

        #endregion

        #region Simple Movements

        public static Transform MoveTowards(this Transform source, Transform target, float speed)
        {
            source.position = Vector3.MoveTowards(source.position, target.position, speed);
            return source;
        }

        public static Transform MoveTowards(this Transform source, Vector3 target, float speed)
        {
            source.position = Vector3.MoveTowards(source.position, target, speed);
            return source;
        }

        public static Transform ContinuesChaseTargetWhile(this Transform agent, Transform target, MonoBehaviour monoBehaviour, float speed = 5, float? minDistanceKeep = null, float? maxDistanceKeep = null, float? delayBetweenSettingDestination = null, Func<bool> loopCondition = null, Func<float> distanceToPlayer = null)
        {
            monoBehaviour.StartCoroutine(ChaseTargetCoroutine());

            IEnumerator ChaseTargetCoroutine()
            {
                WaitForSeconds delay = new WaitForSeconds(delayBetweenSettingDestination ?? 0);
                Vector3 selfPosition = agent.transform.position;

                while (!agent || !target || loopCondition == null || loopCondition())
                {
                    float distance = distanceToPlayer?.Invoke() ?? agent.transform.Distance(target);

                    if (minDistanceKeep == null || maxDistanceKeep == null)
                    {
                        agent.MoveTowards(target.position, speed * Time.deltaTime);
                    }
                    else
                    {
                        if (distance < minDistanceKeep.Value)
                        {
                            Vector3 directionToMoveWhenTooCloseToPlayer = (target.position - selfPosition).normalized;
                            Vector3 positionToMove = selfPosition + directionToMoveWhenTooCloseToPlayer * -1 * (maxDistanceKeep.Value - distance);
                            agent.MoveTowards(positionToMove.WithY(selfPosition.y), speed * Time.deltaTime);
                        }
                        else
                        {
                            agent.MoveTowards(target.position, speed * Time.deltaTime);
                        }
                    }

                    if (delayBetweenSettingDestination == null)
                        yield return null;
                    else
                        yield return delay;
                }
            }

            return agent;
        }

        public static bool HasReachedDestination(this Transform agent, Transform destination, float tolerence = 0.1f) => agent.Distance(destination) < tolerence;

        public static bool HasReachedDestination(this Transform agent, Vector3 destination, float tolerence = 0.1f) => agent.Distance(destination) < tolerence;

        public static bool SetRandomDestination(this Transform agent, out Vector3 randomLocation, float radius, float speed, Vector3? origin = null)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomLocation = randomDirection + (origin ?? agent.transform.position);
            agent.MoveTowards(randomLocation, speed);
            return true;
        }

        public static bool SetRandomDestination(this Transform agent, float radius, float speed, Vector3? origin = null)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            Vector3 randomLocation = randomDirection + (origin ?? agent.transform.position);
            agent.MoveTowards(randomLocation, speed);
            return true;
        }

        public static Transform Wander(this Transform agent, float radius, MonoBehaviour monoBehaviour, float speed, bool isContinues = true, float waitTime = 1, Func<bool> condition = null, bool useSameHeight = true)
        {
            if (isContinues)
                monoBehaviour.StartCoroutine(WanderCoroutine());
            else
                agent.SetRandomDestination(radius, speed * Time.deltaTime);

            IEnumerator WanderCoroutine()
            {
                Vector3 randomLocation = Random.insideUnitSphere * radius;
                if (useSameHeight) randomLocation.SetY(agent.position);
                while (agent != null && condition != null ? condition() : true)
                {
                    if (agent.HasReachedDestination(randomLocation))
                    {
                        yield return new WaitForSeconds(waitTime);
                        randomLocation = Random.insideUnitSphere * radius;
                        if (useSameHeight) randomLocation.SetY(agent.position);
                    }
                    else
                        agent.MoveTowards(randomLocation, 1);

                    yield return null;
                }
            }

            return agent;
        }

        public static Transform ContinuesFleeFromTargetWhile(this Transform agent, Transform target, MonoBehaviour monoBehaviour, float speed, float fleeDistance = 10, Func<bool> condition = null)
        {
            if (agent == null || target == null) return null;

            monoBehaviour.StartCoroutine(FleeFromTargetCoroutine());

            IEnumerator FleeFromTargetCoroutine()
            {
                while (agent != null && target != null && condition != null ? condition() : true)
                {
                    Vector3 fleeDirection = (agent.transform.position - target.position).normalized;
                    Vector3 fleePosition = agent.transform.position + fleeDirection * fleeDistance;

                    agent.MoveTowards(fleePosition, speed * Time.deltaTime);

                    yield return null;
                }
            }

            return agent;
        }

        public static Transform ContinuesPatrolWaypointsWhile(this Transform agent, List<Transform> waypoints, float speed, MonoBehaviour monoBehaviour, bool followWaypointOrder = true, Func<bool> condition = null)
        {
            if (agent == null || waypoints == null || waypoints.Count == 0) return null;
            monoBehaviour.StartCoroutine(PatrolWaypointsCoroutine());

            IEnumerator PatrolWaypointsCoroutine()
            {
                int currentWaypointIndex = 0;

                while (agent != null && condition != null ? condition() : true)
                {
                    Transform currentWaypoint = waypoints[currentWaypointIndex];
                    agent.MoveTowards(currentWaypoint.position, speed * Time.deltaTime);

                    if (agent.HasReachedDestination(currentWaypoint))
                    {
                        currentWaypointIndex = followWaypointOrder ? (currentWaypointIndex + 1) % waypoints.Count : Random.Range(0, waypoints.Count);
                    }

                    yield return null;
                }
            }

            return agent;
        }

        #endregion

        #region LookAt

        public static Transform LookAtIgnoringY(this Transform source, Transform target, Vector3? forward = null)
        {
            return source.LookAtIgnoringY(target.position, forward);
        }

        public static Transform LookAtIgnoringY(this Transform source, Vector3 target, Vector3? forward = null)
        {
            Vector3 direction = (target.WithPosY(source) - source.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                // Apply offset to account for custom forward axis
                if (forward.HasValue && forward.Value != Vector3.forward)
                {
                    Quaternion offset = Quaternion.FromToRotation(Vector3.forward, forward.Value);
                    source.rotation = lookRotation * offset;
                }
                else
                {
                    source.rotation = lookRotation;
                }
            }

            return source;
        }

        #endregion

        #region Dot Product and Cross Product

        public static float Dot(this Transform v, Transform target) => Vector3.Dot(v.forward, (target.position.WithY(0) - v.position.WithY(0)).normalized);

        public static float Dot(this Transform v, Vector3 target) => Vector3.Dot(v.forward, (target.WithY(0) - v.position.WithY(0)).normalized);

        public static Vector3 Cross(this Transform v, Transform target) => Vector3.Cross(v.forward, (target.position.WithY(0) - v.position.WithY(0)).normalized);

        public static Vector3 Cross(this Transform v, Vector3 target) => Vector3.Cross(v.forward, (target.WithY(0) - v.position.WithY(0)).normalized);

        #endregion

        #region Position in world space and local space

        public static Vector3 WorldSpacePositionToLocalSpace(this Transform transform, Vector3 position)
        {
            Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocal.MultiplyPoint3x4(position);
        }

        public static Vector3 LocalSpacePositionToWorldSpace(this Transform transform, Vector3 position)
        {
            Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorld.MultiplyPoint3x4(position);
        }

        #endregion

        /// Getting the total bound size of a game object.
        public static Bounds CalculateBounds(this Transform obj, bool includeInactive = true)
        {
            // get the maximum bounds extent of an object, including all child renderers,
            // but excluding particles and trails, for FOV zooming effect.

            var renderers = obj.GetComponentsInChildren<Renderer>(includeInactive).ToList();
            renderers.AddRange(obj.GetComponents<Renderer>());

            if (renderers.IsNullOrEmpty()) return new Bounds();

            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers.Where(r => r is not (TrailRenderer or ParticleSystemRenderer)))
            {
                bounds.Encapsulate(r.bounds);
            }

            return bounds;
        }

        public static Bounds CalculateLocalBounds(this Transform obj, bool includeInactive = true)
        {
            // Filter out particles and trails as per your original logic
            var renderers = obj.GetComponentsInChildren<Renderer>(includeInactive) .Where(r => r is not (TrailRenderer or ParticleSystemRenderer)).ToList();
            if (renderers.Count == 0) return new Bounds(Vector3.zero, Vector3.zero);

            Bounds bounds = new(Vector3.zero, Vector3.zero);
            bool initialized = false;

            foreach (var renderer in renderers)
            {
                Bounds localMeshBounds;

                // Handle SkinnedMesh vs Standard Mesh vs Sprite/Others
                if (renderer is SkinnedMeshRenderer smr)
                {
                    localMeshBounds = smr.localBounds; // SMR has a built-in localBounds property
                }
                else if (renderer is MeshRenderer && renderer.TryGetComponent<MeshFilter>(out var mf) && mf.sharedMesh != null)
                {
                    localMeshBounds = mf.sharedMesh.bounds;
                }
                else
                {
                    // Fallback for Sprites or other renderers: 
                    // Convert their world bounds back to local space (less accurate for rotation, but safe)
                    localMeshBounds = renderer.bounds;
                    // Since this is world space, we convert it to the renderer's local space first
                    // so the corner logic below works uniformly.
                    var center = renderer.transform.InverseTransformPoint(localMeshBounds.center);
                    var extents = renderer.transform.InverseTransformVector(localMeshBounds.extents);
                    localMeshBounds = new Bounds(center, extents * 2);
                }

                // Create matrix to go directly from child's space to root's space
                // Matrix4x4 childToRoot = root.worldToLocalMatrix * child.localToWorldMatrix;
                Matrix4x4 m = obj.worldToLocalMatrix * renderer.transform.localToWorldMatrix;

                Vector3 centerPoints = localMeshBounds.center;
                Vector3 extentsPoints = localMeshBounds.extents;

                // The 8 corners of the local mesh box
                Vector3[] corners = new Vector3[8]
                {
                    m.MultiplyPoint3x4(centerPoints + new Vector3(-extentsPoints.x, -extentsPoints.y, -extentsPoints.z)),
                    m.MultiplyPoint3x4(centerPoints + new Vector3(extentsPoints.x, -extentsPoints.y, -extentsPoints.z)),
                    m.MultiplyPoint3x4(centerPoints + new Vector3(-extentsPoints.x, extentsPoints.y, -extentsPoints.z)),
                    m.MultiplyPoint3x4(centerPoints + new Vector3(extentsPoints.x, extentsPoints.y, -extentsPoints.z)),
                    m.MultiplyPoint3x4(centerPoints + new Vector3(-extentsPoints.x, -extentsPoints.y, extentsPoints.z)),
                    m.MultiplyPoint3x4(centerPoints + new Vector3(extentsPoints.x, -extentsPoints.y, extentsPoints.z)),
                    m.MultiplyPoint3x4(centerPoints + new Vector3(-extentsPoints.x, extentsPoints.y, extentsPoints.z)),
                    m.MultiplyPoint3x4(centerPoints + new Vector3(extentsPoints.x, extentsPoints.y, extentsPoints.z))
                };

                foreach (var corner in corners)
                {
                    if (!initialized)
                    {
                        bounds = new Bounds(corner, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(corner);
                    }
                }
            }

            return bounds;
        }

        /// Check if the transform is within a certain distance and optionally within a certain angle (FOV) from the target transform.
        /// <param name="source">The transform to check.</param>
        /// <param name="target">The target transform to compare the distance and optional angle with.</param>
        /// <param name="maxDistance">The maximum distance allowed between the two transforms.</param>
        /// <param name="maxAngle">The maximum allowed angle between the transform's forward vector and the direction to the target (default is 360).</param>
        /// <returns>True if the transform is within range and angle (if provided) of the target, false otherwise.</returns>
        public static bool InRangeOf(this Transform source, Transform target, float maxDistance, float maxAngle = 360f)
        {
            Vector3 directionToTarget = (target.position - source.position).WithY(0);
            return directionToTarget.magnitude <= maxDistance && Vector3.Angle(source.forward, directionToTarget) <= maxAngle / 2;
        }

        /// Retrieves all the children of a given Transform.
        /// <remarks>
        /// This method can be used with LINQ to perform operations on all child Transforms. For example,
        /// you could use it to find all children with a specific tag, to disable all children, etc.
        /// Transform implements IEnumerable and the GetEnumerator method which returns an IEnumerator of all its children.
        /// </remarks>
        /// <param name="parent">The Transform to retrieve children from.</param>
        /// <returns>An IEnumerable&lt;Transform&gt; containing all the child Transforms of the parent.</returns>    
        public static IEnumerable<Transform> Children(this Transform parent)
        {
            foreach (Transform child in parent)
            {
                yield return child;
            }
        }

        /// Resets transform's position, scale and rotation
        /// <param name="transform">Transform to use</param>
        public static void Reset(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// Destroys all child game objects of the given transform.
        /// <param name="parent">The Transform whose child game objects are to be destroyed.</param>
        public static void DestroyChildren(this Transform parent)
        {
            parent.ForEveryChild(child => Object.Destroy(child.gameObject));
        }

        /// Immediately destroys all child game objects of the given transform.
        /// <param name="parent">The Transform whose child game objects are to be immediately destroyed.</param>
        public static void DestroyChildrenImmediate(this Transform parent)
        {
            parent.ForEveryChild(child => Object.DestroyImmediate(child.gameObject));
        }

        /// Enables all child game objects of the given transform.
        /// <param name="parent">The Transform whose child game objects are to be enabled.</param>
        public static void EnableChildren(this Transform parent)
        {
            parent.ForEveryChild(child => child.gameObject.SetActive(true));
        }

        /// Disables all child game objects of the given transform.
        /// <param name="parent">The Transform whose child game objects are to be disabled.</param>
        public static void DisableChildren(this Transform parent)
        {
            parent.ForEveryChild(child => child.gameObject.SetActive(false));
        }

        /// Executes a specified action for each child of a given transform.
        /// <param name="parent">The parent transform.</param>
        /// <param name="action">The action to be performed on each child.</param>
        /// <remarks>
        /// This method iterates over all child transforms in reverse order and executes a given action on them.
        /// The action is a delegate that takes a Transform as parameter.
        /// </remarks>
        public static void ForEveryChild(this Transform parent, Action<Transform> action)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                action(parent.GetChild(i));
            }
        }
    }
}