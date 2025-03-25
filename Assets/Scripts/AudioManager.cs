using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public UIManager uiManager;
    public AudioClip backgroundMusic;
    public AudioClip winMusic;
    public AudioClip winSound;
    public AudioSource audioSource;
    public AudioSource SFXSource;
    
    bool playedWinSound = false;
    // Start is called before the first frame update
    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (uiManager.progressBar.IsComplete())
        {
            
            
            if(!playedWinSound)
            {
            
                audioSource.Stop();
                SFXSource.PlayOneShot(winSound);
                audioSource.clip = winMusic;
                audioSource.Play();
                playedWinSound = true;
            }
            
            
        }
        
        
    }
    
    public void ResetAudio()
    {
        audioSource.Stop();
        audioSource.clip = backgroundMusic;
        audioSource.Play();
        playedWinSound = false;
    }
}
