using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : SpecialItem
{
    // Update is called once per frame

    public override IEnumerator ActivateItem()
    {
        var positions = new List<Vector2Int>();
        for (int i = 0; i < _game.GridSize; i++)
        {
            positions.Add(new Vector2Int(i, Position.y));
            positions.Add(new Vector2Int(Position.x, i));
        }
        // Bomb explosion behavior
        _game.DestroyBubbles(positions.ToArray());
        yield return new WaitForSeconds(0f);
        //_game.DestroyBubbles(positions.ToArray());
        Destroy(gameObject);
    }
}