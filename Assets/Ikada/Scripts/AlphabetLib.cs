using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AlphabetLib
{
    public static int FromAlphabet(char c)
    {
        if ('0' <= c && c <= '9') return c - '0';
        else if ('a' <= c && c <= 'z') return c - 'a' + 10;
        else return '~';
    }
    public static char ToAlphabet(int i)
    {
        if (0 <= i && i <= 9) return (char)(i + '0');
        else return (char)((i - 10) + 'a');
    }
    public static bool[] FromAlphabetToBool5(char c)
    {
        int I = FromAlphabet(c);
        int[] pow2 = new int[] { 1, 2, 4, 8, 16, 32 };
        bool[] b = new bool[5];
        for (int i = 0; i < 5; i++) b[i] = (I & pow2[i]) / pow2[i] == 1;
        return b;
    }
    public static char ToAlphabetFromBool5(bool[] b)
    {
        int I = 0;
        for (int i = 0, p2 = 1; i < b.Length; i++, p2 *= 2)
            I += b[i] ? p2 : 0;
        return ToAlphabet(I);
    }
}
