using UnityEngine;

public class Room : MonoBehaviour
{
    public Transform Begin;
    public Transform End;

    public enum TypeEnum
    {
        HORIZONTAL_HORIZONTAL,
        HORIZONTAL_VERTICAL,
        VERTICAL_HORIZONTAL,
        VERTICAL_VERTICAL
    }
    [Space]
    public TypeEnum Type;

    [SerializeField] private TowerGenerator _towerGenerator;

    private bool _isSpawned;

    private void Awake()
    {
        if (_towerGenerator == null)
        {
            _towerGenerator = GetComponentInParent<TowerGenerator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (_isSpawned == false)
            {
                _towerGenerator.SpawnRoom();

                _isSpawned = true;
            }
        }
    }
}
