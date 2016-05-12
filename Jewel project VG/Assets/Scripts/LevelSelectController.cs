using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelectController : MonoBehaviour {


	public void loadMainGame(int level)
    {
        LevelsManager.Instance.selectedLevel = level;
        SceneManager.LoadScene("Main Game Scene");
    }
}
