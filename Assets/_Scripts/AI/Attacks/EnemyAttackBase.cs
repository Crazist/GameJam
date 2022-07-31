using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttackBase : MonoBehaviour
{
    [SerializeField] internal int Damage = 10;

    protected EnemyBase _enemyBase;
    protected Rigidbody _rigidbody;
    protected NavMeshAgent _agent;
    protected Animator _animator;

    public virtual void Awake()
    {
        _enemyBase = GetComponent<EnemyBase>();
        _rigidbody = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    public virtual void Update()
    {

    }

    public virtual void Attack()
    {

    }
}
