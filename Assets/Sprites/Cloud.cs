using UnityEngine;
using System.Collections;

public class Cloud : MonoBehaviour {

	private float speed;

	private float min = -200f;
	private float max = 250f;

    // Use this for initialization
    void Awake()
    {

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        float r = Random.value;

        float depth = r * 5f + 1;

        float xdir = (Random.value < 0.5f) ? 1f : -1f;
        float ydir = (Random.value < 0.5f) ? 1f : -1f;

        transform.localPosition = new Vector3(transform.localPosition.x + Random.Range(-20, 20), transform.localPosition.y + Random.Range(-5, 5), depth);
        float sizeMod = Random.Range(1f, 3f);
        transform.localScale = new Vector3(sizeMod * xdir * (1f + r), sizeMod * ydir * (1f + r), 1f);

        sprite.color = new Color(1, 1, 1, 0.1f + Random.value / 4f);

        speed = 0.1f + Random.value * 2f;
    }

    void Update()
    {
        transform.Translate(Vector3.right * Time.deltaTime * speed);

        if (transform.localPosition.x > max)
        {
            transform.localPosition = new Vector3(min, transform.localPosition.y, transform.localPosition.z);
        }
    }
}
