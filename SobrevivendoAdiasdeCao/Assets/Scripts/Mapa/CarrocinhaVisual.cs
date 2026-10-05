using UnityEngine;

public class CarrocinhaVisual : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite spriteLado;
    [SerializeField] private Sprite spriteCima;
    [SerializeField] private Sprite spriteBaixo;

    [Header("Partículas")]
    [SerializeField] private Transform pontoParticulas;

    [Header("Posição das partículas")]
    [SerializeField] private Vector3 posicaoDireita;
    [SerializeField] private Vector3 posicaoEsquerda;
    [SerializeField] private Vector3 posicaoCima;
    [SerializeField] private Vector3 posicaoBaixo;

    private SpriteRenderer spriteRenderer;
    private Vector3 ultimaPosicao;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ultimaPosicao = transform.position;
    }

    private void Update()
    {
        Vector3 movimento = transform.position - ultimaPosicao;

        if (movimento.magnitude > 0.001f)
        {
            // Movimento horizontal
            if (Mathf.Abs(movimento.x) > Mathf.Abs(movimento.y))
            {
                spriteRenderer.sprite = spriteLado;

                // Indo para a direita
                if (movimento.x > 0)
                {
                    spriteRenderer.flipX = false;
                    pontoParticulas.localPosition = posicaoEsquerda;
                }
                // Indo para a esquerda
                else
                {
                    spriteRenderer.flipX = true;
                    pontoParticulas.localPosition = posicaoDireita;
                }
            }

            // Movimento vertical
            else
            {
                spriteRenderer.flipX = false;

                // Indo para cima
                if (movimento.y > 0)
                {
                    spriteRenderer.sprite = spriteCima;
                    pontoParticulas.localPosition = posicaoBaixo;
                }
                // Indo para baixo
                else
                {
                    spriteRenderer.sprite = spriteBaixo;
                    pontoParticulas.localPosition = posicaoCima;
                }
            }
        }

        ultimaPosicao = transform.position;
    }
}