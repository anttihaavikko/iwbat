using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public Vector3 closePosition = Vector3.zero;
	public float speed = 1f;

	private Vector3 targetPosition;
	private Vector3 startPosition;

	private AudioSource audioSource;

	void Start() {
		targetPosition = transform.position;
		startPosition = transform.position;

		audioSource = GetComponent<AudioSource> ();
	}

	void Update () {
        transform.position = Vector3.MoveTowards (transform.position, targetPosition, Time.deltaTime * speed);
	}

	public void Open() {
		targetPosition = startPosition;
        AudioManager.Instance.PlayEffectAt(12, transform.position, 2.5f);
	}

	public void Close() {
        targetPosition = startPosition + closePosition;
        AudioManager.Instance.PlayEffectAt(12, transform.position, 2.5f);
        Invoke("CloseSound", 0.2f);
	}

    void CloseSound() {
        AudioManager.Instance.PlayEffectAt(13, transform.position, 2.5f);
    }
}
