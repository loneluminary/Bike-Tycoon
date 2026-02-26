using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new();
    
    public static T HasInstance
    {
        get
        {
            lock (_lock)
            {
                if (_instance) return _instance;
                return _instance = (T)FindFirstObjectByType(typeof(T), FindObjectsInactive.Include);
            }
        }
    }

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance) return _instance;

                // Search for existing instance.
                _instance = (T)FindFirstObjectByType(typeof(T), FindObjectsInactive.Include);
                
                // Create new instance if one doesn't already exist.
                if (_instance) return _instance;
                // Need to create a new GameObject to attach the singleton to.
                var singletonObject = new GameObject();
                _instance = singletonObject.AddComponent<T>();
                singletonObject.name = typeof(T).ToString();

                return _instance;
            }
        }
    }
}