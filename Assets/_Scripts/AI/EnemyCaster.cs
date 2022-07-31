using UnityEngine;

public class EnemyCaster : EnemyBase
{
    private float _distance;

    public override void Update()
    {
        base.Update();

        _distance = Vector3.Distance(gameObject.transform.position, PlayerController.getInstance().transform.position);

        if (_distance <= GetAttackRange())
        {
            _navMeshAgent.enabled = false;
            CanAttack = true;
        }
        else
        {
            _navMeshAgent.enabled = true;
            CanAttack = false;
        }
    }
}
