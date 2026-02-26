using UnityEngine;

namespace Utilities.Extensions
{
    public static class BehaviourExtension
    {
        /// <summary>
        /// Extension method for Behaviour that enables it if disabled. If it is already enabled, nothing will happen.
        /// Returns Behaviour for chaining.
        /// </summary>
        public static Behaviour EnableBehaviourIfDisabled(this Behaviour behaviour)
        {
            if (!behaviour.enabled)
                behaviour.enabled = true;
            return behaviour;
        }

        /// <summary>
        /// Extension method for Behaviour that disables it if enabled. If it is already disabled, nothing will happen.
        /// Returns Behaviour for chaining.
        /// </summary>
        public static Behaviour DisableBehaviourIfEnabled(this Behaviour behaviour)
        {
            if (behaviour.enabled)
                behaviour.enabled = false;
            return behaviour;
        }

        /// <summary>
        /// Extension method for Behaviour that enables it.
        /// Returns Behaviour for chaining.
        /// </summary>
        public static Behaviour EnableBehaviour(this Behaviour behaviour)
        {
            behaviour.enabled = true;
            return behaviour;
        }

        /// <summary>
        /// Extension method for Behaviour that disables it.
        /// Returns Behaviour for chaining.
        /// </summary>
        public static Behaviour DisableBehaviour(this Behaviour behaviour)
        {
            behaviour.enabled = false;
            return behaviour;
        }

        /// <summary>
        /// Extension method for Behaviour that toggles its enabled state. If it is already disabled, this method will enable it. If it is already enabled, this method will disable it.
        /// Returns Behaviour for chaining.
        /// </summary>
        public static Behaviour ToggleBehaviour(this Behaviour behaviour)
        {
            behaviour.enabled = !behaviour.enabled;
            return behaviour;
        }
    }
}
