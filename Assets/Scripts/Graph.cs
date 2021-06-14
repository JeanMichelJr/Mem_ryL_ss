using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[Serializable]
public class Graph
{
    [SerializeField] private int pEvent = 33;
    [SerializeField] private int p2Succ = 30;
    [SerializeField] private int p3Succ = 5;
    [SerializeField] private int pDiago = 15;
    [SerializeField] private int pHeigher = 15;
    [SerializeField] private int width = 4;
    [SerializeField] private int wBegin = 0;
    [SerializeField] private int wEnd = 4;

    public enum RoomType
    {
        Blocked = -1,
        Empty,
        Fight,
        Event,
        Boss
    }

    public struct Sommet
    {
        public RoomType type;
        public List<(int, int)> successeurs;

        public Sommet(RoomType rt)
        {
            type = rt;
            successeurs = new List<(int, int)>();
        }
    }

    private Sommet[,] matrice;
    public Dictionary<int, Vector2> positions = new Dictionary<int, Vector2>();
    public List<(int, int)> visitedRooms = new List<(int, int)>();
    public List<(int, int)> reachableRooms {
        get
        {
            if (visitedRooms.Any())
                return this[visitedRooms.LastOrDefault().Item1, visitedRooms.LastOrDefault().Item2].successeurs.OrderBy(x => x.Item2).ToList();

            List<(int, int)> sommets = new List<(int, int)>();
            for (int w = 0; w < width; w++)
            {
                if (this[0, w].type > RoomType.Empty)
                {
                    sommets.Add((0, w));
                }
            }
            return sommets;
        }
    }
    public (int, int) currentRoom => visitedRooms.LastOrDefault(); 
    public RoomType currentRoomType => this[currentRoom.Item1, currentRoom.Item2].type;

    public int id(int h, int w)
    {
        return h * width + w;
    }

    public (int,int) coord(int id)
    {
        return (id / width, id % width);
    }

    public int GetLength(int index)
    {
        return matrice.GetLength(index);
    }

    public Sommet this[int index0, int index1]
    {
        get
        {
            if (index0 >= matrice.GetLength(0) || index1 >= matrice.GetLength(1))
            {
                throw new IndexOutOfRangeException();
            }

            return matrice[index0, index1];
        }
    }

    public Sommet this[int index]
    {
        get
        {
            if (index >= matrice.GetLength(0)*matrice.GetLength(1))
            {
                throw new IndexOutOfRangeException();
            }

            var coords = coord(index);
            return matrice[coords.Item1, coords.Item2];
        }
    }

    //Constructor
    public Graph(int height)
    {
        var random = new System.Random();

        var graph = new Sommet[height + 1, width];
        for (int h = 0; h < height + 1; h++)
        {
            for (int w = 0; w < width; w++)
            {
                if (h == 0 && w >= wBegin && w <= wEnd)
                {
                    graph[h, w] = new Sommet(RoomType.Fight);
                }
                else
                {
                    graph[h, w] = new Sommet(RoomType.Empty);
                }
            }
        }

        for (int h = 0; h < height; h++)
        {
            List<int> sommets = new List<int>();
            for (int w = 0; w < width; w++)
            {
                if (graph[h, w].type > RoomType.Empty)
                {
                    sommets.Add(w);
                    //Toutes les dernières salles mènent au boss
                    if (h == height - 1)
                    {
                        graph[h, w].successeurs.Add((height, 0));
                        continue;
                    }

                    //Détermination du nb de successeurs du sommet courant
                    int randomNbSucc = random.Next(0, 100);
                    int succToAdd = 1;
                    if (randomNbSucc < p2Succ)
                        succToAdd = 2;
                    if (randomNbSucc < p3Succ)
                        succToAdd = 3;

                    //Détermination des sommets atteignables depuis le sommet courant
                    List<int> reachableRooms = new List<int>();
                    if(graph[h + 1, w].type >= RoomType.Empty)
                        reachableRooms.Add(w);
                    if (w != 0 && graph[h + 1, w - 1].type >= RoomType.Empty && !graph[h, w - 1].successeurs.Contains((h + 1, w)))  //Eviter les croisements de chemins
                        reachableRooms.Add(w - 1);
                    if (w != width - 1)
                        reachableRooms.Add(w + 1);

                    //Choix aléatoire des sommets
                    while (reachableRooms.Any() && graph[h, w].successeurs.Count < succToAdd)
                    {
                        //Si un seul sommet est disponible, on le choisi
                        if (reachableRooms.Count == 1)
                        {
                            int wArc = reachableRooms.First();
                            graph[h, w].successeurs.Add((h + 1, wArc));
                            reachableRooms.Remove(wArc);
                        }
                        //Sinon si le sommet en face est disponible, pDiago% de chance de choisir un autre sommet que celui-ci
                        else if (reachableRooms.Contains(w))
                        {
                            int randomSucc = random.Next(0, 100);
                            if (randomSucc < pDiago)
                            {
                                var temp = new List<int>(reachableRooms);
                                temp.Remove(w);

                                int wArc = temp[random.Next(temp.Count)];
                                graph[h, w].successeurs.Add((h + 1, wArc));
                                reachableRooms.Remove(wArc);
                            }
                            //Si le sommet en face est choisi, pHeigher% de chance de sauter un étage 
                            else
                            {
                                int randomHeight = random.Next(0, 100);
                                if (h < height - 2 && randomHeight < pHeigher && graph[h + 1, w].type == RoomType.Empty)
                                {
                                    graph[h, w].successeurs.Add((h + 2, w));
                                    graph[h + 1, w].type = RoomType.Blocked;
                                }
                                else
                                {
                                    graph[h, w].successeurs.Add((h + 1, w));
                                }
                                reachableRooms.Remove(w);
                            }
                        }
                        //Sinon sélection équiprobable d'un sommet
                        else
                        {
                            int wArc = reachableRooms[random.Next(reachableRooms.Count)];
                            graph[h, w].successeurs.Add((h + 1, wArc));
                            reachableRooms.Remove(wArc);
                        }
                    }

                    //Mise à jour des sommet successeurs
                    foreach (var sommet in graph[h, w].successeurs)
                    {
                        graph[sommet.Item1, sommet.Item2].type = random.Next(0,100) <= pEvent ? RoomType.Event : RoomType.Fight;
                    }
                }
            }
        }

        //Salle du boss
        graph[height, 0].type = RoomType.Boss;

        this.matrice = graph;
    }

    public bool visitReachableRoom(int index)
    {
        if (index > reachableRooms.Count())
        {
            return false;
        }

        visitedRooms.Add(reachableRooms[index-1]);
        return true;
    }

    public bool hasVisited((int,int) coords)
    {
        foreach ((int,int) room in visitedRooms)
        {
            if (room.Item1 == coords.Item1 && room.Item2 == coords.Item2)
                return true;
        }

        return false;
    }
}