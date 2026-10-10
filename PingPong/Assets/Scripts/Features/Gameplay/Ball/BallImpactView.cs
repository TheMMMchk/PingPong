using UnityEngine;

public class BallImpactView : MonoBehaviour
{
    private const float RingDuration = 0.18f;
    private const float FlashDuration = 0.12f;
    private const float SquashDuration = 0.11f;

    private AudioSource _audio;
    private LineRenderer _ring;
    private Material _ringMaterial;
    private float _ringTime;
    private SpriteRenderer _flash;
    private Color _originalColor;
    private float _flashTime;
    private AudioClip _hit;
    private AudioClip _strong;
    private AudioClip _wall;

    private SpriteRenderer _squashedSource;
    private bool _sourceWasEnabled;
    private GameObject _squashObject;
    private Transform _squashTransform;
    private Vector3 _squashBaseScale;
    private float _squashTime;

    private Transform _cameraTransform;
    private Vector3 _cameraBasePosition;
    private float _shakeTime;
    private float _shakeDuration;
    private float _shakeStrength;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            _cameraTransform = mainCamera.transform;
            _cameraBasePosition = _cameraTransform.position;
        }

        _hit = MakeClip(560f, 0.06f);
        _strong = MakeClip(850f, 0.09f);
        _wall = MakeClip(360f, 0.035f);
    }

    public void PlayPaddleHit(Vector2 point, Collider2D paddle, bool strong)
    {
        ShowRing(point, strong);
        _audio.PlayOneShot(strong ? _strong : _hit, 0.7f);
        StartCameraShake(strong ? 0.09f : 0.045f, strong ? 0.13f : 0.09f);

        if (paddle == null) return;

        SpriteRenderer renderer = paddle.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = paddle.GetComponentInChildren<SpriteRenderer>();
        if (renderer == null) return;

        FlashPaddle(renderer, strong);
        SquashPaddle(renderer, strong);
    }

    public void PlayWallHit(Vector2 point)
    {
        ShowRing(point, false);
        _audio.PlayOneShot(_wall, 0.45f);
        StartCameraShake(0.018f, 0.055f);
    }

    private void Update()
    {
        UpdateRing();
        UpdateFlash();
        UpdateSquash();
        UpdateCameraShake();
    }

    private void FlashPaddle(SpriteRenderer renderer, bool strong)
    {
        if (_flash != null && _flash != renderer)
            _flash.color = _originalColor;

        _flash = renderer;
        _originalColor = renderer.color;
        _flash.color = strong
            ? new Color(1f, 0.72f, 0.3f, _originalColor.a)
            : new Color(0.55f, 0.9f, 1f, _originalColor.a);
        _flashTime = FlashDuration;
    }

    private void UpdateFlash()
    {
        if (_flash == null || _flashTime <= 0f) return;

        _flashTime -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(_flashTime / FlashDuration);
        _flash.color = Color.Lerp(_flash.color, _originalColor, progress);

        if (_flashTime <= 0f)
        {
            _flash.color = _originalColor;
            _flash = null;
        }
    }

    private void SquashPaddle(SpriteRenderer source, bool strong)
    {
        RestoreSquashedPaddle();

        _squashedSource = source;
        _sourceWasEnabled = source.enabled;

        // Use a temporary sprite-only copy so neither the paddle Transform
        // nor its Collider2D is scaled during the squash animation.
        _squashObject = new GameObject("PaddleImpactVisual");
        _squashTransform = _squashObject.transform;
        _squashTransform.position = source.transform.position;
        _squashTransform.rotation = source.transform.rotation;
        _squashBaseScale = source.transform.lossyScale;
        _squashTransform.localScale = _squashBaseScale;

        SpriteRenderer copy = _squashObject.AddComponent<SpriteRenderer>();
        copy.sprite = source.sprite;
        copy.color = source.color;
        copy.sharedMaterial = source.sharedMaterial;
        copy.sortingLayerID = source.sortingLayerID;
        copy.sortingOrder = source.sortingOrder + 1;
        copy.flipX = source.flipX;
        copy.flipY = source.flipY;
        copy.drawMode = source.drawMode;
        copy.size = source.size;
        copy.maskInteraction = source.maskInteraction;
        copy.spriteSortPoint = source.spriteSortPoint;

        source.enabled = false;
        _squashTime = SquashDuration;
    }

    private void UpdateSquash()
    {
        if (_squashTransform == null || _squashTime <= 0f) return;

        _squashTime -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(_squashTime / SquashDuration);
        float recovery = Mathf.Sin(progress * Mathf.PI);
        float horizontal = Mathf.Lerp(1f, 1.13f, recovery);
        float vertical = Mathf.Lerp(1f, 0.68f, recovery);
        _squashTransform.localScale = new Vector3(
            _squashBaseScale.x * horizontal,
            _squashBaseScale.y * vertical,
            _squashBaseScale.z);

        if (_squashTime <= 0f)
            RestoreSquashedPaddle();
    }

    private void RestoreSquashedPaddle()
    {
        if (_squashedSource != null)
            _squashedSource.enabled = _sourceWasEnabled;

        if (_squashObject != null)
            Destroy(_squashObject);

        _squashedSource = null;
        _squashObject = null;
        _squashTransform = null;
        _squashTime = 0f;
    }

    private void StartCameraShake(float strength, float duration)
    {
        if (_cameraTransform == null) return;

        _shakeStrength = Mathf.Max(_shakeStrength, strength);
        _shakeDuration = Mathf.Max(_shakeDuration, duration);
        _shakeTime = Mathf.Max(_shakeTime, duration);
    }

    private void UpdateCameraShake()
    {
        if (_cameraTransform == null) return;

        if (_shakeTime <= 0f)
        {
            _cameraTransform.position = _cameraBasePosition;
            return;
        }

        _shakeTime -= Time.deltaTime;
        float fade = Mathf.Clamp01(_shakeTime / _shakeDuration);
        Vector2 offset = Random.insideUnitCircle * (_shakeStrength * fade);
        _cameraTransform.position = _cameraBasePosition + new Vector3(offset.x, offset.y, 0f);

        if (_shakeTime <= 0f)
        {
            _cameraTransform.position = _cameraBasePosition;
            _shakeStrength = 0f;
        }
    }

    private void ShowRing(Vector2 point, bool strong)
    {
        if (_ring != null)
        {
            Destroy(_ring.gameObject);
            _ring = null;
        }
        if (_ringMaterial != null)
        {
            Destroy(_ringMaterial);
            _ringMaterial = null;
        }

        GameObject go = new GameObject("BallImpactRing");
        go.transform.position = point;
        _ring = go.AddComponent<LineRenderer>();
        _ring.useWorldSpace = false;
        _ring.loop = true;
        _ring.positionCount = 32;
        _ring.startWidth = strong ? 0.055f : 0.035f;
        _ring.endWidth = _ring.startWidth;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            _ringMaterial = new Material(shader);
            _ring.material = _ringMaterial;
        }
        _ring.sortingOrder = 20;

        Color color = strong
            ? new Color(1f, 0.65f, 0.2f, 1f)
            : new Color(0.4f, 0.85f, 1f, 1f);
        _ring.startColor = color;
        _ring.endColor = color;

        for (int i = 0; i < 32; i++)
        {
            float angle = i * Mathf.PI * 2f / 32f;
            _ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f));
        }

        _ring.transform.localScale = Vector3.one * 0.12f;
        _ringTime = RingDuration;
    }

    private void UpdateRing()
    {
        if (_ring == null || _ringTime <= 0f) return;

        _ringTime -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(_ringTime / RingDuration);
        _ring.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.75f, progress);
        Color color = _ring.startColor;
        color.a = 1f - progress;
        _ring.startColor = color;
        _ring.endColor = color;

        if (_ringTime <= 0f)
        {
            Destroy(_ring.gameObject);
            _ring = null;
            if (_ringMaterial != null)
            {
                Destroy(_ringMaterial);
                _ringMaterial = null;
            }
        }
    }

    private static AudioClip MakeClip(float frequency, float duration)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time)
                * Mathf.Exp(-time * 45f) * 0.2f;
        }

        AudioClip clip = AudioClip.Create("BallImpact", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        RestoreSquashedPaddle();

        if (_cameraTransform != null)
            _cameraTransform.position = _cameraBasePosition;

        if (_hit != null) Destroy(_hit);
        if (_strong != null) Destroy(_strong);
        if (_wall != null) Destroy(_wall);
        if (_ring != null) Destroy(_ring.gameObject);
        if (_ringMaterial != null) Destroy(_ringMaterial);
    }
}