using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : SpecialItem
{
    public override IEnumerator ActivateItem()
    {
        var positions = new List<Vector2Int>();
        // 3x3 area destruction
        for (int x = X - 1; x <= X + 1; x++)
        {
            for (int y = Y - 1; y <= Y + 1; y++)
            {
                if (x >= 0 && x < _game.GridSize && y >= 0 && y < _game.GridSize)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }
        yield return new WaitForSeconds(1f);
        _game.DestroyBubbles(positions.ToArray());
    }

    public override void AnimateMove(int x, int y)
    {
        base.AnimateMove(x, y);
        // Additional bomb-specific update logic can go here
    }
}