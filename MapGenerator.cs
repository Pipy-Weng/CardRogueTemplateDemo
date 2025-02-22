using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("��ͼ���ñ�")]
    public MapConfigSO mapConfig;

    [Header("��ͼ����")]
    public MapLayoutSO mapLayout;

    [Header("Ԥ����")]
    public Room roomPrefab;
    public LineRenderer linePrefab;

    private float screenHeight; //��������ĸ߶�λ�� ��y��
    private float screenWidth;  //��������Ŀ��λ�ã�x��

    private float columnWidth; //col��col֮��������Ŀ��
    private Vector3 generatePoint; //��ǰ������λ��
    
    private float border = 1.5f;

    private List<Room> rooms = new();
    private List<LineRenderer> lines = new();

    public List<RoomDataSO> roomDataList = new();//�������еķ�������
    private Dictionary<RoomType, RoomDataSO> roomDataDict = new(); //Hash type -> RoomdataSO�������

    private void Awake()
    {
        screenHeight = Camera.main.orthographicSize * 2; //��Ļ�ĸ߶ȵ��������ߴ�*2
        screenWidth = screenHeight * Camera.main.aspect; //��Ļ�Ŀ�ȵ��ڵ�ǰ��Ļ�ĳ����

        columnWidth = screenWidth / (mapConfig.roomBlueprints.Count + 1);

        foreach (var roomData in roomDataList)
        {
            roomDataDict.Add(roomData.roomType, roomData);
        }
    }

    private void OnEnable()
    {
        if (mapLayout.mapRoomDataList.Count > 0)
        {
            LoadMap();
        } else
        {
            CreateMap();
        }
    }

    private void OnDisable()
    {
    }

    private void Start()
    {
        //CreateMap();
    }

   
    public void CreateMap() //����mapConfig��������ɵ�ͼ
    {

        List<Room> previousColumnRooms = new(); //ǰһ�е����з���


        for (int column = 0; column < mapConfig.roomBlueprints.Count; ++column)
        {
            var blueprint = mapConfig.roomBlueprints[column]; //��ȡÿһ��col��blueprint

            var amount = UnityEngine.Random.Range(blueprint.min, blueprint.max); //����һ��min max֮��������

            var startHeight = screenHeight / 2 - screenHeight/ (amount + 1); //��Ļ��� �����ƶ�1������ļ��

            generatePoint = new Vector3 (-screenWidth/2 + border + columnWidth*column, startHeight, 0); //������ʼ������λ

            var newPosition = generatePoint; //��ǰ���ɵ�λ

            List<Room> currentColumnRooms = new(); //��ǰ�е����з���

            var roomGapY = screenHeight / (amount + 1);

            for (int i = 0; i < amount; ++i)
            {
                //���һ��col�� BOSS����
                if (column == mapConfig.roomBlueprints.Count - 1)
                {
                    newPosition.x = screenWidth / 2 - border * 2;
                } else if (column != 0) { //�ǵ�һ��
                    newPosition.x = generatePoint.x + UnityEngine.Random.Range(-border/2, border/2); //��ÿ��������������offset
                }

                newPosition.y = startHeight - roomGapY * i;
                //���ɷ���
                var room = Instantiate(roomPrefab, newPosition, Quaternion.identity, transform);
                RoomType newType = GetRandomRoomType(mapConfig.roomBlueprints[column].roomType);

                //����ֻ�е�һ�з����������
                if (column == 0)
                {
                    room.roomState = RoomState.Attainable;
                } else
                {
                    room.roomState = RoomState.Locked;

                }

                room.SetupRoom(column, i, GetRoomData(newType));
                rooms.Add(room);
                currentColumnRooms.Add(room);
            }
            //���privious count > 0 �ǵ�ǰ�в��ǵ�һ��
            if (previousColumnRooms.Count > 0)
            {
                //����2��֮������room������
                CreateConnections(previousColumnRooms, currentColumnRooms);
            }

            previousColumnRooms = currentColumnRooms;
        }

        SaveMap();
    }

    private void CreateConnections(List<Room> column1, List<Room> column2)
    {
        HashSet<Room> connectedColumn2Rooms = new();
        foreach (Room room in column1)
        {
            var targetRoom = ConnectToRandomRoom(room, column2, false);
            connectedColumn2Rooms.Add(targetRoom);
        }

        //ȷ��column2�����з��䶼�����ӵķ���
        foreach (Room room in column2)
        {
            if (!connectedColumn2Rooms.Contains(room))
            {
                ConnectToRandomRoom(room, column1, true);
            }
        }
    }

    private Room ConnectToRandomRoom(Room room, List<Room> column2, bool check)
    {
        Room targetRoom;

        targetRoom = column2[UnityEngine.Random.Range(0, column2.Count)];

        //��������
        var line = Instantiate(linePrefab, transform);
        if (!check) //��������
        {
            room.linkTo.Add(new(targetRoom.column, targetRoom.line));
            line.SetPosition(0, room.transform.position);
            line.SetPosition(1, targetRoom.transform.position);
        } else //��������
        {
            targetRoom.linkTo.Add(new(room.column, room.line));
            line.SetPosition(0, targetRoom.transform.position);
            line.SetPosition(1, room.transform.position);
        }


        lines.Add(line);
        return targetRoom;
    }

    [ContextMenu("RegenerateRoom")]
    public void RegenerateRoom()
    {
        foreach (var room in rooms)
        {
            Destroy(room.gameObject);
        
        }

        foreach (var line in lines)
        {
            Destroy(line.gameObject);
        }

        rooms.Clear();
        lines.Clear();

        CreateMap();
    }

    private RoomDataSO GetRoomData(RoomType roomType)
    {
        return roomDataDict[roomType];
    }

    private RoomType GetRandomRoomType(RoomType flags)
    {
        string[] options = flags.ToString().Split(',');

        string randomOption = options[UnityEngine.Random.Range(0, options.Length)];

        return (RoomType)Enum.Parse(typeof(RoomType), randomOption);
    }

    private void SaveMap()
    {
        mapLayout.mapRoomDataList = new();
        //������������ɵķ���
        for (int i = 0; i < rooms.Count; ++i)
        {
            var room = new MapRoomData()
            {
                posX = rooms[i].transform.position.x,
                posY = rooms[i].transform.position.y,
                column = rooms[i].column,
                line = rooms[i].line,
                roomData = rooms[i].roomData,
                roomState = rooms[i].roomState,
                linkTo = rooms[i].linkTo
            };
            mapLayout.mapRoomDataList.Add(room);
        }
        mapLayout.linePositionList = new();
        //�����������
        for (int i = 0; i < lines.Count; ++i) {
            var line = new LinePosition()
            {
                startPos = new SerializeVector3(lines[i].GetPosition(0)),
                endPos = new SerializeVector3(lines[i].GetPosition(1)),
            };
            mapLayout.linePositionList.Add(line);
        }
    }

    private void LoadMap()
    {
        //��ȡ�����������ɷ���
        for (int i = 0;i < mapLayout.mapRoomDataList.Count; ++i)
        {
            var newPos = new Vector3(mapLayout.mapRoomDataList[i].posX, mapLayout.mapRoomDataList[i].posY, 0);
            var newRoom = Instantiate(roomPrefab, newPos, Quaternion.identity, transform);
            newRoom.roomState = mapLayout.mapRoomDataList[i].roomState;
            newRoom.SetupRoom(mapLayout.mapRoomDataList[i].column, mapLayout.mapRoomDataList[i].line, mapLayout.mapRoomDataList[i].roomData);
            newRoom.linkTo = mapLayout.mapRoomDataList[i].linkTo;
            rooms.Add(newRoom);
        }

        //��ȡ����
        for (int i = 0; i < mapLayout.linePositionList.Count; i++) 
        {
            var line = Instantiate(linePrefab, transform);
            line.SetPosition(0, mapLayout.linePositionList[i].startPos.ToVector3());
            line.SetPosition(1, mapLayout.linePositionList[i].endPos.ToVector3());

            lines.Add(line);
        }
    }
}
