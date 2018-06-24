using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {

    public List<string> disableThese;
    public bool hasBandanna, hasBlade;
    public int growths;
    public int hpMax = 3;
    public Vector3 spawn = Vector3.zero;
    public bool hasDied;

	private static Manager instance = null;
	public static Manager Instance {
		get { return instance; }
	}

	void Awake() {
		if (instance != null && instance != this) {
			Destroy (this.gameObject);
			return;
		} else {
			instance = this;
            DontDestroyOnLoad(instance.gameObject);
            disableThese = new List<string>();
		}
	}
}
