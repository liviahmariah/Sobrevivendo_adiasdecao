
using UnityEngine;

public class ChaveComumTutorial : MonoBehaviour
{
    private MolhoChavesTutorial molho;
    private bool coletada = false;

    public void Configurar(MolhoChavesTutorial referencia)
    {
        molho = referencia;
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (coletada)
            return;

        if (!outro.CompareTag("Player"))
            return;

        Coletar();
    }

    void Coletar()
    {
        coletada = true;

        if (molho != null)
        {
            molho.RegistrarColeta();
        }

        Destroy(gameObject);

        Debug.Log("Sandy coletou a chave.");
    }
}