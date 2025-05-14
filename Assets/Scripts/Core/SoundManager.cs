using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }
    private AudioSource soundSource;
    private AudioSource musicSource;

    //added
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip level1Music;
    [SerializeField] private AudioClip level2Music;
    [SerializeField] private AudioClip creditsMusic;

    private void Awake()
    {
        soundSource = GetComponent<AudioSource>();
        musicSource = transform.GetChild(0).GetComponent<AudioSource>();

        //keep this object even when we go to new scene;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        //destroy duplicate gameobjects
        else if (instance != null && instance != this)
            Destroy(gameObject);

        //assign initial volumes
        ChangeMusicVolume(0);
        ChangeSoundVolume(0);
    }

    //added ----------------------------------------------------------

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "_MainMenu":
                PlayMusic(mainMenuMusic);
                break;
            case "Level1":
                PlayMusic(level1Music);
                break;
            case "Level2":
                PlayMusic(level2Music);
                break;
            case "Credits":
                PlayMusic(creditsMusic);
                break;
            // place cases for other levels here
            default:
                if (musicSource.isPlaying)
                    musicSource.Stop();
                break;
        }
    }

    public void PlayMusic(AudioClip music)
    {
        //if (musicSource.clip == music) return;

        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }

    //added ----------------------------------------------------------

    //this is used to link and play SFX sound
    public void PlaySound(AudioClip _sound)
    {
        soundSource.PlayOneShot(_sound);
    }

    public void ChangeSoundVolume(float _change)
    {
        //base volume = 1
        ChangeSourceVolume(1, "soundVolume", _change, soundSource);
    }

    public void ChangeMusicVolume(float _change)
    {
        //base volume = 0.5f
        ChangeSourceVolume(0.5f, "musicVolume", _change, musicSource);
    }

    public void ChangeSourceVolume(float baseVolume, string volumeName, float change, AudioSource source)
    {
        //get initial value of volume and change it
        float currentVolume = PlayerPrefs.GetFloat(volumeName, 1);
        currentVolume += change;

        //check if we reached the min/max value
        if(currentVolume > 1)
            currentVolume = 0;
        else if(currentVolume < 0)
            currentVolume = 1;

        //assign final value
        float finalVolume = currentVolume * baseVolume;
        source.volume = finalVolume;

        //save final value to player prefs
        PlayerPrefs.SetFloat(volumeName, currentVolume);
    }
}
