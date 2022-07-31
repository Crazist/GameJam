using UnityEngine;

public class EnemyMeleeBasicAttack : EnemyAttackBase
{
    public override void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_enemyBase.GetAttackPoint().position, _enemyBase.GetAttackRange());
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform.TryGetComponent(out PlayerController player))
            {
                player.ReceiveDamage(Damage * _enemyBase.GetDealDamageMultiplier());
            }
        }

        _animator.SetTrigger("ATTACK");
    }
}
