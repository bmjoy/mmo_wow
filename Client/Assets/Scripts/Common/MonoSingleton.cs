using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : Component
{
    private static T instace;
    private static readonly object lockObj = new object();
    public static T Instance
    {
        get
        {
            if (instace == null)
            {
                lock (lockObj)
                {
                    instace = FindObjectOfType<T>();
                    if (instace == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name, typeof(T));
                        instace = obj.GetComponent<T>();
                        DontDestroyOnLoad(obj);
                    }
                }
            }
            return instace;
        }
    }

    private void Awake()
    {
        instace = this as T;
    }
}