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
	}

	public void Close() {
        targetPosition = startPosition + closePosition;
	}
}
