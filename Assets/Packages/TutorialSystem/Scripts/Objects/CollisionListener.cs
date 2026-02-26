
using UnityEngine;

namespace TUTORIAL_SYSTEM
{
    public class CollisionListener : MonoBehaviour
    {
        public TutorialManager.StringAction OnCollision;
        public TutorialManager.StringAction OnTrigger;

        public TutorialManager.StringAction OnCollision2D;
        public TutorialManager.StringAction OnTrigger2D;

        private void OnCollisionEnter(Collision collision) => OnCollision?.Invoke(collision.transform.tag);
        private void OnTriggerEnter(Collider other) => OnTrigger?.Invoke(other.tag);

        private void OnCollisionEnter2D(Collision2D collision) => OnCollision2D?.Invoke(collision.transform.tag);
        private void OnTriggerEnter2D(Collider2D collision) => OnTrigger2D?.Invoke(collision.tag);
    }
}