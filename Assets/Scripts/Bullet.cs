using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public Rigidbody2D body;
    public SpriteRenderer sprite;
    public int damage = 1;
    public bool canBounce = false;

    public void Explode() {
        AudioManager.Instance.PlayEffectAt(19, transform.position, 0.5f);
        AudioManager.Instance.PlayEffectAt(5, transform.position, 0.5f);
        AudioManager.Instance.PlayEffectAt(17, transform.position, 0.5f);

        EffectManager.Instance.AddEffect(1, transform.position);
        EffectManager.Instance.AddEffect(2, transform.position);

        if (Random.value < 0.1f)
            EffectManager.Instance.AddEffect(8, transform.position);

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            if(canBounce) {
                Invoke("HasBounced", 0.25f);
                EffectManager.Instance.AddEffect(2, transform.position);
                AudioManager.Instance.PlayEffectAt(28, collision.contacts[0].point, 0.75f);
                AudioManager.Instance.PlayEffectAt(29, collision.contacts[0].point, 0.75f);
            } else {
                Explode();
            }
        }
    }

    void HasBounced() {
        canBounce = false;
    }
}
