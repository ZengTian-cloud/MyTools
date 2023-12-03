using Basics;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ƶ����
/// </summary>
[DisallowMultipleComponent]
public sealed class AudioManager : Singleton<AudioManager>
{
    /// <summary>
    /// �Ƿ�����ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public bool MuteDefault = false;
    /// <summary>
    /// �����������ȼ���ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public int BackgroundPriorityDefault = 0;
    /// <summary>
    /// ��ͨ����Ч���ȼ���ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public int SinglePriorityDefault = 10;
    /// <summary>
    /// ��ͨ����Ч���ȼ���ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public int MultiplePriorityDefault = 20;
    /// <summary>
    /// ������Ч���ȼ���ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public int WorldPriorityDefault = 30;
    /// <summary>
    /// ��������������ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public float BackgroundVolumeDefault = 0.6f;
    /// <summary>
    /// ��ͨ����Ч������ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public float SingleVolumeDefault = 1;
    /// <summary>
    /// ��ͨ����Ч������ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public float MultipleVolumeDefault = 1;
    /// <summary>
    /// ������Ч������ʼֵ�������ڴ������޸ġ�
    /// </summary>
    public float WorldVolumeDefault = 1;
    /// <summary>
    /// ��ͨ����Ч���Ž����¼�
    /// </summary>
    public event Action SingleSoundEndOfPlayEvent;

    private AudioSource _backgroundAudio;
    private AudioSource _singleAudio;
    private List<AudioSource> _multipleAudios = new List<AudioSource>();
    private Dictionary<GameObject, AudioSource> _worldAudios = new Dictionary<GameObject, AudioSource>();
    private bool _singleSoundPlayDetector = false;
    private bool _isMute = false;
    private int _backgroundPriority = 0;
    private int _singlePriority = 10;
    private int _multiplePriority = 20;
    private int _worldPriority = 30;
    private float _backgroundVolume = 0.6f;
    private float _singleVolume = 1;
    private float _multipleVolume = 1;
    private float _worldVolume = 1;

    public void Awake()
    {
        _backgroundAudio = CreateAudioSource("BackgroundAudio", BackgroundPriorityDefault, BackgroundVolumeDefault, 1, 0);

        _singleAudio = CreateAudioSource("SingleAudio", SinglePriorityDefault, SingleVolumeDefault, 1, 0);

        Mute = MuteDefault;
        BackgroundPriority = BackgroundPriorityDefault;
        SinglePriority = SinglePriorityDefault;
        MultiplePriority = MultiplePriorityDefault;
        WorldPriority = WorldPriorityDefault;
        BackgroundVolume = BackgroundVolumeDefault;
        SingleVolume = SingleVolumeDefault;
        MultipleVolume = MultipleVolumeDefault;
        WorldVolume = WorldVolumeDefault;
    }

    public void Start()
    {
        StopBackgroundMusic();
        StopSingleSound();
        StopAllMultipleSound();
        StopAllWorldSound();
    }

    public void Update()
    {
        if (_singleSoundPlayDetector)
        {
            if (!_singleAudio.isPlaying)
            {
                _singleSoundPlayDetector = false;

                SingleSoundEndOfPlayEvent?.Invoke();
            }
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    public bool Mute
    {
        get
        {
            return _isMute;
        }
        set
        {
            if (_isMute != value)
            {
                _isMute = value;
                _backgroundAudio.mute = _isMute;
                _singleAudio.mute = _isMute;
                for (int i = 0; i < _multipleAudios.Count; i++)
                {
                    _multipleAudios[i].mute = _isMute;
                }
                foreach (var audio in _worldAudios)
                {
                    audio.Value.mute = _isMute;
                }
            }
        }
    }
    /// <summary>
    /// �����������ȼ�
    /// </summary>
    public int BackgroundPriority
    {
        get
        {
            return _backgroundPriority;
        }
        set
        {
            if (_backgroundPriority != value)
            {
                _backgroundPriority = value;
                _backgroundAudio.priority = _backgroundPriority;
            }
        }
    }
    /// <summary>
    /// ��ͨ����Ч���ȼ�
    /// </summary>
    public int SinglePriority
    {
        get
        {
            return _singlePriority;
        }
        set
        {
            if (_singlePriority != value)
            {
                _singlePriority = value;
                _singleAudio.priority = _singlePriority;
            }
        }
    }
    /// <summary>
    /// ��ͨ����Ч���ȼ�
    /// </summary>
    public int MultiplePriority
    {
        get
        {
            return _multiplePriority;
        }
        set
        {
            if (_multiplePriority != value)
            {
                _multiplePriority = value;
                for (int i = 0; i < _multipleAudios.Count; i++)
                {
                    _multipleAudios[i].priority = _multiplePriority;
                }
            }
        }
    }
    /// <summary>
    /// ������Ч���ȼ�
    /// </summary>
    public int WorldPriority
    {
        get
        {
            return _worldPriority;
        }
        set
        {
            if (_worldPriority != value)
            {
                _worldPriority = value;
                foreach (var audio in _worldAudios)
                {
                    audio.Value.priority = _worldPriority;
                }
            }
        }
    }
    /// <summary>
    /// ������������
    /// </summary>
    public float BackgroundVolume
    {
        get
        {
            return _backgroundVolume;
        }
        set
        {
            if (!Mathf.Approximately(_backgroundVolume, value))
            {
                _backgroundVolume = value;
                _backgroundAudio.volume = _backgroundVolume;
            }
        }
    }
    /// <summary>
    /// ��ͨ����Ч����
    /// </summary>
    public float SingleVolume
    {
        get
        {
            return _singleVolume;
        }
        set
        {
            if (!Mathf.Approximately(_singleVolume, value))
            {
                _singleVolume = value;
                _singleAudio.volume = _singleVolume;
            }
        }
    }
    /// <summary>
    /// ��ͨ����Ч����
    /// </summary>
    public float MultipleVolume
    {
        get
        {
            return _multipleVolume;
        }
        set
        {

            if (!Mathf.Approximately(_multipleVolume, value))
            {
                _multipleVolume = value;
                for (int i = 0; i < _multipleAudios.Count; i++)
                {
                    _multipleAudios[i].volume = _multipleVolume;
                }
            }
        }
    }
    /// <summary>
    /// ������Ч����
    /// </summary>
    public float WorldVolume
    {
        get
        {
            return _worldVolume;
        }
        set
        {
            if (!Mathf.Approximately(_worldVolume, value))
            {
                _worldVolume = value;
                foreach (var audio in _worldAudios)
                {
                    audio.Value.volume = _worldVolume;
                }
            }
        }
    }

    /// <summary>
    /// ���ű�������
    /// </summary>
    /// <param name="clip">���ּ���</param>
    /// <param name="isLoop">�Ƿ�ѭ��</param>
    /// <param name="speed">�����ٶ�</param>
    private void PlayBackgroundMusic(AudioClip clip, bool isLoop = true, float speed = 1)
    {
        if (_backgroundAudio.isPlaying)
        {
            _backgroundAudio.Stop();
        }

        _backgroundAudio.clip = clip;
        _backgroundAudio.loop = isLoop;
        _backgroundAudio.pitch = speed;
        _backgroundAudio.Play();
    }

    public void PlayBackgroundMusic(string audioPath, bool isLoop = true, float speed = 1)
    {
        string path = $"Assets/allres/audio/{audioPath}";

        ResourcesManager.Instance.LoadAssetAsync<AudioClip>(path, (audio) =>
        {
            PlayBackgroundMusic(audio);

            Debug.Log("play audio " + audio.name);

        });
    }

    /// <summary>
    /// ��ͣ���ű�������
    /// </summary>
    /// <param name="isGradual">�Ƿ񽥽�ʽ</param>
    public void PauseBackgroundMusic(bool isGradual = true)
    {
        if (isGradual)
        {
            _backgroundAudio.DOFade(0, 2).OnComplete(() =>
            {
                _backgroundAudio.volume = BackgroundVolume;
                _backgroundAudio.Pause();
            });
        }
        else
        {
            _backgroundAudio.Pause();
        }
    }
    /// <summary>
    /// �ָ����ű�������
    /// </summary>
    /// <param name="isGradual">�Ƿ񽥽�ʽ</param>
    public void UnPauseBackgroundMusic(bool isGradual = true)
    {
        if (isGradual)
        {
            _backgroundAudio.UnPause();
            _backgroundAudio.volume = 0;
            _backgroundAudio.DOFade(BackgroundVolume, 2);
        }
        else
        {
            _backgroundAudio.UnPause();
        }
    }
    /// <summary>
    /// ֹͣ���ű�������
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (_backgroundAudio.isPlaying)
        {
            _backgroundAudio.Stop();
        }
    }

    /// <summary>
    /// ���ŵ�ͨ����Ч
    /// </summary>
    /// <param name="clip">���ּ���</param>
    /// <param name="isLoop">�Ƿ�ѭ��</param>
    /// <param name="speed">�����ٶ�</param>
    private void PlaySingleSound(AudioClip clip, bool isLoop = false, float speed = 1)
    {
        if (_singleAudio.isPlaying)
        {
            _singleAudio.Stop();
        }

        _singleAudio.clip = clip;
        _singleAudio.loop = isLoop;
        _singleAudio.pitch = speed;
        _singleAudio.Play();
        _singleSoundPlayDetector = true;
    }

    public void PlaySingleSound(string audioPath)
    {
        string path = $"Assets/allres/audio/{audioPath}.wav";

        ResourcesManager.Instance.LoadAssetAsync<AudioClip>(path, (audio) =>
        {
            PlaySingleSound(audio);
        });
    }


    /// <summary>
    /// ��ͣ���ŵ�ͨ����Ч
    /// </summary>
    /// <param name="isGradual">�Ƿ񽥽�ʽ</param>
    public void PauseSingleSound(bool isGradual = true)
    {
        if (isGradual)
        {
            _singleAudio.DOFade(0, 2).OnComplete(() =>
            {
                _singleAudio.volume = SingleVolume;
                _singleAudio.Pause();
            });
        }
        else
        {
            _singleAudio.Pause();
        }
    }
    /// <summary>
    /// �ָ����ŵ�ͨ����Ч
    /// </summary>
    /// <param name="isGradual">�Ƿ񽥽�ʽ</param>
    public void UnPauseSingleSound(bool isGradual = true)
    {
        if (isGradual)
        {
            _singleAudio.UnPause();
            _singleAudio.volume = 0;
            _singleAudio.DOFade(SingleVolume, 2);
        }
        else
        {
            _singleAudio.UnPause();
        }
    }
    /// <summary>
    /// ֹͣ���ŵ�ͨ����Ч
    /// </summary>
    public void StopSingleSound()
    {
        if (_singleAudio.isPlaying)
        {
            _singleAudio.Stop();
        }
    }

    /// <summary>
    /// ���Ŷ�ͨ����Ч
    /// </summary>
    /// <param name="clip">���ּ���</param>
    /// <param name="isLoop">�Ƿ�ѭ��</param>
    /// <param name="speed">�����ٶ�</param>
    public void PlayMultipleSound(AudioClip clip, bool isLoop = false, float speed = 1)
    {
        AudioSource audio = ExtractIdleMultipleAudioSource();
        audio.clip = clip;
        audio.loop = isLoop;
        audio.pitch = speed;
        audio.Play();
    }

    public void PlayMultipleSound(string audioPath)
    {
        string path = $"Assets/allres/audio/{audioPath}";

        ResourcesManager.Instance.LoadAssetAsync<AudioClip>(path, (audio) =>
        {
            PlayMultipleSound(audio);
        });
    }

    /// <summary>
    /// ֹͣ����ָ���Ķ�ͨ����Ч
    /// </summary>
    /// <param name="clip">���ּ���</param>
    public void StopMultipleSound(AudioClip clip)
    {
        for (int i = 0; i < _multipleAudios.Count; i++)
        {
            if (_multipleAudios[i].isPlaying)
            {
                if (_multipleAudios[i].clip == clip)
                {
                    _multipleAudios[i].Stop();
                }
            }
        }
    }
    /// <summary>
    /// ֹͣ�������ж�ͨ����Ч
    /// </summary>
    public void StopAllMultipleSound()
    {
        for (int i = 0; i < _multipleAudios.Count; i++)
        {
            if (_multipleAudios[i].isPlaying)
            {
                _multipleAudios[i].Stop();
            }
        }
    }
    /// <summary>
    /// �������������еĶ�ͨ����Ч����Դ
    /// </summary>
    public void ClearIdleMultipleAudioSource()
    {
        for (int i = 0; i < _multipleAudios.Count; i++)
        {
            if (!_multipleAudios[i].isPlaying)
            {
                AudioSource audio = _multipleAudios[i];
                _multipleAudios.RemoveAt(i);
                i -= 1;
                Destroy(audio.gameObject);
            }
        }
    }

    /// <summary>
    /// ����������Ч
    /// </summary>
    /// <param name="attachTarget">����Ŀ��</param>
    /// <param name="clip">���ּ���</param>
    /// <param name="isLoop">�Ƿ�ѭ��</param>
    /// <param name="speed">�����ٶ�</param>
    public void PlayWorldSound(GameObject attachTarget, AudioClip clip, bool isLoop = false, float speed = 1)
    {
        if (_worldAudios.ContainsKey(attachTarget))
        {
            AudioSource audio = _worldAudios[attachTarget];
            if (audio.isPlaying)
            {
                audio.Stop();
            }
            audio.clip = clip;
            audio.loop = isLoop;
            audio.pitch = speed;
            audio.Play();
        }
        else
        {
            AudioSource audio = AttachAudioSource(attachTarget, WorldPriority, WorldVolume, 1, 1);
            _worldAudios.Add(attachTarget, audio);
            audio.clip = clip;
            audio.loop = isLoop;
            audio.pitch = speed;
            audio.Play();
        }
    }
    /// <summary>
    /// ��ͣ����ָ����������Ч
    /// </summary>
    /// <param name="attachTarget">����Ŀ��</param>
    /// <param name="isGradual">�Ƿ񽥽�ʽ</param>
    public void PauseWorldSound(GameObject attachTarget, bool isGradual = true)
    {
        if (_worldAudios.ContainsKey(attachTarget))
        {
            AudioSource audio = _worldAudios[attachTarget];
            if (isGradual)
            {
                audio.DOFade(0, 2).OnComplete(() =>
                {
                    audio.volume = WorldVolume;
                    audio.Pause();
                });
            }
            else
            {
                audio.Pause();
            }
        }
    }
    /// <summary>
    /// �ָ�����ָ����������Ч
    /// </summary>
    /// <param name="attachTarget">����Ŀ��</param>
    /// <param name="isGradual">�Ƿ񽥽�ʽ</param>
    public void UnPauseWorldSound(GameObject attachTarget, bool isGradual = true)
    {
        if (_worldAudios.ContainsKey(attachTarget))
        {
            AudioSource audio = _worldAudios[attachTarget];
            if (isGradual)
            {
                audio.UnPause();
                audio.volume = 0;
                audio.DOFade(WorldVolume, 2);
            }
            else
            {
                audio.UnPause();
            }
        }
    }
    /// <summary>
    /// ֹͣ�������е�������Ч
    /// </summary>
    public void StopAllWorldSound()
    {
        foreach (var audio in _worldAudios)
        {
            if (audio.Value.isPlaying)
            {
                audio.Value.Stop();
            }
        }
    }
    /// <summary>
    /// �������������е�������Ч����Դ
    /// </summary>
    public void ClearIdleWorldAudioSource()
    {
        HashSet<GameObject> removeSet = new HashSet<GameObject>();
        foreach (var audio in _worldAudios)
        {
            if (!audio.Value.isPlaying)
            {
                removeSet.Add(audio.Key);
                Destroy(audio.Value);
            }
        }
        foreach (var item in removeSet)
        {
            _worldAudios.Remove(item);
        }
    }

    //����һ����Դ
    private AudioSource CreateAudioSource(string name, int priority, float volume, float speed, float spatialBlend)
    {
        GameObject audioObj = new GameObject(name);
        audioObj.transform.SetParent(transform);
        audioObj.transform.localPosition = Vector3.zero;
        audioObj.transform.localRotation = Quaternion.identity;
        audioObj.transform.localScale = Vector3.one;
        AudioSource audio = audioObj.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.priority = priority;
        audio.volume = volume;
        audio.pitch = speed;
        audio.spatialBlend = spatialBlend;
        audio.mute = _isMute;
        return audio;
    }
    //����һ����Դ
    private AudioSource AttachAudioSource(GameObject target, int priority, float volume, float speed, float spatialBlend)
    {
        AudioSource audio = target.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.priority = priority;
        audio.volume = volume;
        audio.pitch = speed;
        audio.spatialBlend = spatialBlend;
        audio.mute = _isMute;
        return audio;
    }
    //��ȡ�����еĶ�ͨ����Դ
    private AudioSource ExtractIdleMultipleAudioSource()
    {
        for (int i = 0; i < _multipleAudios.Count; i++)
        {
            if (!_multipleAudios[i].isPlaying)
            {
                return _multipleAudios[i];
            }
        }

        AudioSource audio = CreateAudioSource("MultipleAudio", MultiplePriority, MultipleVolume, 1, 0);
        _multipleAudios.Add(audio);
        return audio;
    }
}
