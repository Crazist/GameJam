using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;


[RequireComponent(typeof(NavMeshSurface))]
public class FloorGenerator : MonoBehaviour
{
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private bool _useStaticBatching = false;

    [Header("   ---   TILES   ---------------------------------------------------------")]
    [Header("Room Size")]
    [SerializeField] private int _xLength = 15;
    [SerializeField] private int _zLength = 15;
    [SerializeField] private int _floors = 1;
    [SerializeField] private float _heightOffset;
    [SerializeField] private Vector3 _sizeOfTile = new Vector3(10f, 10f, 10f);
    [SerializeField] private float _buildingSpeed = 0.01f;
    [Space] 
    [SerializeField] private bool _hasRandomTileRotation = true;
    [SerializeField] private GameObject[] _tilesToGenerate;
    [Space] 
    [SerializeField] private float[] _tileWeights;
    [SerializeField] private List<PoolData> _tilesInSceneList;
    [Space]
    [SerializeField] private List<ChangeTile> _changeTileList = new List<ChangeTile>();

    private ObjectPool<GameObject>[] _tilePools;
    private List<GameObject> _tileParents = new List<GameObject>();

    [Header("   ---   ENEMIES   ------------------------------------------------------")]
    [Space(25)]
    [SerializeField] private GameObject[] _enemiesToGenerate;
    [Space]
    //[SerializeField] private float[] _enemyWeights;
    [SerializeField] private int[] _enemyCounts;
    [SerializeField] private List<PoolData> _enemiesInSceneList;
    private int[] _enemyTypeSpawned;

    private ObjectPool<GameObject>[] _enemyPools;

    [Header("   ---   WALLS   --------------------------------------------------------")] 
    [SerializeField] private bool _spawnWalls;
    [SerializeField] private bool _hasRandomWallRotation = true;
    [SerializeField] private float _wallThickness = 1f;
    [SerializeField] private GameObject[] _wallsToGenerate;
    [Space]
    [SerializeField] private float[] _wallWeights;
    [SerializeField] private List<PoolData> _wallsInSceneList;

    private ObjectPool<GameObject>[] _wallPools;
    private bool _usePrefabs;

    [Header("   ---   POOL SETTINGS   ------------------------------------------------")]
    [SerializeField] private bool _collectionChecks = true;
    [SerializeField] private int _defaultCapacity = 50;
    [SerializeField] private int _maxPoolSize = 100;

    private NavMeshSurface _meshSurface;

    public static Action StartGeneratoinAction;
    public static Action RestartAction;
    public static Action FinishAction;

    private void Awake()
    {
        _tilePools = new ObjectPool<GameObject>[_tilesToGenerate.Length];
        _enemyPools = new ObjectPool<GameObject>[_enemiesToGenerate.Length];
        _wallPools = new ObjectPool<GameObject>[_wallsToGenerate.Length];

        _tilesInSceneList = new List<PoolData>();
        _enemiesInSceneList = new List<PoolData>();
        _wallsInSceneList = new List<PoolData>();

        _enemyTypeSpawned = new int[_enemyCounts.Length];

        _meshSurface = GetComponent<NavMeshSurface>();

        OnValidate();
    }

    private void Start()
    {
        SetupTilePools();
        SetupEnemyPools();
        SetupWallPools();

        for (int i = 0; i < _tilesToGenerate.Length; i++)
        {
            GameObject go = new GameObject($"Tile index: {i}");
            go.transform.SetParent(transform.GetChild(0));
            _tileParents.Add(go);
        }

        if (_generateOnStart == false) return;
        GenerateRoom(_xLength, _zLength);
    }

    [ContextMenu("Restart")]
    public void Restart()
    {
        ReleaseTiles();
        ReleaseEnemiesAll();
        ReleaseWalls();

        GenerateRoom(_xLength, _zLength);

        RestartAction?.Invoke();
    }

    [ContextMenu("Restart with Coroutine")]
    public void RestartWithCoroutine()
    {
        ReleaseTiles();
        ReleaseEnemiesAll();
        ReleaseWalls();

        StartCoroutine(GenerateRoomCoroutine(_xLength, _zLength));

        RestartAction?.Invoke();
    }

    #region Generate

    public void GenerateRoom(int xSize, int zSize)
    {
        StartGeneratoinAction?.Invoke();

        // List<GameObject> parents = new List<GameObject>();
        // for (int i = 0; i < _tilesToGenerate.Length; i++)
        // {
        //     GameObject go = new GameObject($"Tile index: {i}");
        //     go.transform.SetParent(transform.GetChild(0));
        //     parents.Add(go); 
        // }

        int id = 0;
        for (int floor = 0; floor < _floors; floor++)
        {
            for (int i = 0; i < xSize; i++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    int indexOfTilePool = GetRandomWeightedIndex(_tileWeights);
                    GameObject go = _tilePools[indexOfTilePool].Get();

                    go.transform.position = new Vector3(
                        i * _sizeOfTile.x + transform.localPosition.x,
                        floor * _sizeOfTile.y + _heightOffset + transform.localPosition.y,
                        z * _sizeOfTile.z + transform.localPosition.z);
                    go.transform.rotation = Quaternion.Euler(0f, 90 * Random.Range(0, 3), 0f);
                    
                    go.transform.SetParent(_tileParents[indexOfTilePool].transform);

                    _tilesInSceneList.Add(new PoolData()
                    {
                        GO = go,
                        IndexOfPool = indexOfTilePool,
                        Id = id,
                        Position = go.transform.position,
                        xCoord = i,
                        zCoord = z
                    });

                    id++;
                }
            }
        }

        ChangeTiles();

        SpawnEnemies();

        CreateWalls();

        if (_useStaticBatching)
        {
            var tileRoot = transform.GetChild(0);
            for (int i = 0; i < _tileParents.Count; i++)
            {
                StaticBatchingUtility.Combine(tileRoot.GetChild(i).gameObject);
            }
        }

        _meshSurface.BuildNavMesh();

        FinishAction?.Invoke();
    }

    private IEnumerator GenerateRoomCoroutine(int xSize, int zSize)
    {
        StartGeneratoinAction?.Invoke();

        // List<GameObject> parents = new List<GameObject>();
        // for (int i = 0; i < _tilesToGenerate.Length; i++)
        // {
        //     GameObject go = new GameObject($"Tile index: {i}");
        //     go.transform.SetParent(transform.GetChild(0));
        //     parents.Add(go);
        // }

        int id = 0;
        for (int floor = 0; floor < _floors; floor++)
        {
            for (int i = 0; i < xSize; i++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    int indexOfTilePool = GetRandomWeightedIndex(_tileWeights);
                    GameObject go = _tilePools[indexOfTilePool].Get();

                    go.transform.position = new Vector3(i * _sizeOfTile.x, floor * _sizeOfTile.y + _heightOffset, z * _sizeOfTile.z);
                    go.transform.rotation = Quaternion.Euler(0f, 90 * Random.Range(0, 3), 0f);

                    _tilesInSceneList.Add(new PoolData()
                    {
                        GO = go,
                        IndexOfPool = indexOfTilePool,
                        Id = id,
                        Position = go.transform.position,
                        xCoord = i,
                        zCoord = z
                    });

                    id++;
                    yield return new WaitForSeconds(_buildingSpeed);
                }
            }
        }

        StartCoroutine(ChangeTilesWithCoroutine());

        StartCoroutine(SpawnEnemiesWithCoroutine());

        StartCoroutine(CreateWallsWithCoroutine());

        yield return new WaitForEndOfFrame();

        _meshSurface.BuildNavMesh();

        if (_useStaticBatching)
        {
            var tileRoot = transform.GetChild(0);
            for (int i = 0; i < _tileParents.Count; i++)
            {
                StaticBatchingUtility.Combine(tileRoot.GetChild(i).gameObject);
            }
        }
    }

    private void ChangeTiles()
    {
        if (_changeTileList.Count == 0) return;

        for (int i = 0; i < _changeTileList.Count; i++)
        {
            var id = 0;
            for (int tileId = 0; tileId < _tilesInSceneList.Count; tileId++)
            {
                if (_tilesInSceneList[tileId].xCoord == (int)_changeTileList[i].CoordinatesByTiles.x &&
                    _tilesInSceneList[tileId].zCoord == (int)_changeTileList[i].CoordinatesByTiles.y)
                {
                    var xpos = _tilesInSceneList[tileId].xCoord;
                    var zpos = _tilesInSceneList[tileId].zCoord;
                    ReleaseTile(_tilesInSceneList[tileId].IndexOfPool, _tilesInSceneList[tileId].GO, id);

                    for (int indexOfTileInPool = 0; indexOfTileInPool < _tilesToGenerate.Length; indexOfTileInPool++)
                    {
                        if (_tilesToGenerate[indexOfTileInPool].name == _changeTileList[i].Tile.name)
                        {
                            GameObject go = _tilePools[indexOfTileInPool].Get();
                            // TODO: Floor
                            go.transform.position = new Vector3(
                                xpos * _sizeOfTile.x + transform.localPosition.x,
                                0f + _heightOffset + transform.localPosition.y,
                                zpos * _sizeOfTile.z + transform.localPosition.z);

                            if (_hasRandomTileRotation)
                            {
                                go.transform.rotation = Quaternion.Euler(0f, 90 * Random.Range(0, 3), 0f);
                            }

                            _tilesInSceneList.Add(new PoolData()
                            {
                                GO = go,
                                IndexOfPool = 0,
                                Id = id
                            });
                        }
                    }
                }

                id++;
            }
        }
    }

    private IEnumerator ChangeTilesWithCoroutine()
    {
        if (_changeTileList.Count > 0)
        {
            for (int i = 0; i < _changeTileList.Count; i++)
            {
                var id = 0;
                for (int tileId = 0; tileId < _tilesInSceneList.Count; tileId++)
                {
                    if (_tilesInSceneList[tileId].xCoord == (int)_changeTileList[i].CoordinatesByTiles.x &&
                        _tilesInSceneList[tileId].zCoord == (int)_changeTileList[i].CoordinatesByTiles.y)
                    {
                        var xpos = _tilesInSceneList[tileId].xCoord;
                        var zpos = _tilesInSceneList[tileId].zCoord;
                        ReleaseTile(_tilesInSceneList[tileId].IndexOfPool, _tilesInSceneList[tileId].GO, id);

                        for (int indexOfTileInPool = 0; indexOfTileInPool < _tilesToGenerate.Length; indexOfTileInPool++)
                        {
                            if (_tilesToGenerate[indexOfTileInPool].name == _changeTileList[i].Tile.name)
                            {
                                GameObject go = _tilePools[indexOfTileInPool].Get();
                                // TODO: Floor
                                go.transform.position = new Vector3(
                                    xpos * _sizeOfTile.x + transform.localPosition.x,
                                    0f + _heightOffset + transform.localPosition.y,
                                    zpos * _sizeOfTile.z + transform.localPosition.z);

                                if (_hasRandomTileRotation)
                                {
                                    go.transform.rotation = Quaternion.Euler(0f, 90 * Random.Range(0, 3), 0f);
                                }

                                _tilesInSceneList.Add(new PoolData()
                                {
                                    GO = go,
                                    IndexOfPool = 0,
                                    Id = id
                                });

                                yield return new WaitForSeconds(_buildingSpeed);
                            }
                        }
                    }

                    id++;
                }
            }
        }
    }

    private void SpawnEnemies()
    {
        int probability = 0;
        int id = 0;
        for (int enemyType = 0; enemyType < _enemiesToGenerate.Length; enemyType++)
        {
            while (_enemyCounts[enemyType] != _enemyTypeSpawned[enemyType])
            {
                for (int tile = 0; tile < _tilesInSceneList.Count; tile++)
                {
                    
                    if (_tilesInSceneList[tile].GO.GetComponent<TileData>().IsObstacle == false && _enemyCounts[enemyType] != _enemyTypeSpawned[enemyType])
                    {
                        probability += 1;
                        if (Random.Range(0, _tilesInSceneList.Count * 5) <= probability)
                        {
                            GameObject enemy = _enemyPools[enemyType].Get();
                            enemy.transform.position = new Vector3(
                                _tilesInSceneList[tile].Position.x,
                                enemy.transform.localPosition.y + transform.localPosition.y,
                                _tilesInSceneList[tile].Position.z);
                            EnemyData data = enemy.AddComponent<EnemyData>();
                            data.Enemy = enemy;
                            data.IndexOfPool = enemyType;

                            _enemiesInSceneList.Add(new PoolData()
                            {
                                GO = enemy,
                                IndexOfPool = enemyType,
                                Id = id
                            });

                            _enemyTypeSpawned[enemyType]++;

                            probability = 0;

                            id++;
                        }
                    }
                }
            }
        }
    }

    private IEnumerator SpawnEnemiesWithCoroutine()
    {
        int probability = 0;
        int id = 0;
        for (int enemyType = 0; enemyType < _enemiesToGenerate.Length; enemyType++)
        {
            while (_enemyCounts[enemyType] != _enemyTypeSpawned[enemyType])
            {
                for (int tile = 0; tile < _tilesInSceneList.Count; tile++)
                {

                    if (_tilesInSceneList[tile].GO.GetComponent<TileData>().IsObstacle == false && _enemyCounts[enemyType] != _enemyTypeSpawned[enemyType])
                    {
                        probability += 1;
                        if (Random.Range(0, _tilesInSceneList.Count * 5) <= probability)
                        {
                            GameObject enemy = _enemyPools[enemyType].Get();
                            enemy.transform.position = new Vector3(
                                _tilesInSceneList[tile].Position.x,
                                enemy.transform.localPosition.y + transform.localPosition.y,
                                _tilesInSceneList[tile].Position.z);
                            EnemyData data = enemy.AddComponent<EnemyData>();
                            data.Enemy = enemy;
                            data.IndexOfPool = enemyType;

                            _enemiesInSceneList.Add(new PoolData()
                            {
                                GO = enemy,
                                IndexOfPool = enemyType,
                                Id = id
                            });

                            _enemyTypeSpawned[enemyType]++;

                            probability = 0;

                            id++;

                            yield return new WaitForSeconds(_buildingSpeed);
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Walls

    private void SetupWallPools()
    {
        var parent = new GameObject("[ WALLS ]");
        parent.transform.SetParent(gameObject.transform);
        for (int i = 0; i < _wallPools.Length; i++)
        {
            var i1 = i;
            _wallPools[i] = new ObjectPool<GameObject>(() =>
                    Instantiate(_wallsToGenerate[i1], parent.transform),
                go => go.SetActive(true),
                go => go.SetActive(false),
                go => Destroy(go.gameObject),
                _collectionChecks,
                _defaultCapacity,
                _maxPoolSize
            );
        }
    }

    private void CreateWalls()
    {
        if (_spawnWalls == false) return;
        
        if (_usePrefabs)
        {
            #region Prefabs

            // Left Side
            for (int i = -1; i < _zLength + 1; i++)
            {
                var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                var leftPrefab = _wallPools[indexOfWallPool].Get();

                if (_hasRandomWallRotation)
                {
                    leftPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                }
                else
                {
                    leftPrefab.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                leftPrefab.transform.position = new Vector3(
                    -_sizeOfTile.x * 0.5f - _wallThickness * 0.5f + transform.localPosition.x,
                    0f + _heightOffset + transform.localPosition.y,
                    (i * _sizeOfTile.z) - (_sizeOfTile.z * 0.5f) + _wallThickness * 0.5f + transform.localPosition.z
                );
                _wallsInSceneList.Add(new PoolData()
                {
                    GO = leftPrefab,
                    IndexOfPool = indexOfWallPool,
                    Id = i
                });
            }

            // Right Side
            for (int i = -1; i < _zLength + 1; i++)
            {
                var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                var rightPrefab = _wallPools[indexOfWallPool].Get();

                if (_hasRandomWallRotation)
                {
                    rightPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                }
                else
                {
                    rightPrefab.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                rightPrefab.transform.position = new Vector3(
                    -_sizeOfTile.x * 0.5f + (_xLength * _sizeOfTile.x) + _wallThickness * 0.5f + transform.localPosition.x,
                    0f + _heightOffset + transform.localPosition.y,
                    (i * _sizeOfTile.z) - (_sizeOfTile.z * 0.5f) + _wallThickness * 0.5f + transform.localPosition.z
                );
                _wallsInSceneList.Add(new PoolData()
                {
                    GO = rightPrefab,
                    IndexOfPool = indexOfWallPool,
                    Id = i
                });
            }

            // Back Side
            for (int i = 0; i < _zLength; i++)
            {
                var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                var backPrefab = _wallPools[indexOfWallPool].Get();

                if (_hasRandomWallRotation)
                {
                    backPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                }
                else
                {
                    backPrefab.transform.rotation = Quaternion.Euler(0f, 0, 0f);
                }

                backPrefab.transform.position = new Vector3(
                    -_sizeOfTile.x * 0.5f + (i * _sizeOfTile.x) + _wallThickness * 0.5f + transform.localPosition.x,
                    0f + _heightOffset + transform.localPosition.y,
                    -(_sizeOfTile.z * 0.5f) - _wallThickness * 0.5f + transform.localPosition.z
                );
                _wallsInSceneList.Add(new PoolData()
                {
                    GO = backPrefab,
                    IndexOfPool = indexOfWallPool,
                    Id = i
                });
            }

            // Front Side
            for (int i = 0; i < _zLength; i++)
            {
                var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                var frontPrefab = _wallPools[indexOfWallPool].Get();

                if (_hasRandomWallRotation)
                {
                    frontPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                }
                else
                {
                    frontPrefab.transform.rotation = Quaternion.Euler(0f, 0, 0f);
                }

                frontPrefab.transform.position = new Vector3(
                    -_sizeOfTile.x * 0.5f + (i * _sizeOfTile.x) + _wallThickness * 0.5f + transform.localPosition.x,
                    0f + _heightOffset + transform.localPosition.y,
                    -(_sizeOfTile.z * 0.5f) + (_zLength * _sizeOfTile.z) + _wallThickness * 0.5f + transform.localPosition.z
                );
                _wallsInSceneList.Add(new PoolData()
                {
                    GO = frontPrefab,
                    IndexOfPool = indexOfWallPool,
                    Id = i
                });
            }

            #endregion
        }
        else
        {
            #region Create Primitives

            var parent = new GameObject("[ WALLS ]");
            parent.transform.SetParent(gameObject.transform);

            // Left Side
            var left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "Left";
            left.transform.SetParent(parent.transform);
            var leftLength = _zLength * _sizeOfTile.z;
            left.transform.position = new Vector3(
                -_sizeOfTile.x * 0.5f - 0.5f + transform.localPosition.x,
                _sizeOfTile.y * 0.5f + _heightOffset + transform.localPosition.y,
                leftLength * 0.5f - (_sizeOfTile.z * 0.5f) + transform.localPosition.z);
            left.transform.localScale = new Vector3(
                1f,
                _sizeOfTile.y,
                _zLength * _sizeOfTile.z);

            // Right Side
            var right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.transform.SetParent(parent.transform);
            right.name = "Right";
            var rightLength = _zLength * _sizeOfTile.z;
            right.transform.position = new Vector3(
                -(_sizeOfTile.x * 0.5f) + 0.5f + (_xLength * _sizeOfTile.x) + transform.localPosition.x,
                _sizeOfTile.y * 0.5f + _heightOffset + transform.localPosition.y,
                rightLength * 0.5f - (_sizeOfTile.x * 0.5f) + transform.localPosition.z);
            right.transform.localScale = new Vector3(
                1f,
                _sizeOfTile.y,
                _zLength * _sizeOfTile.z);

            // Back Side
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.transform.SetParent(parent.transform);
            back.name = "Back";
            var backLength = _xLength * _sizeOfTile.x;
            back.transform.position = new Vector3(
                -(_sizeOfTile.x * 0.5f) + (backLength * 0.5f) + transform.localPosition.x,
                _sizeOfTile.y * 0.5f + _heightOffset + transform.localPosition.y,
                 -(_sizeOfTile.z * 0.5f) - 0.5f + transform.localPosition.z);
            back.transform.localScale = new Vector3(
                _xLength * _sizeOfTile.x,
                _sizeOfTile.y,
                1f);

            // Front Side
            var front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.transform.SetParent(parent.transform);
            front.name = "Front";
            var frontLength = _zLength * _sizeOfTile.z;
            front.transform.position = new Vector3(
                -(_sizeOfTile.x * 0.5f) + (frontLength * 0.5f) + transform.localPosition.x,
                _sizeOfTile.y * 0.5f + _heightOffset + transform.localPosition.y,
                -(_sizeOfTile.z * 0.5f) + 0.5f + frontLength + transform.localPosition.z);
            front.transform.localScale = new Vector3(
                _xLength * _sizeOfTile.x,
                _sizeOfTile.y,
                1f);
            #endregion
        }
    }

    private IEnumerator CreateWallsWithCoroutine()
    {
        if (_spawnWalls)
        {
            if (_usePrefabs)
            {
                #region Prefabs

                // Left Side
                for (int i = -1; i < _zLength + 1; i++)
                {
                    var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                    var leftPrefab = _wallPools[indexOfWallPool].Get();

                    if (_hasRandomWallRotation)
                    {
                        leftPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                    }
                    else
                    {
                        leftPrefab.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }

                    leftPrefab.transform.position = new Vector3(
                        -_sizeOfTile.x * 0.5f - _wallThickness * 0.5f,
                        0f + _heightOffset,
                        (i * _sizeOfTile.z) - (_sizeOfTile.z * 0.5f) + _wallThickness * 0.5f
                    );
                    _wallsInSceneList.Add(new PoolData()
                    {
                        GO = leftPrefab,
                        IndexOfPool = indexOfWallPool,
                        Id = i
                    });

                    yield return new WaitForSeconds(_buildingSpeed);
                }

                // Right Side
                for (int i = -1; i < _zLength + 1; i++)
                {
                    var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                    var rightPrefab = _wallPools[indexOfWallPool].Get();

                    if (_hasRandomWallRotation)
                    {
                        rightPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                    }
                    else
                    {
                        rightPrefab.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }

                    rightPrefab.transform.position = new Vector3(
                        -_sizeOfTile.x * 0.5f + (_xLength * _sizeOfTile.x) + _wallThickness * 0.5f,
                        0f + _heightOffset,
                        (i * _sizeOfTile.z) - (_sizeOfTile.z * 0.5f) + _wallThickness * 0.5f
                    );
                    _wallsInSceneList.Add(new PoolData()
                    {
                        GO = rightPrefab,
                        IndexOfPool = indexOfWallPool,
                        Id = i
                    });

                    yield return new WaitForSeconds(_buildingSpeed);
                }

                // Back Side
                for (int i = 0; i < _zLength; i++)
                {
                    var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                    var backPrefab = _wallPools[indexOfWallPool].Get();

                    if (_hasRandomWallRotation)
                    {
                        backPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                    }
                    else
                    {
                        backPrefab.transform.rotation = Quaternion.Euler(0f, 0, 0f);
                    }

                    backPrefab.transform.position = new Vector3(
                        -_sizeOfTile.x * 0.5f + (i * _sizeOfTile.x) + _wallThickness * 0.5f,
                        0f + _heightOffset,
                        -(_sizeOfTile.z * 0.5f) - _wallThickness * 0.5f
                    );
                    _wallsInSceneList.Add(new PoolData()
                    {
                        GO = backPrefab,
                        IndexOfPool = indexOfWallPool,
                        Id = i
                    });

                    yield return new WaitForSeconds(_buildingSpeed);
                }

                // Front Side
                for (int i = 0; i < _zLength; i++)
                {
                    var indexOfWallPool = GetRandomWeightedIndex(_wallWeights);
                    var frontPrefab = _wallPools[indexOfWallPool].Get();

                    if (_hasRandomWallRotation)
                    {
                        frontPrefab.transform.rotation = Quaternion.Euler(0f, 90f * Random.Range(0, 3), 0f);
                    }
                    else
                    {
                        frontPrefab.transform.rotation = Quaternion.Euler(0f, 0, 0f);
                    }

                    frontPrefab.transform.position = new Vector3(
                        -_sizeOfTile.x * 0.5f + (i * _sizeOfTile.x) + _wallThickness * 0.5f,
                        0f + _heightOffset,
                        -(_sizeOfTile.z * 0.5f) + (_zLength * _sizeOfTile.z) + _wallThickness * 0.5f
                    );
                    _wallsInSceneList.Add(new PoolData()
                    {
                        GO = frontPrefab,
                        IndexOfPool = indexOfWallPool,
                        Id = i
                    });

                    yield return new WaitForSeconds(_buildingSpeed);
                }

                #endregion
            }
            else
            {
                #region Create Primitives

                var parent = new GameObject("[ WALLS ]");
                parent.transform.SetParent(gameObject.transform);

                // Left Side
                var left = GameObject.CreatePrimitive(PrimitiveType.Cube);
                left.name = "Left";
                left.transform.SetParent(parent.transform);
                var leftLength = _zLength * _sizeOfTile.z;
                left.transform.position = new Vector3(
                    -_sizeOfTile.x * 0.5f - 0.5f,
                    _sizeOfTile.y * 0.5f + _heightOffset,
                    leftLength * 0.5f - (_sizeOfTile.z * 0.5f));
                left.transform.localScale = new Vector3(
                    1f,
                    _sizeOfTile.y,
                    _zLength * _sizeOfTile.z);
                yield return new WaitForSeconds(_buildingSpeed);

                // Right Side
                var right = GameObject.CreatePrimitive(PrimitiveType.Cube);
                right.transform.SetParent(parent.transform);
                right.name = "Right";
                var rightLength = _zLength * _sizeOfTile.z;
                right.transform.position = new Vector3(
                    -(_sizeOfTile.x * 0.5f) + 0.5f + (_xLength * _sizeOfTile.x),
                    _sizeOfTile.y * 0.5f + _heightOffset,
                    rightLength * 0.5f - (_sizeOfTile.x * 0.5f));
                right.transform.localScale = new Vector3(
                    1f,
                    _sizeOfTile.y,
                    _zLength * _sizeOfTile.z);
                yield return new WaitForSeconds(_buildingSpeed);

                // Back Side
                var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
                back.transform.SetParent(parent.transform);
                back.name = "Back";
                var backLength = _xLength * _sizeOfTile.x;
                back.transform.position = new Vector3(
                    -(_sizeOfTile.x * 0.5f) + (backLength * 0.5f),
                    _sizeOfTile.y * 0.5f + _heightOffset,
                     -(_sizeOfTile.z * 0.5f) - 0.5f);
                back.transform.localScale = new Vector3(
                    _xLength * _sizeOfTile.x,
                    _sizeOfTile.y,
                    1f);
                yield return new WaitForSeconds(_buildingSpeed);

                // Front Side
                var front = GameObject.CreatePrimitive(PrimitiveType.Cube);
                front.transform.SetParent(parent.transform);
                front.name = "Front";
                var frontLength = _zLength * _sizeOfTile.z;
                front.transform.position = new Vector3(
                    -(_sizeOfTile.x * 0.5f) + (frontLength * 0.5f),
                    _sizeOfTile.y * 0.5f + _heightOffset,
                    -(_sizeOfTile.z * 0.5f) + 0.5f + frontLength);
                front.transform.localScale = new Vector3(
                    _xLength * _sizeOfTile.x,
                    _sizeOfTile.y,
                    1f);
                yield return new WaitForSeconds(_buildingSpeed);

                #endregion
            }
        }

        FinishAction?.Invoke();

        yield return null;
    }

    private void ReleaseWalls()
    {
        foreach (var wall in _wallsInSceneList)
        {
            _wallPools[wall.IndexOfPool].Release(wall.GO);
        }
        //_wallsInSceneList.Clear();
        _wallsInSceneList = new List<PoolData>();
    }

    #endregion

    #region Tile Pool
    private void SetupTilePools()
    {
        var parent = new GameObject("[ TILES ]");
        parent.transform.SetParent(gameObject.transform);
        for (int i = 0; i < _tilePools.Length; i++)
        {
            var i1 = i;
            _tilePools[i] = new ObjectPool<GameObject>(() => 
                    Instantiate(_tilesToGenerate[i1], parent.transform),
                go => go.SetActive(true),
                go => go.SetActive(false),
                go => Destroy(go.gameObject),
                _collectionChecks,
                _defaultCapacity,
                _maxPoolSize
            );
        }
    }

    private void ReleaseTile(int indexOfPool, GameObject go, int id)
    {
        _tilePools[indexOfPool].Release(go);
        _tilesInSceneList.RemoveAt(id);
    }

    private void ReleaseTiles()
    {
        foreach (var tile in _tilesInSceneList)
        {
            _tilePools[tile.IndexOfPool].Release(tile.GO);
        }
        //_tilesInSceneList.Clear();
        _tilesInSceneList = new List<PoolData>();
    }
    #endregion

    #region Enemy Pool
    private void SetupEnemyPools()
    {
        var parent = new GameObject("[ ENEMIES ]");
        parent.transform.SetParent(gameObject.transform);
        for (int i = 0; i < _enemyPools.Length; i++)
        {
            var i1 = i;
            _enemyPools[i] = new ObjectPool<GameObject>(() =>
                    Instantiate(_enemiesToGenerate[i1], parent.transform),
                go => go.SetActive(true),
                go => go.SetActive(false),
                go => Destroy(go.gameObject),
                _collectionChecks,
                _defaultCapacity,
                _maxPoolSize
            );
        }
    }

    public void ReleaseEnemy(EnemyData data)
    {
        _enemyPools[data.IndexOfPool].Release(data.Enemy);
        _enemiesInSceneList.RemoveAt(data.Id);
    }

    public void ReleaseEnemiesAll()
    {
        foreach (var enemy in _enemiesInSceneList)
        {
            _enemyPools[enemy.IndexOfPool].Release(enemy.GO);
        }
        _enemyTypeSpawned = new int[_enemyCounts.Length];
        _enemiesInSceneList.Clear();
    }
    #endregion

    #region Utils
    public int GetRandomWeightedIndex(float[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;

        float w;
        float t = 0;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];

            if (float.IsPositiveInfinity(w))
            {
                return i;
            }
            else if (w >= 0f && !float.IsNaN(w))
            {
                t += weights[i];
            }
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;

            s += w / t;
            if (s >= r) return i;
        }

        return -1;
    }

    private void OnValidate()
    {
        if (_tileWeights.Length != _tilesToGenerate.Length)
        {
            Array.Resize(ref _tileWeights, _tilesToGenerate.Length);
            Array.Resize(ref _tilesToGenerate, _tileWeights.Length);
        }

        if (_enemyCounts.Length != _enemiesToGenerate.Length)
        {
            Array.Resize(ref _enemyCounts, _enemiesToGenerate.Length);
            Array.Resize(ref _enemyTypeSpawned, _enemiesToGenerate.Length);
        }


        if (_wallWeights.Length != _wallsToGenerate.Length)
        {
            Array.Resize(ref _wallWeights, _wallsToGenerate.Length);
            Array.Resize(ref _wallsToGenerate, _wallWeights.Length);
        }


        _usePrefabs = _wallsToGenerate.Length > 0;

        for (int i = 0; i < _changeTileList.Count; i++)
        {
            if (_changeTileList[i].CoordinatesByTiles.x > _xLength)
            {
                ChangeTile changeTile = _changeTileList[i];
                changeTile.CoordinatesByTiles = new Vector2(_xLength, _changeTileList[i].CoordinatesByTiles.y);
            }
        }

        // Tiles looking foe
        // var tempTiles = _tilesToGenerate.ToList();
        // for (var i = tempTiles.Count - 1; i > -1; i--)
        // {
        //     if (tempTiles[i] == null)
        //         tempTiles.RemoveAt(i);
        // }
        // _tilesToGenerate = tempTiles.ToArray();

        // Enemies
        // var tempEnemies = _enemiesToGenerate.ToList();
        // for (var i = tempEnemies.Count - 1; i > -1; i--)
        // {
        //     if (tempEnemies[i] == null)
        //         tempEnemies.RemoveAt(i);
        // }
        // _enemiesToGenerate = tempEnemies.ToArray();
    }

    #endregion

    #region Getters / Setters

    public Vector2 GetRoomSizeInTiles()
    {
        return new Vector2(_xLength, _zLength);
    }

    public void SetRoomSize(int xLength, int zLength)
    {
        _xLength = xLength;
        _zLength = zLength;
    }

    public Vector2 GetRoomSizeInUnits()
    {
        return new Vector2(_xLength * _sizeOfTile.x, _zLength * _sizeOfTile.z);
    }

    public List<PoolData> GetEnemies()
    {
        return _enemiesInSceneList;
    }

    public List<PoolData> GetTiles()
    {
        return _tilesInSceneList;
    }

    public float[] GetTileWeights()
    {
        return _tileWeights;
    }

    public int[] GetEnemyCounts()
    {
        return _enemyCounts;
    }

    public void SetEnemyCounts(int[] enemyCounts)
    {
        _enemyCounts = enemyCounts;
    }

    #endregion
}

public class EnemyData : MonoBehaviour
{
    public GameObject Enemy;
    public int IndexOfPool;
    public int Id;
}

[Serializable]
public struct PoolData
{
    public GameObject GO;
    public int IndexOfPool;
    public int Id;
    public Vector3 Position;
    public int xCoord;
    public int zCoord;
}

[Serializable]
public struct ChangeTile
{
    public GameObject Tile;
    public Vector2 CoordinatesByTiles;
}
