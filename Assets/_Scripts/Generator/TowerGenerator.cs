using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshSurface))]
public class TowerGenerator : MonoBehaviour
{
    public static TowerGenerator Instance;

    [Header("Настройки генерации при старте игры")]
    [SerializeField] private bool _spawnAtStart = false;
    [SerializeField] private int _floorCount = 10;

    [Header("Prefabs")]
    [SerializeField] private Room _startRoom;

    [Header("Совместимости")] 
    [SerializeField] private List<Room> _horizontal_horizontal = new List<Room>();
    [SerializeField] private List<Room> _horizontal_vertical = new List<Room>();
    [SerializeField] private List<Room> _vertical_horizontal= new List<Room>();
    [SerializeField] private List<Room> _vertical_vertical = new List<Room>();
    [Space] 
    [SerializeField] private float[] _endTypeWeight = new float[2];

    private List<Room> _spawnedRoomsList = new List<Room>();

    private NavMeshSurface _surface;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _surface = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        _spawnedRoomsList.Add(_startRoom);

        if (_spawnAtStart)
        {
            for (int i = 0; i < _floorCount; i++)
            {
                SpawnRoom();
            }
        }

        _surface.BuildNavMesh();
    }

    public List<Room> SpawnedRoomList()
    {
        return _spawnedRoomsList;
    }
    public void SpawnRoom()
    {
        Room room = null;
        var randomType = RandomByWeight.Get(_endTypeWeight);

        switch (_spawnedRoomsList[^1].Type)
        {
            case Room.TypeEnum.HORIZONTAL_HORIZONTAL:
                switch (randomType)
                {
                    case 0:
                        room = Instantiate(_horizontal_horizontal[Random.Range(0, _horizontal_horizontal.Count)], gameObject.transform);
                        break;
                    case 1:
                        room = Instantiate(_horizontal_vertical[Random.Range(0, _horizontal_vertical.Count)], gameObject.transform);
                        break;
                }
                break;
            case Room.TypeEnum.HORIZONTAL_VERTICAL:
                switch (randomType)
                {
                    case 0:
                        room = Instantiate(_vertical_vertical[Random.Range(0, _horizontal_horizontal.Count)], gameObject.transform);
                        break;
                    case 1:
                        room = Instantiate(_vertical_horizontal[Random.Range(0, _horizontal_vertical.Count)], gameObject.transform);
                        break;
                }
                break;
            case Room.TypeEnum.VERTICAL_HORIZONTAL:
                switch (randomType)
                {
                    case 0:
                        room = Instantiate(_horizontal_horizontal[Random.Range(0, _horizontal_horizontal.Count)], gameObject.transform);
                        break;
                    case 1:
                        room = Instantiate(_horizontal_vertical[Random.Range(0, _horizontal_vertical.Count)], gameObject.transform);
                        break;
                }
                break;
            case Room.TypeEnum.VERTICAL_VERTICAL:
                switch (randomType)
                {
                    case 0:
                        room = Instantiate(_vertical_vertical[Random.Range(0, _vertical_vertical.Count)], gameObject.transform);
                        break;
                    case 1:
                        room = Instantiate(_vertical_horizontal[Random.Range(0, _vertical_horizontal.Count)], gameObject.transform);
                        break;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        room.transform.rotation = Quaternion.Euler(_spawnedRoomsList[^1].End.eulerAngles);
        room.transform.position = _spawnedRoomsList[^1].End.position - room.Begin.localPosition;
        _spawnedRoomsList.Add(room);
    }
}
