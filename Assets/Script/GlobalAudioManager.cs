using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager instance;

    [Header("Audio Source Reference")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    void Awake()
    {
        // 1. Check if an instance already exists
        if (instance == null)
        {
            instance = this;

            // 2. IMPORTANT: Detach from any parent (like Canvas) 
            // so DontDestroyOnLoad works correctly
            transform.SetParent(null);

            // 3. Tell Unity to keep this object alive between scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 4. If a duplicate tries to spawn (e.g., returning to MainMenu), delete it
            Destroy(gameObject);
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}