using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColliderResizer : MonoBehaviour {

    public BoxCollider2D coll;
    public SpriteRenderer sprite;
	
	// Update is called once per frame
	void Update () {
        if(coll) coll.size = sprite.size;
	}
}
