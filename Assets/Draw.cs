using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour {
	static Mesh cubeMesh;
	static Mesh quadMesh;
	static Mesh cylinderMesh;
	static Material debugMaterial;
	static MaterialPropertyBlock materialPropertyBlock;
	static int defaultLayer;
	static float stroke = 0.03f;
	static int circleDefinition = 40;

	public static Color color{
		set{ 
			materialPropertyBlock.SetColor("_Color", value);
		}
	}

	[RuntimeInitializeOnLoadMethod]
	static void Initialize()
	{
		GameObject go = new GameObject("DrawDebug");
		go.AddComponent<Draw>();

		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cubeMesh = cube.GetComponent<MeshFilter>().mesh;
		Destroy(cube);

		GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quadMesh = quad.GetComponent<MeshFilter>().mesh;
		Destroy(quad);

		GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		cylinderMesh = cylinder.GetComponent<MeshFilter>().mesh;
		Destroy(cylinder);

		debugMaterial = new Material(Shader.Find("Unlit/Color"));
		materialPropertyBlock = new MaterialPropertyBlock();
		defaultLayer = LayerMask.NameToLayer("Default");
	}

	void LateUpdate()
	{
		// clear color
		color = Color.white;
	}

	public static void Line(Vector3 start, Vector3 end)
	{
		Vector3 line = end - start;
		float length = line.magnitude;
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, line.normalized);
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(start + line*0.5f, rot, new Vector3(stroke, length * 0.5f, stroke));
		Graphics.DrawMesh(cylinderMesh, mat, debugMaterial, defaultLayer, Camera.main, 0, materialPropertyBlock);
	}

	public static void WireRect(Vector3 pos, float w, float h)
	{
		Line(pos + Vector3.left * w * 0.5f + Vector3.up * h * 0.5f, pos + Vector3.right * w * 0.5f + Vector3.up * h * 0.5f);
		Line(pos + Vector3.left * w * 0.5f - Vector3.up * h * 0.5f, pos + Vector3.right * w * 0.5f - Vector3.up * h * 0.5f);
		Line(pos + Vector3.up * h * 0.5f + Vector3.left * w * 0.5f, pos + Vector3.down * h * 0.5f + Vector3.left * w * 0.5f);
		Line(pos + Vector3.up * h * 0.5f + Vector3.right * w * 0.5f, pos + Vector3.down * h * 0.5f + Vector3.right * w * 0.5f);
	}

	public static void WireCircle(Vector3 pos, float radius)
	{
		float stepAngle = Mathf.PI*2 / circleDefinition;
		Vector3 prevPos = pos;
		prevPos.x += radius;
		for (int i = 0; i <= circleDefinition; i++)
		{
			float angle = i * stepAngle;
			Vector3 nextPos = pos;
			nextPos.x += Mathf.Cos(angle) * radius;
			nextPos.y += Mathf.Sin(angle) * radius;
			Line(prevPos, nextPos);
			prevPos = nextPos;
		}
	}

	public static void Rect(Vector3 pos, float w, float h)
	{
		Quaternion rot = Quaternion.identity;
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, new Vector3(w, h, 1));
		Graphics.DrawMesh(quadMesh, mat, debugMaterial, defaultLayer);
	}


	public static void Cube(Vector3 pos, Vector3 size)
	{
		Quaternion rot = Quaternion.identity;
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		Graphics.DrawMesh(cubeMesh, mat, debugMaterial, defaultLayer);
	}

	public static void Cube(Vector3 pos, Vector3 size, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		Graphics.DrawMesh(cubeMesh, mat, debugMaterial, defaultLayer);
	}

}
