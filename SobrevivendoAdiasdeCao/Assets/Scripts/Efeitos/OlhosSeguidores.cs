
using UnityEngine;

public class OlhosSeguidores : MonoBehaviour
{
    [Header("Personagem observado")]
    public Transform personagem;

    [Header("Movimento dos olhos")]
    public float distanciaMovimento = 0.08f;
    public float velocidade = 8f;

    private Vector3 posicaoInicial;

    void Start()
    {
        posicaoInicial = transform.localPosition;
    }

    void LateUpdate()
    {
        if (personagem == null)
            return;

        Vector3 direcao = personagem.position - transform.position;
        direcao.z = 0f;

        if (direcao.sqrMagnitude > 0f)
            direcao.Normalize();

        Vector3 deslocamento = direcao * distanciaMovimento;
        Vector3 destino = posicaoInicial + deslocamento;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            destino,
            velocidade * Time.deltaTime
        );
    }
}
