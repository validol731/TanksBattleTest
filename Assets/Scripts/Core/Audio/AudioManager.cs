using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [Header("Sources")] 
        [SerializeField] private AudioSource musicSourceA;
        [SerializeField] private AudioSource musicSourceB;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource sfxSource;
        
        [Header("Settings")] 
        [SerializeField, Range(0f,1f)] 
        private float musicVolume = 0.5f;
        [SerializeField, Range(0f,1f)] 
        private float uiVolume = 1f;
        [SerializeField, Range(0f,1f)] 
        private float sfxVolume = 0.75f;
        
        private Coroutine _playlistRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        
        public void PlayUI(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            uiSource.PlayOneShot(clip, uiVolume);
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            StopMusic();
            if (clip == null)
            {
                return;
            }

            musicSourceA.loop = loop;
            musicSourceA.clip = clip;
            musicSourceA.Play();
        }

        public void StopMusic()
        {
            if (_playlistRoutine != null)
            {
                StopCoroutine(_playlistRoutine);
                _playlistRoutine = null;
            }
            if (musicSourceA.isPlaying)
            {
                musicSourceA.Stop();
            }
            if (musicSourceB.isPlaying)
            {
                musicSourceB.Stop();
            }
        }

        public void PlayMusicList(AudioClip[] clips, float crossFadeSeconds = 0.1f, bool loopPlaylist = true, bool shuffle = true)
        {
            StopMusic();

            if (clips == null)
            {
                return;
            }

            if (clips.Length == 0)
            {
                return;
            }

            if (clips.Length == 1)
            {
                PlayMusic(clips[0]);
                return;
            }

            _playlistRoutine = StartCoroutine(PlaylistRoutine(clips, crossFadeSeconds, loopPlaylist, shuffle));
        }

        private IEnumerator PlaylistRoutine(AudioClip[] clips, float crossFadeSeconds, bool loopPlaylist, bool shuffle)
        {
            var list = new List<AudioClip>(clips);

            if (shuffle)
            {
                Shuffle(list);
            }

            int index = 0;

            AudioSource cur = musicSourceA;
            AudioSource nxt = musicSourceB;

            cur.clip = list[index];
            cur.volume = musicVolume;
            cur.loop = false;
            cur.Play();

            while (true)
            {
                float safeFade = Mathf.Max(0.05f, crossFadeSeconds);
                float waitTime = cur.clip.length - safeFade;
                if (waitTime < 0f)
                {
                    waitTime = 0f;
                }

                yield return new WaitForSeconds(waitTime);

                index += 1;
                bool hasNext = index < list.Count;

                if (hasNext == false)
                {
                    if (loopPlaylist)
                    {
                        index = 0;
                        if (shuffle)
                        {
                            Shuffle(list);
                        }

                        hasNext = true;
                    }
                }

                if (hasNext == false)
                {
                    yield return StartCoroutine(FadeVolume(cur, 0f, safeFade));
                    cur.Stop();
                    _playlistRoutine = null;
                    yield break;
                }

                nxt.clip = list[index];
                nxt.volume = 0f;
                nxt.loop = false;
                nxt.Play();

                yield return StartCoroutine(CrossFade(cur, nxt, safeFade));
                (cur, nxt) = (nxt, cur);
            }
        }

        private IEnumerator CrossFade(AudioSource from, AudioSource to, float time)
        {
            if (time < 0.01f)
            {
                if (from != null && from.isPlaying)
                {
                    from.Stop();
                }

                if (to != null)
                {
                    to.volume = musicVolume;
                }

                yield break;
            }

            float t = 0f;
            float startFrom = from != null ? from.volume : 0f;
            float startTo = to != null ? to.volume : 0f;

            while (t < time)
            {
                t += Time.deltaTime;
                float k = t / time;

                if (to != null)
                {
                    to.volume = Mathf.Lerp(startTo, musicVolume, k);
                }

                if (from != null)
                {
                    from.volume = Mathf.Lerp(startFrom, 0f, k);
                }

                yield return null;
            }

            if (to != null)
            {
                to.volume = musicVolume;
            }

            if (from != null && from.isPlaying)
            {
                from.Stop();
            }
        }

        private IEnumerator FadeVolume(AudioSource src, float target, float time)
        {
            if (src == null)
            {
                yield break;
            }

            if (time < 0.01f)
            {
                src.volume = target;
                yield break;
            }

            float start = src.volume;
            float t = 0f;

            while (t < time)
            {
                t += Time.deltaTime;
                float k = t / time;
                src.volume = Mathf.Lerp(start, target, k);
                yield return null;
            }

            src.volume = target;
        }

        private void Shuffle(List<AudioClip> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        
    }
}