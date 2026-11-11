using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EfeitoVisaoRuimUI : MonoBehaviour
{
    public static EfeitoVisaoRuimUI Instance { get; private set; }

    [Header("Referências da UI")]
    [Tooltip("Imagem do Canvas que cobre toda a tela (Overlay).")]
    public Image imagemOverlay;

    [Header("Configurações do Efeito")]
    [Tooltip("Duração total da visão ruim em segundos.")]
    public float duracaoEfeito = 3f;
    [Tooltip("Cor e transparência máxima no pico do efeito.")]
    public Color corEfeito = new Color(0.5f, 0f, 0.2f, 0.6f); // Roxo/Vermelho turvo

    private Coroutine rotinaEfeito;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Garante que o painel comece invisível
        if (imagemOverlay != null)
        {
            Color c = imagemOverlay.color;
            c.a = 0f;
            imagemOverlay.color = c;
            imagemOverlay.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Chama o efeito de visão ruim/tontura de forma simples.
    /// </summary>
    public void AtivarVisaoRuim()
    {
        if (imagemOverlay == null) return;

        if (rotinaEfeito != null)
            StopCoroutine(rotinaEfeito);

        rotinaEfeito = StartCoroutine(RotinaPiscarVisao());
    }

    private IEnumerator RotinaPiscarVisao()
    {
        float tempo = 0f;

        // FADE IN: A tela fica ruim rapidamente
        while (tempo < 0.3f)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, corEfeito.a, tempo / 0.3f);
            imagemOverlay.color = new Color(corEfeito.r, corEfeito.g, corEfeito.b, alpha);
            yield return null;
        }

        // PERMANÊNCIA COM PULSAÇÃO SUAVE (Efeito tontura)
        float tempoManted = 0f;
        while (tempoManted < duracaoEfeito)
        {
            tempoManted += Time.deltaTime;
            // Cria uma leve oscilação na opacidade simulando pulsação/tontura
            float pulsacao = Mathf.PingPong(Time.time * 3f, 0.2f);
            float alphaAtual = Mathf.Clamp01(corEfeito.a - 0.1f + pulsacao);

            imagemOverlay.color = new Color(corEfeito.r, corEfeito.g, corEfeito.b, alphaAtual);
            yield return null;
        }

        // FADE OUT: A visão da Sandy vai voltando ao normal gradualmente
        tempo = 0f;
        while (tempo < 0.8f)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(corEfeito.a, 0f, tempo / 0.8f);
            imagemOverlay.color = new Color(corEfeito.r, corEfeito.g, corEfeito.b, alpha);
            yield return null;
        }

        // Reseta o alpha para zero
        imagemOverlay.color = new Color(corEfeito.r, corEfeito.g, corEfeito.b, 0f);
        rotinaEfeito = null;
    }
}