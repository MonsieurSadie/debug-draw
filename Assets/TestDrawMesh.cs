using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrawMesh : MonoBehaviour
{
	public Mesh mesh;
	Vector3[] meshVertices;
	int[] meshIndices;
 
  void Start()
  {
    meshVertices 	= mesh.vertices;
    meshIndices 	= mesh.triangles;

    StartCoroutine(DrawMesh());
  }

  IEnumerator DrawMesh()
  {
  	Draw.infiniteMode = true;
    Draw.color 	= Color.white;
    Draw.stroke = 0.003f;

    for(int i=0; i<meshIndices.Length; i+=3)
    {
    	Vector3 p0 = meshVertices[ meshIndices[i] ];
    	Vector3 p1 = meshVertices[ meshIndices[i+1] ];
    	Vector3 p2 = meshVertices[ meshIndices[i+2] ];

    	Draw.color = Color.white;
    	Draw.WireTriangle(p0, p1, p2);

    	Draw.color = Color.red;
    	Draw.Sphere(p0, 0.01f);
    	Draw.Sphere(p1, 0.01f);
    	Draw.Sphere(p2, 0.01f);

    	// temps drawing of vertices index
    	Draw.StartTempCommands();
    	Draw.color = Color.black;
    	Draw.Text(meshIndices[i].ToString(), p0, 0.0005f);
    	Draw.Text(meshIndices[i+1].ToString(), p1, 0.0005f);
    	Draw.Text(meshIndices[i+2].ToString(), p2, 0.0005f);

    	yield return new WaitForInput("triangle", KeyCode.Space);
    	Draw.StopTempCommands(); // clears previous text
    }

    yield return null;
  }
}
