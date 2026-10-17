
using UnityEngine;

public class ProjetilFeirante : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 6f;
    public float tempoDeVida = 4f;

    private Transform alvo;
    private Vector2 direcao;
    private bool configurado;

    private void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    public void Configurar(Transform novoAlvo)
    {
        if (novoAlvo == null)
        {
            Destroy(gameObject);
            return;
        }

        alvo = novoAlvo;

        direcao = (
            (Vector2)alvo.position -
            (Vector2)transform.position
        ).normalized;

        configurado = true;
    }

    private void Update()
    {
        if (!configurado)
            return;

        transform.position +=
            (Vector3)(direcao * velocidade * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnergiaSandy energia =
            other.GetComponentInParent<EnergiaSandy>();

        if (energia == null)
            return;

        energia.PerderEnergia();
        Destroy(gameObject);
    }
}
