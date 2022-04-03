using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Text winLoseText, highscoreText, /*highestlevelText,*/shotsFiredText, shotsHitText, shotAccText;
    
    void Start()
    {
        if (ScoreHolder.lives > 0)
        {
            winLoseText.text = "You Win!";
        }
        else
        {
            winLoseText.text = "You Lose!";
        }

        
        highscoreText.text = "Highscore: " + ScoreHolder.score;
        //highestlevelText.text = "Highest Stage: " + ScoreHolder.level;
        shotsFiredText.text = "Shots Fired: " + ScoreHolder.shotsFired;
        //Debug.Log("Shots fired = " + ScoreHolder.shotsFired);
        //Debug.Log("Shots missed = " + ScoreHolder.shotsMissed);
        //Debug.Log("Shots hit = " + (ScoreHolder.shotsFired - ScoreHolder.shotsMissed));
        shotsHitText.text = "Number of Hits: " + (ScoreHolder.shotsFired - ScoreHolder.shotsMissed);
        try
        {
            shotAccText.text = "Hit-Miss Ratio: " + (((ScoreHolder.shotsFired - ScoreHolder.shotsMissed) / ScoreHolder.shotsFired) * 100).ToString("F1") + "%";
        }
        catch { }
    }

     void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
