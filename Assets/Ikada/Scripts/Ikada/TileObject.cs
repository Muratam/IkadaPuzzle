using UnityEngine;
using System.Collections;
using System;
public class TileObject : MonoBehaviour
{
    public Tile Tile;
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
