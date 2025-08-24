using System;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public Color Color;

    public int X;
    public int Y;
    private Action<Bubble> OnBubbleClicked;
    protected (int x, int y)? moveTarget = null;
    protected Game _game;

    void Awake()
    {
        _game = FindAnyObjectByType<Game>();
    }

    public virtual void OnTap()
    {
        Debug.Log($"Bubble tapped at ({X},{Y}) with color {Color}");
    }

    public virtual void OnSwipe(Vector2 direction)
    {
        Debug.Log($"Bubble swiped in direction {direction} at ({X},{Y}) with color {Color}");
    }

    public virtual void Update()
    {
        if (moveTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(moveTarget.Value.x * _game.Padding, -moveTarget.Value.y * _game.Padding, 0) + _game.Offset, Time.deltaTime * 5f);
            if (Vector3.Distance(transform.position, new Vector3(moveTarget.Value.x * _game.Padding, -moveTarget.Value.y * _game.Padding, 0) + _game.Offset) < 0.01f)
            {
                X = moveTarget.Value.x;
                Y = moveTarget.Value.y;
                moveTarget = null; // Reset move target when close enough
            }
        }
    }

    public virtual void AnimateMove(int x, int y)
    {
        moveTarget = (x, y);
    }

    internal void Initialize(int x, int y,  Color color, Action<Bubble> onBubbleClicked)
    {
        transform.position = new Vector3(x * _game.Padding, -y * _game.Padding, 0) + _game.Offset;
        Color = color;
        X = x;
        Y = y;

        OnBubbleClicked = onBubbleClicked;
        GetComponent<SpriteRenderer>().color = Color;
    }
}
