using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    [SerializeField] private CanvasGroup toFadeOut;
    [SerializeField] private float timeToFade;
    [SerializeField] private AnimationCurve fadeCurve;

    private float startTime;

    private void Start()
    {
        this.startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float progress = (Time.time - startTime) / timeToFade;
        this.toFadeOut.alpha = this.fadeCurve.Evaluate(progress);
        if (progress >= 1f)
            this.gameObject.SetActive(false);
    }
}
