using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDraw : MonoBehaviour {

	public GameObject target;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//Draw.DrawCube(Vector3.zero, Vector3.one * 2);
		//Draw.DrawCube(Vector3.right, Vector3.one * 5, Quaternion.AngleAxis(45, Vector3.up));

		//Draw.Rect(transform.position, 0.5f, 0.1f);

		//Draw.color = Color.white;
		Draw.Line(new Vector3(-2, 0,0), new Vector3(2, 0, 0));
		Draw.color = Color.red;
		Draw.Line(new Vector3(-2, -1,0), new Vector3(2, 2, 0));
		Draw.color = Color.blue;
		Draw.Line(new Vector3(0, -1,0), target.transform.position);
		Draw.color = Color.gray;
		Draw.WireRect(Vector3.one, 3, 1.5f);
		Draw.color = new Color(0.9f, 0.5f, 0.2f);
		Draw.WireCircle(Vector3.zero, 2);

		//Draw.Line(new Vector3(-2, 0, 0), new Vector3(2, 2, 0));
	}
}
