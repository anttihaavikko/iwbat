using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlatformerController : MonoBehaviour {

	// configurables
	public float speed = 7.5f;
	public float acceleration = 0.7f;
	public float jump = 16f;
	public float inputBuffer = 0.05f;
	public bool canDoubleJump = false;
	public bool canWallJump = false;
	public bool mirrorWhenTurning = true;

	// physics
	private Rigidbody2D body;
	public Transform[] groundChecks;
	public Transform[] wallChecks;
	public LayerMask groundLayer, canJumpLayers;
	public float groundCheckRadius = 0.05f;
	public bool checkForEdges = false;
	private float groundAngle = 0;
	private float gravity;
	private float walljumpCooldown = 0f;
	private float wallHugBuffer = 0f;
	private float walljumpDir = 0f;
    private int jumpFrames = 0;
    private int allowedJumpFrames = 10;

	// flags
	private bool canControl = true;
	private bool running = false;
	private bool grounded = false;
	private bool doubleJumped = false;
    private bool respawning = false;
    private bool jumped = false;

	// misc
	private float jumpBufferedFor = 0;
	public Transform spriteObject;
	public Transform shadow;
    private float swingCooldown, swingCooldownMax = 0.3f;

	// particles
	public GameObject jumpParticles, landParticles;

	// sound stuff
	private AudioSource audioSource;
	public AudioClip jumpClip, landClip;

	// animations
	private Animator anim;
    private FollowCamera cam;

    private int hp = 3, hpMax = 3;
    private float hitCooldown = 0.3f;

    private int currentGrowth = 0;
    public Sprite[] growthSprites;
    public SpriteRenderer growthSprite;

    public Image imageHpMax, imageHp;

    public GameObject blade, bandanna, eyepatch;

    int damage = 1;

    private Checkpoint checkpoint;

	// ###############################################################

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody2D> ();
		audioSource = GetComponent<AudioSource> ();
		anim = GetComponentInChildren<Animator> ();

		gravity = body.gravityScale;

        cam = Camera.main.GetComponent<FollowCamera>();

        // disable things already got/killed
        foreach(var d in Manager.Instance.disableThese) {
            var go = GameObject.Find(d);
            if(go)
                GameObject.Find(d).SetActive(false);
        }

        // load stuffs from manager
        canDoubleJump = Manager.Instance.hasBandanna;
        damage = Manager.Instance.hasBlade ? 2 : 1;

        bandanna.SetActive(canDoubleJump);
        blade.SetActive(Manager.Instance.hasBlade);

        currentGrowth = Manager.Instance.growths;
        growthSprite.sprite = growthSprites[currentGrowth];

        if(Manager.Instance.spawn != Vector3.zero)
            transform.position = Manager.Instance.spawn;

        hp = hpMax = Manager.Instance.hpMax;
        UpdateHp();

        if (Manager.Instance.hasDied)
            eyepatch.SetActive(true);

        cam.transform.position = new Vector3(transform.position.x, transform.position.y, cam.transform.position.z);
	}
	
	// Update is called once per frame
	void Update () {

        if(Application.isEditor && Input.GetKeyDown(KeyCode.R)) {
            Respawn();
        }

        if (Application.isEditor && Input.GetKeyDown(KeyCode.G))
        {
            Grow();
        }

        if (respawning) return;

		bool wasGrounded = grounded;

        if (hitCooldown > 0f) hitCooldown -= Time.deltaTime;

		if (!checkForEdges) {

			grounded = false;

			for (int i = 0; i < groundChecks.Length; i++) {

				Transform groundCheck = groundChecks [i];

                bool tempGrounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, canJumpLayers);

				grounded = tempGrounded ? tempGrounded : grounded;

				// draw debug lines
				Color debugLineColor = grounded ? Color.green : Color.red;
				Debug.DrawLine (transform.position, groundCheck.position, debugLineColor, 0.2f);
				Debug.DrawLine (groundCheck.position + Vector3.left * groundCheckRadius, groundCheck.position + Vector3.right * groundCheckRadius, debugLineColor, 0.2f);
			}
		} else {
			grounded = Physics2D.Raycast (transform.position, Vector2.down, 1f);

			// draw debug lines
			Color debugLineColor = grounded ? Color.green : Color.red;
			Debug.DrawRay (transform.position, Vector2.down, debugLineColor, 0.2f);
		}

		// double fall gravity
        if (!grounded && body.velocity.y <= 0.5f && jumped) {
			body.gravityScale = gravity * 1.5f;

            //float maxFallSpeed = -10f;

            //if (body.velocity.y < -maxFallSpeed)
                //body.velocity = new Vector2(body.velocity.x, -maxFallSpeed);
		}

		// just landed
		if (!wasGrounded && grounded) {
			Land ();
		}

		// just left the ground
		if (wasGrounded && !grounded) {
			groundAngle = 0;
		}

		// jump buffer timing
		if (jumpBufferedFor > 0) {
			jumpBufferedFor -= Time.deltaTime;
		}

		if (shadow) {
            RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector2.down, 20f, groundLayer);
            shadow.position = hit.point;
		}

		// controls
		if (canControl) {

			float inputDirection = InputMagic.Instance.GetAxis (InputMagic.STICK_OR_DPAD_X);

            if (Mathf.Abs(inputDirection) < 0.1f) inputDirection = 0f;

            if (InputMagic.Instance.GetButton(InputMagic.A) && jumpFrames < allowedJumpFrames)
            {
                EnhanceJump();
            }

            if (InputMagic.Instance.GetButtonUp(InputMagic.A))
            {
                jumpFrames = allowedJumpFrames + 1;
            }

            swingCooldown -= Time.deltaTime;

            // swing
            if(InputMagic.Instance.GetButtonDown(InputMagic.X) && swingCooldown <= 0f) {
                anim.ResetTrigger("swing");
                anim.SetTrigger("swing");
                swingCooldown = swingCooldownMax;
                GameObject go = EffectManager.Instance.AddEffectToParent(0, transform.position + Vector3.right * spriteObject.transform.localScale.x, transform);
                go.GetComponent<Swing>().damage = damage;
                go.transform.localScale = spriteObject.transform.localScale;
            }

			// jump
			if ((grounded || (canDoubleJump && !doubleJumped)) && (InputMagic.Instance.GetButtonDown(InputMagic.A) || jumpBufferedFor > 0)) {

                Jump();

			} else if (canControl && InputMagic.Instance.GetButtonDown(InputMagic.A)) {
			
				// jump command buffering
				jumpBufferedFor = 0.2f;
			}

			// moving
			Vector2 moveVector = new Vector2 (speed * inputDirection, body.velocity.y);

			if (Mathf.Sign (body.velocity.x) == Mathf.Sign (moveVector.x) || walljumpCooldown > 0f) {
				body.velocity = Vector2.MoveTowards (body.velocity, moveVector, acceleration);
			} else {
				body.velocity = moveVector;
			}

			// direction
			if (mirrorWhenTurning && Mathf.Abs(inputDirection) > inputBuffer) {
				float dir = Mathf.Sign (inputDirection);
				spriteObject.localScale = new Vector2 (dir, 1);

//				Transform sprite = transform.Find("Character");
//				Vector3 scl = sprite.localScale;
//				scl.x = dir;
//				sprite.localScale = scl;

//				transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 90f - dir * 90f, transform.localEulerAngles.z);
			}
				
			bool wallHug = false; 

			if (!checkForEdges) {
				for (int i = 0; i < wallChecks.Length; i++) {
					Vector2 p = wallChecks[i].position;
                    wallHug = Physics2D.OverlapCircle (p, groundCheckRadius, groundLayer) ? true : wallHug;
					Color hugLineColor = grounded ? Color.green : Color.red;
					Debug.DrawLine (transform.position, p, hugLineColor, 0.2f);
				}
			}

			if (wallHug) {
				walljumpDir = -transform.localScale.x;
                body.velocity = new Vector2(0, body.velocity.y);
			}

			if ((wallHug || wallHugBuffer > 0f) && !checkForEdges) {

				if (walljumpCooldown <= 0f) {
					float vertVel = (body.velocity.y > 0 || Mathf.Abs (inputDirection) < 0.5f || !canWallJump) ? body.velocity.y : Mathf.MoveTowards (body.velocity.y, 0, 0.5f);
					body.velocity = new Vector2 (0, vertVel);
				}

				if (wallHug && Mathf.Abs (inputDirection) > 0.5f) {
					wallHugBuffer = 0.2f;
				} else {
					wallHugBuffer -= Time.deltaTime;
				}

				if (canWallJump) {
					if ((Input.GetButtonDown ("Jump") || jumpBufferedFor > 0) && !grounded && walljumpCooldown <= 0f && (Mathf.Abs (inputDirection) > 0.5f || wallHugBuffer > 0f)) {

						jumpBufferedFor = 0;

						body.velocity = Vector2.zero;

						float dir = Mathf.Sign (walljumpDir);
						transform.localScale = new Vector2 (dir, 1);

						body.AddForce (Vector2.up * jump + Vector2.right * walljumpDir * jump * 0.5f, ForceMode2D.Impulse);
						walljumpCooldown = 0.5f;
					}
				}
			}

			if (walljumpCooldown > 0f) {
				walljumpCooldown -= Time.deltaTime;
			}

			running = inputDirection < -inputBuffer || inputDirection > inputBuffer;

            if (!grounded || wallHug) {
				running = false; 
			}

			if (anim) {
                anim.SetFloat("vertical", body.velocity.y);

				if (running) {
					anim.speed = Mathf.Abs (body.velocity.x * 0.2f);
					anim.SetFloat ("speed", Mathf.Abs(body.velocity.x));
				} else {
					anim.speed = 1f;
					anim.SetFloat ("speed", 0);
				}
			}
		}
	}

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, 0); // reset vertical speed

        jumped = true;

        if (!grounded)
        {
            doubleJumped = true;
        }

        if (canDoubleJump && doubleJumped)
        {
            EffectManager.Instance.AddEffect(7, transform.position + Vector3.down * 0.5f);
            EffectManager.Instance.AddEffect(9, transform.position + Vector3.down * 0.5f);
        }

        jumpBufferedFor = 0;

        // jump sounds
        if (audioSource && jumpClip)
        {
            audioSource.PlayOneShot(jumpClip);
        }

        //AudioManager.Instance.PlayEffectAt (0, transform.position, 0.5f);

        EffectManager.Instance.AddEffect(5, transform.position + Vector3.down * 0.5f);
        EffectManager.Instance.AddEffect(6, transform.position + Vector3.down * 0.5f);

        // animation
        if (anim)
        {
            anim.speed = 1f;
            anim.SetTrigger("jump");
            anim.ResetTrigger("land");
        }

        body.gravityScale = gravity;

        jumpFrames = 0;
        EnhanceJump();
    }

    private void EnhanceJump()
    {
        body.AddForce(Vector2.up * jump * 1.12f / allowedJumpFrames, ForceMode2D.Impulse);
        jumpFrames++;
    }

	private void Land() {

        jumped = false;

		anim.ResetTrigger ("jump");

		doubleJumped = false;

		body.gravityScale = gravity;

		// landing sound
		if (audioSource && landClip) {
			audioSource.PlayOneShot (landClip);
		}

		//AudioManager.Instance.PlayEffectAt (1, transform.position, 0.5f);

        EffectManager.Instance.AddEffect(5, transform.position + Vector3.down * 0.5f);
        EffectManager.Instance.AddEffect(6, transform.position + Vector3.down * 0.5f);

		// animation
		if (anim) {
			anim.speed = 1f;
			anim.SetTrigger ("land");
		}
	}

	public bool IsGrounded() {
		return grounded;
	}

	void OnCollisionStay2D(Collision2D coll) {
		//groundAngle = Mathf.Atan2(coll.contacts [0].normal.y, coll.contacts [0].normal.x) * Mathf.Rad2Deg - 90;
	}

	void OnCollisionEnter2D(Collision2D coll) {
		//groundAngle = Mathf.Atan2(coll.contacts [0].normal.y, coll.contacts [0].normal.x) * Mathf.Rad2Deg - 90;

        if(coll.gameObject.tag == "Enemy") {
            TakeDamage(coll.gameObject.GetComponent<Slime>().damage, coll.contacts[0].point);
        }
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
        if (collision.gameObject.tag == "Water")
        {
            Manager.Instance.hasDied = true;
            Respawn();
        }

        if (collision.gameObject.tag == "Pickup")
        {
            var pu = collision.gameObject.GetComponent<Pickup>();

            if (pu.type == Pickup.Type.MaxHp) {
                Manager.Instance.disableThese.Add(pu.gameObject.name);
                hpMax++;
                hp++;
                cam.BaseEffect(1.5f);
                Grow();
            }
               
            if (pu.type == Pickup.Type.Hp)
            {
                hp++;

                if (hp > hpMax) hp = hpMax;
                cam.BaseEffect(0.5f);
            }

            if (pu.type == Pickup.Type.DoubleJump)
            {
                Manager.Instance.disableThese.Add(pu.gameObject.name);
                bandanna.SetActive(true);
                canDoubleJump = true;
                cam.BaseEffect(2.5f);
                Manager.Instance.hasBandanna = true;
            }

            if (pu.type == Pickup.Type.Damage)
            {
                Manager.Instance.disableThese.Add(pu.gameObject.name);
                blade.SetActive(true);
                damage = 2;
                cam.BaseEffect(2.5f);
                Manager.Instance.hasBlade = true;
            }

            EffectManager.Instance.AddEffect(1, collision.transform.position);
            EffectManager.Instance.AddEffect(2, collision.transform.position);

            UpdateHp();

            Destroy(pu.gameObject);

        }
	}

	public float GetGroundAngle() {
		if (Mathf.Abs (groundAngle) > 90) {
			groundAngle = 0;
		}
		return groundAngle;
	}

    public void TakeDamage(int damage, Vector3 from)
    {
        if (hitCooldown > 0f || respawning)
            return;

        anim.ResetTrigger("flash");
        anim.SetTrigger("flash");

        if (damage > 0)
        {
            hp -= damage;
            cam.BaseEffect(1f);
            EffectManager.Instance.AddEffect(4, transform.position);

            var diff = transform.position - from;
            body.AddForce(diff.normalized * 5f, ForceMode2D.Impulse);

            UpdateHp();
        }

        if (hp <= 0)
        {
            Die();
        }

        hitCooldown = 0.3f;
    }

    void Die() {

        if (respawning) return;

        Manager.Instance.hasDied = true;

        AudioManager.Instance.BackToDefaultMusic();

        cam.BaseEffect(3f);

        respawning = true;

        spriteObject.gameObject.SetActive(false);

        EffectManager.Instance.AddEffect(1, transform.position);
        EffectManager.Instance.AddEffect(2, transform.position);
        EffectManager.Instance.AddEffect(3, transform.position);

        cam.BaseEffect(2f);

        Invoke("Respawn", 1f);
    }

    void Respawn() {
        respawning = false;
        //hp = hpMax;
        //transform.position = Vector3.zero;
        //spriteObject.gameObject.SetActive(true);

        SceneManager.LoadSceneAsync("Main");
    }

    public void Grow() {
        currentGrowth++;

        if (currentGrowth >= growthSprites.Length)
            currentGrowth = growthSprites.Length - 1;

        growthSprite.sprite = growthSprites[currentGrowth];

        Manager.Instance.growths = currentGrowth;
    }

    void UpdateHp() {
        imageHp.rectTransform.sizeDelta = new Vector2(85 * hp, imageHp.rectTransform.sizeDelta.y);
        imageHpMax.rectTransform.sizeDelta = new Vector2(85 * hpMax, imageHpMax.rectTransform.sizeDelta.y);
        Manager.Instance.hpMax = hpMax;
    }

    public void SetCheckpoint(Checkpoint cp) {
        
        if (checkpoint && checkpoint != cp)
            checkpoint.Reset();

        checkpoint = cp;

        Manager.Instance.spawn = cp.transform.position;
    }
}
