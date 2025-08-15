using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public AudioSource audioSource;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
