using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Extensions
{
    public static class ColliderExtensions
    {
        public static bool IsCollidingWith(this Collider collider, Collider otherCollider) => collider.bounds.Intersects(otherCollider.bounds);

        public static Collider DisableCollisionWith(this Collider collider, Collider otherCollider)
        {
            Physics.IgnoreCollision(collider, otherCollider, true);
            return collider;
        }

        public static Collider EnableCollisionWith(this Collider collider, Collider otherCollider)
        {
            Physics.IgnoreCollision(collider, otherCollider, false);
            return collider;
        }

        public static Collider DisableCollisionWith(this Collider collider, List<Collider> otherColliders)
        {
            if (!collider || otherColliders == null) return null;

            foreach (var t in otherColliders) collider.DisableCollisionWith(t);

            return collider;
        }

        public static Collider EnableCollisionWith(this Collider collider, List<Collider> otherColliders)
        {
            if (!collider || otherColliders == null) return null;

            foreach (var t in otherColliders) collider.EnableCollisionWith(t);

            return collider;
        }

        public static bool IsCollidingWithLayer(this Collider collider, LayerMask layerMask)
        {
            if (!collider) return false;

            return Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, collider.transform.rotation, layerMask).Length > 0;
        }
    }
}