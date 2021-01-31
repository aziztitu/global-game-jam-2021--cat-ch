using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Cage : MonoBehaviour
{
    public Transform cageTransform;
    public GameObject pointLight;

    public void DisablePointLight()
    {
        pointLight.SetActive(false);
    }
}
