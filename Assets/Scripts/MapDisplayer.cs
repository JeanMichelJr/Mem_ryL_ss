using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MapDisplayer : MonoBehaviour
{
    public Sprite monsterRoom;
    public Sprite selectedMonsterRoom;
    public Sprite eventRoom;
    public Sprite selectedEventRoom;
    public Sprite bossRoom;
    public GameObject dungeonRoom;
    public RectTransform displayGraphTransform;
    public GameObject Tuto;
    private ScrollRect scroll;

    private int deltaHeight = 100;
    private int minHeight = 60;
    private float border = 0.1f;

    private int scrollDirection = 0;
    float scrollAccelerator = 1f;

    private Graph graph;

    private void Awake()
    {
        if(Player.instance.graph.currentRoomType == Graph.RoomType.Boss)
        {
            Player.instance.visitNextLevel();
        }
        graph = Player.instance.graph;
        scroll = GetComponentInChildren<ScrollRect>();
        
        // Set user inputs
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressNumber += SelectRoom;
            InputManager.instance.onUIVertical += ChangeScrollingDirection;
        }
        
        DisplayGraph();
    }
    
    private void LateUpdate()
    {
        if (scrollDirection == 0)
        {
            if (scrollAccelerator > 1f)
                scrollAccelerator = 1f;
            return;
        }

        if (scrollDirection == 1)
        {
            scroll.verticalNormalizedPosition += 0.001f * scrollAccelerator;
            if (scroll.verticalNormalizedPosition > 1f)
                scroll.verticalNormalizedPosition = 1f;
        }
        else if (scrollDirection == -1)
        {
            scroll.verticalNormalizedPosition -= 0.001f * scrollAccelerator;
            if (scroll.verticalNormalizedPosition < 0f)
                scroll.verticalNormalizedPosition = 0f;
        }

        scrollAccelerator += 0.02f;
    }
    
    private void ChangeScrollingDirection(int dir)
    {
        scrollDirection = dir;
    }
    
    private void CreateRoom(Vector2 pos, int id, Graph.RoomType type)
    {

        GameObject go = Instantiate(dungeonRoom, displayGraphTransform, false);
        go.name = id.ToString();
        switch (type)
        {
            case Graph.RoomType.Boss:
                go.GetComponent<Image>().sprite = bossRoom;
                break;
            case Graph.RoomType.Fight:
                go.GetComponent<Image>().sprite = monsterRoom;
                if (graph.hasVisited(graph.coord(id)))
                {
                    go.GetComponent<Image>().sprite = selectedMonsterRoom;
                }
                break;
            case Graph.RoomType.Event:
                go.GetComponent<Image>().sprite = eventRoom;
                if (graph.hasVisited(graph.coord(id)))
                {
                    go.GetComponent<Image>().sprite = selectedEventRoom;
                }
                break;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = type == Graph.RoomType.Boss ? new Vector2(100, 100) : new Vector2(40, 40);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
    }
    
    private void CreateArc((int,int) room1, (int,int) room2)
    {
        var pos1 = graph.positions[graph.id(room1.Item1, room1.Item2)];
        var pos2 = graph.positions[graph.id(room2.Item1, room2.Item2)];

        GameObject go = new GameObject("arc", typeof(Image));
        go.transform.SetParent(displayGraphTransform, false);
        go.GetComponent<Image>().color = graph.hasVisited(room1) && graph.hasVisited(room2) ? new Color32(0,0,0, 100) : new Color32(125, 125, 125, 100);
        RectTransform rt = go.GetComponent<RectTransform>();
        Vector2 dir = (pos2 - pos1).normalized;
        float distance = Vector2.Distance(pos1, pos2);

        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);

        Vector2 differenceVector = pos2 - pos1;

        rt.sizeDelta = new Vector2(differenceVector.magnitude, 2f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = pos1;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
        rt.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void DisplayGraph()
    {
        int height = graph.GetLength(0);
        int width =  graph.GetLength(1);
        var random = new System.Random();

        List<((int,int),(int,int))> arcsToAdd = new List<((int, int), (int, int))>();

        //Randomize des positions des sommets lors du premier affichage
        if (!graph.positions.Any())
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                { 
                    if(graph[h,w].type > Graph.RoomType.Empty)
                    {
                        if(h == height-1)
                        {
                            graph.positions.Add(graph.id(h, w), new Vector2(displayGraphTransform.rect.width / 2, (height) * deltaHeight));
                            continue;
                        }
                        float containerWidth = displayGraphTransform.rect.width;
                        var itemWidth = (containerWidth - 2 * border * containerWidth) / graph.GetLength(1);
                        var temp = graph.id(h, w);
                        graph.positions.Add(graph.id(h,w), new Vector2(
                            itemWidth * w + border * containerWidth + random.Next(0, (int)itemWidth / 2), 
                            deltaHeight * h + minHeight + random.Next(0, deltaHeight / 2)
                        ));
                    }
                }
            }
        }
        
        for (int h = 0; h < height;  h++)
        {
            for (int w = 0; w < width; w++)
            {
                if(graph[h,w].type > Graph.RoomType.Empty)
                {
                    foreach(var succ in graph[h, w].successeurs)
                    {
                        CreateArc((h, w), (succ.Item1, succ.Item2));
                    }

                    CreateRoom(graph.positions[graph.id(h,w)], (h*width + w), graph[h,w].type);
                }
            }
        }

        displayGraphTransform.sizeDelta = new Vector2(displayGraphTransform.rect.width, (height) * deltaHeight + minHeight*2);

        UpdateSelectedRooms();
    }
    
    private void SelectRoom(int k)
    {
        var previousReachableRooms = graph.reachableRooms;

        if (!graph.visitReachableRoom(k))
        {
            return;
        }

        var selectedRoomType = graph.currentRoomType;
        var selectedRoomCoord = graph.currentRoom;
        var selectedRoom = GameObject.Find(graph.id(selectedRoomCoord.Item1, selectedRoomCoord.Item2).ToString());

        if (selectedRoomType == Graph.RoomType.Event)
                selectedRoom.GetComponent<Image>().sprite = selectedEventRoom;
        if(selectedRoomType == Graph.RoomType.Fight)
                selectedRoom.GetComponent<Image>().sprite = selectedMonsterRoom;

        foreach (var r in previousReachableRooms)
        {
            GameObject.Find(graph.id(r.Item1, r.Item2).ToString()).GetComponentInChildren<Text>().enabled = false;
        }

        UpdateSelectedRooms();

        switch (graph.currentRoomType)
        {
            case Graph.RoomType.Fight:
                GameController.instance.MoveToState(GameState.Combat);
                break;
            case Graph.RoomType.Boss:
                if (Player.instance.roomLevel == Player.instance.finalRoomLevel)
                {
                    if (InputManager.instance != null)
                    {
                        InputManager.instance.onPressNumber -= SelectRoom;
                        InputManager.instance.onUIVertical -= ChangeScrollingDirection;
                    }
                    EventManager.instance.TriggerNewEvent(true);
                }
                else
                {
                    GameController.instance.MoveToState(GameState.Combat);
                }
                break;
            case Graph.RoomType.Event:
                if (InputManager.instance != null)
                {
                    InputManager.instance.onPressNumber -= SelectRoom;
                    InputManager.instance.onUIVertical -= ChangeScrollingDirection;
                }
                EventManager.instance.TriggerNewEvent();
                break;

        }
    }

    private void UpdateSelectedRooms()
    {
        //Salles sélectionnables
        var i = 1;
        foreach (var sommet in graph.reachableRooms.OrderBy(x => x.Item2))
        {
            var nextRoom = GameObject.Find((sommet.Item1 * graph.GetLength(1) + sommet.Item2).ToString());
            var text = nextRoom.GetComponentInChildren<Text>();
            text.enabled = true;
            text.text = i.ToString();
            i++;
        }

        var selectedRoomCoord = graph.currentRoom;
        var selectedRoom = GameObject.Find(graph.id(selectedRoomCoord.Item1, selectedRoomCoord.Item2).ToString());

        var newPosition = displayGraphTransform.localPosition;
        newPosition.y = -selectedRoom.GetComponent<RectTransform>().localPosition.y - displayGraphTransform.parent.gameObject.GetComponent<RectTransform>().rect.height / 2 + minHeight;
        displayGraphTransform.localPosition = newPosition;

        if (graph.visitedRooms.Count == 0)
        {
            Tuto.SetActive(true);
        }
        else if (Tuto.activeSelf)
        {
            Tuto.SetActive(false);
        }
    }

    public void EventMenuClosed()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressNumber += SelectRoom;
            InputManager.instance.onUIVertical += ChangeScrollingDirection;
        }

        if(graph.currentRoomType == Graph.RoomType.Boss && Player.instance.roomLevel == Player.instance.finalRoomLevel)
        {
            GameController.instance.MoveToState(GameState.Combat);
        }
    }

    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressNumber -= SelectRoom;
            InputManager.instance.onUIVertical -= ChangeScrollingDirection;
        }
    }
}