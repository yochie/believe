using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scorer : MonoBehaviour
{
    public static Scorer Instance { get; private set; }

    [SerializeField] TextMeshProUGUI scoreIndicator;

    private int score;

    private void Awake()
    {
        Scorer.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.score = 0;
        this.UpdateDisplayedScore();
    }

    public void AddToScore(int toAdd)
    {
        this.score = Mathf.Clamp(score + toAdd, 0, System.Int32.MaxValue);
        this.UpdateDisplayedScore();
    }

    private void UpdateDisplayedScore()
    {
        this.scoreIndicator.text = string.Format("Score : {0}", this.score);
    }

}
