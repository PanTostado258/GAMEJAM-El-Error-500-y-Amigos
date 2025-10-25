using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName = "Objeto";
    public AudioClip pickupSound;

    public void OnPickup()
    {
        // Aquí puedes añadir lógica: sumar a inventario, reproducir sonido, partículas, UI...
        Debug.Log("Recogido: " + itemName);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // Por ahora desactivamos el objeto
        gameObject.SetActive(false);
    }
}

