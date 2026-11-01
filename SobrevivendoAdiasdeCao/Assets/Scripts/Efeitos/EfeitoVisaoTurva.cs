using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EfeitoVisaoTurva : MonoBehaviour
{
    [Header("Referência")]
    public Image imagemEfeito;

    [Header("Duração")]
    public float duracaoEfeito = 5f;

    [Header("Intensidade")]
    public float intensidadeMinima = 0.12f;
    public float intensidadeMaxima = 0.38f;

    [Header("Oscilação")]
    public float velocidadeOscilacao = 2f;
    public float intensidadeMovimento = 15f;

    private Coroutine rotinaEfeito;
    private RectTransform rectTransform;
    private Vector2 posicaoOriginal;
    private Vector3 escalaOriginal;
    private Color corOriginal;

    private void Awake()
    {
        rectTransform = imagemEfeito.rectTransform;

        posicaoOriginal = rectTransform.anchoredPosition;
        escalaOriginal = rectTransform.localScale;
        corOriginal = imagemEfeito.color;

        imagemEfeito.gameObject.SetActive(false);
    }

    public void AtivarEfeito()
    {
        if (rotinaEfeito != null)
            StopCoroutine(rotinaEfeito);

        rotinaEfeito = StartCoroutine(ExecutarEfeito());
    }

    private IEnumerator ExecutarEfeito()
    {
        imagemEfeito.gameObject.SetActive(true);

        float tempo = 0f;

        while (tempo < duracaoEfeito)
        {
            tempo += Time.deltaTime;

            float oscilacao = Mathf.Sin(tempo * velocidadeOscilacao);

            float pulso = Mathf.PingPong(
                tempo * velocidadeOscilacao,
                1f
            );

            Color cor = corOriginal;
            cor.a = Mathf.Lerp(
                intensidadeMinima,
                intensidadeMaxima,
                pulso
            );

            imagemEfeito.color = cor;

            rectTransform.anchoredPosition =
                posicaoOriginal +
                new Vector2(
                    oscilacao * intensidadeMovimento,
                    Mathf.Cos(tempo * velocidadeOscilacao * 0.7f)
                    * intensidadeMovimento
                );

            float escala = 1f + Mathf.Abs(oscilacao) * 0.025f;

            rectTransform.localScale =
                escalaOriginal * escala;

            yield return null;
        }

        imagemEfeito.color = corOriginal;
        rectTransform.anchoredPosition = posicaoOriginal;
        rectTransform.localScale = escalaOriginal;

        imagemEfeito.gameObject.SetActive(false);
        rotinaEfeito = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            AtivarEfeito();
    }
}