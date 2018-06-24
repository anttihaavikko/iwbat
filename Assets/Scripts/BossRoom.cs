using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : MonoBehaviour {

    public Door[] doors;
    public Slime boss;
    private bool done = false;
    private bool started = false;
    private string bossName;

	private void Start()
	{
        bossName = boss.name;
	}

	private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player" && !started) {

            started = true;

            AudioManager.Instance.ChangeMusic(1, 0.5f, 0.5f, 0f);

            foreach(var d in doors) {
                d.Close();
            }
        }
    }

	private void Update()
	{
        if(!boss && !done) {
            done = true;

            Manager.Instance.disableThese.Add(name);
            Manager.Instance.disableThese.Add(bossName);

            AudioManager.Instance.ChangeMusic(0, 2f, 2f, 2f);

            Invoke("OpenDoors", 3f);
        }
	}

    void OpenDoors() {
        foreach (var d in doors) {
            d.Open();
        }
    }
}
