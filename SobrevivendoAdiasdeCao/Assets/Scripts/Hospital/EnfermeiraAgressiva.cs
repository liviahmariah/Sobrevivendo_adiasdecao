
using UnityEngine;

public class EnfermeiraAgressiva : MonoBehaviour
{
    [Header("Alvo")]
    public Transform sandy;

    [Header("Percepção")]
    public float distanciaPercepcao = 5f;

    [Header("Ataque")]
    public GameObject prefabProjetil;
    public Transform pontoLancamento;
    public float intervaloAtaques = 3f;

    [Header("Referências")]
    public EnergiaSandy energiaSandy;
    public OndulacaoCamera efeitoOndulacao;

    private float proximoAtaque;

    private void Update()
    {
        if (FaseManager.Instance == null ||
            !FaseManager.Instance.PodeColetar())
            return;

        if (sandy == null || prefabProjetil == null ||
            pontoLancamento == null)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            sandy.position
        );

        // Só ataca quando Sandy entra no alcance.
        if (distancia > distanciaPercepcao)
            return;

        // Aguarda o intervalo entre os disparos.
        if (Time.time < proximoAtaque)
            return;

        Atacar();
        proximoAtaque = Time.time + intervaloAtaques;
    }

    private void Atacar()
    {
        Vector2 direcao = (
            (Vector2)sandy.position -
            (Vector2)pontoLancamento.position
        ).normalized;

        GameObject objetoProjetil = Instantiate(
            prefabProjetil,
            pontoLancamento.position,
            Quaternion.identity
        );

        ProjetilMedicamento projetil =
            objetoProjetil.GetComponent<ProjetilMedicamento>();

        if (projetil != null)
        {
            projetil.Configurar(
                direcao,
                energiaSandy,
                efeitoOndulacao
            );
        }
        else
        {
            Debug.LogError(
                "O prefab não possui ProjetilMedicamento!"
            );

            Destroy(objetoProjetil);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaPercepcao);
    }
}
