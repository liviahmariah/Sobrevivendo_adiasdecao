
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class QuadroReativoLatido : MonoBehaviour
{
    [Header("Latido do cachorro")]
    public AudioClip latido;

    [Range(0f, 1f)]
    public float volumeLatido = 1f;

    [Header("Movimento do quadro")]
    public float anguloMaximo = 7f;
    public float duracaoSacudida = 0.35f;
    public float atrasoReacao = 0f;

    private Quaternion rotacaoOriginal;
    private Coroutine animacaoAtual;
    private AudioSource audioSource;

    private void Awake()
    {
        rotacaoOriginal = transform.localRotation;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // Chamado quando o jogador clica no quadro.
    private void OnMouseDown()
    {
        TocarLatido();
        ReagirAoLatido();
    }

    // Toca somente o latido atribuído a este cachorro.
    private void TocarLatido()
    {
        if (latido == null)
        {
            Debug.LogWarning(
                "Nenhum latido foi atribuído ao quadro: "
                + gameObject.name
            );

            return;
        }

        audioSource.PlayOneShot(latido, volumeLatido);
    }

    // Animação de sacudida já existente.
    public void ReagirAoLatido()
    {
        if (animacaoAtual != null)
        {
            StopCoroutine(animacaoAtual);
        }

        animacaoAtual = StartCoroutine(Sacudir());
    }

    private IEnumerator Sacudir()
    {
        if (atrasoReacao > 0f)
        {
            yield return new WaitForSeconds(atrasoReacao);
        }

        float tempo = 0f;

        while (tempo < duracaoSacudida)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoSacudida
            );

            float intensidade = 1f - progresso;

            float angulo = Mathf.Sin(tempo * 35f)
                         * anguloMaximo
                         * intensidade;

            transform.localRotation =
                rotacaoOriginal *
                Quaternion.Euler(0f, 0f, angulo);

            yield return null;
        }

        transform.localRotation = rotacaoOriginal;
        animacaoAtual = null;
    }
}
