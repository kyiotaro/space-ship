using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedSprite : MonoBehaviour
{
    [Header("Frames")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Playback")]
    [SerializeField, Min(1f)] private float framesPerSecond = 12f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool destroyAfterPlaying;

    private int currentFrame;
    private float frameTimer;
    private bool isPlaying;
    private bool playbackLoop;

    public bool IsPlaying => isPlaying;
    public int CurrentFrame => currentFrame;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float secondsPerFrame = 1f / Mathf.Max(1f, framesPerSecond);

        while (frameTimer >= secondsPerFrame)
        {
            frameTimer -= secondsPerFrame;
            AdvanceFrame();

            if (!isPlaying)
            {
                break;
            }
        }
    }

    public void Play()
    {
        Play(loop);
    }

    public void PlayOnce()
    {
        Play(false);
    }

    private void Play(bool shouldLoop)
    {
        if (frames == null || frames.Length == 0)
        {
            isPlaying = false;
            return;
        }

        currentFrame = 0;
        frameTimer = 0f;
        isPlaying = true;
        playbackLoop = shouldLoop;
        ApplyCurrentFrame();
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void SetFrame(int frameIndex)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        currentFrame = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
        frameTimer = 0f;
        ApplyCurrentFrame();
    }

    private void AdvanceFrame()
    {
        currentFrame++;

        if (currentFrame < frames.Length)
        {
            ApplyCurrentFrame();
            return;
        }

        if (playbackLoop)
        {
            currentFrame = 0;
            ApplyCurrentFrame();
            return;
        }

        currentFrame = frames.Length - 1;
        ApplyCurrentFrame();
        isPlaying = false;

        if (destroyAfterPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyCurrentFrame()
    {
        if (spriteRenderer != null && frames[currentFrame] != null)
        {
            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}