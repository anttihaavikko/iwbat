using UnityEngine;
using System.Collections;

public class Slime : MonoBehaviour {

    public Transform target;

	private int hangTime = 0;
	public float straighteningMod = 1f;

    protected Rigidbody2D body;
    public SpriteRenderer sprite;
    protected int direction;

    protected bool grounded = false;
    protected float groundCheckRadius = 0.2f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    public float jump = 200;
    public float speed = 1;

    private int hp;
    public int hpMax = 10;
    public int damage = 1;
    public int armor = 0;

    public float aggroRadius = 30f;

    public Animator anim;

    private float hitCooldown = 0f;

    public Transform shadow;
    private FollowCamera cam;

    public SpriteRenderer ears, wing1, wing2;

    public bool flying = false;

	// Use this for initialization
	public void Start () {

		body = GetComponent<Rigidbody2D> ();
		direction = (Random.value < 0.5f) ? 1 : -1;

        sprite.color = ears.color = Color.HSVToRGB(Random.value, 0.5f, 0.99f);
        wing1.color = wing2.color = sprite.color;

        hp = hpMax;

        cam = Camera.main.GetComponent<FollowCamera>();
	}
	
	// Update is called once per frame
	void Update () {

        if (shadow)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
            shadow.position = hit.point;
            shadow.rotation = Quaternion.Euler(Vector3.zero);

            shadow.gameObject.SetActive(hit);
        }

        if (hitCooldown > 0f) hitCooldown -= Time.deltaTime;

        if(!TurnToPlayer ()) {
            return;
        }

        if(flying) {

            if(Random.value < 0.075f) {
                Vector2 dir = target.position - transform.position;
                body.AddForce(dir.normalized * speed, ForceMode2D.Impulse);
                return;
            }

            return;
        }
		
		Vector2 p1 = new Vector2 (groundCheck.position.x - transform.localScale.x / 2 + groundCheckRadius, groundCheck.position.y - groundCheckRadius);
		Vector2 p2 = new Vector2 (groundCheck.position.x + transform.localScale.x / 2 - groundCheckRadius, groundCheck.position.y + groundCheckRadius);

		grounded = Physics2D.OverlapArea (p1, p2, groundLayer);
		//animator.SetBool ("grounded", grounded);

        if (grounded && Random.value < 0.1f) {
			body.AddForce (new Vector2 (direction * jump/5.0f * speed, 1 * jump));
		}

		if (!grounded) {
			hangTime++;
		} else {
			hangTime = 0;
		}

		// try to get unstuck
		if (!grounded && hangTime > 150) {
			int dir = Random.value < 0.5f ? 1 : -1;
			body.AddTorque (dir * body.mass * jump / 20f * straighteningMod, ForceMode2D.Impulse);
			hangTime = 0;
		}
	}

	bool TurnToPlayer() {
		if (target) {
			float dist = Vector2.Distance (target.position, transform.position);

            if (dist < aggroRadius || (dist < aggroRadius * 2f && hp < hpMax)) {
				direction = target.position.x < transform.position.x ? -1 : 1;
                return true;
			}
		}

        return false;
	}

	public void Launch(int direction) {
		
		if (!body) {
			body = GetComponent<Rigidbody2D> ();
		}

		body.AddForce (new Vector2 (direction * 10f, 40f), ForceMode2D.Impulse);
		body.AddTorque (-direction * 5f, ForceMode2D.Impulse);
	}

    public void TakeDamage(int damage, Vector3 from)
    {
        if (hitCooldown > 0f)
            return;

        anim.ResetTrigger("flash");
        anim.SetTrigger("flash");
        
        damage -= armor;

        if (damage > 0)
        {
            hp -= damage;
            cam.BaseEffect(0.5f);
            EffectManager.Instance.AddEffect(4, transform.position);

            var diff = transform.position - from;
            body.AddForce(diff.normalized * 5f, ForceMode2D.Impulse);
        }

        if (hp <= 0)
        {
            EffectManager.Instance.AddEffect(1, transform.position);
            EffectManager.Instance.AddEffect(2, transform.position);
            EffectManager.Instance.AddEffect(3, transform.position);

            cam.BaseEffect(2f);

            Destroy(gameObject);
        }

        hitCooldown = 0.3f;
    }
}
