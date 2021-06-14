using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtension
{
    private static Random _rnd;
    private static Random rnd { get => _rnd ?? (_rnd = new Random()); }
    public static List<T> Shuffle<T>(this List<T> l)
    {
        int n = l.Count;
        while (n > 1)
        {
            n--;
            var k = rnd.Next(n + 1);
            T value = l[k];
            l[k] = l[n];
            l[n] = value;
        }

        return l;
    }

    public static List<T> Shuffle<T>(this List<T> l, Converter<T,T> copier)
    {
        var new_list = l.ConvertAll(copier);
        
        return new_list.Shuffle();
    }

    public static List<T> TakeNFirstRnd<T>(this List<T> l, int n_elements, Converter<T,T> copier, IEnumerable<T> except = null)
    {
        var ul = l;
        if (except != null)
        {
            ul = l.Where( e => !except.Contains(e) ).ToList();
        }
        
        if (n_elements > ul.Count)
        {
            n_elements = ul.Count;
        }

        return ul.Shuffle(copier).Take(n_elements).ToList();
    }
}
