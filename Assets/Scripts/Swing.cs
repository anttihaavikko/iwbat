using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swing : MonoBehaviour {

    private bool isDeadly = false;

	// Use this for initialization
	void Start () {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-10f, 10f)));
        Invoke("Disappear", 0.3f);
        Invoke("MakeDeadly", 0.075f);
	}

    void Disappear() {
        Destroy(gameObject);
    }

    void MakeDeadly() {
        isDeadly = true;
    }

    void OnTriggerEnter2D(Collider2D collision) {

        if (!isDeadly)
            return;
        
        if(collision.gameObject.tag == "Enemy") {
            collision.gameObject.GetComponent<Slime>().TakeDamage(1, transform.position);
        }
    }

	private void OnTriggerStay2D(Collider2D collision)
	{
        if (!isDeadly)
            return;

        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<Slime>().TakeDamage(1, transform.position);
        }
	}
}
