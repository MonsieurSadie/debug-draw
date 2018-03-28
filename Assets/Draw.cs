using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO : 
// - find solution for alpha (can't use standard shader from script)
// - find better solution for wireframe shaders
// - wire cone2D and circle in 3D (giving an up vector or a quaternion as parameter)

public class Draw : MonoBehaviour {
	static Mesh cubeMesh;
	static Mesh quadMesh;
	static Mesh sphereMesh;
	static Mesh cylinderMesh;
	static Mesh circleMesh;

	static Material debugMaterial;
	static Material debugWireframeMaterial;
	static MaterialPropertyBlock materialPropertyBlock;
	static MaterialPropertyBlock wireframeMaterialPropertyBlock;
	
	static int defaultLayer;
	static float stroke = 0.03f;
	static int circleDefinition = 40;

	public static Color color{
		set{ 
			materialPropertyBlock.SetColor("_Color", value);
			wireframeMaterialPropertyBlock.SetColor("_WireColor", value);
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

		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphereMesh = sphere.GetComponent<MeshFilter>().mesh;
		Destroy(sphere);

		GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		cylinderMesh = cylinder.GetComponent<MeshFilter>().mesh;
		Destroy(cylinder);

		/// Create a circle mesh
		circleMesh = new Mesh();
		Vector3[] vertices = new Vector3[circleDefinition+1];
		int[] triangles = new int[circleDefinition*3];
		float stepAngle = Mathf.PI*2 / circleDefinition;

		vertices[0] = Vector3.zero;
		vertices[1] = Vector3.right;
		int triIndex = 0;

		for (int i = 2; i <= circleDefinition; i++)
		{
			float angle = -i * stepAngle;
			vertices[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

			triangles[triIndex++]	= 0;
			triangles[triIndex++] = i-1;
			triangles[triIndex++]	= i;
		}
		circleMesh.vertices = vertices;
		circleMesh.triangles = triangles;
		circleMesh.RecalculateNormals();
		/// end circle mesh

		debugMaterial = new Material(Shader.Find("Unlit/Color"));
		debugWireframeMaterial = new Material((Shader)Resources.Load("Wireframe"));
		materialPropertyBlock = new MaterialPropertyBlock();
		wireframeMaterialPropertyBlock = new MaterialPropertyBlock();
		defaultLayer = LayerMask.NameToLayer("Default");
	}

	void LateUpdate()
	{
		// clear color
		color = Color.white;
	}


	//=================
	//
	// DRAW FUNCTIONS
	//
	//=================

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

	public static void WireCone2D(Vector3 pos, Vector3 direction, float radius, float angle)
	{
		float stepAngle = Mathf.PI*2 / circleDefinition;
		Quaternion halfrot = Quaternion.Euler(0, 0, angle * 0.5f);
		Vector3 prevPos = pos + halfrot * (direction * radius);
		
		Line(pos, prevPos);
		for (float finalAngle = 0; finalAngle <= angle; finalAngle += stepAngle)
		{
			Vector3 nextPos = pos + Quaternion.Euler(0,0, angle * 0.5f - finalAngle) * direction * radius;
			Line(prevPos, nextPos);
			prevPos = nextPos;
		}
		Line(pos, prevPos);
	}



	public static void Rect(Vector3 pos, float w, float h)
	{
		Quaternion rot = Quaternion.identity;
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, new Vector3(w, h, 1));
		Graphics.DrawMesh(quadMesh, mat, debugMaterial, defaultLayer, Camera.main, 0, materialPropertyBlock);
	}

	public static void Circle(Vector3 pos, float radius)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, Quaternion.identity, Vector3.one);
		Graphics.DrawMesh(circleMesh, mat, debugMaterial, defaultLayer, Camera.main, 0, materialPropertyBlock);
	}






	public static void Cube(Vector3 pos, Vector3 size)
	{
		Quaternion rot = Quaternion.identity;
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		Graphics.DrawMesh(cubeMesh, mat, debugMaterial, defaultLayer, Camera.main, 0, materialPropertyBlock);
	}

	public static void Cube(Vector3 pos, Vector3 size, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		Graphics.DrawMesh(cubeMesh, mat, debugMaterial, defaultLayer, Camera.main, 0, materialPropertyBlock);
	}

	public static void Sphere(Vector3 pos, float radius)
	{
		Quaternion rot = Quaternion.identity;
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, Vector3.one * radius);
		Graphics.DrawMesh(sphereMesh, mat, debugMaterial, defaultLayer, Camera.main, 0, materialPropertyBlock);
	}

	public static void Sphere(Vector3 pos, float radius, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, Vector3.one * radius);
		Graphics.DrawMesh(sphereMesh, mat, debugMaterial, defaultLayer, Camera.main, 0, materialPropertyBlock);
	}




	public static void WireCube(Vector3 pos, Vector3 size)
	{
		Quaternion rot = Quaternion.identity;
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		Graphics.DrawMesh(cubeMesh, mat, debugWireframeMaterial, defaultLayer, Camera.main, 0, wireframeMaterialPropertyBlock);
	}

	public static void WireCube(Vector3 pos, Vector3 size, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		Graphics.DrawMesh(cubeMesh, mat, debugWireframeMaterial, defaultLayer, Camera.main, 0, wireframeMaterialPropertyBlock);
	}


	public static void WireSphere(Vector3 pos, float radius)
	{
		Quaternion rot = Quaternion.identity;
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, Vector3.one * radius);
		Graphics.DrawMesh(sphereMesh, mat, debugWireframeMaterial, defaultLayer, Camera.main, 0, wireframeMaterialPropertyBlock);
	}

	public static void WireSphere(Vector3 pos, float radius, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, Vector3.one * radius);
		Graphics.DrawMesh(sphereMesh, mat, debugWireframeMaterial, defaultLayer, Camera.main, 0, wireframeMaterialPropertyBlock);
	}



}
