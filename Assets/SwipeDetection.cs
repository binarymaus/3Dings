using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    [SerializeField]
    private float minimumDistance = .2f;
    [SerializeField]
    private float maximumTime = 1f;
    [SerializeField, Range(0f, 1f)]
    private float directionThreshold = .9f;
    private InputManager inputManager;
    private int uiLayer;
    private Vector2 startPosition;
    private float startTime;
    private Vector2 endPosition;
    private float endTime;
    private Game game;
    private Bubble FirstBubble;

    void Awake()
    {
        inputManager = InputManager.Instance;
        uiLayer = LayerMask.GetMask("UI");
    }

    void Start()
    {
        game = FindAnyObjectByType<Game>();
    }

    void OnEnable()
    {
        inputManager.OnStartTouch += SwipeStart;
        inputManager.OnEndTouch += SwipeEnd;
    }

    void OnDisable()
    {
        inputManager.OnStartTouch -= SwipeStart;
        inputManager.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 position, float time)
    {
        startPosition = position;
        startTime = time;
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, Mathf.Infinity, uiLayer);
        if (hit.collider != null)
        {
            Debug.Log($"Hit {hit.collider.name}");
            Bubble bubble = hit.collider.GetComponent<Bubble>();
            if (bubble != null)
            {
                Debug.Log($"Bubble {bubble.name} selected");
                FirstBubble = bubble;
            }
        }
        else
        {
            FirstBubble = null;
            Debug.Log("No bubble hit with raycast");
        }
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        if(startPosition.magnitude < Mathf.Epsilon)
        {
            // No valid start position, ignore
            return;
        }
        endPosition = position;
        endTime = time;
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (Vector3.Distance(startPosition, endPosition) >= minimumDistance &&
            (endTime - startTime) <= maximumTime)
        {
            Debug.DrawLine(startPosition, endPosition, Color.red, 5f);
            Debug.Log("Swipe Detected");
            Vector3 direction = endPosition - startPosition;
            Vector2 direction2d = new Vector2(direction.x, direction.y).normalized;
            if (FirstBubble != null)
            {
                DetectSwipeDirection(direction2d);
                return;
            }
            Debug.Log("No bubble selected for swipe");
        }
    }

    private void DetectSwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            Debug.Log("Swipe Up");
            game.BubbleSwiped(FirstBubble, SwipeDirection.Up);
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            Debug.Log("Swipe Down");
            game.BubbleSwiped(FirstBubble, SwipeDirection.Down);
        }
        else if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            Debug.Log("Swipe Left");
            game.BubbleSwiped(FirstBubble, SwipeDirection.Left);
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            Debug.Log("Swipe Right");
            game.BubbleSwiped(FirstBubble, SwipeDirection.Right);
        }
    }
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right
}