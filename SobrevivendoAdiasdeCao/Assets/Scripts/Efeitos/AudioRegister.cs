using UnityEngine;

public class AudioRegister : MonoBehaviour
{
    public enum AudioType { Music, SFX }

    [SerializeField] private AudioType category = AudioType.SFX;

    private void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (audioSource == null) return;

        // Verifica se o AudioManager existe e faz o registro
        if (AudioManager.Instance != null)
        {
            if (category == AudioType.Music)
            {
                AudioManager.Instance.RegisterMusicSource(audioSource);
            }
            else
            {
                AudioManager.Instance.RegisterSFXSource(audioSource);
            }
        }
    }
}