using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapons
{
    [SerializeField] private int damage;
    [SerializeField] private int speedAttack;

    private List<IDamageable> enemy = new List<IDamageable>();
    private bool canAttack = true;

    public void AddDamage(int dm)
    {
        damage = damage * dm;
    }

    public void Attack()
    {
       if(enemy != null)
        {
            for (int i = 0; i < enemy.Count; i++)
            {
                if (enemy[i] == null || enemy[i].IsDeath())
                    enemy.Remove(enemy[i]);
            }
            foreach (var item in enemy)
            {
                if (item != null || item.IsDeath())
                item.ReceiveDamage(damage);
            }
            
            Debug.Log("damage = " + damage);
            StartCoroutine(PauseInAttack());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null)
        {
            enemy.Add(other.GetComponent<IDamageable>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < enemy.Count; i++)
        {
          if (other.GetComponent<IDamageable>() == enemy[i])
            enemy.Remove(enemy[i]);
        }
    }

    private IEnumerator PauseInAttack()
    {
        canAttack = false;
        yield return new WaitForSeconds(speedAttack);
        canAttack = true;
        StopCoroutine(PauseInAttack());
    }
}
