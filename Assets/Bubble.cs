using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    public Color Color;

    public int X;
    public int Y;
    protected (int x, int y)? moveTarget = null;
    protected Game _game;

    void Awake()
    {
        _game = FindAnyObjectByType<Game>();
    }

    public virtual void OnSwap(SwipeDirection direction)
    {
        Debug.Log($"Bubble swapped in direction {direction} at ({X},{Y})");
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

    internal void Initialize(int x, int y,  Color color)
    {
        transform.position = new Vector3(x * _game.Padding, -y * _game.Padding, 0) + _game.Offset;
        Color = color;
        X = x;
        Y = y;

        GetComponent<SpriteRenderer>().color = Color;
    }
}
