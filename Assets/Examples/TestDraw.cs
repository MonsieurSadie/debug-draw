using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDraw : MonoBehaviour {

	public GameObject target;
	void Start () {
		
	}
	
	float angle = 0;
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

		Draw.color = new Color(0.2f, 0.9f, 0.9f, 0.1f);
		Draw.Rect(Vector3.zero, 1.5f, 2.2f);

		Draw.color = Color.yellow;
		Draw.Circle(Vector3.zero, 0.5f, new Vector3(0, 1, -1));

		Draw.color = Color.green;
		Draw.WireCube(-Vector3.one, Vector3.one, Quaternion.Euler(-30, 30, 0));

		Draw.color = new Color(0.8f, 0.3f, 0.4f, 1f);
		Draw.WireSphere(Vector3.right * 2, 1, Quaternion.Euler(0, 0, 30));
		Draw.Sphere(Vector3.right * 3, 0.5f, Quaternion.Euler(0, 0, 30));

		Draw.color = Color.white;
		//angle += 90 * Time.deltaTime;
		Draw.WireCone2D(target.transform.position, Quaternion.Euler(0, 0, angle) * Vector3.right, 2, 45, Vector3.up);
	}
}
