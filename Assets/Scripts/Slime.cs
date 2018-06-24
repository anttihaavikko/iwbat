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

    public bool flying = false, spitter, spitBounces;

    public int extraSound = 17;

    public Bullet bulletPrefab;

    public Vector2 dirToPlayer;

    public float spitDelay = 5f, spitSpeed = 0.7f;

    public Face face;

	// Use this for initialization
	public void Start () {

		body = GetComponent<Rigidbody2D> ();
		direction = (Random.value < 0.5f) ? 1 : -1;

        sprite.color = ears.color = Color.HSVToRGB(Random.value, 0.5f, 0.99f);
        wing1.color = wing2.color = sprite.color;

        hp = hpMax;

        cam = Camera.main.GetComponent<FollowCamera>();
	}

    void Spit() {
        
        if (!TurnToPlayer())
        {
            return;
        }

        var b = Instantiate(bulletPrefab, (Vector2)transform.position + dirToPlayer.normalized * 2f, Quaternion.identity) as Bullet;
        b.sprite.color = sprite.color;
        b.damage = damage;
        b.canBounce = spitBounces;

        b.body.AddForce(dirToPlayer.normalized * spitSpeed, ForceMode2D.Impulse);

        EffectManager.Instance.AddEffect(2, b.transform.position);
        EffectManager.Instance.AddEffect(9, b.transform.position);

        AudioManager.Instance.PlayEffectAt(8, b.transform.position, 0.7f);

        InitSpit();
    }

    void SpitWarning() {
        face.Emote(Face.Emotion.Angry, Face.Emotion.Default, 1.5f);
        AudioManager.Instance.PlayEffectAt(10, transform.position, 0.7f);
    }

    void InitSpit() {
        var d = Random.Range(0.5f, spitDelay);
        Invoke("SpitWarning", d * 0.6f);
        Invoke("Spit", d);
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

        dirToPlayer = target.position - transform.position;

        if(spitter){
            InitSpit();
            spitter = false;
        }

        if(flying) {

            if(Random.value < 0.075f) {
                body.AddForce(dirToPlayer.normalized * speed, ForceMode2D.Impulse);
                AudioManager.Instance.PlayEffectAt(16, transform.position, 2f);
                return;
            }

            return;
        }
		
		Vector2 p1 = new Vector2 (groundCheck.position.x - transform.localScale.x / 2 + groundCheckRadius, groundCheck.position.y - groundCheckRadius);
		Vector2 p2 = new Vector2 (groundCheck.position.x + transform.localScale.x / 2 - groundCheckRadius, groundCheck.position.y + groundCheckRadius);

		grounded = Physics2D.OverlapArea (p1, p2, groundLayer);
		//animator.SetBool ("grounded", grounded);

        if (grounded && Random.value < 0.1f) {
            JumpSound();
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
            JumpSound();
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

    void JumpSound() {
        AudioManager.Instance.PlayEffectAt(5, transform.position, 0.3f);
        AudioManager.Instance.PlayEffectAt(extraSound, transform.position, 0.3f);
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

            AudioManager.Instance.PlayEffectAt(19, transform.position, 0.5f);
            AudioManager.Instance.PlayEffectAt(5, transform.position, 0.5f);
            AudioManager.Instance.PlayEffectAt(17, transform.position, 0.5f);

            if(Random.value < 0.2f)
                AudioManager.Instance.PlayEffectAt(20, transform.position, 0.75f);
        }

        if (hp <= 0)
        {
            EffectManager.Instance.AddEffect(1, transform.position);
            EffectManager.Instance.AddEffect(2, transform.position);
            EffectManager.Instance.AddEffect(3, transform.position);

            AudioManager.Instance.PlayEffectAt(3, transform.position, 0.5f);
            AudioManager.Instance.PlayEffectAt(5, transform.position, 1f);
            AudioManager.Instance.PlayEffectAt(9, transform.position, 1f);
            AudioManager.Instance.PlayEffectAt(17, transform.position, 1f);
            AudioManager.Instance.PlayEffectAt(15, transform.position, 1f);

            if(hpMax >= 7) {
                AudioManager.Instance.PlayEffectAt(4, transform.position, 0.1f);
                AudioManager.Instance.PlayEffectAt(15, transform.position, 1.5f);
                AudioManager.Instance.PlayEffectAt(25, transform.position, 1f);
            }

            if(Random.value < 0.5f)
                EffectManager.Instance.AddEffect(8, transform.position);

            cam.BaseEffect(2f);

            Destroy(gameObject);
        }

        hitCooldown = 0.3f;
    }
}
