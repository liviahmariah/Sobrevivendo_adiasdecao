using UnityEngine;
using System.Collections;

public class ChaveFinalTutorial : MonoBehaviour
{
    [Header("Salto da chave")]
    public float distanciaSalto = 1.5f;
    public float alturaSalto = 1.5f;
    public float duracaoSalto = 0.5f;

    private bool podeColetar = false;
    private bool chaveAtiva = false;

    private Collider2D colisor;

    private void Awake()
    {
        colisor = GetComponent<Collider2D>();

        if (colisor != null)
        {
            colisor.isTrigger = true;
            colisor.enabled = false;
        }
    }

    public void SoltarChave(Vector3 posicaoInicial)
    {
        chaveAtiva = true;
        podeColetar = false;

        transform.position = posicaoInicial;

        if (colisor != null)
        {
            colisor.enabled = false;
        }

        StartCoroutine(SaltoDaChave(posicaoInicial));
    }

    private IEnumerator SaltoDaChave(Vector3 inicio)
    {
        float tempo = 0f;

        Vector3 destino = inicio + Vector3.right * distanciaSalto;

        while (tempo < duracaoSalto)
        {
            tempo += Time.deltaTime;

            float progresso = tempo / duracaoSalto;

            float x = Mathf.Lerp(
                inicio.x,
                destino.x,
                progresso
            );

            float y = Mathf.Lerp(
                inicio.y,
                destino.y,
                progresso
            );

            float salto = Mathf.Sin(progresso * Mathf.PI)
                * alturaSalto;

            transform.position = new Vector3(
                x,
                y + salto,
                inicio.z
            );

            yield return null;
        }

        transform.position = destino;

        podeColetar = true;

        if (colisor != null)
        {
            colisor.enabled = true;
        }

        Debug.Log("A chave final caiu e pode ser coletada.");
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!chaveAtiva || !podeColetar)
            return;

        if (!outro.CompareTag("Player"))
            return;

        ColetarChave();
    }

    private void ColetarChave()
    {
        chaveAtiva = false;
        podeColetar = false;

        if (TutorialChavesManager.instance != null)
        {
            TutorialChavesManager.instance.ColetarChaveFinal();
        }

        Destroy(gameObject);

        Debug.Log("Sandy coletou a chave final!");
    }
}