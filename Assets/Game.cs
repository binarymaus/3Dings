using Action = System.Action;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using UnityEngine;


public class Game : MonoBehaviour
{
    public int GridSize = 8;
    public Vector3 Offset = new(-1.8f, 1.5f, 0);
    public float Padding = 0.5f;
    private Vector3 _lastOffset;
    private float _lastPadding;
    private Bubble[,] _matrix;
    public GameObject BubblePrefab;
    private Bubble SelectedBubble;

    public GameObject[] SpecialItemPrefabs;

    private List<SpecialItem> SpecialItems = new();

    public static readonly Color[] Colors =
    {
        Color.green, // Green
        Color.yellow, // Yellow
        Color.red, // Red
        Color.cyan, // Cyan
        Color.blue // Blue
    };

    #if !UNITY_STANDALONE
        protected Vector2 m_StartingTouch;
        protected bool m_IsSwiping = false;
    #endif

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var loader = new LevelLoader();
        loader.CreateSampleLevel();
        loader.SaveLevelData();
        loader.LoadLevelData();
        _lastOffset = Offset;
        _lastPadding = Padding;
        _matrix = new Bubble[GridSize, GridSize];
        GridSize = loader.levelData.gridSize.x; // Assuming square grid
        var colorMatrix = loader.levelData.colorMatrix;
        for (int x = 0; x < colorMatrix.Count; x++)
        {
            for (int y = 0; y < colorMatrix[x].Count; y++)
            {
                var instance = Instantiate(BubblePrefab);
                var bubble = instance.GetComponent<Bubble>();
                // random colors are green, yellow, red, cyan, blue
                var color = colorMatrix[x][y].ToColor();
                bubble.Initialize(x, y, color, BubbleClicked);
                _matrix[x, y] = bubble;
            }
        }
        //Cleanup();
    }

    void Update()
    {
        if (_lastOffset != Offset || _lastPadding != Padding)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    var bubble = _matrix[x, y];
                    bubble?.AnimateMove(x, y);
                }
            }
            _lastOffset = Offset;
            _lastPadding = Padding;
        }
        #if !(UNITY_EDITOR || UNITY_STANDALONE)
        // Use touch input on mobile
        if (Input.touchCount == 1)
        {
            if(m_IsSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;

                // Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
                diff = new Vector2(diff.x/Screen.width, diff.y/Screen.width);

                if(diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
                {
                    if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if(diff.y < 0)
						{
							// Down
                            // Get Bubble using raycast
                            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector2.down);
                            if (hit.collider != null)
                            {
                                Bubble bubble = hit.collider.GetComponent<Bubble>();
                                if (bubble != null)
                                {
                                    BubbleSwiped(bubble, Vector2.down);
                                }
                            }
						}
						else
						{
							// Up
						}
                    }
                    else
                    {
                        if(diff.x < 0)
                        {
                            // Left
                        }
                        else
                        {
                            // Right
                        }
                    }
                        
                    m_IsSwiping = false;
                }
            }

            // Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
            // a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
            if(Input.GetTouch(0).phase == TouchPhase.Began)
            {
                m_StartingTouch = Input.GetTouch(0).position;
                m_IsSwiping = true;
            }
            else if(Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                m_IsSwiping = false;
            }
        }
        #endif
    }

    private void BubbleSwiped(Bubble bubble, Vector2 direction)
    {
        // Handle bubble swipe logic
    }

    private void BubbleClicked(Bubble bubble)
    {
        if (SelectedBubble != null)
        {
            if (Distance(SelectedBubble, bubble) > 1)
            {
                Debug.Log("Bubbles are too far apart to swap.");
                SelectedBubble = null; // Deselect if too far
                return;
            }
            SwapWith(bubble);
        }
        else
        {
            SelectedBubble = bubble;
        }
    }

    private int Distance(Bubble selectedBubble, Bubble bubble)
    {
        int dx = selectedBubble.X - bubble.X;
        int dy = selectedBubble.Y - bubble.Y;
        return Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
    }

    private bool RecognizePatterns(bool delete = true)
    {
        var match = false;
        // Add pattern recognition
        match |= TwoByTwoGroupPattern(delete);
        match |= ThreeInAColumnPattern(delete);
        match |= ThreeInARowPattern(delete);
        return match;
    }

    private bool TwoByTwoGroupPattern(bool delete)
    {
        var match = false;
        for (int x = 0; x < GridSize - 1; x++)
        {
            for (int y = 0; y < GridSize - 1; y++)
            {
                var bubble0 = _matrix[x, y];
                var bubble1 = _matrix[x + 1, y];
                var bubble2 = _matrix[x, y + 1];
                var bubble3 = _matrix[x + 1, y + 1];
                if (bubble0 != null && bubble1 != null && bubble2 != null && bubble3 != null &&
                    bubble0.Color == bubble1.Color && bubble0.Color == bubble2.Color && bubble0.Color == bubble3.Color)
                {
                    Debug.Log($"2x2 pattern found at ({x}, {y})");
                    match = true;
                    if (!delete)
                    {
                        Debug.Log("Pattern found but not deleting.");
                        continue; // Skip if not deleting
                    }
                    DestroyMatchedBubbles(new List<Bubble> { bubble0, bubble1, bubble2, bubble3 });
                    SpawnSpecialItem(SpecialItemType.Destroyer, bubble0.X, bubble0.Y);
                }
            }
        }

        return match;
    }

    private bool ThreeInAColumnPattern(bool delete)
    {
        var match = false;
        for (int j = 0; j < GridSize; j++)
        {
            for (int i = 0; i < GridSize - 2; i++)
            {
                // Only if i < GridSize - 2
                var bubble0 = _matrix[i, j];
                var bubble1 = _matrix[i + 1, j];
                var bubble2 = _matrix[i + 2, j];
                switch (bubble0, bubble1, bubble2)
                {
                    case (null, _, _) or (_, null, _) or (_, _, null):
                        continue; // Skip if any bubble is null
                }
                if (bubble0.Color == bubble1.Color && bubble0.Color == bubble2.Color)
                {
                    var bubbles = new List<Bubble>();
                    Debug.Log($"Three in a column found at ({i}, {j})");
                    bubbles.AddRange(new List<Bubble> { bubble0, bubble1, bubble2 });
                    // There could be more than three in a column
                    for (int k = i + 3; k < GridSize; k++)
                    {
                        var additionalBubble = _matrix[k, j];
                        if (additionalBubble == null) break;
                        if (additionalBubble.Color == bubble0.Color)
                        {
                            bubbles.Add(additionalBubble);
                        }
                        else
                        {
                            break;
                        }
                    }
                    match = true;
                    if (!delete)
                    {
                        Debug.Log("Pattern found but not deleting.");
                        continue; // Skip if not deleting
                    }
                    DestroyMatchedBubbles(bubbles);
                    SpawnSpecialItem(SpecialItemType.Bomb, bubble0.X, bubble0.Y);
                }
            }
        }
        return match;
    }

    private bool ThreeInARowPattern(bool delete)
    {
        var match = false;
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize - 2; j++)
            {
                var bubble0 = _matrix[i, j];
                var bubble1 = _matrix[i, j + 1];
                var bubble2 = _matrix[i, j + 2];
                switch (bubble0, bubble1, bubble2)
                {
                    case (null, _, _) or (_, null, _) or (_, _, null):
                        continue; // Skip if any bubble is null
                }
                if (bubble0.Color == bubble1.Color && bubble0.Color == bubble2.Color)
                {
                    Debug.Log($"Three in a row found at ({i}, {j})");
                    var bubbles = new List<Bubble>();
                    bubbles.AddRange(new List<Bubble> { bubble0, bubble1, bubble2 });
                    // There could be more than three in a row
                    for (int k = j + 3; k < GridSize; k++)
                    {
                        var additionalBubble = _matrix[i, k];
                        if (additionalBubble == null) break;
                        if (additionalBubble.Color == bubble0.Color)
                        {
                            bubbles.Add(additionalBubble);
                        }
                        else
                        {
                            break;
                        }
                    }
                    match = true;
                    if (!delete)
                    {
                        Debug.Log("Pattern found but not deleting.");
                        continue; // Skip if not deleting
                    }
                    DestroyMatchedBubbles(bubbles);
                    SpawnSpecialItem(SpecialItemType.Bomb, bubble0.X, bubble0.Y);
                }
            }
        }
        return match;
    }

    private void DestroyMatchedBubbles(List<Bubble> bubbles)
    {
        // Clear matched bubbles from the matrix
        foreach (var bubble in bubbles)
        {
            if(bubble == null) continue;
            var color = bubble.GetComponent<SpriteRenderer>().color;
            bubble.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0.5f); // Fade out the bubble
            _matrix[bubble.X, bubble.Y] = null;
        }
        if (bubbles.Count == 0) return;
        foreach (var bubble in bubbles.Where(b => b != null))
        {
            DestroyImmediate(bubble.gameObject);
        }
    }

    private IEnumerator DropBubbles(Action after)
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = GridSize - 1; y >= 0; y--)
            {
                if (_matrix[x, y] == null)
                {
                    Debug.Log($"Empty bubble found at ({x}, {y})");
                    // Move all bubbles above down
                    for (int newY = y - 1; newY >= 0; newY--)
                    {
                        var bubbleAbove = _matrix[x, newY];
                        if (bubbleAbove == null) continue;
                        if (bubbleAbove is Bomb bomb)
                        {
                            Debug.Log($"Bomb found at ({bubbleAbove.X}, {bubbleAbove.Y}), activating.");
                        }
                        Debug.Log($"Bubble above: {bubbleAbove.X},{bubbleAbove.Y} ({bubbleAbove.Color})");
                        Debug.Log($"Moving bubble from ({bubbleAbove.X}, {bubbleAbove.Y}) to ({x}, {y})");
                        _matrix[bubbleAbove.X, bubbleAbove.Y] = null; // Clear the old position
                        _matrix[x, y] = bubbleAbove;
                        bubbleAbove.X = x; // Update the X position
                        bubbleAbove.Y = y; // Update the Y position
                        bubbleAbove.AnimateMove(x, y);
                        yield return new WaitForSeconds(0.1f);
                        break; // Stop after the first bubble found
                    }
                }
            }
        }
        after?.Invoke();
    }

    private IEnumerator FillEmptySpaces()
    {
        var filled = false;
        var bubbles = new List<Bubble>();
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = GridSize - 1; y >= 0; y--)
            {
                if (_matrix[x, y] == null)
                {
                    filled = true;
                    Debug.Log($"Filling empty space at ({x}, {y})");
                    var instance = Instantiate(BubblePrefab);
                    var bubble = instance.GetComponent<Bubble>();
                    bubble.Initialize(x, 0, Colors[Random.Range(0, Colors.Length)], BubbleClicked);
                    _matrix[x, y] = bubble;
                    bubble.AnimateMove(x, y);
                    yield return new WaitForSeconds(0.2f);
                    bubble.Y = y; // Update the Y position
                }
            }
        }
        if (filled)
        {
            yield return new WaitForSeconds(0.5f);
            Cleanup();
        }
    }

    private void SwapWith(Bubble targetBubble)
    {
        Debug.Log($"Swapping bubbles at ({targetBubble.X}, {targetBubble.Y}) and ({SelectedBubble.X}, {SelectedBubble.Y})");
        AnimateSwap(SelectedBubble, targetBubble);
    }

    private void AnimateSwap(Bubble bubbleA, Bubble bubbleB)
    {
        bubbleA.AnimateMove(bubbleB.X, bubbleB.Y);
        bubbleB.AnimateMove(bubbleA.X, bubbleA.Y);
        // Swap positions
        (bubbleB.X, bubbleA.X) = (bubbleA.X, bubbleB.X);
        (bubbleB.Y, bubbleA.Y) = (bubbleA.Y, bubbleB.Y);
        // Update the matrix
        _matrix[bubbleA.X, bubbleA.Y] = bubbleA;
        _matrix[bubbleB.X, bubbleB.Y] = bubbleB;
        var match = RecognizePatterns(delete: false);
        if (!match)
        {
            StartCoroutine(UndoSwap(bubbleA, bubbleB));
            return;
        }
        Cleanup();
    }

    private IEnumerator UndoSwap(Bubble bubbleA, Bubble bubbleB)
    {
        // Undo swap
        yield return new WaitForSeconds(0.2f);
        bubbleA.AnimateMove(bubbleB.X, bubbleB.Y);
        bubbleB.AnimateMove(bubbleA.X, bubbleA.Y);
        (bubbleB.X, bubbleA.X) = (bubbleA.X, bubbleB.X);
        (bubbleB.Y, bubbleA.Y) = (bubbleA.Y, bubbleB.Y);
        _matrix[bubbleA.X, bubbleA.Y] = bubbleA;
        _matrix[bubbleB.X, bubbleB.Y] = bubbleB;
    }

    private void Cleanup()
    {
        var match = RecognizePatterns();
        if (match)
        {
            Debug.Log("Patterns recognized, cleaning up.");
            StartCoroutine(DropBubbles(() =>
            {
                StartCoroutine(FillEmptySpaces());
            }));
        }
        else
        {
            SpecialItems.Clear();
            StartCoroutine(FillEmptySpaces());
        }
        SelectedBubble = null;
    }

    private void SpawnSpecialItem(SpecialItemType itemType, int x, int y)
    {
        var instance = Instantiate(SpecialItemPrefabs[(int)itemType]);
        var specialItem = instance.GetComponent<SpecialItem>();
        _matrix[x, y] = specialItem;
        specialItem.InitializeItem(x, y);
        SpecialItems.Add(specialItem);
    }

    internal void DestroyBubbles(Vector2Int[] positions)
    {
        var bubbles = positions.Select(position => _matrix[position.x, position.y]).Where(bubble => bubble != null).ToList();
        if (!bubbles.Any())
        {
            Debug.Log("No bubbles to destroy.");
            return;
        }
        DestroyMatchedBubbles(bubbles.Distinct().ToList());
        StartCoroutine(DropBubbles(() =>
        {
            StartCoroutine(FillEmptySpaces());
        }));
    }
}