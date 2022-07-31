using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lawa : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
            other.gameObject.GetComponent<PlayerController>().Death();
    }
}
