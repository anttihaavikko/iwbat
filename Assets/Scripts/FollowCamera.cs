using UnityEngine;
using System.Collections;
using UnityEngine.PostProcessing;

public class FollowCamera : MonoBehaviour {

	public Transform target;
	public float dampTime = 0.15f;
	public Vector3 offset = Vector3.zero;

	private Vector3 velocity = Vector3.zero;

	public Rigidbody2D body;

    public float baseZoom = -6f;
    public float zoomVelocityMultiplier = 0.1f;

    private PostProcessingBehaviour filters;
    private float chromaAmount = 0f;
    private float chromaSpeed = 0.1f;

    private float shakeAmount = 0f, shakeTime = 0f;

    void Start()
    {
        filters = GetComponent<PostProcessingBehaviour>();
    }

	// Update is called once per frame
	void Update () 
	{
		if (target) {
			Vector3 point = Camera.main.WorldToScreenPoint(target.position);
			Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
            float yoffset = InputMagic.Instance.GetAxis(InputMagic.STICK_OR_DPAD_Y);
            Vector3 destination = transform.position + delta + new Vector3(offset.x * target.localScale.x, yoffset * 3f, offset.z);
            float z = Mathf.MoveTowards (transform.position.z, baseZoom - body.velocity.magnitude * zoomVelocityMultiplier, Time.deltaTime * 10f);
			destination.z = z;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);

   //         Quaternion targetRot = Quaternion.Euler(new Vector3 (0, Mathf.Clamp(body.velocity.magnitude * -1f, -7f, 7f), 0f));
			//transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRot, Time.deltaTime * 2f);
		}

        // chromatic aberration update
        if (filters)
        {
            chromaAmount = Mathf.MoveTowards(chromaAmount, 0, Time.deltaTime * chromaSpeed);
            ChromaticAberrationModel.Settings g = filters.profile.chromaticAberration.settings;
            g.intensity = chromaAmount;
            filters.profile.chromaticAberration.settings = g;
        }

        var originalPos = transform.position;

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            transform.position = originalPos + new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0);
        }
        else
        {
            transform.position = originalPos;
        }
    }

    public void Chromate(float amount, float speed)
    {
        chromaAmount = amount;
        chromaSpeed = speed;
    }

    public void Shake(float amount, float time)
    {
        shakeAmount = amount;
        shakeTime = time;
    }

    public void BaseEffect(float mod = 1f)
    {
        Shake(0.04f * mod, 0.075f * mod);
        Chromate(0.25f * mod, 0.1f * mod);
    }
}