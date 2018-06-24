using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
    public enum Type {
        MaxHp,
        Hp,
        DoubleJump,
        WallJump,
        Dash,
        Damage
    }

    public Type type;
    private float startSpeed = 1.5f;
    private Rigidbody2D body;

	private void Start()
	{
        if(type == Type.Hp)
        {
            body = GetComponent<Rigidbody2D>();
            body.AddForce(new Vector2(Random.Range(-startSpeed, startSpeed), Random.Range(-startSpeed, startSpeed)), ForceMode2D.Impulse);
            body.AddTorque(Random.Range(-3f, 3f));
        }
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
        if(body.velocity.magnitude > 1f && collision.gameObject.tag != "Player") {
            AudioManager.Instance.PlayEffectAt(28, collision.contacts[0].point, 0.75f);
            AudioManager.Instance.PlayEffectAt(29, collision.contacts[0].point, 0.75f);
        }
	}
}
