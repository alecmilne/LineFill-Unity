using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineClass {
	
	private IntVector2 lineArrayPosition;			// Integer value for line origin position within game layout
	private Vector2 lineOrigin;						// Pixel value for corner of origin position in gamespace
	private Vector2 lineCenter;						// Pixel value for center of origin circle in gamespace

	private IntVector2 lineLastPosition;			// Last Point of the line

	private Transform parent;						// parent reference

	private GameObject lineGO;						// Game Object for holding the whole line object
	private GameObject circleGO;					// Game Object for holding the circle

	private Texture2D texCircle;					// Texture for line origin circle
	private Texture2D texLine;						// Texture for forward line
	private Texture2D texDashed;					// Texture for dashes reverse line

	private IntVector2 blockPixelSize;				// Pixel value for the size of the blocks

	private IntVector2 gameArraySize;				// integer size of game grid

	private IntVector2 screenSize;

	private GameObject lineObject;					// Game Object holding the forward line drawing object
	private GameObject lineObjectReverse;			// Game Object holding the reverse line drawing object
	private LineRenderer lineRenderer;				// Line drawing object for the forward line
	private LineRenderer lineRendererReverse;		// Line drawing object for the reverse line
	private List<IntVector2> linePathList;			// Holds the list of points the forward line passes through
	private List<IntVector2> linePathListReverse;	// Holds the list of points the reverse line passes through

	private Color lineColour;						// Colour for the circle and lines

	public LineClass (
		Transform _parent,
		IntVector2 _lineArrayPosition,
		IntVector2 _gameArraySize,
		Texture2D _texCircle,
		Texture2D _texLine,
		Texture2D _texDashed,
		IntVector2 _blockPixelSize,
		IntVector2 _screenSize) {

		parent = _parent;
		gameArraySize = _gameArraySize;
		lineArrayPosition = _lineArrayPosition;

		lineLastPosition = lineArrayPosition;

		texCircle = _texCircle;
		texLine = _texLine;
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

		lineColour = new Color (Random.Range (0, 256) * 1.0f / 256.0f,
			Random.Range (0, 256) * 1.0f / 256.0f,
			Random.Range (0, 256) * 1.0f / 256.0f);

		addCircle ();
		setupLines ();
	}

	public IntVector2 getOriginBlock() {
		return lineArrayPosition;
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

		return (_testPoint == lastPoint);
	}

	public bool isValidMove (IntVector2 _testPoint) {
		if (!isPointNextToLastPoint(_testPoint)) {
			return false;
		}

		if (linePathList.Count >= 2) {
			IntVector2 previousPoint = linePathList [linePathList.Count - 2];

			if (_testPoint == previousPoint) {
				// Going back one spot in the forward line is an acceptable move
				return true;
			}
		}

		for (int i = 0; i < linePathList.Count; ++i) {
			if (i != linePathList.Count - 2) {
				if (linePathList [i] == _testPoint) {
					return false;
				}
			}
		}

		if (linePathListReverse.Contains (_testPoint)) {
			return false;
		}

		return true;
	}

	private bool isPointNextToLastPoint (IntVector2 _testPoint) {
		IntVector2 lastPoint = linePathList [linePathList.Count - 1];

		int diffX = Mathf.Abs (lastPoint.x - _testPoint.x);
		int diffY = Mathf.Abs (lastPoint.y - _testPoint.y);

		return (diffX + diffY == 1);
	}

	public bool doMove (IntVector2 _newPoint, bool _allowUndo = true) {
		bool isTestPointNextToEnd = isPointNextToLastPoint (_newPoint);
		if (isTestPointNextToEnd) {
			bool doneMove = false;

			if (linePathList.Count >= 2) {
				IntVector2 previousPoint = linePathList [linePathList.Count - 2];

				if (_newPoint == previousPoint) {

					//Debug.Log ("Removing last point");
					if (_allowUndo) {
						removeLastPoint ();
					}

					doneMove = true;

					return false;

					//Go back one space
				}
			}

			if (!doneMove) {

				bool isValidMove = true;

				for (int i = 0; i < linePathList.Count; ++i) {
					if (i != linePathList.Count - 2) {
						if (linePathList [i] == _newPoint) {
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

	public bool goDirection (int _direction, bool _allowUndo = true) {
		return doMove (getDirectionPoint (_direction), _allowUndo);
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
		}
	}
	
	public void removeLastPoint () {
		linePathList.RemoveAt (linePathList.Count - 1);
		lineRenderer.positionCount = linePathList.Count;

		lineLastPosition = linePathList [linePathList.Count - 1];

		linePathListReverse.RemoveAt (linePathListReverse.Count - 1);
		lineRendererReverse.positionCount = linePathListReverse.Count;
	}

	public IntVector2 getReverse (IntVector2 _inPoint) {
		return lineArrayPosition - (_inPoint - lineArrayPosition);
	}

	private Vector2 convertPointToWorldSpace (IntVector2 _inPoint) {
		IntVector2 gameArea = blockPixelSize * gameArraySize;
		Vector2 position = (screenSize.toFloat() - gameArea.toFloat()) / 2.0f + blockPixelSize.toFloat() * _inPoint;
		return position;
	}

	private Vector2 getCenterOfBlockFromWorldSpace (Vector2 _inPoint) {
		return _inPoint + blockPixelSize.toFloat() / 2;
	}


	private void setupLines()
	{
		lineObject = new GameObject("LineForward");
		lineRenderer = lineObject.AddComponent<LineRenderer>();
		lineRenderer.transform.SetParent (lineGO.transform);
		lineRenderer.startWidth = blockPixelSize.x * 0.3f;
		lineRenderer.endWidth = blockPixelSize.x * 0.3f;
		lineRenderer.startColor = lineColour;
		lineRenderer.endColor = lineColour;
		lineRenderer.material = new Material (Shader.Find ("Sprites/Default"));
		//lineRenderer.material = new Material (Shader.Find ("Particles/Alpha Blended"));
		lineRenderer.useWorldSpace = true;  
		lineRenderer.material.mainTexture = texLine;
		lineRenderer.positionCount = 1;
		lineRenderer.SetPosition (0, new Vector3 (lineCenter.x, lineCenter.y, -1.0f));



		lineObjectReverse = new GameObject("LineReverse");
		lineRendererReverse = lineObjectReverse.AddComponent<LineRenderer>();
		lineRendererReverse.transform.SetParent (lineGO.transform);
		lineRendererReverse.startWidth = blockPixelSize.x * 0.3f;
		lineRendererReverse.endWidth = blockPixelSize.x * 0.3f;
		lineRendererReverse.startColor = lineColour;
		lineRendererReverse.endColor = lineColour;
		lineRendererReverse.material = new Material (Shader.Find ("Sprites/Default"));
		//lineRendererReverse.material = new Material (Shader.Find ("Particles/Alpha Blended"));
		lineRendererReverse.useWorldSpace = true;  
		lineRendererReverse.material.mainTexture = texDashed;
		//lineRendererReverse.material.mainTextureScale = new Vector2 (0.01f, 0.01f);
		//lineRendererReverse.textureMode = LineTextureMode.Tile;
		lineRendererReverse.positionCount = 1;
		lineRendererReverse.SetPosition (0, new Vector3 (lineCenter.x, lineCenter.y, -1.0f));
	}

	private void addCircle() {
		circleGO = new GameObject("circle");
		circleGO.transform.SetParent (lineGO.transform);
		SpriteRenderer sr = circleGO.AddComponent<SpriteRenderer>() as SpriteRenderer;
		sr.color = lineColour;

		//float gap = texCircle.width * 0.1f;

		Sprite mySprite = Sprite.Create( texCircle, new Rect(0.0f, 0.0f, texCircle.width, texCircle.height), new Vector2(0.0f, 0.0f), 1.0f);

		sr.sprite = mySprite;

		float scaleFactor = 0.8f;

		float imageScaleWidth = scaleFactor * blockPixelSize.x / sr.sprite.bounds.size.x;

		circleGO.transform.localScale = new Vector2 (imageScaleWidth, imageScaleWidth);

		float gapX = blockPixelSize.x * (1.0f - scaleFactor) / 2;
		float gapY = blockPixelSize.y * (1.0f - scaleFactor) / 2;

		

		circleGO.transform.position = new Vector2 (gapX + lineOrigin.x, gapY + lineOrigin.y);
	}
}

