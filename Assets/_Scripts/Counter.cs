using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] private GameObject blocker;
    private List<IDamageable> enemy = new List<IDamageable>();

    private void Update()
    {
       if(transform.childCount == 0)
        {
            blocker.SetActive(false);
            enabled = false;
        }
    }

    public List<IDamageable> getEnemy()
    {
        return enemy;
    }
    public void getEnemyFromVhild()
    {
        if (transform.childCount != 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<IDamageable>() != null)
                {
                    var item = transform.GetChild(i).GetComponent<IDamageable>();
                    enemy.Add(item);
                }
            }
        }
    }
}
