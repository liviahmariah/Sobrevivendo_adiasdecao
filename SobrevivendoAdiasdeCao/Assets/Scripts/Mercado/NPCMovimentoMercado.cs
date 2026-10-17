
using UnityEngine;

public class NPCMovimentoMercado : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 2f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    private Vector2 direcao;
    private Transform pontoSaida;
    private bool configurado;
    private bool saidaValida;

    public Vector2 DirecaoAtual => direcao;

    public void Configurar(Vector2 novaDirecao, Transform saida)
    {
        direcao = novaDirecao.normalized;
        pontoSaida = saida;

        // Confere se o ponto de saída está na direção correta.
        saidaValida = pontoSaida != null &&
            ((direcao.x > 0f &&
              pontoSaida.position.x > transform.position.x) ||
             (direcao.x < 0f &&
              pontoSaida.position.x < transform.position.x));

        if (!saidaValida)
        {
            Debug.LogWarning(
                $"{name}: ponto de saída inválido ou do lado errado. " +
                "O NPC continuará andando sem ser destruído."
            );
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.flipX = direcao.x < 0f;

        configurado = true;
    }

    private void Update()
    {
        if (!configurado)
            return;

        // Move o NPC continuamente.
        transform.position +=
            (Vector3)(direcao * velocidade * Time.deltaTime);

        // Só destrói se a saída estiver corretamente configurada.
        if (!saidaValida || pontoSaida == null)
            return;

        if (direcao.x > 0f &&
            transform.position.x >= pontoSaida.position.x)
        {
            Destroy(gameObject);
        }
        else if (direcao.x < 0f &&
                 transform.position.x <= pontoSaida.position.x)
        {
            Destroy(gameObject);
        }
    }
}
