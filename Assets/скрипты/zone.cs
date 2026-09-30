using UnityEngine;

public class EscapeZoneTrigger : MonoBehaviour
{
    public bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            isPlayerInside = true;
        }
    }
}