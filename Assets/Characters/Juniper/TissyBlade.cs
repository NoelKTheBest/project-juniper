using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TissyBlade : MonoBehaviour, IPooledObject
{
    public int amountOfForce;
    public float deactivateTime;
    public Rigidbody2D rb;

    public void OnObjectSpawn(Vector2 direction)
    {
        rb.AddForce(direction * amountOfForce);

        Invoke("Deactivate", deactivateTime);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
