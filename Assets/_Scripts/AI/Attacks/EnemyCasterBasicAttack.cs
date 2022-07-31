using System.Collections;
using UnityEngine;

public class EnemyCasterBasicAttack : EnemyAttackBase
{
    [SerializeField] private float _castTime;
    [SerializeField] private float _damageZoneRadius;
    [SerializeField] private float _damageZoneHeight;
    [SerializeField] private GameObject _damageZoneGameObject;

    [HideInInspector] public bool IsPlayerInZone;

    private Coroutine _castSpellCoroutine;

    public override void Awake()
    {
        base.Awake();

        _damageZoneGameObject.transform.localScale = new Vector3(_damageZoneRadius, _damageZoneHeight, _damageZoneRadius);
    }

    public override void Attack()
    {
        if (_castSpellCoroutine == null)
        {
            _castSpellCoroutine = StartCoroutine(CastSpell());
        }
    }

    private IEnumerator CastSpell()
    {
        _damageZoneGameObject.SetActive(true);
        _damageZoneGameObject.transform.position = PlayerController.getInstance().transform.position;

        yield return new WaitForSeconds(_castTime);

        if (IsPlayerInZone)
        {
            PlayerController.getInstance().ReceiveDamage(Damage * _enemyBase.GetDealDamageMultiplier());
        }

        _damageZoneGameObject.SetActive(false);

        _castSpellCoroutine = null;
    }

    private void OnValidate()
    {
        _damageZoneGameObject.transform.localScale = new Vector3(_damageZoneRadius, _damageZoneHeight, _damageZoneRadius);
    }
}
