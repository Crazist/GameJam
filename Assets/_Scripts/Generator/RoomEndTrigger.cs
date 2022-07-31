using UnityEngine;

public class RoomEndTrigger : MonoBehaviour
{
    private RandomHolder randHolder;
    private bool _isSpawned;

    private void Start()
    {
        randHolder = RandomHolder.getInstance();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (_isSpawned == false)
            {
                TowerGenerator.Instance.SpawnRoom();
                randHolder.getEbenyFromRooms();
                randHolder.RandomEffect();
                _isSpawned = true;
            }
        }
    }
}
