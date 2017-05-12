using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCreator {

	private int usedSpaces = 0;
	public int numBlocks = 0;

	private Transform parent;
	private IntVector2 gameArraySize;

	private List<LineClass> linesList;

	public bool[,] blockArray;
	public bool[,] usedArray;



	float percentFree = 0.1f;

	private Texture2D texCircle;					// Texture for line origin circle
	private Texture2D texDashed;					// Texture for dashes reverse line

	private IntVector2 blockPixelSize;

	private IntVector2 screenSize;

	public List<LineClass> createGame(Transform _parent,
		IntVector2 _gameArraySize,
		Texture2D _texCircle,
		Texture2D _texDashed,
		IntVector2 _blockPixelSize,
		IntVector2 _screenSize) {

		parent = _parent;
		gameArraySize = _gameArraySize;
		texCircle = _texCircle;
		texDashed = _texDashed;
		blockPixelSize = _blockPixelSize;
		screenSize = _screenSize;

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
			//thisLine.resetLine ();
		}

		return linesList;
	}

	private LineClass createLine() {
		bool isFreeSpace = false;
		IntVector2 linePosition = new IntVector2 (0, 0);

		do {
			isFreeSpace = true;

			linePosition = new IntVector2 (Random.Range (0, gameArraySize.x), Random.Range (0, gameArraySize.y));

			Debug.Log ("test line position: " + linePosition.x + ", " + linePosition.y);
			foreach (LineClass thisLine in linesList) {
				if (thisLine.hitsLine (linePosition)) {
					Debug.Log ("Point hits another line: " + linePosition.x + ", " + linePosition.y);
					isFreeSpace = false;
				}
			}
		} while (!isFreeSpace);

		LineClass tempLine = new LineClass (parent, linePosition, gameArraySize, texCircle,
			texDashed, blockPixelSize, screenSize);

		usedArray [linePosition.x, linePosition.y] = true;
		usedSpaces++;

		for (int i = 0; i < 40; ++i) {
			int directionValue = Random.Range (0, 4);
			IntVector2 directionPoint = tempLine.getDirectionPoint (directionValue);
			if (this.isValidMove (directionPoint, tempLine)) {
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

	private bool isValidMove(IntVector2 _testPoint, LineClass _lineIn) {
		IntVector2 testPointReverse = _lineIn.getReverse (_testPoint);

		if (isOutsideGameGrid (_testPoint)) {
			return false;
		}
		if (isOutsideGameGrid (testPointReverse)) {
			return false;
		}

		/*if (isSquareABlocker (_testPoint)) {
			return false;
		}
		if (isSquareABlocker (testPointReverse)) {
			return false;
		}*/


		if (!_lineIn.isValidMove (_testPoint)) {
			return false;
		}

		if (usedArray [_testPoint.x, _testPoint.y]) {
			return false;
		}
		if (usedArray [testPointReverse.x, testPointReverse.y]) {
			return false;
		}

		return true;
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

	private bool isOutsideGameGrid(IntVector2 _testPoint) {
		if (_testPoint.x < 0 || _testPoint.x >= gameArraySize.x ||
			_testPoint.y < 0 || _testPoint.y >= gameArraySize.y) {
			return true;
		}
		return false;
	}
}
