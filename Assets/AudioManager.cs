using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source --------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("---------- Audio Clip --------")]
    public AudioClip background;
    public AudioClip playerDeath;
    public AudioClip doorTouch;
    public AudioClip goblinDeath;
    public AudioClip playerAttack;
    public AudioClip footSteps;
    public AudioClip bossLaugh;
    public AudioClip Collect;
    public AudioClip Transition;
    public AudioClip playerHit;
    public AudioClip Trap;
    public AudioClip chestOpen;
    public AudioClip coinDrop;
    public AudioClip coinCollect;
    public AudioClip bossDeath;



    // Start is called before the first frame update
    void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    // Update is called once per frame
    void Update()
    {

    }
}