using UnityEngine;

public class AtaqueFeirante : MonoBehaviour
{
    [Header("Configuração do Ataque")]
    public float distanciaAtaque = 1.5f;
    public float intervaloAtaque = 2f;

    private Transform player;
    private float proximoAtaque = 0f;

    private void Start()
    {
        GameObject objetoPlayer = GameObject.FindGameObjectWithTag("Player");

        if (objetoPlayer != null)
        {
            player = objetoPlayer.transform;
        }
        else
        {
            Debug.LogWarning(
                "AtaqueFeirante: Player não encontrado!"
            );
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distancia <= distanciaAtaque &&
            Time.time >= proximoAtaque)
        {
            Atacar();
        }
    }

    private void Atacar()
    {
        proximoAtaque = Time.time + intervaloAtaque;

        EnergiaSandy energia = player.GetComponent<EnergiaSandy>();

        if (energia != null)
        {
            energia.PerderEnergia();

            Debug.Log("FEIRANTE ATACOU SANDY!");
        }
        else
        {
            Debug.LogWarning(
                "AtaqueFeirante: EnergiaSandy não encontrada na Sandy!"
            );
        }
    }
}