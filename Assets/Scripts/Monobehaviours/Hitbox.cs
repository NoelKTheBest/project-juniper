using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HitDetected : UnityEvent<Vector2> { }

//for calling the takedamage method
[System.Serializable]
public class DamageDone : UnityEvent<float> { }

[System.Serializable]
public class DashAttackConnected : UnityEvent<Vector2> { }

//may rename later to DetectCollisions
public class Hitbox : MonoBehaviour
{
    [HideInInspector] public bool wasHit;
    private bool checkForNoContact;
    public float hitboxHealth;//and this
    private float currenthitboxHealth;//and this
    public bool hitByProjectile;
    [HideInInspector] public PlayerStatistics myStats;
    private Rigidbody2D rb;
    public HitDetected OnHitDetected;
    private Vector2 hitDirection;
    public Vector2 hitboxPoint;
    public Vector2 hitboxOffset;
    public Vector2 hitboxSize;
    public Vector2 animHitboxOffset;
    public Vector2 hitboxSizeOffset;
    public enum HitboxStrength
    {
        medium,
        strong,
        weak
    }
    public HitboxStrength hitboxStrength;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //myStats = GetComponent<Stats>();
        //anim = GetComponent<Animator>();
        wasHit = false;
        checkForNoContact = false;
    }

    void Update()
    {
        hitboxPoint = (Vector2)transform.position + hitboxOffset + animHitboxOffset;
        DetectHit();
    }

    void DetectHit()
    {
        //review the tutorial on melee combat
        //the mask makes it so that the collider only checks for things under the mask
        LayerMask mask1 = LayerMask.GetMask("Player Hitboxes");
        LayerMask mask2 = LayerMask.GetMask("Enemy Hitboxes");
        Collider2D collider1 = new Collider2D();// = Physics2D.OverlapBox(hitboxPoint, hitboxSize, 0, mask1);
        Collider2D collider2 = new Collider2D();// = Physics2D.OverlapBox(hitboxPoint, hitboxSize, 0, mask2);

        //collider1.GetComponent<AttackCollider>();

        if (gameObject.tag == "Enemy Attacks")
        {
            collider1 = Physics2D.OverlapBox(hitboxPoint, hitboxSize + hitboxSizeOffset, 0, mask1);
        }

        if (gameObject.tag == "Player Attacks")
        {
            collider2 = Physics2D.OverlapBox(hitboxPoint, hitboxSize + hitboxSizeOffset, 0, mask2);
            //Debug.Log(collider2); - mouse hitbox detected when collider is enabled
        }

        if (wasHit == false && collider1 != null && gameObject.tag == "Enemy")
        {
            wasHit = true;
            float b = 0;
            float hbMultiplier = 0;

            //finding the hitbox component of the boss might be fine, but i need to ditch the stats class
            Hitbox hb = collider1.GetComponentInParent<Hitbox>();
            PlayerStatistics stats = hb.myStats; // Delete this later
            Vector2 attackVector = transform.position - collider1.transform.position;

            if (hb.gameObject.tag == "Projectile") hitByProjectile = true;

            switch (hitboxStrength)
            {
                case HitboxStrength.medium:
                    hbMultiplier = 1;
                    break;
                case HitboxStrength.strong:
                    hbMultiplier = 1;
                    break;
                case HitboxStrength.weak:
                    hbMultiplier = 1;
                    break;
            }

            rb.velocity = attackVector * (stats.force);
            Debug.Log(((stats.currentAtk * stats.attackPotential) + b) * hbMultiplier);
            myStats.TakeDamage(((stats.currentAtk * stats.attackPotential) + b) * hbMultiplier);
            if (OnHitDetected != null) OnHitDetected.Invoke(attackVector);

            checkForNoContact = true;
        }

        if (wasHit == false && collider2 != null && gameObject.tag == "Player")
        {
            wasHit = true;

            Hitbox hb = collider2.GetComponentInParent<Hitbox>();
            //Debug.Log(hb); - no hitbox detected, no damage done
            PlayerStatistics stats = hb.myStats; // Delete this later
            Vector2 a = transform.position - collider2.transform.position;

            if (hb.gameObject.tag == "Projectile") hitByProjectile = true;

            rb.velocity = a * (stats.force);
            myStats.TakeDamage(stats.currentAtk * stats.attackPotential);
            if (OnHitDetected != null) OnHitDetected.Invoke(a);

            checkForNoContact = true;
        }

        if (checkForNoContact)
        {
            if (gameObject.tag == "Enemy")
            {
                if (collider1 == null)
                {
                    wasHit = false;
                }
            }

            if (gameObject.tag == "Player")
            {
                if (collider2 == null)
                {
                    wasHit = false;
                }
            }
        }
    }
    
    public void ChangeHitboxStrength(int num)
    {
        switch (num)
        {
            case 0:
                hitboxStrength = HitboxStrength.medium;
                //myStats.hb.SetBarColor(0);
                break;
            case 1:
                hitboxStrength = HitboxStrength.strong;
                //myStats.hb.SetBarColor(1);
                break;
            case 2:
                hitboxStrength = HitboxStrength.weak;
                //myStats.hb.SetBarColor(2);
                break;
        }
    }
    
    /*
    //this method will get called by the player's main animator through its own hit animation events
    public void ResetHitVar(bool a)
    {
        wasHit = false;
    }
    */
    
    //must always show the hitbox for the character in question
    void OnDrawGizmos()
    {
        hitboxPoint = (Vector2)transform.position + hitboxOffset + animHitboxOffset;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(hitboxPoint, hitboxSize + hitboxSizeOffset);
    }
}
