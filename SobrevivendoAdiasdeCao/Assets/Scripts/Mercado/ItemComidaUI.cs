using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemComidaUI : MonoBehaviour
{
    [Header("Imagem da comida")]
    public Image imagem;

    [Header("Animação")]
    public float duracaoAnimacao = 0.35f;

    public float escalaInicial = 0.3f;

    public float alturaEntrada = 20f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void ColocarImagem(Sprite sprite)
    {
        if (imagem == null)
        {
            Debug.LogError(
                "ItemComidaUI: Image não foi configurada!"
            );

            return;
        }

        imagem.sprite = sprite;
        imagem.preserveAspect = true;

        StartCoroutine(AnimacaoEntrada());
    }

    private IEnumerator AnimacaoEntrada()
    {
        if (rect == null)
            yield break;

        // Guarda a posição que o CaixaComidasUI escolheu
        Vector2 posicaoFinal =
            rect.anchoredPosition;

        // Começa um pouco abaixo
        Vector2 posicaoInicial =
            posicaoFinal +
            Vector2.down * alturaEntrada;

        rect.anchoredPosition =
            posicaoInicial;

        // Começa pequeno
        transform.localScale =
            Vector3.one * escalaInicial;

        float tempo = 0f;

        while (tempo < duracaoAnimacao)
        {
            tempo += Time.deltaTime;

            float progresso =
                Mathf.Clamp01(
                    tempo / duracaoAnimacao
                );

            // Movimento suave
            float suavizado =
                1f -
                Mathf.Pow(
                    1f - progresso,
                    3f
                );

            // Faz a comida subir até sua posição
            rect.anchoredPosition =
                Vector2.Lerp(
                    posicaoInicial,
                    posicaoFinal,
                    suavizado
                );

            // Faz a comida crescer
            transform.localScale =
                Vector3.Lerp(
                    Vector3.one * escalaInicial,
                    Vector3.one,
                    suavizado
                );

            yield return null;
        }

        // Garante o estado final
        rect.anchoredPosition =
            posicaoFinal;

        transform.localScale =
            Vector3.one;
    }
}