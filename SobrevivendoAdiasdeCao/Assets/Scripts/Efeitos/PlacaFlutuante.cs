using UnityEngine;

public class PlacaFlutuante : MonoBehaviour
{
    [Header("Flutuação")]
    [SerializeField] private float altura = 0.15f;
    [SerializeField] private float velocidade = 1.5f;

    private Vector3 posicaoInicial;
    private float deslocamento;

    private void Start()
    {
        posicaoInicial = transform.localPosition;

        // Faz cada placa ter um ritmo diferente
        deslocamento = Random.Range(0f, 10f);
    }

    private void Update()
    {
        float movimentoY = Mathf.Sin(
            (Time.time + deslocamento) * velocidade
        ) * altura;

        transform.localPosition = posicaoInicial + new Vector3(
            0f,
            movimentoY,
            0f
        );
    }
}