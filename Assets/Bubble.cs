using System;
using System.Collections;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public Color Color;

    public int X;
    public int Y;
    private Action<Bubble> OnBubbleClicked;
    private (float x, float y)? moveTarget = null;
    private Game _game;

    void Awake()
    {
        _game = FindAnyObjectByType<Game>();
    }

    void OnMouseUp()
    {
        Debug.Log("Bubble clicked!");
        OnBubbleClicked?.Invoke(this);
    }

    void Update()
    {
        if (moveTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(moveTarget.Value.x * _game.Padding, -moveTarget.Value.y * _game.Padding, 0) + _game.Offset, Time.deltaTime * 5f);
            if (Vector3.Distance(transform.position, new Vector3(moveTarget.Value.x * _game.Padding, -moveTarget.Value.y * _game.Padding, 0) + _game.Offset) < 0.01f)
            {
                moveTarget = null; // Reset move target when close enough
            }
        }
    }

    public void AnimateMove(float x, float y)
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
