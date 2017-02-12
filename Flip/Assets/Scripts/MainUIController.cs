using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIController : MonoBehaviour {

	public void NextRound_onClick () {
        GameManager.Instance.NextRound();
	}
    public void Retry_onClick()
    {
        GameManager.Instance.ResetState();
    }
    public void Quit_onClick()
    {
        //If we are running in a standalone build of the game
#if UNITY_STANDALONE
        //Quit the application
        Application.Quit();
#endif

        //If we are running in the editor
#if UNITY_EDITOR
        //Stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void PlayAgain_onClick()
    {
        GameManager.Instance.StartOver();
    }

}
