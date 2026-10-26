
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlertaFugaMercado : MonoBehaviour
{
    [Header("Tela vermelha")]
    public Image telaVermelha;

    [Range(0f, 1f)]
    public float intensidadeMaxima = 0.45f;

    public float velocidadePiscada = 5f;

    [Header("Som de apito")]
    public AudioSource audioSourceApito;
    public AudioClip somApito;

    [Range(0f, 1f)]
    public float volumeApito = 1f;

    [Header("Músicas")]
    public AudioSource musicaNormal;
    public AudioSource musicaTensa;

    private Coroutine rotinaPiscada;

    private void Awake()
    {
        if (telaVermelha != null)
        {
            telaVermelha.color = new Color(1f, 0f, 0f, 0f);
            telaVermelha.raycastTarget = false;
        }

        // A música tensa deve ficar parada até a fuga começar.
        if (musicaTensa != null)
        {
            musicaTensa.Stop();
            musicaTensa.loop = true;
        }
    }

    private void OnEnable()
    {
        IniciarAlerta();
    }

    private void OnDisable()
    {
        PararAlerta();
    }

    public void IniciarAlerta()
    {
        if (rotinaPiscada != null)
            StopCoroutine(rotinaPiscada);

        if (telaVermelha != null)
            rotinaPiscada = StartCoroutine(PiscarTela());

        // Para a música normal da fase.
        if (musicaNormal != null)
            musicaNormal.Stop();

        // Começa a música tensa em loop.
        if (musicaTensa != null)
        {
            musicaTensa.loop = true;

            if (!musicaTensa.isPlaying)
                musicaTensa.Play();
        }

        // Toca o apito uma vez.
        if (audioSourceApito != null && somApito != null)
            audioSourceApito.PlayOneShot(somApito, volumeApito);
        else
            Debug.LogWarning(
                "Configure o AudioSource e o AudioClip do apito.",
                this
            );
    }

    private IEnumerator PiscarTela()
    {
        float tempo = 0f;

        while (true)
        {
            tempo += Time.unscaledDeltaTime * velocidadePiscada;

            float alpha = (Mathf.Sin(tempo) + 1f) * 0.5f;
            alpha *= intensidadeMaxima;

            telaVermelha.color = new Color(1f, 0f, 0f, alpha);

            yield return null;
        }
    }

    public void PararAlerta()
    {
        if (rotinaPiscada != null)
        {
            StopCoroutine(rotinaPiscada);
            rotinaPiscada = null;
        }

        if (telaVermelha != null)
            telaVermelha.color = new Color(1f, 0f, 0f, 0f);
    }
}
