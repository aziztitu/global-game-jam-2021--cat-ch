using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class AnnouncerText
{
    public string text;
    public AudioClip audioClip;
}

public class AnnouncerPopups : MonoBehaviour
{
    public List<AnnouncerText> announcerTexts = new List<AnnouncerText>();
    
    public TMPro.TextMeshProUGUI textToAssign;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ShowAnnouncement()
    {
        int randNumb = Random.Range(0, announcerTexts.Count);

        textToAssign.text = announcerTexts[randNumb].text;
    }
}
