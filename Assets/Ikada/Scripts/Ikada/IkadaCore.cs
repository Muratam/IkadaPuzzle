using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.UI;

public struct Pos { public int x, y; public Pos(int _x, int _y) { x = _x; y = _y; } };

// パズルの基本ロジック。

public class IkadaCore : MonoBehaviour
{
    public const int w = StageMapUtil.w, h = StageMapUtil.h;
    protected TileObject[,] Tiles = new TileObject[w, h];
    protected virtual int tileSize => 120;
    // 位置を継承先から操作しやすいように関数化しておく
    protected virtual Vector3 GetPositionFromPuzzlePosition(int x, int y)
    {
        return tileSize * new Vector3(x - w / 2 + 0.5f, y - h / 2 + 0.5f - 0.8f, 0);
    }

    // ゲームロジック
    // キャラクターの搭乗可能判定。 :ExAcross
    protected bool CanAcrossRide(int x, int y, Across Direction, Across Position)
    {
        if (!IsInRange(x, y)) return false;
        if (!Direction.HaveDirection) return false;
        if (!Position.HaveDirection) return false;
        //if (!(Direction & Position).HaveDirection) return false;
        if (!Across.NearlyEqualDirection(Direction, Position)) return false;
        int Desx = x + Direction.Horizontal;
        int Desy = y + Direction.Vertical;
        if (!IsInRange(Desx, Desy)) return false;
        var tile = Tiles[x, y].Tile;
        var DesTile = Tiles[Desx, Desy].Tile;
        return (Direction & tile.ExAcross).HaveDirection &&
            (Direction.ReversePosition() & DesTile.ExAcross).HaveDirection;
    }
    // キャラクターが筏の内部へ入れるか判定 :InAcross
    protected bool CanGoToInside(int x, int y, Across Direction, Across Position)
    {
        if (!IsInRange(x, y)) return false;
        if (!Direction.HaveDirection) return false;
        if (!Position.HaveDirection) return false;
        if (Position != Direction.ReversePosition()) return false;
        var tile = Tiles[x, y].Tile;
        return (Position & tile.InAcross) == Position;
    }
    // キャラクターが筏の内部から出られるか判定 :InAcross
    protected bool CanGoFromInside(int x, int y, Across Direction)
    {
        if (!IsInRange(x, y)) return false;
        if (!Direction.HaveDirection) return false;
        var tile = Tiles[x, y].Tile;
        return (Direction & tile.InAcross) == Direction;
    }
    // 筏の移動は水のマスとのSwapで実装する
    // そのためオーバーライド可能にしておく
    // 結果はUnityWorld上で位置を直接入れ替えて反映する。
    protected virtual void SwapTileMaps(int x1, int y1, int x2, int y2)
    {
        var tmp = Tiles[x1, y1];
        Tiles[x1, y1] = Tiles[x2, y2];
        Tiles[x2, y2] = tmp;
        Tiles[x1, y1].transform.position = GetPositionFromPuzzlePosition(x1, y1);
        Tiles[x2, y2].transform.position = GetPositionFromPuzzlePosition(x2, y2);
        var e1 = Tiles[x1, y1].GetComponent<EditableTileObject>();
        if (e1 != null)
        {
            e1.ForClickX = x1;
            e1.ForClickY = y1;
        }
        var e2 = Tiles[x2, y2].GetComponent<EditableTileObject>();
        if (e2 != null)
        {
            e2.ForClickX = x2;
            e2.ForClickY = y2;
        }
    }
    protected bool IsInRange(int x, int y) { return (x >= 0 && y >= 0 && x < Tiles.GetLength(0) && y < Tiles.GetLength(1)); }

    // 上陸出来るなら上陸する / プレイヤーの移動
    protected Across PlayerTilePos = new Across(false, false, false, false, true);
    protected int px, py;
    protected bool isPlayerInside() { return PlayerTilePos.C; }
    // 1 乗り継ぎ
    // 2 下が筏で目の前が水->動かせる
    // 3 下が筏で目の前が筏->上陸できないかつ動かせる(行き先が水なら)なら押す
    protected enum MoveType { Moved, Pushed, DidntMove }
    protected MoveType MoveIkada(int x, int y, Across Direction)
    {
        if (!Direction.HaveDirection) return MoveType.DidntMove;
        if (Direction.HaveTiltDirection) return MoveType.DidntMove;
        //if (CanAcrossRide(x, y, Direction, Position)) return false;
        var tile = Tiles[x, y].Tile;
        if (tile.tileType != Tile.TileType.Ikada) return MoveType.DidntMove;
        if (!(tile.ExAcross & Direction).HaveDirection) return MoveType.DidntMove;
        int Desx = x + Direction.Horizontal;
        int Desy = y + Direction.Vertical;
        if (!IsInRange(Desx, Desy)) return MoveType.DidntMove;
        var DesTile = Tiles[Desx, Desy].Tile;
        if (DesTile.tileType == Tile.TileType.Water)
        {
            px += Direction.Horizontal;
            py += Direction.Vertical;
            SwapTileMaps(Desx, Desy, x, y);
            return MoveType.Moved;
        }
        else if (DesTile.tileType == Tile.TileType.Ikada)
        {
            int Des2x = Desx + Direction.Horizontal;
            int Des2y = Desy + Direction.Vertical;
            if (!IsInRange(Des2x, Des2y)) return MoveType.DidntMove;
            var Des2Tile = Tiles[Des2x, Des2y].Tile;
            if (Des2Tile.tileType == Tile.TileType.Water && (tile.ExAcross & Direction).HaveDirection)
            {
                SwapTileMaps(Desx, Desy, Des2x, Des2y);
                return MoveType.Pushed;
            }
        }
        return MoveType.DidntMove;
    }
    // キャラクターの移動
    protected MoveType MoveCharacters(int dx, int dy)
    {
        Across direction = new Across(dx == 1, dx == -1, dy == 1, dy == -1, false);
        var centerPosition = new Across(false, false, false, false, true);
        //基本的に内側に行かせて、行けないときのみ端にする
        var mtlist = new Stack<MoveType>();
        while (true)
        {
            int prepx = px, prepy = py;
            var prepos = PlayerTilePos;
            if (isPlayerInside())
            {
                if (CanGoFromInside(px, py, direction)) PlayerTilePos = direction;
                else mtlist.Push(MoveIkada(px, py, direction));
            }
            else
            {
                if (CanGoToInside(px, py, direction, PlayerTilePos))
                    PlayerTilePos = centerPosition;
                else if (CanAcrossRide(px, py, direction, PlayerTilePos))
                {
                    mtlist.Push(MoveType.Moved);
                    PlayerTilePos = PlayerTilePos.ReversePosition();
                    px += dx; py += dy;
                    break;
                }
                else
                {
                    mtlist.Push(MoveIkada(px, py, direction));
                    if (mtlist.Peek() != MoveType.DidntMove) break;
                }
            }
            if (prepx == px && prepy == py && prepos == PlayerTilePos) break;
            if (isPlayerInside()) break;
        }

        if ((Tiles[px, py].Tile.InAcross & PlayerTilePos).HaveDirection) { PlayerTilePos = new Across(false, false, false, false, true); }
        if (mtlist.Contains(MoveType.Moved)) return MoveType.Moved;
        else if (mtlist.Contains(MoveType.Pushed)) return MoveType.Pushed;
        else return MoveType.DidntMove;
    }
}

