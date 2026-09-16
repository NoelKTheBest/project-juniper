using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestEnemyReference : MonoBehaviour
{
    Vector2 north;
    Vector2 south;
    Vector2 east;
    Vector2 west;
    Vector2 northEast;
    Vector2 northWest;
    Vector2 southEast;
    Vector2 southWest;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int eID = 0;
        
        float minDistance = 1000000000;
        
        // Debug.Log(enemies.Length);

        for (int i = 0; i <= enemies.Length - 1; i++)
        {
            // Debug.Log("i: " + i);
            float distance = Vector2.Distance(transform.position, enemies[i].transform.position);
            // Debug.Log(distance + "; " + enemies[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                eID = i;
            }
        }

        Vector2 enemyPosition = enemies[eID].transform.position;

        Debug.DrawLine(enemyPosition, transform.position, Color.magenta);

        north = (enemyPosition + Vector2.up).normalized;
        // south = (enemyPosition + Vector2.down).normalized;
        // east = (enemyPosition + Vector2.right).normalized;
        // west = (enemyPosition + Vector2.left).normalized;
        // northEast = (enemyPosition + Vector2.up + Vector2.right).normalized;
        // northWest = (enemyPosition + Vector2.up + Vector2.left).normalized;
        // southEast = (enemyPosition + Vector2.down + Vector2.right).normalized;
        // southWest = (enemyPosition + Vector2.down + Vector2.left).normalized;

        Debug.DrawLine(enemyPosition, north, Color.green);
        // Debug.DrawLine(enemyPosition, south, Color.green);
        // Debug.DrawLine(enemyPosition, east, Color.green);
        // Debug.DrawLine(enemyPosition, west, Color.green);
        // Debug.DrawLine(enemyPosition, northEast, Color.green);
        // Debug.DrawLine(enemyPosition, northWest, Color.green);
        // Debug.DrawLine(enemyPosition, southEast, Color.green);
        // Debug.DrawLine(enemyPosition, southWest, Color.green);
    }
}
