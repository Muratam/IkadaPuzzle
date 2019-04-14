using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
public class EditableTileObject : TileObject
{

    public int ForClickX, ForClickY;
    public void SetInitButtonState()
    {
        if (Tile.tileType != Tile.TileType.Ikada) return;
        Action<Image, bool> SetColor = (image, b) =>
        {
            var c = image.color;
            image.color = b ?
                new Color(c.r, c.g, c.b, 1f) :
                new Color(c.r, c.g, c.b, 0.15f);
        };
        var eb = Tile.ExAcross.GetRLTBC();
        var ib = Tile.InAcross.GetRLTBC();
        for (int i = 0; i < 4; i++)
            SetColor(transform.Find("e" + i).gameObject.GetComponent<Image>(), eb[i]);
        for (int i = 0; i < 4; i++)
            SetColor(transform.Find("e" + i + "/i" + i).gameObject.GetComponent<Image>(), ib[i]);
        SetColor(transform.Find("t0").GetComponent<Image>(), Tile.InAcross.C);
    }

}
