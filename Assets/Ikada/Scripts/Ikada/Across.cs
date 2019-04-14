using UnityEngine;
using System;


// 筏の端8方向+真ん中に対応した移動を管理する
// 内部的には 3x3の{0,1}の行列。
// 筏の形状に合わせた3x3のbool値から移動可能かを判断するためのもの。
// 現在では4+1方向しか使っていないが、今後斜め移動の可能性もあったのでこのような実装。

public class Across
{
    public readonly bool[,] Mat = new bool[3, 3];
    // Top Left Center Right Bottom
    public bool T { get { return Mat[1, 0]; } set { Mat[1, 0] = value; } }
    public bool L { get { return Mat[0, 1]; } set { Mat[0, 1] = value; } }
    public bool C { get { return Mat[1, 1]; } set { Mat[1, 1] = value; } }
    public bool R { get { return Mat[2, 1]; } set { Mat[2, 1] = value; } }
    public bool B { get { return Mat[1, 2]; } set { Mat[1, 2] = value; } }

    public Across(bool _b) { for (int w = 0; w < 3; w++) for (int h = 0; h < 3; h++) Mat[w, h] = _b; }
    public Across(bool _R, bool _L, bool _T, bool _B, bool _C)
    {
        R = _R; L = _L; T = _T; B = _B; C = _C;
    }
    public Across(bool[,] Mat)
    {
        if (Mat.GetLength(0) == 3 && Mat.GetLength(1) == 3)
            this.Mat = (bool[,])Mat.Clone();
    }

    public bool[] GetRLTBC() { bool[] b = new bool[5]; b[0] = R; b[1] = L; b[2] = T; b[3] = B; b[4] = C; return b; }

    public static bool NearlyEqualDirection(Across d1, Across d2)
    {
        if (!d1.HaveDirection || !d2.HaveDirection) return false;
        return (d1.Vertical * d2.Vertical + d1.Horizontal * d2.Horizontal) >= 1;
    }

    public static Across operator &(Across a1, Across a2)
    {
        var Mat = new bool[3, 3];
        for (int w = 0; w < 3; w++)
            for (int h = 0; h < 3; h++)
                Mat[w, h] = a1.Mat[w, h] & a2.Mat[w, h];
        return new Across(Mat);
    }
    public static bool operator ==(Across a1, Across a2)
    {
        for (int w = 0; w < 3; w++)
            for (int h = 0; h < 3; h++)
                if (a1.Mat[w, h] != a2.Mat[w, h]) return false;
        return true;
    }
    public static bool operator !=(Across a1, Across a2) { return !(a1 == a2); }
    //上下左右を反転する
    public Across ReversePosition()
    {
        bool[,] Mat = new bool[3, 3];
        for (int w = 0; w < 3; w++)
            for (int h = 0; h < 3; h++)
                Mat[w, h] = this.Mat[2 - w, 2 - h];
        return new Across(Mat);
    }


    // 一方向だけの行列に特化
    public int Vertical
    {
        get
        {
            if (T & !B) return 1;
            else if (B & !T) return -1;
            else return 0;
        }
    }
    public int Horizontal
    {
        get
        {
            if (R & !L) return 1;
            else if (L & !R) return -1;
            else return 0;
        }
    }
    public bool HaveDirection
    {
        get
        {
            int n = 0;
            for (int w = 0; w < 3; w++)
                for (int h = 0; h < 3; h++)
                    if (Mat[w, h] && !(w == 1 && h == 1)) n++;
            return n == 1;
        }
    }
    public bool HaveTiltDirection
    {
        get
        {
            if (!HaveDirection) return false;
            return !(T || R || L || B);
        }
    }
    public int OneDirectionToAngle()
    {
        if (this.L) return 2;
        if (this.B) return 1;
        if (this.R) return 0;
        return 3;
    }

}
