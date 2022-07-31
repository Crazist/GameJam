using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    public static Action PlayerSpotted;
    public static Action<GameObject> Spawned;
    public static Action DeathAction;

    [Header("Stats")]
    [SerializeField] private float _healthMax;
    [SerializeField][Min(0f)] private float _speedBase;
    [SerializeField] private float _attackRange = 10f;

    [Header("Multipliers")]
    [SerializeField] protected float _receiveDamageMultiplier = 1.0f;
    [SerializeField] protected float _dealDamageMultiplier = 1.0f;
    [SerializeField] protected float _moveSpeedMultiplier = 1.0f;
    
    [Header("Actions")]
    [SerializeField] protected Vector2 _delayBetweenAttacks;
    [Space]
    [SerializeField] protected EnemyAttackBase _attackBase;

    [Header("Components")] 
    [SerializeField] private Transform _attackPoint;

    private float _healthCurrent;
    private float _speedFinal;
    internal bool CanAttack;

    private PlayerController _player;
    private Transform _playerTransform;
    private Animator _animator;
    private Coroutine _actionCoroutine;
    protected NavMeshAgent _navMeshAgent;

    #region INIT

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _healthCurrent = _healthMax;
        SetSpeedMultiplier(1.0f);
        _navMeshAgent.speed = _speedBase;

        Spawned?.Invoke(gameObject);
    }

    #endregion
    
    public virtual void Update()
    {
        if (_playerTransform != null)
        {
            if (_navMeshAgent.enabled)
            {
                _navMeshAgent.destination = _playerTransform.position;
                if (_navMeshAgent.velocity != Vector3.zero)
                {
                    _animator.SetBool("IS_RUNNING", true);
                }
                else
                {
                    _animator.SetBool("IS_RUNNING", false);
                }
            }
        }

        if (CanAttack)
        {
            if (_actionCoroutine == null)
            {
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(_delayBetweenAttacks.x, _delayBetweenAttacks.y));

        _attackBase.Attack();

        _actionCoroutine = null;
    }

    #region Multipliers

    public void SetSpeedMultiplier(float multiplier)
    {
        _moveSpeedMultiplier = multiplier;

        _speedFinal = _speedBase * _moveSpeedMultiplier;
        _navMeshAgent.speed = _speedFinal;
    }

    public void SetReceiveDamageMultiplier(float multiplier)
    {
        _receiveDamageMultiplier = multiplier;
    }

    public void SetDealDamageMultiplier(float multiplier)
    {
        _dealDamageMultiplier = multiplier;
    }

    #endregion

    public void ReceiveDamage(int amount)
    {
        _healthCurrent -= amount * _receiveDamageMultiplier;
        if (_healthCurrent <= 0)
        {
            Death();
        }
    }

    private bool isDeath = false;
    protected virtual void Death()
    {
          // _animator.SetTrigger("DEATH");
        if(isDeath == false)
        {
            isDeath = true;
            gameObject.SetActive(false);
            Destroy(gameObject);

            DeathAction?.Invoke();
        }
       
    }

    #region Getters / Setters

    public Animator GetAnimator()
    {
        return _animator;
    }

    public Transform GetAttackPoint()
    {
        return _attackPoint.transform;
    }

    public float GetAttackRange()
    {
        return _attackRange;
    }

    public float GetDealDamageMultiplier()
    {
        return _dealDamageMultiplier;
    }

    #endregion

    private void OnEnable()
    {
        _player = PlayerController.getInstance();
        _playerTransform = _player.GetComponent<Transform>();
    }

    private void OnDrawGizmosSelected()
    {
        if (_attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_attackPoint.position, _attackRange);
        }
        
    }

    public bool IsDeath()
    {
        return isDeath;
    }
}
