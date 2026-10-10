using UnityEngine;

public class BallImpactView : MonoBehaviour
{
    private AudioSource _audio;
    private LineRenderer _ring;
    private float _ringTime;
    private SpriteRenderer _flash;
    private Color _originalColor;
    private float _flashTime;
    private AudioClip _hit;
    private AudioClip _strong;
    private AudioClip _wall;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f;
        _hit = MakeClip(560f, 0.06f);
        _strong = MakeClip(850f, 0.09f);
        _wall = MakeClip(360f, 0.035f);
    }

    public void PlayPaddleHit(Vector2 point, Collider2D paddle, bool strong)
    {
        ShowRing(point, strong);
        _audio.PlayOneShot(strong ? _strong : _hit, 0.7f);
        if (paddle == null) return;
        _flash = paddle.GetComponentInChildren<SpriteRenderer>();
        if (_flash == null) return;
        _originalColor = _flash.color;
        _flash.color = strong ? new Color(1f, 0.72f, 0.3f, _originalColor.a) : new Color(0.55f, 0.9f, 1f, _originalColor.a);
        _flashTime = 0.12f;
    }

    public void PlayWallHit(Vector2 point)
    {
        ShowRing(point, false);
        _audio.PlayOneShot(_wall, 0.45f);
    }

    private void Update()
    {
        if (_ring != null && _ringTime > 0f)
        {
            _ringTime -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(_ringTime / 0.18f);
            _ring.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.75f, t);
            Color c = _ring.startColor;
            c.a = 1f - t;
            _ring.startColor = c;
            _ring.endColor = c;
            if (_ringTime <= 0f)
            {
                Destroy(_ring.gameObject);
                _ring = null;
            }
        }

        if (_flash != null && _flashTime > 0f)
        {
            _flashTime -= Time.deltaTime;
            _flash.color = Color.Lerp(_originalColor, _flash.color, Mathf.Clamp01(_flashTime / 0.12f));
            if (_flashTime <= 0f)
            {
                _flash.color = _originalColor;
                _flash = null;
            }
        }
    }

    private void ShowRing(Vector2 point, bool strong)
    {
        if (_ring != null) Destroy(_ring.gameObject);
        GameObject go = new GameObject("BallImpactRing");
        go.transform.position = point;
        _ring = go.AddComponent<LineRenderer>();
        _ring.useWorldSpace = false;
        _ring.loop = true;
        _ring.positionCount = 32;
        _ring.startWidth = strong ? 0.055f : 0.035f;
        _ring.endWidth = _ring.startWidth;
        _ring.material = new Material(Shader.Find("Sprites/Default"));
        Color c = strong ? new Color(1f, 0.65f, 0.2f, 1f) : new Color(0.4f, 0.85f, 1f, 1f);
        _ring.startColor = c;
        _ring.endColor = c;
        for (int i = 0; i < 32; i++)
        {
            float a = i * Mathf.PI * 2f / 32f;
            _ring.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
        }
        _ring.transform.localScale = Vector3.one * 0.12f;
        _ringTime = 0.18f;
    }

    private static AudioClip MakeClip(float frequency, float duration)
    {
        const int rate = 44100;
        int count = Mathf.CeilToInt(rate * duration);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / rate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * Mathf.Exp(-t * 45f) * 0.2f;
        }
        AudioClip clip = AudioClip.Create("BallImpact", count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (_hit != null) Destroy(_hit);
        if (_strong != null) Destroy(_strong);
        if (_wall != null) Destroy(_wall);
        if (_ring != null) Destroy(_ring.gameObject);
    }
}