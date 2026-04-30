using UnityEngine;

public class Item : MonoBehaviour
{
    public int valor = 1;
    public GameObject popupPrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.Coletar(valor);

            Instantiate(popupPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}