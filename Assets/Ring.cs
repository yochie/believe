using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    //used to have neutral transform as starting point since own transform is altered to have different default scale/orientation
    public Transform parentTransform;
    public GameObject arrow;
    public float TimeOutDuration = 6f;
    public float minScale = 1f;
    public float maxScale = 3f;

    public float ConsumptionAnimationDuration = 0.75f;

    public AudioClip ConsumedAudio;
    [Range(0,1)]
    public float ConsumedAudioVolume;
    public AudioClip timeoutAudio;
    [Range(0,1)]
    public float TimeOutAudioVolume;
    private Coroutine _timeoutCoroutine;


    public void Consume()
    {
        Scorer.Instance.AddToScore(1);
        if(_timeoutCoroutine != null)
            StopCoroutine(_timeoutCoroutine);
        RingSpawner.Instance.SpawnNextRing(this);
        Destroy(this.arrow);
        AudioSource.PlayClipAtPoint(this.ConsumedAudio, this.parentTransform.position, ConsumedAudioVolume);
        StartCoroutine(this.ConsumeAnimationCoroutine());

    }

    public void StartTimeout()
    {
        _timeoutCoroutine = StartCoroutine(this.TimeOutCoroutine());

    }

    private IEnumerator TimeOutCoroutine()
    {
        float ellapsedTime = 0;
        while(ellapsedTime < this.TimeOutDuration)
        {
            float scale = Mathf.Lerp(this.minScale, this.maxScale, (ellapsedTime / this.TimeOutDuration));
            this.parentTransform.localScale = Vector3.one * scale; 
            ellapsedTime += Time.deltaTime;
            yield return null;
        }

        AudioSource.PlayClipAtPoint(this.timeoutAudio, this.parentTransform.position, TimeOutAudioVolume);
        Scorer.Instance.AddToScore(-1);
        this.parentTransform.gameObject.SetActive(false);
        RingSpawner.Instance.SpawnNewStartRing();
        Destroy(this.parentTransform.gameObject);
        
    }

    private IEnumerator ConsumeAnimationCoroutine()
    {

        float ellapsedTime = 0;
        float startScale = this.parentTransform.localScale.x;
        while (ellapsedTime < this.ConsumptionAnimationDuration)
        {
            this.parentTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 0.1f, (ellapsedTime / this.ConsumptionAnimationDuration));
            ellapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(this.parentTransform.gameObject);
    }
}
