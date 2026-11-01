
using UnityEngine;

public class ProjetilMedicamento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 6f;

    private Vector2 direcao;
    private EnergiaSandy energia;
    private OndulacaoCamera efeitoOndulacao;
    private bool jaAtingiu = false;

    public void Configurar(
        Vector2 direcaoAlvo,
        EnergiaSandy energiaAlvo,
        OndulacaoCamera efeitoAlvo)
    {
        direcao = direcaoAlvo.normalized;
        energia = energiaAlvo;
        efeitoOndulacao = efeitoAlvo;
    }

    private void Update()
    {
        transform.Translate(
            direcao * velocidade * Time.deltaTime,
            Space.World
        );
    }

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (jaAtingiu)
            return;

        if (colisao.GetComponentInParent<PlayerMovement>() != null)
        {
            jaAtingiu = true;

            if (energia != null)
                energia.PerderEnergia();

            if (efeitoOndulacao != null)
                efeitoOndulacao.AtivarOndulacao();

            Destroy(gameObject);
            return;
        }

        if (!colisao.isTrigger)
            Destroy(gameObject);
    }
}
