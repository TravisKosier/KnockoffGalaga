using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    int damage;
    public float speed = 10f;

    public enum Targets //bullet allegiance (value is what it will harm)
    {
        ENEMY,
        PLAYER
    }

    public Targets target;

    void Start()
    {
        Destroy(gameObject, 1.5f);//Automatically destroys itself after x seconds
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed); //Move bullet forward
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter(Collider col)
    {
        if(target == Targets.PLAYER)
        {
            if (col.tag == "Player" && gameObject.tag == "Powerup")
            {
                Destroy(gameObject); //Destroy without hurting player - powerup handled in PlayerBehavior
            }
            else if (col.tag == "Player" && gameObject.tag != "Capture")
            {
                col.gameObject.GetComponent<PlayerBehavior>().TakeDamage(damage);

                Destroy(gameObject);
            }
            else if (col.tag == "Wall") // Destroy bullets when they hit the wall at the bottom of the screen
            {
                Destroy(gameObject);
            }
        }
        
        if(target == Targets.ENEMY)
        {
            if (col.tag == "Enemy" || col.tag == "CapturedShip")
            {
                col.gameObject.GetComponent<EnemyBehavior>().TakeDamage(damage);

                Destroy(gameObject);
            }
            else if (col.tag == "Wall") // Destroy bullets when they hit the wall at the top of the screen
            {
                Destroy(gameObject);
                GameManager.instance.AddShotsMissed();//Hitting wall counts as missed
            }
        }
        
    }
}
