using UnityEngine;

public class FinishScript : MonoBehaviour
{
    [SerializeField] private HUDManager hudManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hudManager.GameWin();
        }
    }
}
