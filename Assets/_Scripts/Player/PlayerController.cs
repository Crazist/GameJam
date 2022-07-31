using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
[DefaultExecutionOrder(-1000)]
public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject weapon;
    [SerializeField] float hp;
    private IWeapons currentWrapon;
    private static PlayerController instance;
    private bool death = false;
    private int reciveDmMulti = 0;
   // private Animator animator;
    public PlayerController() {}
    private void Start()
    {
       // animator = gameObject.GetComponent<Animator>();
        currentWrapon = weapon.GetComponent<IWeapons>();
    }
    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }
    public static PlayerController getInstance()
    {
        return instance;
    }

    
    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && !death)
        currentWrapon.Attack();
    }

    public void ReceiveDamage(float amount)
    {
        Debug.Log($"Get Damage: {amount}");

        if (death) return;
        if(reciveDmMulti != 0)
        amount = amount * reciveDmMulti;
        hp = hp - amount;
        Debug.Log("recive dm = " + amount + "cur hp = " + hp);
        if(hp <= 0)
        {
            hp = 0;
            Death();
        }
    }

    public  void Death()
    {
       hp = 0;
    //   animator.SetTrigger("Death");
       Debug.Log("Death");
       death = true;
       gameObject.GetComponent<ScriptMachine>().enabled = false;
       enabled = false;
    }

    public void ChangeWeapon(IWeapons weapon)
    {
        currentWrapon = weapon;
    }

    public void AddDamage(int dm)
    {
        currentWrapon.AddDamage(dm);
    }

    public void AddSpeed(float dm)
    {
        var _dm = (float)Variables.Object(gameObject).Get("PlayerSpeed");
        _dm = _dm * dm;
        Variables.Object(gameObject).Set("PlayerSpeed", _dm);
    }

    public void AddreciveDm(int dm)
    {
        reciveDmMulti = dm;
    }
}
