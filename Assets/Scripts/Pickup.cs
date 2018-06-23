using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
    public enum Type {
        MaxHp,
        Hp,
        DoubleJump,
        WallJump,
        Dash
    }

    public Type type;
    private float startSpeed = 1.5f;

	private void Start()
	{
        if(type == Type.Hp)
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.AddForce(new Vector2(Random.Range(-startSpeed, startSpeed), Random.Range(-startSpeed, startSpeed)), ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-3f, 3f));
        }
	}
}
