
using System.Collections;
using UnityEngine;

public class EfeitoChaveSolta : MonoBehaviour
{
    [Header("Animação ao aparecer")]
    public float alturaPulo = 0.7f;
    public float duracaoPulo = 0.45f;
    public float rotacoes = 1.5f;

    [Header("Som")]
    public AudioClip somAoSoltar;
    [Range(0f, 1f)]
    public float volumeSom = 0.8f;

    private Vector3 posicaoInicial;
    private Quaternion rotacaoInicial;

    private void Start()
    {
        posicaoInicial = transform.position;
        rotacaoInicial = transform.rotation;

        // Toca o som quando a chave aparece.
        if (somAoSoltar != null)
        {
            AudioSource.PlayClipAtPoint(
                somAoSoltar,
                transform.position,
                volumeSom
            );
        }

        StartCoroutine(AnimarChave());
    }

    private IEnumerator AnimarChave()
    {
        float tempo = 0f;

        while (tempo < duracaoPulo)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoPulo
            );

            // A chave sobe e depois retorna à posição inicial.
            float altura = Mathf.Sin(
                progresso * Mathf.PI
            ) * alturaPulo;

            transform.position = posicaoInicial
                + Vector3.up * altura;

            // Gira a chave durante o salto.
            float angulo = 360f * rotacoes * progresso;

            transform.rotation = rotacaoInicial
                * Quaternion.Euler(0f, 0f, angulo);

            yield return null;
        }

        // Garante que a chave termine na posição correta.
        transform.position = posicaoInicial;
        transform.rotation = rotacaoInicial;
    }
}
