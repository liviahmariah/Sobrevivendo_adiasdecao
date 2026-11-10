using UnityEngine;

public class ProjetilFeirante : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 6f;
    public float tempoDeVida = 4f;

    [Header("Colisão e Camadas")]
    public LayerMask camadasObstaculos;

    private Vector2 direcaoFixa;
    private bool configurado;

    private void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    // Aceita a posição estática congelada no momento do tiro
    public void Configurar(Vector3 posicaoAlvo)
    {
        direcaoFixa = ((Vector2)posicaoAlvo - (Vector2)transform.position).normalized;

        float angulo = Mathf.Atan2(direcaoFixa.y, direcaoFixa.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);

        configurado = true;
    }

    private void Update()
    {
        if (!configurado)
            return;

        // Avança em linha reta na direção capturada
        transform.position += (Vector3)(direcaoFixa * velocidade * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnergiaSandy energia = other.GetComponentInParent<EnergiaSandy>();
        if (energia == null) energia = other.GetComponentInChildren<EnergiaSandy>();

        if (energia != null)
        {
            energia.PerderEnergia();
            Destroy(gameObject);
            return;
        }

        // Se bater em paredes/obstáculos do cenário quando a Sandy se esquivar
        if (((1 << other.gameObject.layer) & camadasObstaculos) != 0)
        {
            Destroy(gameObject);
        }
    }
}