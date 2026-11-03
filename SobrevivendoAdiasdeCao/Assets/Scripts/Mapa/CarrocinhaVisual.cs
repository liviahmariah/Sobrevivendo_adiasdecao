
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

    [Header("Rotação das partículas")]
    [SerializeField] private float rotacaoDireita = 179.33f;

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
            if (Mathf.Abs(movimento.x) > Mathf.Abs(movimento.y))
            {
                spriteRenderer.sprite = spriteLado;

                if (movimento.x > 0)
                {
                    spriteRenderer.flipX = false;
                    pontoParticulas.localPosition = posicaoEsquerda;
                    pontoParticulas.localRotation = Quaternion.Euler(0, 0, rotacaoDireita);
                }
                else
                {
                    spriteRenderer.flipX = true;
                    pontoParticulas.localPosition = posicaoDireita;
                    pontoParticulas.localRotation = Quaternion.Euler(0, 0, rotacaoDireita + 180f);
                }
            }
            else
            {
                spriteRenderer.flipX = false;

                if (movimento.y > 0)
                {
                    spriteRenderer.sprite = spriteCima;
                    pontoParticulas.localPosition = posicaoBaixo;
                    pontoParticulas.localRotation = Quaternion.Euler(0, 0, rotacaoDireita - 90f);
                }
                else
                {
                    spriteRenderer.sprite = spriteBaixo;
                    pontoParticulas.localPosition = posicaoCima;
                    pontoParticulas.localRotation = Quaternion.Euler(0, 0, rotacaoDireita + 90f);
                }
            }
        }

        ultimaPosicao = transform.position;
    }
}
