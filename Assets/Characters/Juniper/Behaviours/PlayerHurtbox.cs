using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[System.Serializable]
//public class HitDetected : UnityEvent<Vector2> { }

//for calling the takedamage method - might delete later
//[System.Serializable]
//public class DamageDone : UnityEvent<float> { }

public class PlayerHurtbox : MonoBehaviour
{
    [HideInInspector] public bool wasHit;
    private bool checkForNoContact;
    public bool hitByProjectile;

    private PlayerStatistics myStats;
    private PlayerController myController;
    private Rigidbody2D rb;

    private Vector2 hitDirection;
    public Vector2 hitboxPoint;
    public Vector2 hitboxOffset;
    public Vector2 hitboxSize;
    public Vector2 animHitboxOffset;
    public Vector2 hitboxSizeOffset;

    //public HitDetected OnHitDetected;

    //Add code from Hitbox.cs bit by bit and test it. We need to make sure that it everything that we want in
    //that script is working.
    //Do the same for the boss hurtbox.
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myStats = GetComponent<PlayerStatistics>();
        myController = GetComponent<PlayerController>();
        //anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        wasHit = false;
        checkForNoContact = false;
    }

    // Update is called once per frame
    void Update()
    {
        hitboxPoint = (Vector2)transform.position + hitboxOffset + animHitboxOffset;
        DetectHit();
    }

    void DetectHit()
    {
        //review the tutorial on melee combat
        //the mask makes it so that the collider only checks for things under the mask
        LayerMask mask = LayerMask.GetMask("Enemy Hitboxes");
        Collider2D collider = new Collider2D();// = Physics2D.OverlapBox(hitboxPoint, hitboxSize, 0, mask1);

        //collider1.GetComponent<AttackCollider>();

        collider = Physics2D.OverlapBox(hitboxPoint, hitboxSize + hitboxSizeOffset, 0, mask);
        //Debug.Log(collider);

        if (wasHit == false && collider != null)
        {
            wasHit = true;
            //float b = 0;
            //float hbMultiplier = 0;

            //finding the hitbox component of the boss might be fine, but i need to ditch the stats class
            //Hitbox hb = collider.GetComponentInParent<Hitbox>();
            BossStatistics boss = collider.GetComponentInParent<BossStatistics>();
            Vector2 attackVector = transform.position - collider.transform.position;
            myController.ReceiveKnockback(attackVector, boss.force);
            myStats.TakeDamage(boss.currentAtk);

            //if (hb.gameObject.tag == "Projectile") hitByProjectile = true;
            
            //Debug.Log(((stats.currentAtk * stats.attackPotential) + b) * hbMultiplier);
            //myStats.TakeDamage(((stats.currentAtk * stats.attackPotential) + b) * hbMultiplier);
            //if (OnHitDetected != null) OnHitDetected.Invoke(attackVector);

            checkForNoContact = true;
        }

        if (checkForNoContact)
        {
            if (collider == null)
            {
                wasHit = false;
                // the line below was not previously here. I'm not sure why i didn't include it. 
                //  i might have just forgot to include it
                checkForNoContact = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        hitboxPoint = (Vector2)transform.position + hitboxOffset + animHitboxOffset;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(hitboxPoint, hitboxSize + hitboxSizeOffset);
    }
}
