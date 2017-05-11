using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineClass /*: MonoBehaviour */{
	
	public IntVector2 lineArrayPosition;			// Integer value for line origin position within game layout
	private Vector2 lineOrigin;						// Pixel value for corner of origin position in gamespace
	private Vector2 lineCenter;						// Pixel value for center of origin circle in gamespace

	private IntVector2 lineLastPosition;			// Last Point of the line

	private Transform parent;						// parent reference

	private GameObject lineGO;						// Game Object for holding the whole line object
	private GameObject circleGO;					// Game Object for holding the circle

	private Texture2D texCircle;					// Texture for line origin circle
	private Texture2D texDashed;					// Texture for dashes reverse line

	private IntVector2 blockPixelSize;					// Pixel value for the size of the blocks
	//private float blockWidth = 60.0f;				// Pixel width of a square
	//private float blockHeight = 60.0f;				// Pixel height of a square

	private IntVector2 gameArraySize;				// integer size of game grid

	private IntVector2 screenSize;

	private GameObject lineObject;					// Game Object holding the forward line drawing object
	private GameObject lineObjectReverse;			// Game Object holding the reverse line drawing object
	private LineRenderer lineRenderer;				// Line drawing object for the forward line
	private LineRenderer lineRendererReverse;		// Line drawing object for the reverse line
	private List<IntVector2> linePathList;			// Holds the list of points the forward line passes through
	private List<IntVector2> linePathListReverse;	// Holds the list of points the reverse line passes through

	public LineClass (
		Transform _parent,
		IntVector2 _lineArrayPosition,
		IntVector2 _gameArraySize,
		Texture2D _texCircle,
		Texture2D _texDashed,
		IntVector2 _blockPixelSize,
		IntVector2 _screenSize) {

		parent = _parent;
		gameArraySize = _gameArraySize;
		lineArrayPosition = _lineArrayPosition;

		lineLastPosition = lineArrayPosition;

		texCircle = _texCircle;
		texDashed = _texDashed;
		blockPixelSize = _blockPixelSize;
		screenSize = _screenSize;
		
		linePathList = new List<IntVector2>();
		linePathListReverse = new List<IntVector2>();

		linePathList.Add (lineArrayPosition);
		linePathListReverse.Add (lineArrayPosition);

		lineOrigin = convertPointToWorldSpace (lineArrayPosition);
		lineCenter = getCenterOfBlockFromWorldSpace (lineOrigin);

		lineGO = new GameObject("line_"+lineArrayPosition.x+","+lineArrayPosition.y);
		lineGO.transform.SetParent (parent);

		addCircle ();
		setupLines ();


		/*for (int i = 0; i < 100; ++i) {
			if (goDirection (Random.Range (0, 4)) == false) {
				i--;
			}
		}*/

	}

	public bool hitsLine (IntVector2 _testPoint) {
		if (linePathList.Contains (_testPoint)) {
			return true;
		}
		if (linePathListReverse.Contains (_testPoint)) {
			return true;
		}

		return false;
	}

	public bool pointActivatesLine (IntVector2 _testPoint) {
		IntVector2 lastPoint = linePathList [linePathList.Count - 1];

		return (_testPoint.x == lastPoint.x && _testPoint.y == lastPoint.y);
	}

	public bool isValidMove (IntVector2 _testPoint) {
		IntVector2 lastPoint = linePathList [linePathList.Count - 1];

		bool isTestPointNextToEnd = false;
		if (lastPoint.x == _testPoint.x) {
			if (lastPoint.y == _testPoint.y + 1 ||
			   lastPoint.y == _testPoint.y - 1) {
				isTestPointNextToEnd = true;
			}
		} else if (lastPoint.y == _testPoint.y) {
			if (lastPoint.x == _testPoint.x + 1 ||
				lastPoint.x == _testPoint.x - 1) {
				isTestPointNextToEnd = true;
			}
		}

		//Debug.Log ("ALEC - point A");

		if (!isTestPointNextToEnd) {
			return false;
		}

		//Debug.Log ("ALEC - point B");

		//bool isTestPointInsideLine = false;

		if (linePathList.Count >= 2) {
			IntVector2 previousPoint = linePathList [linePathList.Count - 2];

			if (_testPoint.x == previousPoint.x &&
				_testPoint.y == previousPoint.y) {
				// Going back one spot in the forward line is an acceptable move
				//isTestPointInsideLine = true;
				return true;
			}
		}

		//Debug.Log ("ALEC - point C");

		for (int i = 0; i < linePathList.Count; ++i) {
			if (i != linePathList.Count - 2) {
				if (linePathList [i].x == _testPoint.x &&
				    linePathList [i].y == _testPoint.y) {
					return false;
				}
			}
		}

		//Debug.Log ("ALEC - point D");
		//if (linePathList.Contains (_testPoint)) {
		//	return false;
		//}

		//bool isTestPointInsideReverseLine = false;

		if (linePathListReverse.Contains (_testPoint)) {
			//isTestPointInsideReverseLine = true;
			return false;
		}

		//Debug.Log ("ALEC - point E");

		//ALEC - THIS FUNCTION NEEDS WORK

		return true;
	}

	/*private bool isPointNextToLastPoint (IntVector2 _testPoint) {
		IntVector2 lastPoint = linePathList [linePathList.Count - 1];

		bool isTestPointNextToEnd = false;
		if (lastPoint.x == _newPoint.x) {
			if (lastPoint.y == _newPoint.y + 1 ||
				lastPoint.y == _newPoint.y - 1) {
				isTestPointNextToEnd = true;
			}
		} else if (lastPoint.y == _newPoint.y) {
			if (lastPoint.x == _newPoint.x + 1 ||
				lastPoint.x == _newPoint.x - 1) {
				isTestPointNextToEnd = true;
			}
		}

		return isTestPointNextToEnd;
	}*/

	public bool doMove (IntVector2 _newPoint) {
		IntVector2 lastPoint = linePathList [linePathList.Count - 1];

		bool isTestPointNextToEnd = false;
		if (lastPoint.x == _newPoint.x) {
			if (lastPoint.y == _newPoint.y + 1 ||
				lastPoint.y == _newPoint.y - 1) {
				isTestPointNextToEnd = true;
			}
		} else if (lastPoint.y == _newPoint.y) {
			if (lastPoint.x == _newPoint.x + 1 ||
				lastPoint.x == _newPoint.x - 1) {
				isTestPointNextToEnd = true;
			}
		}

		//bool isTestPointNextToEnd = isPointNextToLastPoint (_newPoint);
		if (isTestPointNextToEnd) {
			bool doneMove = false;

			if (linePathList.Count >= 2) {
				IntVector2 previousPoint = linePathList [linePathList.Count - 2];

				if (_newPoint.x == previousPoint.x &&
					_newPoint.y == previousPoint.y) {

					//Debug.Log ("Removing last point");
					removeLastPoint();

					doneMove = true;

					return false;

					//Go back one space
				}
			}

			if (!doneMove) {

				bool isValidMove = true;

				for (int i = 0; i < linePathList.Count; ++i) {
					if (i != linePathList.Count - 2) {
						if (linePathList [i].x == _newPoint.x &&
							linePathList [i].y == _newPoint.y) {
							isValidMove = false;
						}
					}
				}

				if (linePathListReverse.Contains (_newPoint)) {
					//isTestPointInsideReverseLine = true;
					isValidMove = false;
					//return false;
				}

				if (isValidMove) {
					//Debug.Log ("Adding Point");
					addPoint (_newPoint);

					return true;
				}
			}
		}
		return false;
	}

	public IntVector2 getDirectionPoint (int _direction) {
		switch (_direction) {
		case 0:
			return new IntVector2 (lineLastPosition.x, lineLastPosition.y + 1);
		case 1:
			return new IntVector2 (lineLastPosition.x + 1, lineLastPosition.y);
		case 2:
			return new IntVector2 (lineLastPosition.x, lineLastPosition.y - 1);
		case 3:
			return new IntVector2 (lineLastPosition.x - 1, lineLastPosition.y);
		}
		return lineLastPosition;
	}

	public bool goDirection (int _direction) {
		switch (_direction) {
		case 0:
			return doMove (new IntVector2 (lineLastPosition.x, lineLastPosition.y + 1));
			//break;
		case 1:
			return doMove (new IntVector2 (lineLastPosition.x + 1, lineLastPosition.y));
			//break;
		case 2:
			return doMove (new IntVector2 (lineLastPosition.x, lineLastPosition.y - 1));
			//break;
		case 3:
			return doMove (new IntVector2 (lineLastPosition.x - 1, lineLastPosition.y));
			//break;
		}
		return false;
	}

	public int lineLength() {
		return linePathList.Count;
	}

	public void resetLine() {

		linePathList.Clear ();
		linePathList.Add (lineArrayPosition);

		linePathListReverse.Clear ();
		linePathListReverse.Add (lineArrayPosition);

		lineRenderer.positionCount = linePathList.Count;
		lineRendererReverse.positionCount = linePathListReverse.Count;
	}

	public void addPoint (IntVector2 _newPoint) {

		if (isValidMove (_newPoint)) {

			linePathList.Add (_newPoint);
			lineRenderer.positionCount = linePathList.Count;

			lineLastPosition = _newPoint;

			Vector2 newPointPixels = convertPointToWorldSpace (_newPoint);
			Vector2 newPointCenterPixels = getCenterOfBlockFromWorldSpace (newPointPixels);
			lineRenderer.SetPosition (linePathList.Count - 1, new Vector3 (newPointCenterPixels.x, newPointCenterPixels.y, -1.0f));


			IntVector2 newPointReverse = getReverse (_newPoint);

			linePathListReverse.Add (newPointReverse);
			lineRendererReverse.positionCount = linePathListReverse.Count;

			Vector2 newPointPixelsReverse = convertPointToWorldSpace (newPointReverse);
			Vector2 newPointCenterPixelsReverse = getCenterOfBlockFromWorldSpace (newPointPixelsReverse);
			lineRendererReverse.SetPosition (linePathListReverse.Count - 1, new Vector3 (newPointCenterPixelsReverse.x, newPointCenterPixelsReverse.y, -1.0f));

		//	return true;
		}// else {
		//	return false;
		//}
	}
	
	public void removeLastPoint () {

		linePathList.RemoveAt (linePathList.Count - 1);
		lineRenderer.positionCount = linePathList.Count;

		lineLastPosition = linePathList [linePathList.Count - 1];

		linePathListReverse.RemoveAt (linePathListReverse.Count - 1);
		lineRendererReverse.positionCount = linePathListReverse.Count;

	}

	public IntVector2 getReverse (IntVector2 _inPoint) {
		//return new IntVector2 (lineArrayPosition.x - (_inPoint.x - lineArrayPosition.x),
		//						lineArrayPosition.y - (_inPoint.y - lineArrayPosition.y));

		return lineArrayPosition - (_inPoint - lineArrayPosition);
	}

	private Vector2 convertPointToWorldSpace (IntVector2 _inPoint) {
		IntVector2 gameArea = blockPixelSize * gameArraySize;
		Vector2 position = (screenSize.toFloat() - gameArea.toFloat()) / 2.0f + blockPixelSize.toFloat() * _inPoint;
		return position;




		/*
		float gameAreaWidth = blockPixelSize.x * gameArraySize.x;
		float gameAreaHeight = blockPixelSize.y * gameArraySize.y;
		float blockX = (screenSize.x - gameAreaWidth) / 2.0f + blockPixelSize.x * _inPoint.x;
		float blockY = (screenSize.y - gameAreaHeight) / 2.0f + blockPixelSize.y * _inPoint.y;

		Debug.Log ( "C - " + blockX + ", " + blockY);

		return new Vector2 (blockX, blockY);*/
	}

	private Vector2 getCenterOfBlockFromWorldSpace (Vector2 _inPoint) {
		//return new Vector2 (_inPoint.x + blockPixelSize.x / 2, _inPoint.y + blockPixelSize.y / 2);
		return _inPoint + blockPixelSize.toFloat() / 2;
	}


	private void setupLines()
	{
		lineObject = new GameObject("LineForward");
		lineRenderer = lineObject.AddComponent<LineRenderer>();
		lineRenderer.transform.SetParent (lineGO.transform);
		lineRenderer.startWidth = blockPixelSize.x * 0.3f;
		lineRenderer.endWidth = blockPixelSize.x * 0.3f;
		lineRenderer.startColor = Color.red;
		lineRenderer.endColor = Color.red;
		lineRenderer.material = new Material (Shader.Find ("Particles/Alpha Blended"));
		lineRenderer.useWorldSpace = true;  
		//lineRenderer.material.mainTexture = texDashed;

		lineRenderer.positionCount = 1;
		lineRenderer.SetPosition (0, new Vector3 (lineCenter.x, lineCenter.y, -1.0f));






		//lineRenderer.enabled = false;

		lineObjectReverse = new GameObject("LineReverse");
		lineRendererReverse = lineObjectReverse.AddComponent<LineRenderer>();
		lineRendererReverse.transform.SetParent (lineGO.transform);
		lineRendererReverse.startWidth = blockPixelSize.x * 0.3f;
		lineRendererReverse.endWidth = blockPixelSize.x * 0.3f;
		//lineRendererReverse.startColor = Color.blue;
		//lineRendererReverse.endColor = Color.blue;
		lineRendererReverse.material = new Material (Shader.Find ("Particles/Alpha Blended"));
		lineRendererReverse.useWorldSpace = true;  
		lineRendererReverse.material.mainTexture = texDashed;
		lineRendererReverse.material.mainTextureScale = new Vector2 (0.01f, 0.01f);
		lineRendererReverse.textureMode = LineTextureMode.Tile;
		lineRendererReverse.positionCount = 1;
		lineRendererReverse.SetPosition (0, new Vector3 (lineCenter.x, lineCenter.y, -1.0f));

		//lineRendererReverse.enabled = false;
	}

	//public void add

	private void addCircle()
	{
		circleGO = new GameObject("circle");
		circleGO.transform.SetParent (lineGO.transform);
		SpriteRenderer sr = circleGO.AddComponent<SpriteRenderer>() as SpriteRenderer;
		sr.color = new Color(0.9f, 0.1f, 0.1f, 1.0f);

		Sprite mySprite = Sprite.Create( texCircle, new Rect(0.0f, 0.0f, texCircle.width, texCircle.height), new Vector2(0.0f, 0.0f), 1.0f);

		sr.sprite = mySprite;

		float imageScaleWidth = blockPixelSize.x / sr.sprite.bounds.size.x;
		circleGO.transform.localScale = new Vector2 (imageScaleWidth, imageScaleWidth);

		circleGO.transform.position = new Vector2 (lineOrigin.x, lineOrigin.y);
	}



	/*private void OnDestroy()
	{
		//Need to destroy materials
		//Destroy(this.renderer.material);
	}*/


	/*PolygonCollider2D pc2 ;
	void Start () {
		pc2 = gameObject.GetComponent<PolygonCollider2D>();
		//Render thing
		int pointCount = 0;
		pointCount = pc2.GetTotalPointCount();
		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		Vector2[] points = pc2.points;
		Vector3[] vertices = new Vector3[pointCount];
		Vector2[] uv = new Vector2[pointCount];
		for(int j=0; j<pointCount; j++){
			Vector2 actual = points[j];
			vertices[j] = new Vector3(actual.x, actual.y, 0);
			uv[j] = actual;
		}
		Triangulator tr = new Triangulator(points);
		int [] triangles = tr.Triangulate();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mf.mesh = mesh;
		//Render thing
	}*/


	/*

	void OnPostRender() {
		//if (!mat) {
		Debug.LogError("Please Assign a material on the inspector");
		//	return;
		//}
		GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.LINES);
		GL.Color(Color.red);
		GL.Vertex(startVertex);
		GL.Vertex(new Vector3(10, 10, -10));
		GL.End();
		GL.PopMatrix();
	}*/


}

