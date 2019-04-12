using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Tile {
    public enum TileType {
        Normal, Water, Ikada, Wall
    }
    public readonly TileType tileType;
    public readonly Across ExAcross;//外側
    public readonly Across InAcross;//筏用の内側8方向の出入りに用いる
    public Tile(TileType _tileType, Across _InAcross = null, Across _ExAcross = null) {
        tileType = _tileType;
        switch (tileType) {
            case TileType.Normal:
                ExAcross = new Across(true);
                InAcross = new Across(true); break;
            case TileType.Water:
            case TileType.Wall:
                ExAcross = new Across(false);
                InAcross = new Across(false); break;
            case TileType.Ikada:
                ExAcross = new Across(_ExAcross.Mat);
                InAcross = new Across(_InAcross.Mat); break;
        }
    }

}

public class TileObject : MonoBehaviour{
    public Tile tile;
    public int ForClickX, ForClickY;
	public void SetInitButtonState() {
        if (tile.tileType == Tile.TileType.Ikada) {
			Action<Image, bool> SetColor = (image, b) => {
				var c = image.color;
				image.color = b ?
					new Color(c.r, c.g, c.b, 1f) :
					new Color(c.r, c.g, c.b, 0.15f);
			};
			var eb = tile.ExAcross.GetRLTBC();
			var ib = tile.InAcross.GetRLTBC();
			for (int i = 0; i < 4; i++)
                SetColor(transform.Find("e" + i).gameObject.GetComponent<Image>(),eb[i]);
            for (int i = 0; i < 4; i++)
                SetColor(transform.Find("e" + i + "/i" + i).gameObject.GetComponent<Image>(),ib[i]);
            SetColor(transform.Find("t0").GetComponent<Image>(), tile.InAcross.C);
        }
    }

	public void SetInitgoIkadaState() {
		if (tile.tileType == Tile.TileType.Ikada) {
			var eb = tile.ExAcross.GetRLTBC();
			var ib = tile.InAcross.GetRLTBC();
			for (int i = 0; i < 4; i++)
				transform.Find("e" + i).gameObject.SetActive(eb[i]);
			for (int i = 0; i < 4; i++)
				transform.Find("e" + i + "/i" + i).gameObject.SetActive(ib[i]);
			transform.Find("t0").gameObject.SetActive(tile.InAcross.C);
		}
	}

}