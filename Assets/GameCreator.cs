using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCreator {

	private int usedSpaces = 0;
	private int numBlocks = 0;

	private Transform parent;
	private IntVector2 gameArraySize;

	private Transform gameHolder;

	private List<LineClass> linesList;

	public bool[,] blockArray;
	private bool[,] usedArray;

	private int activeLine = -1;

	private float percentFree = 0.1f;

	private Texture2D texCircle;					// Texture for line origin circle
	private Texture2D texLine;						// Texture for forward line
	private Texture2D texDashed;					// Texture for dashes reverse line

	private Texture2D texBlock;
	private Texture2D texClear;

	private IntVector2 blockPixelSize;

	private IntVector2 screenSize;

	private GameObject[,] blockArrayGO;

	public void createGame(Transform _parent,
		Transform _gameHolder,
		IntVector2 _gameArraySize,
		Texture2D _texCircle,
		Texture2D _texLine,
		Texture2D _texDashed,
		Texture2D _texBlock,
		Texture2D _texClear,
		IntVector2 _blockPixelSize,
		IntVector2 _screenSize,
		float _percentFree) {

		parent = _parent;
		gameHolder = _gameHolder;
		gameArraySize = _gameArraySize;
		texCircle = _texCircle;
		texLine = _texLine;
		texDashed = _texDashed;
		texBlock = _texBlock;
		texClear = _texClear;
		blockPixelSize = _blockPixelSize;
		screenSize = _screenSize;
		percentFree = _percentFree;

		blockArray = new bool[gameArraySize.x, gameArraySize.y];
		usedArray = new bool[gameArraySize.x, gameArraySize.y];
		usedSpaces = 0;

		linesList = new List<LineClass> ();

		do {
			LineClass tempLine = createLine ();

			if (tempLine != null) {
				linesList.Add (tempLine);
			}
		} while (usedSpaces < gameArraySize.x * gameArraySize.y * (1.0f - percentFree));

		fillInBlanks ();

		foreach (LineClass thisLine in linesList) {
			thisLine.resetLine ();
		}

		resetUsedArray ();


		blockArrayGO = new GameObject[gameArraySize.x, gameArraySize.y];

		for (int y = 0; y < gameArraySize.y; ++y) {
			for (int x = 0; x < gameArraySize.x; ++x) {
				addSquare (new IntVector2 (x, y), blockArray [x, y]);
			}
		}
	}

	private void addSquare(IntVector2 _testPoint, bool isBlock) {
		blockArrayGO [_testPoint.x, _testPoint.y] = new GameObject ("block_" + _testPoint.x + "," + _testPoint.y);
		blockArrayGO [_testPoint.x, _testPoint.y].transform.SetParent (gameHolder);
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

	private LineClass createLine() {
		bool isFreeSpace = false;
		IntVector2 linePosition = new IntVector2 (0, 0);

		do {
			isFreeSpace = true;

			linePosition = new IntVector2 (Random.Range (0, gameArraySize.x), Random.Range (0, gameArraySize.y));

			//Debug.Log ("test line position: " + linePosition.x + ", " + linePosition.y);
			foreach (LineClass thisLine in linesList) {
				if (thisLine.hitsLine (linePosition)) {
					//Debug.Log ("Point hits another line: " + linePosition.x + ", " + linePosition.y);
					isFreeSpace = false;
				}
			}
		} while (!isFreeSpace);

		LineClass tempLine = new LineClass (parent, linePosition, gameArraySize, texCircle, texLine,
			texDashed, blockPixelSize, screenSize);

		usedArray [linePosition.x, linePosition.y] = true;
		usedSpaces++;

		for (int i = 0; i < 40; ++i) {
			int directionValue = Random.Range (0, 4);
			IntVector2 directionPoint = tempLine.getDirectionPoint (directionValue);
			if (this.isValidMove (directionPoint, tempLine, true)) {
				if (tempLine.goDirection (directionValue, false)) {
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
		return true;
	}

	private bool isSquareABlocker(IntVector2 _testPoint) {
		return blockArray [_testPoint.x, _testPoint.y];
	}

	private void fillInBlanks() {
		for (int y = 0; y < gameArraySize.y; ++y) {
			for (int x = 0; x < gameArraySize.x; ++x) {
				if (usedArray [x, y]) {
					blockArray [x, y] = false;
				} else {
					blockArray [x, y] = true;
					numBlocks++;
				}
			}
		}
	}

	private void resetUsedArray() {
		usedSpaces = 0;

		foreach (LineClass thisLine in linesList) {
			IntVector2 lineOriginBlock = thisLine.getOriginBlock ();
			usedArray [lineOriginBlock.x, lineOriginBlock.y] = true;
			usedSpaces++;
		}
	}

	private bool isOutsideGameGrid(IntVector2 _testPoint) {
		if (_testPoint.x < 0 || _testPoint.x >= gameArraySize.x ||
			_testPoint.y < 0 || _testPoint.y >= gameArraySize.y) {
			return true;
		}
		return false;
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

	public void doDown (Vector2 _mousePos) {

		IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (_mousePos.x, _mousePos.y));

		if (!isOutsideGameGrid (clickedBlock)) {

			activeLine = -1;
			//Debug.Log ("inside game grid: " + linesList.Count);

			for (int i = 0; i < linesList.Count; ++i) {
				//Debug.Log ("checking line: " + i);
				if (linesList [i].pointActivatesLine (clickedBlock)) {
					//Debug.Log ("activates line: " + i);
					activeLine = i;
					break;
				}
			}
		}
	}

	public void doMoved (Vector2 _mousePos) {

		IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (_mousePos.x, _mousePos.y));

		if (!isOutsideGameGrid (clickedBlock)) {
			if (activeLine >= 0) {
				if (linesList.Count >= activeLine + 1) {
					bool isBlockAValidMove = isValidMove (clickedBlock, linesList [activeLine], false);
					//Debug.Log (isBlockAValidMove);
					if (isBlockAValidMove) {
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
	}

	public void doUp (Vector2 _mousePos) {
		IntVector2 clickedBlock = getBlockFromPoint (new Vector2 (_mousePos.x, _mousePos.y));

		activeLine = -1;
	}
}
