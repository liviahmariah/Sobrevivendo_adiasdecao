
using UnityEngine;
using System.Collections;

public class PomboVoador : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3f;

    [Header("Tempo entre aparições")]
    public float tempoMinimo = 8f;
    public float tempoMaximo = 18f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        StartCoroutine(ControlarVoo());
    }

    IEnumerator ControlarVoo()
    {
        while (true)
        {
            float espera = Random.Range(tempoMinimo, tempoMaximo);
            yield return new WaitForSeconds(espera);

            bool vemDaEsquerda = Random.value > 0.5f;

            float altura = Random.Range(-1f, 2f);
            float limite = cam.orthographicSize * cam.aspect;

            float inicio = vemDaEsquerda ? -limite - 1f : limite + 1f;
            float fim = vemDaEsquerda ? limite + 1f : -limite - 1f;

            transform.position = new Vector3(inicio, altura, 0);

            if (vemDaEsquerda)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);

            gameObject.SetActive(true);

            while ((vemDaEsquerda && transform.position.x < fim) ||
                   (!vemDaEsquerda && transform.position.x > fim))
            {
                transform.position += Vector3.right * (vemDaEsquerda ? 1 : -1)
                                      * velocidade * Time.deltaTime;

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
