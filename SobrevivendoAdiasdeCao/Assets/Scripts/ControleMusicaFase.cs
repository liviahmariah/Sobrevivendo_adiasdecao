
using System.Collections;
using UnityEngine;

public class ControleMusicaFase : MonoBehaviour
{
    [Header("Música da fase")]
    [SerializeField] private AudioSource musicaFase;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeNormal = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float volumeTelasFinais = 0.2f;

    [SerializeField] private float duracaoTransicao = 0.5f;

    private Coroutine transicaoAtual;

    private void Awake()
    {
        if (musicaFase == null)
            musicaFase = GetComponent<AudioSource>();

        if (musicaFase != null)
            musicaFase.volume = volumeNormal;
        else
            Debug.LogError(
                "ControleMusicaFase: AudioSource não configurado!",
                this
            );
    }

    public void MostrarTelaFinal()
    {
        AlterarVolume(volumeTelasFinais);
    }

    public void EsconderTelaFinal()
    {
        AlterarVolume(volumeNormal);
    }

    private void AlterarVolume(float volumeDestino)
    {
        if (musicaFase == null)
            return;

        if (transicaoAtual != null)
            StopCoroutine(transicaoAtual);

        transicaoAtual = StartCoroutine(
            TransicionarVolume(volumeDestino)
        );
    }

    private IEnumerator TransicionarVolume(float volumeDestino)
    {
        float volumeInicial = musicaFase.volume;
        float tempo = 0f;

        while (tempo < duracaoTransicao)
        {
            tempo += Time.unscaledDeltaTime;

            musicaFase.volume = Mathf.Lerp(
                volumeInicial,
                volumeDestino,
                Mathf.Clamp01(tempo / duracaoTransicao)
            );

            yield return null;
        }

        musicaFase.volume = volumeDestino;
        transicaoAtual = null;
    }
}
