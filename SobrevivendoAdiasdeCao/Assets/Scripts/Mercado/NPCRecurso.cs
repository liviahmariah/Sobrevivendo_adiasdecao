using UnityEngine;

public class NPCRecurso : MonoBehaviour
{
    [Header("Recurso que o NPC está segurando")]
    public GameObject recursoPrefab;

    [Header("Ponto onde o recurso será solto")]
    public Transform pontoDeSolta;

    [Header("Interação")]
    public float distanciaInteracao = 1.5f;

    private bool recursoSolto = false;

    private Transform player;

    private void Start()
    {
        GameObject objetoPlayer = GameObject.FindGameObjectWithTag("Player");

        if (objetoPlayer != null)
        {
            player = objetoPlayer.transform;
        }
        else
        {
            Debug.LogWarning("NPCRecurso: Player não encontrado!");
        }
    }

    private void Update()
    {
        if (recursoSolto || player == null)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distancia <= distanciaInteracao && Input.GetKeyDown(KeyCode.Z))
        {
            SoltarRecurso();
        }
    }

    private void SoltarRecurso()
    {
        if (recursoPrefab == null)
        {
            Debug.LogWarning(
                "NPCRecurso: Nenhum recurso foi colocado no campo Recurso Prefab!"
            );

            return;
        }

        if (pontoDeSolta == null)
        {
            Debug.LogWarning(
                "NPCRecurso: Nenhum Ponto de Solta foi configurado!"
            );

            return;
        }

        Instantiate(
            recursoPrefab,
            pontoDeSolta.position,
            Quaternion.identity
        );

        recursoSolto = true;

        Debug.Log("NPC SOLTOU O RECURSO!");
    }
}