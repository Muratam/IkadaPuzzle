using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;


// エディットも可能な実態。タイルとクリック箇所を持つ。
public class TileObject : MonoBehaviour
{
    public Tile Tile;
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

    public void SetInitGoIkadaState()
    {
        if (Tile.tileType != Tile.TileType.Ikada) return;
        var eb = Tile.ExAcross.GetRLTBC();
        var ib = Tile.InAcross.GetRLTBC();
        for (int i = 0; i < 4; i++)
            transform.Find("e" + i).gameObject.SetActive(eb[i]);
        for (int i = 0; i < 4; i++)
            transform.Find("e" + i + "/i" + i).gameObject.SetActive(ib[i]);
        transform.Find("t0").gameObject.SetActive(Tile.InAcross.C);
    }
}
