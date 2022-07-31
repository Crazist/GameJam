using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomHolder : MonoBehaviour
{
    [SerializeField] private TowerGenerator generator;
    [SerializeField] private int speedAdd;
    [SerializeField] private int damageAdd;
    [SerializeField] private int recivedAdd;

    [SerializeField] private float playerSpeedAdd;
    [SerializeField] private int playerDamageAdd;
    [SerializeField] private int playerRecivedAdd;

    private static RandomHolder instance;

    private List<IDamageable> enemy = new List<IDamageable>();
    private Counter conter;
    private PlayerController player;

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }
    public static RandomHolder getInstance()
    {
        return instance;
    }
    
     public void getEbenyFromRooms()
    {
        var roomList = generator.SpawnedRoomList();
        player = PlayerController.getInstance();
        foreach (var item in roomList)
        {
            if(item.GetComponentInChildren<Counter>() != null) 
            {
                item.GetComponentInChildren<Counter>().getEnemyFromVhild();
                var _enemy = item.GetComponentInChildren<Counter>().getEnemy();
                if(_enemy != null)
                {
                    foreach (var _item in _enemy)
                    {
                        enemy.Add(_item);
                    }
                }
            }
        }
    }
    public void RandomEffect()
    {
        var rand = Random.Range(1, 7);

        switch (rand)
        {
            case 1:
                AddSpeedEnemy();
                break;
            case 2:
                AddDamageEnemy();
                break;
            case 3:
                AddDamageReciveEnemy();
                break;
            case 4:
                AddSpeedPlayer();
                break;
            case 5:
                AddDamagePlayer();
                break;
            case 6:
                AddDamageRecivePlayer();
                break;
            default:
                break;
        }

    }
    public void AddSpeedEnemy()
    {
        foreach (var item in enemy)
        {
            item.SetSpeedMultiplier(speedAdd);
        }
    }
    public void AddDamageEnemy()
    {
        foreach (var item in enemy)
        {
            item.SetDealDamageMultiplier(damageAdd);
        }
    }
    public void AddDamageReciveEnemy()
    {
        foreach (var item in enemy)
        {
            item.SetReceiveDamageMultiplier(damageAdd);
        }
    }
    public void AddSpeedPlayer()
    {
        player.AddSpeed(playerSpeedAdd);
    }
    public void AddDamagePlayer()
    {
        foreach (var item in enemy)
        {
            item.SetDealDamageMultiplier(damageAdd);
        }
    }
    public void AddDamageRecivePlayer()
    {
        foreach (var item in enemy)
        {
            item.SetReceiveDamageMultiplier(recivedAdd);
        }
    }
}
