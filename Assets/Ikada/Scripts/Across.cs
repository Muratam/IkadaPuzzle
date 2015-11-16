using UnityEngine;
using System;

public class Across  {
    public readonly bool[,] Mat = new bool[3, 3];
    public bool T { get { return Mat[1, 0]; } set { Mat[1, 0] = value; } }
    public bool L { get { return Mat[0, 1]; } set { Mat[0, 1] = value; } }
    public bool C { get { return Mat[1, 1]; } set { Mat[1, 1] = value; } }
    public bool R { get { return Mat[2, 1]; } set { Mat[2, 1] = value; } }
    public bool B { get { return Mat[1, 2]; } set { Mat[1, 2] = value; } }
	public int GetDiffOrderByLBRT(Across across) {
		Func<Across,int> Order = (ac)=>ac.L ? 0 : ac.B ? 1 : ac.R ? 2 : 3;
		return Order(this) - Order(across);
	}
    public int Vertical {
        get {
            if (T & !B) return 1;
            else if (B & !T) return -1;
            else return 0;
        }
    }
    public int Horizontal {
        get {
            if (R & !L) return 1;
            else if (L & !R) return -1;
            else return 0;
        }
    }
    public bool HaveDirection {
        get {
            int n = 0;
            for (int w = 0; w < 3; w++)
                for (int h = 0; h < 3; h++)
                    if (Mat[w, h] && !(w == 1 && h == 1)) n++;
            return n == 1;
        }
    }
    public bool HaveTiltDirection {
        get {
            if (!HaveDirection) return false;
            return !(T || R || L || B);
        }
    }
    public Across(bool _b) { for (int w = 0; w < 3; w++) for (int h = 0; h < 3; h++) Mat[w, h] = _b; }
    public Across(bool _R, bool _L, bool _T, bool _B, bool _C) {
        R = _R; L = _L; T = _T; B = _B; C = _C;
    }
    public Across(bool[,] Mat) {
        if (Mat.GetLength(0) == 3 && Mat.GetLength(1) == 3)
            this.Mat = (bool[,])Mat.Clone();
    }

    public bool[] GetRLTBC() { bool[] b = new bool[5];b[0] = R; b[1] = L; b[2] = T; b[3] = B; b[4] = C;return b; }

    public static bool NearlyEqualDirection(Across d1, Across d2) {
        if (!d1.HaveDirection || !d2.HaveDirection) return false;
        return (d1.Vertical * d2.Vertical + d1.Horizontal * d2.Horizontal) >= 1;
    }

    public static Across operator &(Across a1, Across a2) {
        var Mat = new bool[3, 3];
        for (int w = 0; w < 3; w++)
            for (int h = 0; h < 3; h++)
                Mat[w, h] = a1.Mat[w, h] & a2.Mat[w, h];
        return new Across(Mat);
    }
    public static bool operator ==(Across a1, Across a2) {
        for (int w = 0; w < 3; w++)
            for (int h = 0; h < 3; h++)
                if (a1.Mat[w, h] != a2.Mat[w, h]) return false;
        return true;
    }
    public static bool operator !=(Across a1, Across a2) { return !(a1 == a2); }
    //上下左右を反転する
    public Across ReversePosition() {
        bool[,] Mat = new bool[3, 3];
        for (int w = 0; w < 3; w++)
            for (int h = 0; h < 3; h++)
                Mat[w, h] = this.Mat[2 - w, 2 - h];
        return new Across(Mat);
    }

}
