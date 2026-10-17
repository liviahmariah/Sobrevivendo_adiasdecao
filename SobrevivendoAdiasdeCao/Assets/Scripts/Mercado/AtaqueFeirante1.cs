
using UnityEngine;
using System.Collections;

public class AtaqueFeirante : MonoBehaviour
{
    [Header("Referências")]
    public SpriteRenderer spriteRenderer;
    public Transform pontoDisparo;
    public GameObject projetilPrefab;

    [Header("Diálogo")]
    public BalaoFalaFeirante balaoFala;
    public float tempoAmeaca = 1.2f;

    [Header("Visão")]
    public float alcanceVisao = 7f;

    [Range(1f, 180f)]
    public float anguloVisao = 100f;

    public LayerMask obstaculos;

    [Header("Ataque à distância")]
    public float distanciaMinimaProjetil = 2.5f;
    public float intervaloProjetil = 2f;

    [Header("Ataque por proximidade")]
    public float distanciaAtaque = 1.5f;
    public float intervaloAtaque = 1.5f;

    [Header("Orientação")]
    public bool comecaViradoParaDireita = true;

    private Transform player;
    private float proximoProjetil;
    private float proximoAtaque;
    private bool viradoParaDireita;
    private bool ameacaEmAndamento;
    private bool jaAmeacou;
    private Coroutine rotinaAmeaca;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject objetoPlayer =
            GameObject.FindGameObjectWithTag("Player");

        if (objetoPlayer != null)
            player = objetoPlayer.transform;
        else
            Debug.LogWarning(
                "AtaqueFeirante: Player não encontrado!"
            );

        viradoParaDireita = comecaViradoParaDireita;
        AtualizarOrientacao();

        if (balaoFala != null)
            balaoFala.EsconderFala();
    }

    private void Update()
    {
        if (player == null)
            return;

        Vector2 diferenca =
            (Vector2)player.position -
            (Vector2)transform.position;

        float distancia = diferenca.magnitude;

        // Sandy precisa estar dentro do alcance.
        if (distancia > alcanceVisao || distancia <= 0.01f)
        {
            CancelarAmeaca();
            jaAmeacou = false;
            return;
        }

        // Verifica o campo de visão atual.
        Vector2 frente = viradoParaDireita
            ? Vector2.right
            : Vector2.left;

        float angulo = Vector2.Angle(frente, diferenca);

        if (angulo > anguloVisao * 0.5f)
        {
            CancelarAmeaca();
            jaAmeacou = false;
            return;
        }

        // Verifica obstáculos que bloqueiam a visão.
        RaycastHit2D obstaculo = Physics2D.Raycast(
            transform.position,
            diferenca.normalized,
            distancia,
            obstaculos
        );

        if (obstaculo.collider != null)
        {
            CancelarAmeaca();
            jaAmeacou = false;
            return;
        }

        // O Feirante percebe Sandy e vira para ela.
        viradoParaDireita = diferenca.x >= 0f;
        AtualizarOrientacao();

        // Primeiro, ameaça Sandy com uma fala.
        if (!jaAmeacou && !ameacaEmAndamento)
        {
            rotinaAmeaca = StartCoroutine(AmeacarEAguardar());
            return;
        }

        // Não ataca enquanto a fala está sendo exibida.
        if (ameacaEmAndamento)
            return;

        // Escolhe o ataque conforme a distância.
        if (distancia <= distanciaAtaque)
        {
            AtacarDePerto();
        }
        else if (distancia >= distanciaMinimaProjetil)
        {
            AtacarDeLonge();
        }
    }

    private IEnumerator AmeacarEAguardar()
    {
        ameacaEmAndamento = true;

        if (balaoFala != null)
            balaoFala.MostrarFala();

        yield return new WaitForSeconds(tempoAmeaca);

        if (balaoFala != null)
            balaoFala.EsconderFala();

        ameacaEmAndamento = false;
        jaAmeacou = true;
        rotinaAmeaca = null;
    }

    private void CancelarAmeaca()
    {
        if (rotinaAmeaca != null)
        {
            StopCoroutine(rotinaAmeaca);
            rotinaAmeaca = null;
        }

        ameacaEmAndamento = false;

        if (balaoFala != null)
            balaoFala.EsconderFala();
    }

    private void AtualizarOrientacao()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = !viradoParaDireita;
    }

    private void AtacarDePerto()
    {
        if (Time.time < proximoAtaque)
            return;

        proximoAtaque = Time.time + intervaloAtaque;

        EnergiaSandy energia =
            player.GetComponentInParent<EnergiaSandy>();

        if (energia == null)
            energia = player.GetComponentInChildren<EnergiaSandy>();

        if (energia != null)
        {
            energia.PerderEnergia();
            Debug.Log("FEIRANTE ATACOU SANDY DE PERTO!");
        }
        else
        {
            Debug.LogWarning(
                "AtaqueFeirante: EnergiaSandy não encontrada!"
            );
        }
    }

    private void AtacarDeLonge()
    {
        if (projetilPrefab == null)
            return;

        if (Time.time < proximoProjetil)
            return;

        proximoProjetil = Time.time + intervaloProjetil;

        Vector3 origem = pontoDisparo != null
            ? pontoDisparo.position
            : transform.position;

        GameObject projetil = Instantiate(
            projetilPrefab,
            origem,
            Quaternion.identity
        );

        ProjetilFeirante script =
            projetil.GetComponent<ProjetilFeirante>();

        if (script != null)
        {
            script.Configurar(player);
        }
        else
        {
            Debug.LogWarning(
                "O prefab do projétil não possui ProjetilFeirante."
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            alcanceVisao
        );

        Vector3 frente = comecaViradoParaDireita
            ? Vector3.right
            : Vector3.left;

        float metadeAngulo = anguloVisao * 0.5f;

        Vector3 limiteEsquerdo =
            Quaternion.Euler(0, 0, metadeAngulo) * frente;

        Vector3 limiteDireito =
            Quaternion.Euler(0, 0, -metadeAngulo) * frente;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            limiteEsquerdo * alcanceVisao
        );

        Gizmos.DrawRay(
            transform.position,
            limiteDireito * alcanceVisao
        );
    }

    private void OnDisable()
    {
        CancelarAmeaca();
    }
}
