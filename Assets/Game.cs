using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private readonly Color[] _colors =
    {
        Color.green, // Green
        Color.yellow, // Yellow
        Color.red, // Red
        Color.cyan, // Cyan
        Color.blue // Blue
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lastOffset = Offset;
        _lastPadding = Padding;
        _matrix = new Bubble[GridSize, GridSize];
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                var instance = Instantiate(BubblePrefab);
                var bubble = instance.GetComponent<Bubble>();
                // random colors are green, yellow, red, cyan, blue
                var color = _colors[Random.Range(0, _colors.Length)];
                bubble.Initialize(x, y, color, BubbleClicked);
                _matrix[x, y] = bubble;
            }
        }
        Cleanup();
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

    private List<Bubble> CheckThreeInAColumn()
    {
        var bubbles = new List<Bubble>();
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
                }
            }
        }
        return bubbles;
    }

    private List<Bubble> CheckThreeInARow()
    {
        var bubbles = new List<Bubble>();
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
                }
            }
        }
        return bubbles;
    }

    private IEnumerator DestroyMatchedBubbles(List<Bubble> bubbles)
    {
        // Clear matched bubbles from the matrix
        foreach (var bubble in bubbles)
        {
            var color = bubble.GetComponent<SpriteRenderer>().color;
            bubble.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0.5f); // Fade out the bubble
            yield return new WaitForSeconds(0.1f); // Wait for a moment before destroying
            _matrix[bubble.X, bubble.Y] = null;
        }
        if (bubbles.Count == 0) yield break;
        foreach (var bubble in bubbles.Where(b => b != null))
        {
            DestroyImmediate(bubble.gameObject);
        }

        StartCoroutine(DropBubbles());
    }

    private IEnumerator DropBubbles()
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
                        Debug.Log($"Bubble above: {bubbleAbove.X},{bubbleAbove.Y} ({bubbleAbove.Color})");
                        Debug.Log($"Moving bubble from ({bubbleAbove.X}, {bubbleAbove.Y}) to ({x}, {y})");
                        _matrix[bubbleAbove.X, bubbleAbove.Y] = null; // Clear the old position
                        _matrix[x, y] = bubbleAbove;
                        bubbleAbove.X = x; // Update the X position
                        bubbleAbove.Y = y; // Update the Y position
                        bubbleAbove.AnimateMove(x, y);
                        yield return new WaitForSeconds(0.1f); // Wait for a moment before destroying
                        break; // Stop after the first bubble found
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        Cleanup();
    }

    private IEnumerator FillEmptySpaces()
    {
        var empty = false;
        var bubbles = new List<Bubble>();
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = GridSize - 1; y >= 0; y--)
            {
                if (_matrix[x, y] == null)
                {
                    empty = true;
                    Debug.Log($"Empty bubble found at ({x}, {y})");
                    var instance = Instantiate(BubblePrefab);
                    var bubble = instance.GetComponent<Bubble>();
                    bubble.Initialize(x, 0, _colors[Random.Range(0, _colors.Length)], BubbleClicked);
                    _matrix[x, y] = bubble;
                    bubble.AnimateMove(x, y);
                    yield return new WaitForSeconds(0.5f);
                    bubble.Y = y; // Update the Y position
                }
            }
        }
        if (empty)
        {
            yield return new WaitForSeconds(0.5f);
            Cleanup();
        }
        else
        {
            yield return new WaitForSeconds(2f);
            foreach (var specialItem in SpecialItems)
            {
                StartCoroutine(specialItem.ActivateItem());
            }
            SpecialItems.Clear();
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
        Cleanup();
    }

    private void Cleanup()
    {
        var bubbles = CheckThreeInARow();
        bubbles.AddRange(CheckThreeInAColumn());
        if (bubbles.Count > 0)
        {
            Debug.Log("Initial match found, destroying bubbles.");
            StartCoroutine(DestroyMatchedBubbles(bubbles.Distinct().ToList()));
            if (bubbles.Count > 3)
            {
                SpawnSpecialItem(bubbles[0].X, bubbles[0].Y);
            }
        }
        else
        {
            Debug.Log("No matches found, filling empty spaces.");
            StartCoroutine(FillEmptySpaces());
        }
        SelectedBubble = null;
    }

    private void SpawnSpecialItem(int x, int y)
    {
        var randomIndex = Random.Range(0, SpecialItemPrefabs.Length);
        var instance = Instantiate(SpecialItemPrefabs[randomIndex]);
        var specialItem = instance.GetComponent<SpecialItem>();
        specialItem.Initialize(x, y);
        SpecialItems.Add(specialItem);
    }

    internal void DestroyBubbles(Vector2Int[] positions)
    {
        var bubbles = positions.Select(position => _matrix[position.x, position.y]).Where(bubble => bubble != null).ToList();
        StartCoroutine(DestroyMatchedBubbles(bubbles.Distinct().ToList()));
    }
}