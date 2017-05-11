using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using UnityEditor;

public struct IntVector2
{
	public int x;
	public int y;

	public IntVector2 (int _x, int _y) {
		x = _x;
		y = _y;
	}

	public Vector2 toFloat() {
		return new Vector2 (x, y);
	}

	public static IntVector2 operator *(IntVector2 c1, IntVector2 c2) 
	{
		return new IntVector2(c1.x * c2.x, c1.x * c2.y);
	}

	public static Vector2 operator *(Vector2 c1, IntVector2 c2) 
	{
		return new Vector2(c1.x * c2.x, c1.y * c2.y);
	}

	public static IntVector2 operator -(IntVector2 c1, IntVector2 c2) 
	{
		return new IntVector2(c1.x - c2.x, c1.y - c2.y);
	}

	public static IntVector2 operator +(IntVector2 c1, IntVector2 c2) 
	{
		return new IntVector2(c1.x + c2.x, c1.x + c2.y);
	}

	public static Vector2 operator +(Vector2 c1, IntVector2 c2) 
	{
		return new Vector2(c1.x + c2.x, c1.x + c2.y);
	}

	public static Vector2 operator /(IntVector2 c1, float c2) 
	{
		return new Vector2 (c1.x * 1.0f / c2, c1.x * 1.0f / c2);
	}
}

public class GameLogic : MonoBehaviour {

	public int gameArrayWidth;
	public int gameArrayHeight;
	private IntVector2 gameArraySize;

	private bool[,] blockArray;
	private bool[,] usedArray;
	private int usedSpaces = 0;
	private int numBlocks = 0;
	private GameObject[,] blockArrayGO;

	private GameObject blockArrayHolder;

	public Texture2D texBlock;
	public Texture2D texClear;
	public Texture2D texCircle;
	public Texture2D texDashed;

	private IntVector2 blockPixelSize;
	//private float blockWidth = 60.0f;
	//private float blockHeight = 60.0f;

	private IntVector2 screenSize;

	//public EventSystem eventSystem;

	private Vector3 mousePosition = new Vector3 (0, 0, 0);
	private Vector3 mousePositionPrevious = new Vector3 (0, 0, 0);

	private GameObject lineObject;
	private LineRenderer line;

	private List<LineClass> linesList;

	private bool isMousePressed = false;

	private int activeLine = -1;

	//public Material mat;
	private Vector3 mousePos;

	public float percentFree = 0.1f;

	void Start () {
		
		gameArraySize.x = gameArrayWidth;
		gameArraySize.y = gameArrayHeight;

		screenSize = new IntVector2 (Screen.width, Screen.height);

		float blockWidthTemp = screenSize.x / gameArraySize.x;
		float blockHeightTemp = screenSize.y / gameArraySize.y;

		float minTemp = Mathf.Min (blockWidthTemp, blockHeightTemp);

		blockPixelSize.x = Mathf.RoundToInt (Mathf.Floor (minTemp));;
		blockPixelSize.y = Mathf.RoundToInt (Mathf.Floor (minTemp));

		blockArrayGO = new GameObject[gameArraySize.x, gameArraySize.y];

		usedArray = new bool[gameArraySize.x, gameArraySize.y];

		blockArray = new bool[gameArraySize.x, gameArraySize.y];
			
		blockArrayHolder = new GameObject("blockArrayHolder");
		blockArrayHolder.transform.SetParent (this.transform);

		linesList = new List<LineClass> ();


		do {
			LineClass tempLine = createLine ();

			if (tempLine != null) {
				linesList.Add (tempLine);
			}
			//Debug.Log( "usedSpaces: " + usedSpaces + "     " + (gameArraySize.x * gameArraySize.y * (1.0f - percentFree)));
		} while (usedSpaces < gameArraySize.x * gameArraySize.y * (1.0f - percentFree));

		//fillInBlanks ();

		resetUsedArray ();

		for (int y = 0; y < gameArraySize.y; ++y)
		{
			for (int x = 0; x < gameArraySize.x; ++x)
			{
				//addSquare (new IntVector2(x, y), !usedArray[x, y]);
				addSquare (new IntVector2(x, y), blockArray[x, y]);
			}
		}

		foreach (LineClass thisLine in linesList) {
			//thisLine.resetLine ();
		}

		//linesList.Add (new LineClass (this.transform, new IntVector2 (8, 5), gameArraySize, texCircle, texDashed, blockPixelSize, screenSize));
	}

	private void resetUsedArray() {
		for (int x = 0; x < gameArraySize.x; ++x) {
			for (int y = 0; y < gameArraySize.y; ++y) {
				usedArray [x, y] = false;
			}
		}

		usedSpaces = 0;

		foreach (LineClass thisLine in linesList) {
			usedArray [thisLine.lineArrayPosition.x, thisLine.lineArrayPosition.y] = true;
			usedSpaces++;
		}
	}

	private LineClass createLine() {
		bool isFreeSpace = false;

		IntVector2 linePosition = new IntVector2 (0, 0);

		do {
			//for (int i = 0; !isFreeSpace; ++i) {
			//for (int i = 0; i < 100 && !isFreeSpace; ++i) {
			isFreeSpace = true;

			linePosition = new IntVector2 (Random.Range (0, gameArraySize.x), Random.Range (0, gameArraySize.y));

			if (isOutsideGameGrid (linePosition)) {
				//Debug.Log ("Point is outside of game grid");
				isFreeSpace = false;
			} else {
				Debug.Log ("test line position: " + linePosition.x + ", " + linePosition.y);
				foreach (LineClass thisLine in linesList) {
					if (thisLine.hitsLine (linePosition)) {
						Debug.Log ("Point hits another line: " + linePosition.x + ", " + linePosition.y);
						isFreeSpace = false;
					}
				}
			}
		} while (!isFreeSpace);

		//if (isFreeSpace == false) {
		//	return null;
		//} else {

		LineClass tempLine = new LineClass (this.transform, linePosition, gameArraySize, texCircle,
			texDashed, blockPixelSize, screenSize);

		usedArray [linePosition.x, linePosition.y] = true;
		usedSpaces++;

		for (int i = 0; i < 40; ++i) {
			int directionValue = Random.Range (0, 4);
			IntVector2 directionPoint = tempLine.getDirectionPoint (directionValue);
			if (isValidMove (directionPoint, tempLine, true)) {
				if (tempLine.goDirection (directionValue)) {
					// fill in array of used spaces with normal and reverse
					usedArray [directionPoint.x, directionPoint.y] = true;
					usedSpaces++;

					IntVector2 directionPointReverse = tempLine.getReverse (directionPoint);
					usedArray [directionPointReverse.x, directionPointReverse.y] = true;
					usedSpaces++;
				} else {
					//Debug.Log ("Unable to do direction move here - " + i);
					//i--;
					i++;
				}
			} else {
				//Debug.Log ("Unable to move here - " + i);
				//i--;
				i++;
			}
		}

		return tempLine;
	}

	private void fillInBlanks() {
		for (int y = 0; y < gameArraySize.y; ++y) {
			for (int x = 0; x < gameArraySize.x; ++x) {


				/*
				bool isFree = true;
				foreach (LineClass thisLine in linesList) {
					if (thisLine.hitsLine (new IntVector2 (x, y))) {
						isFree = false;
					}
				}*/

				if (usedArray [x, y]) {
					blockArray [x, y] = false;
				} else {
					blockArray [x, y] = true;
					numBlocks++;
				}
			}
		}
	}

	private bool isValidMove(IntVector2 _testPoint, LineClass _lineIn, bool checkUsedArray) {
		IntVector2 testPointReverse = _lineIn.getReverse (_testPoint);

		if (isOutsideGameGrid (_testPoint)) {
			return false;
		}
		if (isOutsideGameGrid (testPointReverse)) {
			return false;
		}

		if (isSquareABlocker (_testPoint)) {
			return false;
		}
		if (isSquareABlocker (testPointReverse)) {
			return false;
		}


		if (!_lineIn.isValidMove (_testPoint)) {
			return false;
		}


		if (checkUsedArray) {
			if (usedArray [_testPoint.x, _testPoint.y]) {
				return false;
			}
			if (usedArray [testPointReverse.x, testPointReverse.y]) {
				return false;
			}
		} else {
			for (int i = 0; i < linesList.Count; ++i) {
				if (i != activeLine) {
					if (linesList [i].hitsLine (_testPoint)) {
						return false;
					}
					if (linesList [i].hitsLine (testPointReverse)) {
						return false;
					}
				}
			}
		}


		//IntVector2 reversePoint = _lineIn.getReverse (_testPoint);

		/*for (int i = 0; i < linesList.Count; ++i) {
			if (i != activeLine) {
				if (linesList [i].hitsLine (_testPoint)) {
					return false;
				}
				if (linesList [i].hitsLine (testPointReverse)) {
					return false;
				}
			}
		}*/

		return true;
	}

	private bool isOutsideGameGrid(IntVector2 _testPoint) {
		if (_testPoint.x < 0 || _testPoint.x >= gameArraySize.x ||
			_testPoint.y < 0 || _testPoint.y >= gameArraySize.y) {
			return true;
		}
		return false;
	}

	private bool isSquareABlocker(IntVector2 _testPoint) {
		return blockArray [_testPoint.x, _testPoint.y];
	}

	private bool isSquare(IntVector2 _testPoint) {
		return blockArray [_testPoint.x, _testPoint.y];
	}

	private void addSquare(IntVector2 _testPoint, bool isBlock)
	{
		blockArrayGO [_testPoint.x, _testPoint.y] = new GameObject ("block_" + _testPoint.x + "," + _testPoint.y);
		blockArrayGO [_testPoint.x, _testPoint.y].transform.SetParent (blockArrayHolder.transform);
		SpriteRenderer sr = blockArrayGO [_testPoint.x, _testPoint.y].AddComponent<SpriteRenderer> () as SpriteRenderer;

		//Sprite mySprite = Sprite.Create ((isBlock ? texBlock : texClear), new Rect (0.0f, 0.0f, texBlock.width, texBlock.height), new Vector2 (0.0f, 0.0f), 1.0f);

		sr.sprite = Sprite.Create ((isBlock ? texBlock : texClear), new Rect (0.0f, 0.0f, texBlock.width, texBlock.height), new Vector2 (0.0f, 0.0f), 1.0f);

		float imageScaleWidth = blockPixelSize.x / sr.sprite.bounds.size.x;
		blockArrayGO [_testPoint.x, _testPoint.y].transform.localScale = new Vector2 (imageScaleWidth, imageScaleWidth);
		
		IntVector2 gameArea = blockPixelSize * gameArraySize;

		Vector2 block = (screenSize.toFloat () - gameArea.toFloat ()) / 2 + blockPixelSize.toFloat () * _testPoint;

		blockArrayGO [_testPoint.x, _testPoint.y].transform.position = new Vector3 (block.x, block.y, 1.0f);
	}

	IntVector2 getBlockFromPoint (Vector2 _testPoint) {
		for (int x = 0; x < gameArraySize.x; ++x) {
			for (int y = 0; y < gameArraySize.y; ++y) {

				if (_testPoint.x >= blockArrayGO [x, y].transform.position.x &&
					_testPoint.x < blockArrayGO [x, y].transform.position.x + blockPixelSize.x &&
					_testPoint.y >= blockArrayGO [x, y].transform.position.y &&
					_testPoint.y < blockArrayGO [x, y].transform.position.y + blockPixelSize.y) {

					return new IntVector2 (x, y);
				}
			}
		}

		return new IntVector2 (-1, -1);
	}

	// Update is called once per frame
	void Update ()
	{
		mousePositionPrevious = mousePosition;
		mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (mousePosition != mousePositionPrevious) {
			doMoved (mousePosition);
		}

		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			doDown (mousePosition);
		} else if (Input.GetKeyUp (KeyCode.Mouse0)) {
			doUp (mousePosition);
		}


		for (int x = 0; x < gameArraySize.x; ++x) {
			for (int y = 0; y < gameArraySize.y; ++y) {
				SpriteRenderer sr = blockArrayGO [x, y].GetComponent<SpriteRenderer> ();

				sr.color = new Color (0.0f,
				//sr.color = new Color (usedArray[x, y] ? 1.0f : 0.0f,
					1.0f,
					1.0f);
			}
		}
		//Debug.Log ("usedSpaces: " + usedSpaces);
	}

	private void doMoved (Vector3 _movedPos) {
		//Debug.Log ("doMoved");

		if (isMousePressed && activeLine >= 0) {

			//Debug.Log ("mouse is pressed and active line is " + activeLine);

			if (linesList.Count >= activeLine + 1) {
				IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (mousePosition.x, mousePosition.y));

				bool isBlockAValidMove = isValidMove (clickedBlock, linesList [activeLine], false);

				if (isBlockAValidMove) {

					//SpriteRenderer sr = blockArrayGO [clickedBlock.x, clickedBlock.y].GetComponent<SpriteRenderer> ();

					//sr.color = new Color (Random.Range (0.0f, 1.0f),
					//	Random.Range (0.0f, 1.0f),
					//	Random.Range (0.0f, 1.0f));

					bool goneForward = linesList [activeLine].doMove (new IntVector2 (clickedBlock.x, clickedBlock.y));

					usedArray [clickedBlock.x, clickedBlock.y] = goneForward;
					usedSpaces += goneForward ? 1 : -1;

					IntVector2 clickedBlockReverse = linesList [activeLine].getReverse (clickedBlock);
					usedArray [clickedBlockReverse.x, clickedBlockReverse.y] = goneForward;
					usedSpaces += goneForward ? 1 : -1;

					Debug.Log ("usedSpaces: " + usedSpaces);
					Debug.Log ("FreeBlocks: " + (gameArraySize.x * gameArraySize.y - (numBlocks + usedSpaces)));
				}
			}
		}
	}

	private void doDown (Vector3 _downPos) {
		isMousePressed = true;

		IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (mousePosition.x, mousePosition.y));

		//Debug.Log ("clicked on: " + clickedBlock.x + ", " + clickedBlock.y);

		if (!isOutsideGameGrid (clickedBlock)) {
			activeLine = -1;

			//Debug.Log ("click is inside game grid");

			for (int i = 0; i < linesList.Count; ++i) {
				if (linesList [i].pointActivatesLine (clickedBlock)) {
					activeLine = i;

					//Debug.Log ("clicked on line: " + i);

					break;
				}
			}
		}
	}

	private void doUp (Vector3 _upPos) {
		isMousePressed = false;
		activeLine = -1;
	}


}
