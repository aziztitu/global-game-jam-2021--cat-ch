using BasicTools.ButtonInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnnouncerText
{
    [TextArea]
    public string text;
    public AudioClip audioClip;
}

public class AnnouncerPopups : MonoBehaviour
{
    public List<AnnouncerText> announcerTexts = new List<AnnouncerText>();
    private Randomizer<AnnouncerText> randomizer;
    
    public TMPro.TextMeshProUGUI textToAssign;
    private AudioSource audioSource;
    private Animator anim;

    [ButtonAttribute("Show Announcement", "ShowAnnouncement")] [SerializeField]
    private bool _btnShowAnnouncement;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        randomizer = new Randomizer<AnnouncerText>(announcerTexts);
    }

    public void ShowAnnouncement()
    {
        if (anim.GetBool("Announcing"))
        {
            anim.SetTrigger("MoveOn");
        }
        else
        {
            anim.SetBool("Announcing", true);
        }
    }

    public void AssignVariables()
    {
        var announcerText = randomizer.GetRandomItem();

        textToAssign.text = announcerText.text;
        audioSource.clip = announcerText.audioClip;
        audioSource.Play();
    }

    public void SetAnnouncingFalse()
    {
        anim.SetBool("Announcing", false);
    }
}