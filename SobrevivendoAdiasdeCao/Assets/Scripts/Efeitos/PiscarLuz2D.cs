
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PiscarLuz2D : MonoBehaviour
{
    [Header("Configuração da luz")]
    public Light2D luz;

    [Header("Intervalo entre piscadas")]
    public float intervaloMinimo = 3f;
    public float intervaloMaximo = 8f;

    [Header("Duração da piscada")]
    public float duracaoMinima = 0.05f;
    public float duracaoMaxima = 0.2f;

    [Header("Quantidade de piscadas")]
    public int piscadasMinimas = 1;
    public int piscadasMaximas = 3;

    private void Start()
    {
        if (luz == null)
            luz = GetComponent<Light2D>();

        if (luz != null)
            StartCoroutine(PiscarAleatoriamente());
        else
            Debug.LogWarning("Nenhuma Light2D foi encontrada!");
    }

    private IEnumerator PiscarAleatoriamente()
    {
        while (true)
        {
            float intervalo = Random.Range(
                intervaloMinimo, intervaloMaximo
            );

            yield return new WaitForSeconds(intervalo);

            int quantidade = Random.Range(
                piscadasMinimas, piscadasMaximas + 1
            );

            for (int i = 0; i < quantidade; i++)
            {
                luz.enabled = false;

                yield return new WaitForSeconds(
                    Random.Range(duracaoMinima, duracaoMaxima)
                );

                luz.enabled = true;

                yield return new WaitForSeconds(
                    Random.Range(0.05f, 0.15f)
                );
            }
        }
    }
}
