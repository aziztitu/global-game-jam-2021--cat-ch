using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopCatReverbEnd : MonoBehaviour
{
    public float maxMusicVolume = 0;

    void Start()
    {
        maxMusicVolume = BackgroundMusic.Instance.audioSource.volume;
    }

    public void FadeOutMusic()
    {
        BackgroundMusic.Instance.audioSource.volume = 0;
        //BackgroundMusic.Instance.audioSource.DOFade(0, 0.5f);
    }

    public void FadeInMusic()
    {
        BackgroundMusic.Instance.audioSource.volume = maxMusicVolume;
        //BackgroundMusic.Instance.audioSource.DOFade(maxMusicVolume, 0.5f);
    }

    public void PlayReverb()
    {
        SoundEffectsManager.Instance.Play("PopReverb");
        HUDScreenManager.Instance.showEndStats();
    }

    public void PlayBoing()
    {
        SoundEffectsManager.Instance.Play("CatAppear");
    }
}
