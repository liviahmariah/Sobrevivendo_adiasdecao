using UnityEngine;
using System.Collections;

public class AnimacaoSpawnRecurso : MonoBehaviour
{
    [Header("Entrada")]
    public float duracaoEntrada = 0.25f;

    [Header("Pulinhos")]
    public float alturaPulo = 0.08f;
    public float duracaoPulo = 0.3f;
    public float intervaloPulos = 1.5f;

    private Vector3 posicaoOriginal;

    private void Start()
    {
        posicaoOriginal = transform.localPosition;

        StartCoroutine(AnimacaoEntrada());
    }

    private IEnumerator AnimacaoEntrada()
    {
        float tempo = 0f;

        Vector3 inicio = posicaoOriginal + Vector3.down * 0.15f;

        transform.localPosition = inicio;

        while (tempo < duracaoEntrada)
        {
            tempo += Time.deltaTime;

            float progresso = tempo / duracaoEntrada;

            // Entrada suave
            float suavizado =
                1f - Mathf.Pow(1f - progresso, 3f);

            transform.localPosition = Vector3.Lerp(
                inicio,
                posicaoOriginal,
                suavizado
            );

            yield return null;
        }

        transform.localPosition = posicaoOriginal;

        // Começa os pulinhos
        StartCoroutine(Pulinhos());
    }

    private IEnumerator Pulinhos()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloPulos);

            yield return StartCoroutine(ExecutarPulo());
        }
    }

    private IEnumerator ExecutarPulo()
    {
        float tempo = 0f;

        while (tempo < duracaoPulo)
        {
            tempo += Time.deltaTime;

            float progresso = tempo / duracaoPulo;

            // Curva em arco: sobe e depois desce
            float altura =
                Mathf.Sin(progresso * Mathf.PI) * alturaPulo;

            transform.localPosition =
                posicaoOriginal + Vector3.up * altura;

            yield return null;
        }

        transform.localPosition = posicaoOriginal;
    }
}