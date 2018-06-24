using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartView : MonoBehaviour {
	
	private bool interacted = false;
	private bool canStart = false;
	public Dimmer dimmer;
	public Animator anim;

    public Text textArea;
    public Transform effectSpot;

	void Start() {
		SceneManager.LoadSceneAsync ("Options", LoadSceneMode.Additive);
		Cursor.visible = true;
		Invoke ("EnableStarting", 1.5f);
        Invoke("ShowStartHelp", 5f);
	}

    void ShowStartHelp()
    {
        textArea.text = "PRESS ANY KEY TO START";

        EffectManager.Instance.AddEffect(2, effectSpot.position);
        EffectManager.Instance.AddEffect(9, effectSpot.position);

        AudioManager.Instance.PlayEffectAt(12, effectSpot.position, 1f);
    }

	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.Escape) && Application.platform != RuntimePlatform.WebGLPlayer && !Application.isEditor) {
			interacted = true;
			Debug.Log ("Quit...");
			Application.Quit ();
			return;
		}

		if (Input.GetMouseButton (0)) {
			return;
		}

		if (canStart && Input.anyKeyDown && !interacted && !Input.GetKey(KeyCode.Escape)) {
			interacted = true;

            if(anim)
			    anim.SetTrigger ("hide");

            textArea.text = "";
            EffectManager.Instance.AddEffect(2, effectSpot.position);
            EffectManager.Instance.AddEffect(9, effectSpot.position);
            
			dimmer.FadeIn (0.5f);
			Invoke ("StartGame", 2f);

            AudioManager.Instance.Highpass(true);

            AudioManager.Instance.PlayEffectAt (1, effectSpot.position, 1f);
		}
	}

	void EnableStarting() {
		canStart = true;
	}

	void StartGame() {

		SceneManager.LoadSceneAsync ("Main");

		if (!Application.isEditor) {
			Cursor.visible = false;
		}
	}

	void DoSound(int clip) {
		//AudioManager.Instance.PlayEffectAt(clip, Vector3.zero, 0.5f);
	}
}
