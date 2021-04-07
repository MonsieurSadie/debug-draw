using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO : 
// - wire cone2D/3D and circle in 3D (giving an up vector or a quaternion as parameter)
// - Do arrow without depending on a static mesh
// - avoid generating text meshes.
// - text centering ? (not a priority)
// - Store user calls as CommandBuffers ?
// - add commands to scale things according to camera zoom
//   should give the option to enter sizes in viewport space and stay consistant with zoom
// - add a feature to make drawings selectable

public class Draw : MonoBehaviour {
	
	/// OPTIONS
	public static bool drawInSceneView = true;
	public static bool infiniteMode = false;
	static public float stroke = 0.03f;
	static public int circleDefinition = 40;
	static public float sphereDefaultSize = 0.1f;
	

	public class DrawData
	{
		public Mesh meshModel;
		public Matrix4x4 matrix;
		public Material drawMaterial;
		public MaterialPropertyBlock propertyBlock;
		public float timeOnScreen;
		public Color color;
	}

	public static List<DrawData> meshesToDraw = new List<DrawData>();
	
	
	static Mesh cubeMesh;
	static Mesh quadMesh;
	static Mesh sphereMesh;
	static Mesh cylinderMesh;
	static Mesh capsuleMesh;
	static Mesh circleMesh;
	static Mesh arrowMesh;
	static Mesh textMesh;
	static Mesh triangleMesh;

	static Vector3 anchor = Vector3.zero;
	static Vector3 toPixels = Vector3.one;

	static Material currentMaterial;
	static Material debugMaterial;
	static Material transparentMaterial;
	static Material instancedCallsMat;
	static Material wireInstancedCallsMat;
	static Material debugWireframeMaterial;
	static Material fontMaterial;
	static MaterialPropertyBlock materialPropertyBlock;
	static MaterialPropertyBlock wireframeMaterialPropertyBlock;
	static List<TextMesh> textMeshes;
	static GameObject textMeshHolder;

	static Camera mainCamera;

	static Font debugTextFont;
	
	static int defaultLayer;

	private static Color _color = Color.white;
	public static Color color{
		set{ 
			materialPropertyBlock.SetColor("_Color", value);
			wireframeMaterialPropertyBlock.SetColor("_WireColor", value);
			_color = value;
		}
		get{
			return _color;
		}
	}

	// transparent drawing costs more
	public static void EnableTransparentMode(bool enable)
    {
		currentMaterial = enable ? transparentMaterial : debugMaterial;
    }

	public static void SetValuesAsPixels(bool enable)
    {
		if(enable)
        {
			float camH = mainCamera.orthographicSize * 2;
			float camW = camH * mainCamera.aspect;
			toPixels.x = camW / Screen.width;
			toPixels.y = camH / Screen.height;

			anchor.x = -camW * 0.5f;
			anchor.y = -camH * 0.5f;
		}
		else
        {
			toPixels = Vector3.one;
			anchor = Vector3.zero;
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

		GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		capsuleMesh = capsule.GetComponent<MeshFilter>().mesh;
		Destroy(capsule);

		arrowMesh = ((GameObject)Resources.Load("arrow")).GetComponent<MeshFilter>().sharedMesh;

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


		// triangle
		triangleMesh = new Mesh();
		triangleMesh.vertices = new Vector3[]{ new Vector3(-1, -1, 0), new Vector3(0, 1, 0), new Vector3(1, -1, 0) };
		triangleMesh.triangles = new int[]{0,1,2};
		triangleMesh.RecalculateNormals();



		textMesh = new Mesh();
		string[] osFonts = Font.GetOSInstalledFontNames();
		if(osFonts.Length > 0)
		{
			debugTextFont = Font.CreateDynamicFontFromOSFont(osFonts[0], 100);
		}
		else
		{
			Debug.LogError("couldn't build debug font because OS has no installed font");
		}
		fontMaterial = new Material(Shader.Find("GUI/Text Shader"));
		fontMaterial.mainTexture = debugTextFont.material.mainTexture;


		// Unity legacy Text Mesh
		textMeshHolder = new GameObject("TextMeshPool");
		textMeshes = new List<TextMesh>();
		for (int i = 0; i < 200; i++)
		{
			GameObject gob = new GameObject("temp-text");
			TextMesh tm = gob.AddComponent<TextMesh>();
			gob.transform.parent = textMeshHolder.transform;
			textMeshes.Add(tm);
		}

		debugMaterial = new Material(Shader.Find("Unlit/Color"));
		transparentMaterial = new Material(Shader.Find("Unlit/TransparentColor"));
		instancedCallsMat = new Material(Shader.Find("Standard"));
		instancedCallsMat.enableInstancing = true;
		wireInstancedCallsMat = new Material((Shader)Resources.Load("Wireframe"));
		wireInstancedCallsMat.enableInstancing = true;
		debugWireframeMaterial = new Material((Shader)Resources.Load("Wireframe"));
		materialPropertyBlock = new MaterialPropertyBlock();
		wireframeMaterialPropertyBlock = new MaterialPropertyBlock();
		defaultLayer = LayerMask.NameToLayer("Default");

		currentMaterial = debugMaterial;

		mainCamera = Camera.main;
	}

	bool drawWithDurationEnabled = true;
	void Update()
	{
		for (int i = 0; i < meshesToDraw.Count; i++)
		{
			color = meshesToDraw[i].color;
			DrawMesh(meshesToDraw[i]);
			meshesToDraw[i].timeOnScreen -= Time.deltaTime;
			if(!infiniteMode && drawWithDurationEnabled && meshesToDraw[i].timeOnScreen < 0)
			{
				meshesToDraw.RemoveAt(i);
				i--;
			}
		}
	}

	void LateUpdate()
	{
		// clear color
		color = Color.white;
		if(!infiniteMode) meshesToDraw.Clear();

		// TEMP: clear usage of Text meshes to prevent creating them again and again
		foreach (Transform item in textMeshHolder.transform)
		{
			TextMesh tm = item.GetComponent<TextMesh>();
			if(!textMeshes.Contains(tm)) textMeshes.Add(tm);
		}
	}


	//=================
	//
	// DRAW FUNCTIONS
	//
	//=================
	// used when you want certain commands to be temporary
	// (will be erased by StopTempCommands)
	static int numTempCommands = 0;
	public static void StartTempCommands()
	{
		numTempCommands = 0;
	}

	public static void StopTempCommands()
	{
		UndoCommands(numTempCommands);
		numTempCommands = 0;
	}

	public static void ClearDrawBuffer()
	{
		meshesToDraw.Clear();
	}

	// Undo N commands (don't have to be temp commands)
	public static void UndoCommands(int numCommandsBackward)
	{
		meshesToDraw.RemoveRange(meshesToDraw.Count-numCommandsBackward, numCommandsBackward);
	}

	public static void UndoCommands(int numCommandsBackward, int count)
	{
		count = Mathf.Min(numCommandsBackward, count);
		meshesToDraw.RemoveRange(meshesToDraw.Count-numCommandsBackward, numCommandsBackward);
	}

	public static void Text(string str, Vector3 pos, float size)
	{
		debugTextFont.RequestCharactersInTexture(str);

		// generate vertices
		Vector3[] vertices 	= new Vector3[str.Length * 4];
		Vector2[] uv 				= new Vector2[str.Length * 4];
		int[] triangles 		= new int[str.Length * 6];
		Vector3 posInText		= Vector3.zero;

		for (int i = 0; i < str.Length; i++)
		{
			CharacterInfo char_info;
			debugTextFont.GetCharacterInfo(str[i], out char_info);
			vertices[4*i + 0] = posInText + new Vector3(char_info.minX, char_info.maxY, 0);
			vertices[4*i + 1] = posInText + new Vector3(char_info.maxX, char_info.maxY, 0);
			vertices[4*i + 2] = posInText + new Vector3(char_info.maxX, char_info.minY, 0);
			vertices[4*i + 3] = posInText + new Vector3(char_info.minX, char_info.minY, 0);
      
			uv[4 * i + 0] = char_info.uvTopLeft;
			uv[4 * i + 1] = char_info.uvTopRight;
			uv[4 * i + 2] = char_info.uvBottomRight;
			uv[4 * i + 3] = char_info.uvBottomLeft;

			triangles[6 * i + 0] = 4 * i + 0;
			triangles[6 * i + 1] = 4 * i + 1;
			triangles[6 * i + 2] = 4 * i + 2;

			triangles[6 * i + 3] = 4 * i + 0;
			triangles[6 * i + 4] = 4 * i + 2;
			triangles[6 * i + 5] = 4 * i + 3;

			posInText.x += char_info.advance; // advance char position
		}
		//textMesh.Clear();
		textMesh = new Mesh();
		textMesh.vertices = vertices;
		textMesh.uv = uv;
		textMesh.triangles = triangles;


		Quaternion orientation = mainCamera.transform.rotation;
		#if UNITY_EDITOR
		if(UnityEditor.SceneView.lastActiveSceneView)
		{
			orientation = UnityEditor.SceneView.lastActiveSceneView.camera.transform.rotation;
		}
		#endif
		Matrix4x4 matrix = Matrix4x4.TRS(pos, mainCamera.transform.rotation * Quaternion.AngleAxis(180,Vector3.up), Vector3.one * size);
		DrawMesh(textMesh, matrix, fontMaterial, materialPropertyBlock);
	}

	// TEMP: Done with TextMesh, size is in float units (scaling)
	public static void TextMesh(string str, Vector3 pos, Vector3 normal, float size)
	{
		TextMesh tm = null;
		if(textMeshes.Count <= 0) 
		{
			GameObject o = new GameObject();
			o.transform.parent = textMeshHolder.transform;
			textMeshes.Add(o.AddComponent<TextMesh>());
		}
		tm = textMeshes[0];
		textMeshes.RemoveAt(0);
		tm.text = str;
		tm.color = Draw.color;
		tm.transform.localScale = Vector3.one * size;
		tm.transform.position = pos;
		tm.transform.forward = -normal;
	}



	public static void Line(Vector3 start, Vector3 end)
	{
		Vector3 line = end - start;
		float length = line.magnitude;
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, line.normalized);
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(start + line*0.5f, rot, new Vector3(stroke, length * 0.5f, stroke));
		DrawMesh(cylinderMesh, mat, currentMaterial, materialPropertyBlock);
	}

	public static void Line(Vector3 center, Quaternion rot, float length)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(center, rot, new Vector3(stroke, length * 0.5f, stroke));
		DrawMesh(cylinderMesh, mat, currentMaterial, materialPropertyBlock);
	}

	public static void RoundedLine(Vector3 start, Vector3 end)
	{
		Vector3 line = end - start;
		float length = line.magnitude;
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, line.normalized);
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(start + line*0.5f, rot, new Vector3(stroke, length * 0.5f, stroke));
		DrawMesh(capsuleMesh, mat, currentMaterial, materialPropertyBlock);
	}

	public static void RoundedLine(Vector3 center, Quaternion rot, float length)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(center, rot, new Vector3(stroke, length * 0.5f, stroke));
		DrawMesh(capsuleMesh, mat, currentMaterial, materialPropertyBlock);
	}

	public static void WireTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Line(p0, p1);
		Line(p1, p2);
		Line(p2, p0);
	}


	public static void OrientedTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Arrow(p0, p1);
		Arrow(p1, p2);
		Arrow(p2, p0);
	}


	public static void WirePolygonOutline(Vector3[] vertices, Vector3 normal)
	{
		Vector3 center = Vector3.zero;
		foreach (var point in vertices) center += point;
		center /= vertices.Length;

		Vector3 refVertex = vertices[0];
		System.Array.Sort(vertices,
			(a,b) =>  
			Vector3.SignedAngle( 
				(refVertex - center).normalized, 
				(a - center).normalized, 
				normal).
			CompareTo(
				Vector3.SignedAngle( 
					(refVertex - center).normalized, 
					(b - center).normalized, 
					normal)
			)
		);
		refVertex = vertices[0];
		
		for (int j = 1; j < vertices.Length; j++)
		{
			Line(vertices[j-1], vertices[j]);
		}
		Line(vertices[vertices.Length-1], vertices[0]);
	}

	// optimized for buffer arrays
	public static void WirePolygonOutline(Vector3[] vertices, int vertexCount, Vector3 normal)
	{
		Vector3 center = Vector3.zero;
		for (int i = 0; i < vertexCount; i++) center += vertices[i];
		center /= vertexCount;

		Vector3 refVertex = vertices[0];
		System.Array.Sort(vertices,
			(a,b) =>  
			Vector3.SignedAngle( 
				(refVertex - center).normalized, 
				(a - center).normalized, 
				normal).
			CompareTo(
				Vector3.SignedAngle( 
					(refVertex - center).normalized, 
					(b - center).normalized, 
					normal)
			)
		);
		refVertex = vertices[0];
		
		for (int j = 1; j < vertexCount; j++)
		{
			Line(vertices[j-1], vertices[j]);
		}
		Line(vertices[vertexCount-1], vertices[0]);
	}

	public static void WirePolygonOutline(List<Vector3> vertices, Vector3 normal)
	{
		Vector3 center = Vector3.zero;
		foreach (var point in vertices) center += point;
		center /= vertices.Count;

		Vector3 refVertex = vertices[0];
		vertices.Sort(
			(a,b) =>  
			Vector3.SignedAngle( 
				(refVertex - center).normalized, 
				(a - center).normalized, 
				normal).
			CompareTo(
				Vector3.SignedAngle( 
					(refVertex - center).normalized, 
					(b - center).normalized, 
					normal)
			)
		);
		refVertex = vertices[0];
		
		for (int j = 1; j < vertices.Count; j++)
		{
			Line(vertices[j-1], vertices[j]);
		}
		Line(vertices[vertices.Count-1], vertices[0]);
	}


	public static void WirePolygonTriangles(List<Vector3> vertices, Vector3 normal)
	{
		Vector3 center = Vector3.zero;
		foreach (var point in vertices) center += point;
		center /= vertices.Count;

		Vector3 refVertex = vertices[0];
		vertices.Sort(
			(a,b) =>  
			Vector3.SignedAngle( 
				(refVertex - center).normalized, 
				(a - center).normalized, 
				normal).
			CompareTo(
				Vector3.SignedAngle( 
					(refVertex - center).normalized, 
					(b - center).normalized, 
					normal)
			)
		);
		refVertex = vertices[0];
		
		for (int j = 2; j < vertices.Count; j++)
		{
			Draw.OrientedTriangle(refVertex, vertices[j-1], vertices[j]);
		}
	}


	
	static Vector3 scale = new Vector3(0.03f, 1, 0.03f);
	const int MAX_INSTANCES_LINES = 1023;
	static Matrix4x4[] instanced_matrices = new Matrix4x4[MAX_INSTANCES_LINES];
	static void GetLineMatrixNonAlloc(Vector3 start, Vector3 end, ref Matrix4x4 mat)
	{
		Vector3 line = end - start;
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, line);
		scale.y = line.magnitude * 0.5f;
		mat.SetTRS(start + line*0.5f, rot, scale);
	}

	public static void Triangles(int count, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		int nbLines = count * 3;
		while(nbLines > 0)
		{
			int subcount = nbLines > 1023 ? 1023 : nbLines;
			int maxTri = Mathf.FloorToInt((float)subcount/3);
			int j = 0;
			for (int i = 0; i < maxTri; i++)
			{
				GetLineMatrixNonAlloc(p0, p1, ref instanced_matrices[j++]);
				GetLineMatrixNonAlloc(p1, p2, ref instanced_matrices[j++]);
				GetLineMatrixNonAlloc(p2, p0, ref instanced_matrices[j++]);
			}
			Graphics.DrawMeshInstanced(cylinderMesh, 0, instancedCallsMat, instanced_matrices, maxTri * 3);
			nbLines -= maxTri*3;
		}
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
		WireCone2D(pos, direction, radius, angle, Vector3.back);
	}

	public static void WireCone2D(Vector3 pos, Vector3 direction, float radius, float angle, Vector3 up)
	{
		float stepAngle = Mathf.PI*2 / circleDefinition;
		Quaternion rot = Quaternion.FromToRotation(Vector3.back, up);
		Quaternion halfconerot = Quaternion.Euler(0, 0, angle * 0.5f);
		Vector3 prevPos = pos + halfconerot * rot * (direction * radius);
		
		Line(pos, prevPos);
		for (float finalAngle = 0; finalAngle <= angle; finalAngle += stepAngle)
		{
			Vector3 to_next_point = Quaternion.Euler(0,0, angle * 0.5f - finalAngle) * direction * radius;
			to_next_point = rot * to_next_point;
			Vector3 nextPos = pos + to_next_point;
			Line(prevPos, nextPos);
			prevPos = nextPos;
		}
		Line(pos, prevPos);
	}



	public static void Rect2D(float x, float y, float w, float h)
	{
		Rect(new Vector3(x, y, 0), w, h, Vector3.back);
	}

	public static void Rect(Vector3 pos, float w, float h)
	{
		Rect(pos, w, h, Vector3.back);
	}

	public static void Rect(Vector3 pos, float w, float h, Vector3 up)
	{
		pos = anchor + Vector3.Scale(pos, toPixels);
		Matrix4x4 mat = new Matrix4x4();
		Vector3 scale = new Vector3(w, h, 1);
		mat.SetTRS(pos, Quaternion.FromToRotation(Vector3.back, up), Vector3.Scale(scale, toPixels));
		DrawMesh(quadMesh, mat, currentMaterial, materialPropertyBlock);
	}

	public static void Circle(Vector3 pos, float radius)
	{
		Circle(pos, radius, Vector3.back);
	}

	public static void Circle(Vector3 pos, float radius, Vector3 up)
	{
		pos = anchor + Vector3.Scale(pos, toPixels);
		Matrix4x4 mat = new Matrix4x4();
		Vector3 scale = Vector3.one;
		mat.SetTRS(pos, Quaternion.FromToRotation(Vector3.back, up), Vector3.Scale(scale, toPixels));
		DrawMesh(circleMesh, mat, currentMaterial, materialPropertyBlock);
	}




	public static void Cube(Vector3 pos, Vector3 size)
	{
		Cube(pos, size, Quaternion.identity);
	}

	public static void Cube(Vector3 pos, Vector3 size, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		DrawMesh(cubeMesh, mat, currentMaterial, materialPropertyBlock);
	}

	public static void Arrow(Vector3 start, Vector3 end)
	{
		Vector3 dirVec = end - start;
		Matrix4x4 matrix = Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.forward, dirVec.normalized), new Vector3(stroke, stroke, dirVec.magnitude));
		DrawMesh(arrowMesh, matrix, currentMaterial, materialPropertyBlock);
	} 

	public static void DirectionArrow(Vector3 start, Vector3 direction)
	{
		Vector3 dirVec = direction;
		Matrix4x4 matrix = Matrix4x4.TRS(start, Quaternion.FromToRotation(Vector3.forward, dirVec.normalized), new Vector3(stroke, stroke, dirVec.magnitude));
		DrawMesh(arrowMesh, matrix, currentMaterial, materialPropertyBlock);
	}

	public static void Sphere(Vector3 pos)
	{
		Sphere(pos, sphereDefaultSize);
	}

	public static void Sphere(Vector3 pos, float radius, float duration=0)
	{
		Sphere(pos, radius, Quaternion.identity, duration);
	}

	public static void Sphere(Vector3 pos, float radius, Quaternion rot, float duration=0)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, Vector3.one * radius);
		DrawMesh(sphereMesh, mat, debugWireframeMaterial, wireframeMaterialPropertyBlock, duration);
	}


	public static void WireCube(Vector3 pos, Vector3 size)
	{
		WireCube(pos, size, Quaternion.identity);
	}

	public static void WireCube(Vector3 pos, Vector3 size, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, size);
		DrawMesh(cubeMesh, mat, debugWireframeMaterial, wireframeMaterialPropertyBlock);
	}

	public static void WireCubeInstanced(Vector3[] positions, Vector3[] sizes)
	{
		for (int i = 0; i < positions.Length; i++)
		{
			instanced_matrices[i].SetTRS(positions[i], Quaternion.identity, sizes[i]);
		}
		Graphics.DrawMeshInstanced(cubeMesh, 0, wireInstancedCallsMat, instanced_matrices, positions.Length);
	}


	public static void WireSphere(Vector3 pos, float radius)
	{
		WireSphere(pos, radius, Quaternion.identity);
	}

	public static void WireSphere(Vector3 pos, float radius, Quaternion rot)
	{
		Matrix4x4 mat = new Matrix4x4();
		mat.SetTRS(pos, rot, Vector3.one * radius);
		DrawMesh(sphereMesh, mat, debugWireframeMaterial, wireframeMaterialPropertyBlock);
	}


	static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, MaterialPropertyBlock propertyBlock, float duration = 0)
	{
		DrawData drawData = new DrawData();
		drawData.meshModel = mesh;
		drawData.matrix = matrix;
		drawData.drawMaterial = material;
		drawData.propertyBlock = propertyBlock;
		drawData.timeOnScreen = duration;
		drawData.color = color;
		
		meshesToDraw.Add(drawData);
		numTempCommands++;
	}

	static void DrawMesh(DrawData data)
	{
		Graphics.DrawMesh(data.meshModel, data.matrix, data.drawMaterial, defaultLayer, mainCamera, 0, data.propertyBlock);
		
		#if UNITY_EDITOR
		if(drawInSceneView)
		{
			if(UnityEditor.SceneView.lastActiveSceneView)
			{
				Graphics.DrawMesh(data.meshModel, data.matrix, data.drawMaterial, defaultLayer, UnityEditor.SceneView.lastActiveSceneView.camera, 0, data.propertyBlock);
			}else
			{
				//Debug.LogWarning("no drawing scene view found");
			}
		}
		#endif
	}

}
