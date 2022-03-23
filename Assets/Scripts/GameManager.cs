using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    static int level = 1; //Current level
    static int score;
    public static int lives = 5;
    static int shotsFired;
    static int shotsMissed;
    static int bulletLevel = 1;

    int enemyAmount;

    int scoreToBonusLife = 30000;

    static int bonusScore;
    static bool hasLost;

    public int maxLives;

    void Awake()
    {
        instance = this; //ensure there is only one, static GameManager

        if (hasLost) //Reset on loss
        {
            level = 1;
            score = 0;
            lives = maxLives;
            bonusScore = 0;
            hasLost = false;
            bulletLevel = 1;
        }
    }

    void Start()
    {
        UiScript.instance.UpdateScoreText(score); //Set initial vals
        UiScript.instance.UpdateLivesText(lives);
        UiScript.instance.ShowStageText(level);
    }

    public void AddScore(int amount)
    {
        score += amount;

        UiScript.instance.UpdateScoreText(score); //Update UI Value

        bonusScore += amount;
        if (bonusScore>=scoreToBonusLife)
        {
            lives++;
            UiScript.instance.UpdateLivesText(lives); //Update UI Value
            bonusScore %= scoreToBonusLife;
        }
    }

    public void AddShotsFired()
    {
        shotsFired++;
    }

    public void AddShotsMissed()
    {
        shotsMissed++;
    }

    public void DecreaseLives()
    {
        lives--;

        UiScript.instance.UpdateLivesText(lives); //Update UI Value

        if (lives < 0)
        {
            //Game over
            ScoreHolder.level = level;
            ScoreHolder.score = score;
            ScoreHolder.shotsFired = shotsFired;
            ScoreHolder.shotsMissed = shotsMissed;
            hasLost = true;
            SceneManager.LoadScene("GameOver");
        }
    }

    public void WinCondition()
    {
        level++;
        if (level >= 3)//Game over, kill player, take to 'win' version of end screen
        {
            ScoreHolder.lives = lives; //Keep track of remaining lives
            lives = 0;
            DecreaseLives();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public int getBulletLevel()
    {
        return bulletLevel;
    }

    public void IncreaseBulletLevel()
    {
        if (bulletLevel == 1)
        {
            bulletLevel++;
        } 
    }

    public int GetLevel()
    {
        return level;
    }
}
