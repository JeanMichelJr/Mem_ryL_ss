using System;
public static class StringExtension
{
    public static Tuple<string, string> Slice(this string origin, int index)
    {
        if (index <= 0)
        {
            return Tuple.Create<string, string>(string.Empty, origin);
        }
        
        if(index >= origin.Length)
        {
            return Tuple.Create<string, string>(origin, string.Empty);
        }

        return Tuple.Create<string, string>(origin.Substring(0,index), origin.Substring(index, origin.Length - index));
    }
}
