using System.Collections;
using UnityEngine;

public class EnemyJumperBasicAttack : EnemyAttackBase
{
    [SerializeField] private float _delayBeforeJump;
    [SerializeField] private float _shockwaveRadius;
    [SerializeField] private float _initialAngle;
    [SerializeField] private LayerMask _layerMask;

    private bool _isJumped;
    private bool _isShockwave;

    private Coroutine _jumpCoroutine;

    public override void Attack()
    {
        if (_jumpCoroutine == null)
        {
            _jumpCoroutine = StartCoroutine(JumpAtPlayer());
        }
    }

    public override void Update()
    {
        base.Update();

        if (_isJumped)
        {
            if (Physics.Raycast(transform.position, Vector3.down, 0.15f, _layerMask))
            {
                _rigidbody.isKinematic = true;
                _rigidbody.velocity = Vector3.zero;
                _agent.enabled = true;
                _enemyBase.CanAttack = true;
                _isJumped = false;
                _enemyBase.GetAnimator().SetBool("IS_GROUNDED", true);

                if (_isShockwave == false)
                {
                    Shockwave();

                    _isShockwave = true;
                }
            }
        }
    }

    private IEnumerator JumpAtPlayer()
    {
        _agent.enabled = false;
        _rigidbody.isKinematic = false;
        _enemyBase.CanAttack = false;
        yield return new WaitForSeconds(_delayBeforeJump);

        _enemyBase.GetAnimator().SetTrigger("ATTACK");
        
        JumpCalculated();

        yield return new WaitForSeconds(.2f);
        _isJumped = true;
        _enemyBase.GetAnimator().SetBool("IS_GROUNDED", false);

        _jumpCoroutine = null;
    }

    private void Shockwave()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _shockwaveRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform.TryGetComponent(out PlayerController player))
            {
                player.ReceiveDamage(Damage * _enemyBase.GetDealDamageMultiplier());
            }
        }
    }

    private void JumpCalculated()
    {
        Vector3 p = PlayerController.getInstance().transform.position;

        float gravity = Physics.gravity.magnitude;
        float angle = _initialAngle * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        float distance = Vector3.Distance(planarTarget, planarPostion);
        float yOffset = transform.position.y - p.y;
        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.SignedAngle(Vector3.forward, planarTarget - planarPostion, Vector3.up);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        _rigidbody.AddForce(finalVelocity * _rigidbody.mass, ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z));
    }
}
