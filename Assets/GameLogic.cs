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

	//private GameObject[,] blockArrayGO;

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



		blockArrayHolder = new GameObject ("blockArrayHolder");
		blockArrayHolder.transform.SetParent (this.transform);

		gameCreator = new GameCreator ();

		gameCreator.createGame (this.transform, blockArrayHolder.transform, gameArraySize, texCircle, texLine, texDashed, texBlock, texClear, blockPixelSize, screenSize, percentFree);
	}

	private bool isOutsideGameGrid(IntVector2 _testPoint) {
		if (_testPoint.x < 0 || _testPoint.x >= gameArraySize.x ||
			_testPoint.y < 0 || _testPoint.y >= gameArraySize.y) {
			return true;
		}
		return false;
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
			gameCreator.doMoved (_movedPos);
		}
	}

	private void doDown (Vector3 _downPos) {
		isMousePressed = true;

		gameCreator.doDown (_downPos);
	}

	private void doUp (Vector3 _upPos) {
		isMousePressed = false;

		gameCreator.doUp (_upPos);
	}

}
