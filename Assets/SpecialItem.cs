using System.Collections;
using UnityEngine;

public class SpecialItem : Bubble
{
    public override void Update()
    {
        base.Update();
    }

    protected Vector2Int Position;

    void Awake()
    {
        _game = FindAnyObjectByType<Game>();
    }
    public virtual IEnumerator ActivateItem()
    {
        yield return new WaitForSeconds(0f);
    }

    internal void InitializeItem(int x, int y)
    {
        _game = FindAnyObjectByType<Game>();
        transform.position = new Vector3(x * _game.Padding, -y * _game.Padding, 0f) + _game.Offset;
        X = x;
        Y = y;
    }
}

public enum SpecialItemType
{
    Bomb,
    Destroyer,
    // ColorChanger,
    // RowClearer,
    // ColumnClearer
}