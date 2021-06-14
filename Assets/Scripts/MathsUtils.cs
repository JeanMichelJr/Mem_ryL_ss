using UnityEngine;

public static class MathsUtils
{
    public static int RoundToIntNonZero(float f)
    {
        var t = Mathf.RoundToInt(f);
        return t == 0 ? t + 1 : t;
    }
}
