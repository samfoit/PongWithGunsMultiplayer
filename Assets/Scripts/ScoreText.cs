using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    public Text text;
    public bool firstPlayerScore;

    private void Update()
    {
        if (firstPlayerScore)
        {
            text.text = GameManager.instance.p1Score.Value.ToString();
        }
        else
        {
            text.text = GameManager.instance.p2Score.Value.ToString();
        }
    }
}
