using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int collisionCount;
    public ParticleSystem bulletImpact;
    public ParticleSystem playerImpact;
    public float maxBulletTime = 5f;

    private void Awake()
    {
        collisionCount = 0;
    }

    private void Update()
    {
        if(maxBulletTime > 0)
        {
            maxBulletTime -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collisionCount >0)
        {
            Debug.Log("Hit Player");
            Debug.Log("Collision count "+collisionCount);
            ParticleSystem impact = Instantiate(bulletImpact, transform.position, Quaternion.identity);
            impact.GetComponent<ParticleSystemRenderer>().material = gameObject.transform.GetComponent<Renderer>().material;

            FindObjectOfType<AudioManager>().Play("PlayerDeath");

            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        else if(collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Hit Bullet");
            ParticleSystem impact = Instantiate(bulletImpact, transform.position, Quaternion.identity);
            impact.GetComponent<ParticleSystemRenderer>().material = gameObject.GetComponent<Renderer>().material;
            FindObjectOfType<AudioManager>().Play("SmallDeath");
            Destroy(gameObject);
        }

        else 
        {
            collisionCount++;
            FindObjectOfType<AudioManager>().Play("Bounce");
        }

        if(collisionCount > 5)
        {
            ParticleSystem impact = Instantiate(bulletImpact, transform.position, Quaternion.identity);
            impact.GetComponent<ParticleSystemRenderer>().material = gameObject.GetComponent<Renderer>().material;
            FindObjectOfType<AudioManager>().Play("SmallDeath");
            Destroy(gameObject);
        }
    }

}
