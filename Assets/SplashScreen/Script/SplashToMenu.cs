using UnityEngine;
using System.Collections;

public class SlashToMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
		new WaitForSeconds( 3f );
		Application.LoadLevel( Constants.Screens.MainScreen );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
