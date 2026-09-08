using UnityEngine;

public class AudioBetweenScenes : MonoBehaviour
{
    private static AudioBetweenScenes instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}