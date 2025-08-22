using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : SpecialItem
{
    public override IEnumerator ActivateItem()
    {
        var positions = new List<Vector2Int>();
        for (int i = 0; i < _game.GridSize; i++)
        {
            positions.Add(new Vector2Int(i, Y));
            positions.Add(new Vector2Int(X, i));
        }
        // Bomb explosion behavior
        yield return new WaitForSeconds(2f);
        _game.DestroyBubbles(positions.ToArray());
    }
}