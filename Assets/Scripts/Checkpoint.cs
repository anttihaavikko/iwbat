using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    private Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}
	
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            anim.ResetTrigger("down");
            anim.SetTrigger("up");
            collision.gameObject.GetComponent<PlatformerController>().SetCheckpoint(this);
        }
    }

	public void Reset()
	{
        anim.ResetTrigger("up");
        anim.SetTrigger("down");
	}
}
