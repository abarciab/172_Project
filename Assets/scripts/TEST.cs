using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;

[ExecuteAlways]
public class TEST : MonoBehaviour 
{
    public bool solve;
    public int n;
    public int k;
    public int l;
    int moves;


    private void Update()
    {
        n = (int)Mathf.Pow(4, l);
        k = (int)Mathf.Pow(2, l) + 1;

        if (solve) {
            moves = 0;
            solve = false;

            BetterHanoi(3, 1, 3, 3);

            print("MOVES: " + moves);
        }
    }

    void BetterHanoi(int T, int S, int D, int n)
    {
        if (this.n == 0) return;

        int m = (int) MathF.Ceiling(MathF.Sqrt(2 * (float) n + 1)) - 1;
        int k = this.n - m * (m + 1) / 2;

        BetterHanoi(T, S, D, k);

        List<int> Auxiliary = Enumerable.Range(1, m)
        .Where(i => i != T && i != S && i != D).ToList();

        BetterHanoi(T, S, Auxiliary[0], m);
        print("Moving " + n + " from " + T + " to " + D);
        moves += 1;
        BetterHanoi(Auxiliary[0], S, D, m);
    }



    void Hannoi(int numDisks, int origin, int dest, int aux)
    {
        if (numDisks == 0) return;
        if (numDisks > 1) Hannoi(numDisks - 1, origin, aux, dest);

        if (Mathf.Abs(dest - origin) > 1) {
            Hannoi(numDisks - 1, aux, dest, origin);
            print("Moving " + numDisks + " from " + origin + " to " + aux);
            moves += 1;
            Hannoi(numDisks - 1, dest, origin, aux);
            print("Moving " + numDisks + " from " + aux + " to " + dest);
            moves += 1;
            Hannoi(numDisks - 1, origin, aux, dest);
        }
        else {
            print("Moving " + numDisks + " from " + origin + " to " + dest);
            moves += 1;
        }

        Hannoi(numDisks-1, aux, dest, origin);
    }
}
