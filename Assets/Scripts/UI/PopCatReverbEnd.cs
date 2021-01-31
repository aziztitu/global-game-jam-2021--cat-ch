using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopCatReverbEnd : MonoBehaviour
{
    public void PlayReverb()
    {
        SoundEffectsManager.Instance.Play("PopReverb");
        HUDScreenManager.Instance.showEndStats();
    }
}
