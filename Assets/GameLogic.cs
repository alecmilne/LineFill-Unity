using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using UnityEditor;

public class GameLogic : MonoBehaviour {

	public int gameArrayWidth;
	public int gameArrayHeight;
	private IntVector2 gameArraySize;

	private GameObject[,] blockArrayGO;

	private GameObject blockArrayHolder;

	public Texture2D texBlock;
	public Texture2D texClear;
	public Texture2D texCircle;
	public Texture2D texLine;
	public Texture2D texDashed;

	private IntVector2 blockPixelSize;
	private IntVector2 screenSize;

	private Vector3 mousePosition = new Vector3 (0, 0, 0);
	private Vector3 mousePositionPrevious = new Vector3 (0, 0, 0);

	private GameObject lineObject;
	private LineRenderer line;

	private bool isMousePressed = false;

	private Vector3 mousePos;

	public float percentFree = 0.1f;

	private GameCreator gameCreator;

	void Start () {
		
		gameArraySize.x = gameArrayWidth;
		gameArraySize.y = gameArrayHeight;

		screenSize = new IntVector2 (Screen.width, Screen.height);

		float blockWidthTemp = screenSize.x / gameArraySize.x;
		float blockHeightTemp = screenSize.y / gameArraySize.y;

		float minTemp = Mathf.Min (blockWidthTemp, blockHeightTemp);

		blockPixelSize.x = Mathf.RoundToInt (Mathf.Floor (minTemp));
		blockPixelSize.y = Mathf.RoundToInt (Mathf.Floor (minTemp));

		blockArrayGO = new GameObject[gameArraySize.x, gameArraySize.y];

		blockArrayHolder = new GameObject ("blockArrayHolder");
		blockArrayHolder.transform.SetParent (this.transform);

		gameCreator = new GameCreator ();

		gameCreator.createGame (this.transform, gameArraySize, texCircle, texLine, texDashed, blockPixelSize, screenSize, percentFree);

		for (int y = 0; y < gameArraySize.y; ++y) {
			for (int x = 0; x < gameArraySize.x; ++x) {
				addSquare (new IntVector2 (x, y), gameCreator.blockArray [x, y]);
			}
		}
	}

	private bool isOutsideGameGrid(IntVector2 _testPoint) {
		if (_testPoint.x < 0 || _testPoint.x >= gameArraySize.x ||
			_testPoint.y < 0 || _testPoint.y >= gameArraySize.y) {
			return true;
		}
		return false;
	}

	private void addSquare(IntVector2 _testPoint, bool isBlock) {
		blockArrayGO [_testPoint.x, _testPoint.y] = new GameObject ("block_" + _testPoint.x + "," + _testPoint.y);
		blockArrayGO [_testPoint.x, _testPoint.y].transform.SetParent (blockArrayHolder.transform);
		SpriteRenderer sr = blockArrayGO [_testPoint.x, _testPoint.y].AddComponent<SpriteRenderer> () as SpriteRenderer;

		sr.sprite = Sprite.Create ((isBlock ? texBlock : texClear),
			new Rect (0.0f, 0.0f, texBlock.width, texBlock.height),
			new Vector2 (0.0f, 0.0f),
			1.0f);

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
		//SpriteRenderer sr = blockArrayGO [clickedBlock.x, clickedBlock.y].GetComponent<SpriteRenderer> ();

		//sr.color = new Color (Random.Range (0.0f, 1.0f),
		//	Random.Range (0.0f, 1.0f),
		//	Random.Range (0.0f, 1.0f));

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
	}

	private void doMoved (Vector3 _movedPos) {
		if (isMousePressed) {
			IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (_movedPos.x, _movedPos.y));

			if (!isOutsideGameGrid (clickedBlock)) {
				gameCreator.doMoved (clickedBlock);
			}
		}
	}

	private void doDown (Vector3 _downPos) {
		isMousePressed = true;

		IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (mousePosition.x, mousePosition.y));

		if (!isOutsideGameGrid (clickedBlock)) {

			gameCreator.doDown (clickedBlock);
		}
	}

	private void doUp (Vector3 _upPos) {
		isMousePressed = false;

		IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (mousePosition.x, mousePosition.y));

		gameCreator.doUp (clickedBlock);
	}

}
