using UnityEngine;
using TMPro;

public class MercadoUI : MonoBehaviour
{
    [Header("Texto do Timer")]
    public TextMeshProUGUI textoTempo;

    [Header("Som da contagem")]
    public AudioSource audioSourceBip;
    public AudioClip somBip;

    [Range(0f, 1f)]
    public float volumeMinimo = 0.15f;

    [Range(0f, 1f)]
    public float volumeMaximo = 1f;

    [Tooltip("Tempo restante a partir do qual o volume chega ao máximo.")]
    public float tempoParaVolumeMaximo = 10f;

    private int ultimoSegundoExibido = -1;
    private bool possuiSegundoAnterior = false;

    private void Start()
    {
        AtualizarUI();
    }

    private void Update()
    {
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        if (MercadoManager.Instance == null)
            return;

        float tempo = Mathf.Max(
            0f,
            MercadoManager.Instance.ObterTempoRestante()
        );

        int segundos = Mathf.CeilToInt(tempo);

        // Formato fixo MM:SS.
        int minutos = segundos / 60;
        int segundosRestantes = segundos % 60;

        if (textoTempo != null)
        {
            textoTempo.text =
                minutos.ToString("00") + ":" +
                segundosRestantes.ToString("00");
        }

        // Toca o bip apenas quando o segundo muda.
        if (
            !possuiSegundoAnterior ||
            segundos != ultimoSegundoExibido
        )
        {
            if (
                possuiSegundoAnterior &&
                segundos < ultimoSegundoExibido
            )
            {
                TocarBip(tempo);
            }

            ultimoSegundoExibido = segundos;
            possuiSegundoAnterior = true;
        }
    }

    private void TocarBip(float tempoRestante)
    {
        if (audioSourceBip == null || somBip == null)
            return;

        float duracaoVolume = Mathf.Max(
            0.01f,
            tempoParaVolumeMaximo
        );

        float progresso = 1f - Mathf.Clamp01(
            tempoRestante / duracaoVolume
        );

        float volume = Mathf.Lerp(
            volumeMinimo,
            volumeMaximo,
            progresso
        );

        audioSourceBip.PlayOneShot(somBip, volume);
    }

    private void OnDisable()
    {
        ultimoSegundoExibido = -1;
        possuiSegundoAnterior = false;
    }
}