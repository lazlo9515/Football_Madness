using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    // This 'static' variable stays in the computer's memory 
    // even when we change scenes.
    public static PersistentObject instance;

    void Awake()
    {
        // 1. Check if an instance already exists in the game
        if (instance != null && instance != this)
        {
            // 2. If one exists and it's not ME, destroy myself!
            Destroy(gameObject);
            return;
        }

        // 3. If I'm the first one, set the 'instance' to me
        instance = this;

        // 4. Tell Unity not to kill this object when changing scenes
        DontDestroyOnLoad(gameObject);
    }
}