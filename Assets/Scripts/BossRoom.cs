﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : MonoBehaviour {

    public Door[] doors;
    public Slime boss;
    private bool done = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player" && !done) {
            foreach(var d in doors) {
                d.Close();
            }
        }
    }

	private void Update()
	{
        if(!boss) {
            done = true;

            Invoke("OpenDoors", 2f);
        }
	}

    void OpenDoors() {
        foreach (var d in doors) {
            d.Open();
        }
    }
}