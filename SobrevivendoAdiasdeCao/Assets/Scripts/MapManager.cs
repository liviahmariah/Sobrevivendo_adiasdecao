using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public void IrParaMercado()
    {
        SceneManager.LoadScene("Mercado");
    }

    public void IrParaHospital()
    {
        SceneManager.LoadScene("Hospital");
    }

    public void IrParaPraca()
    {
        SceneManager.LoadScene("Praca");
    }
}