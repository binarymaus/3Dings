using System.Collections;
using UnityEngine;

public class SpecialItem : MonoBehaviour
{
    protected Game _game;

    protected Vector2Int Position;

    void Awake()
    {
        _game = FindAnyObjectByType<Game>();
    }
    public virtual IEnumerator ActivateItem()
    {
        yield return new WaitForSeconds(0f);
    }

    internal void Initialize(int x, int y)
    {
        _game = FindAnyObjectByType<Game>();
        transform.position = new Vector3(x * _game.Padding, -y * _game.Padding, -0.1f) + _game.Offset;
        Position = new Vector2Int(x, y);
    }
}