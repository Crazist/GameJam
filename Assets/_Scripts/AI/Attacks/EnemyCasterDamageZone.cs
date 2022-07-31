using UnityEngine;

public class EnemyCasterDamageZone : MonoBehaviour
{
    [SerializeField] private EnemyCasterBasicAttack _basicAttack;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            _basicAttack.IsPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            _basicAttack.IsPlayerInZone = false;
        }
    }
}
