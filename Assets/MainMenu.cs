using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	[SerializeField]
	private Text message = null;

	// Use this for initialization
	/*void Start () {
		message.text = "Hello me";
	}
	*/
	public void hello() {
		message.text = "Hello";
		//Rect tempRect = new Rect (10, 10, 10, 10);
		//Color rempRed = Color.red;
		//DrawQuad (tempRect, rempRed);
		//Application.LoadLevel ("Game");
		//SceneManager.
		//Scene
		SceneManager.LoadScene ("Game");
	}

	void DrawQuad(Rect position, Color color) {
		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0,0,color);
		texture.Apply();
		GUI.skin.box.normal.background = texture;
		GUI.Box(position, GUIContent.none);
	}

	// Update is called once per frame
	/*void Update () {
		
	}*/
}
