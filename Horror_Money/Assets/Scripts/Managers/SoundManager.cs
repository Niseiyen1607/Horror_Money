using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("AudioMixer Groups")]
    [SerializeField] private AudioMixerGroup footstepsMixerGroup;
    [SerializeField] private AudioMixerGroup globalSFXMixerGroup;
    [SerializeField] private AudioMixerGroup enemySFXMixerGroup;
    [SerializeField] private AudioMixerGroup jumpScareSFXMixerGroup;

    [Header("Global Audio")]
    public AudioSource globalSource;
    public AudioClip[] flashClips;
    public AudioClip[] zoomClips;
    public AudioClip playerDeath;
    public AudioClip playerDamageClip;
    public AudioClip[] pickUp;
    public AudioClip phase2Enemy;
    public AudioClip enemyFlashStun;
    public AudioClip stealthJumpscareSounds;

    [Header("Footsteps")]
    public AudioClip[] playerFootstepClips;
    public AudioClip[] enemyFootstepClips;

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float globalVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float spatialVolume = 1f;

    [Header("Pitch Variation")]
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (globalSource == null)
        {
            globalSource = gameObject.AddComponent<AudioSource>();
            globalSource.playOnAwake = false;
            globalSource.loop = false;
            globalSource.outputAudioMixerGroup = globalSFXMixerGroup;
        }
    }

    public void PlayRandomGlobalSFX(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            globalSource.pitch = Random.Range(minPitch, maxPitch);
            globalSource.PlayOneShot(clip, globalVolume);
            globalSource.pitch = 1f; 
        }
    }

    public void PlayGlobalOneShot(AudioClip clip)
    {
        if (clip == null) return;

        globalSource.pitch = Random.Range(minPitch, maxPitch);
        globalSource.PlayOneShot(clip, globalVolume);
        globalSource.pitch = 1f;
    }

    public void PlayRandomSFXAtPoint(AudioClip[] clips, Vector3 position, AudioMixerGroup mixerGroup, float maxDistance = 20f)
    {
        if (clips != null && clips.Length > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];

            GameObject tempGO = new GameObject("TempAudio");
            tempGO.transform.position = position;
            AudioSource aSource = tempGO.AddComponent<AudioSource>();
            aSource.clip = clip;
            aSource.spatialBlend = 1f;
            aSource.volume = spatialVolume;
            aSource.pitch = Random.Range(minPitch, maxPitch);
            aSource.outputAudioMixerGroup = mixerGroup;

            aSource.maxDistance = maxDistance;
            aSource.rolloffMode = AudioRolloffMode.Linear; 

            aSource.Play();

            Destroy(tempGO, clip.length / aSource.pitch);
        }
    }

    public void PlayJumpScareSFX(AudioClip clip)
    {
        if (clip != null)
        {
            GameObject tempGO = new GameObject("JumpScareAudio");
            AudioSource source = tempGO.AddComponent<AudioSource>();

            source.clip = clip;
            source.outputAudioMixerGroup = jumpScareSFXMixerGroup;
            source.volume = globalVolume;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.Play();

            Destroy(tempGO, clip.length);
        }
    }

    public void PlayPlayerFootstep(Vector3 position)
    {
        PlayRandomSFXAtPoint(playerFootstepClips, position, footstepsMixerGroup);
    }

    public void PlayEnemyFootstep(Vector3 position, float maxDistance)
    {
        PlayRandomSFXAtPoint(enemyFootstepClips, position, enemySFXMixerGroup, maxDistance);
    }

    public void PlayFlash(Vector3 position) { 
        PlayRandomSFXAtPoint(flashClips, position, globalSFXMixerGroup); 
    }

    public void PlayPhase2Enemy(Vector3 position, float maxDistance)
    {
        PlayRandomSFXAtPoint(new AudioClip[] { phase2Enemy }, position, jumpScareSFXMixerGroup, maxDistance);
    }   

    public void PlayLooping(AudioSource source, AudioClip clip, AudioMixerGroup mixerGroup)
    {
        if (source != null && clip != null && !source.isPlaying)
        {
            source.clip = clip;
            source.loop = true;
            source.spatialBlend = 1f;
            source.volume = spatialVolume;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.outputAudioMixerGroup = mixerGroup;
            source.Play();
        }
    }

    public void StopLooping(AudioSource source)
    {
        if (source != null && source.isPlaying)
            source.Stop();
    }
}
