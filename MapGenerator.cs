using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("地图配置表")]
    public MapConfigSO mapConfig;

    [Header("地图布局")]
    public MapLayoutSO mapLayout;

    [Header("预制体")]
    public Room roomPrefab;
    public LineRenderer linePrefab;

    private float screenHeight; //世界坐标的高度位置 （y轴
    private float screenWidth;  //世界坐标的宽度位置（x轴

    private float columnWidth; //col和col之间所间隔的宽度
    private Vector3 generatePoint; //当前的生成位置
    
    private float border = 1.5f;

    private List<Room> rooms = new();
    private List<LineRenderer> lines = new();

    public List<RoomDataSO> roomDataList = new();//储存所有的房间类型
    private Dictionary<RoomType, RoomDataSO> roomDataDict = new(); //Hash type -> RoomdataSO方便查找

    private void Awake()
    {
        screenHeight = Camera.main.orthographicSize * 2; //屏幕的高度等于正交尺寸*2
        screenWidth = screenHeight * Camera.main.aspect; //屏幕的宽度等于当前屏幕的长宽比

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

   
    public void CreateMap() //根据mapConfig来随机生成地图
    {

        List<Room> previousColumnRooms = new(); //前一列的所有房间


        for (int column = 0; column < mapConfig.roomBlueprints.Count; ++column)
        {
            var blueprint = mapConfig.roomBlueprints[column]; //获取每一个col的blueprint

            var amount = UnityEngine.Random.Range(blueprint.min, blueprint.max); //生成一个min max之间的随机数

            var startHeight = screenHeight / 2 - screenHeight/ (amount + 1); //屏幕最顶点 往下移动1个房间的间距

            generatePoint = new Vector3 (-screenWidth/2 + border + columnWidth*column, startHeight, 0); //设置起始点生成位

            var newPosition = generatePoint; //当前生成点位

            List<Room> currentColumnRooms = new(); //当前列的所有房间

            var roomGapY = screenHeight / (amount + 1);

            for (int i = 0; i < amount; ++i)
            {
                //最后一个col， BOSS房间
                if (column == mapConfig.roomBlueprints.Count - 1)
                {
                    newPosition.x = screenWidth / 2 - border * 2;
                } else if (column != 0) { //非第一列
                    newPosition.x = generatePoint.x + UnityEngine.Random.Range(-border/2, border/2); //让每个房间的左右随机offset
                }

                newPosition.y = startHeight - roomGapY * i;
                //生成房间
                var room = Instantiate(roomPrefab, newPosition, Quaternion.identity, transform);
                RoomType newType = GetRandomRoomType(mapConfig.roomBlueprints[column].roomType);

                //设置只有第一列房间允许进入
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
            //如果privious count > 0 那当前列不是第一列
            if (previousColumnRooms.Count > 0)
            {
                //创建2列之间所有room的连接
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

        //确保column2的所有房间都有连接的房间
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

        //创建连线
        var line = Instantiate(linePrefab, transform);
        if (!check) //正向连接
        {
            room.linkTo.Add(new(targetRoom.column, targetRoom.line));
            line.SetPosition(0, room.transform.position);
            line.SetPosition(1, targetRoom.transform.position);
        } else //反向连接
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
        //添加所有已生成的房间
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
        //添加所有连线
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
        //读取房间数据生成房间
        for (int i = 0;i < mapLayout.mapRoomDataList.Count; ++i)
        {
            var newPos = new Vector3(mapLayout.mapRoomDataList[i].posX, mapLayout.mapRoomDataList[i].posY, 0);
            var newRoom = Instantiate(roomPrefab, newPos, Quaternion.identity, transform);
            newRoom.roomState = mapLayout.mapRoomDataList[i].roomState;
            newRoom.SetupRoom(mapLayout.mapRoomDataList[i].column, mapLayout.mapRoomDataList[i].line, mapLayout.mapRoomDataList[i].roomData);
            newRoom.linkTo = mapLayout.mapRoomDataList[i].linkTo;
            rooms.Add(newRoom);
        }

        //读取连线
        for (int i = 0; i < mapLayout.linePositionList.Count; i++) 
        {
            var line = Instantiate(linePrefab, transform);
            line.SetPosition(0, mapLayout.linePositionList[i].startPos.ToVector3());
            line.SetPosition(1, mapLayout.linePositionList[i].endPos.ToVector3());

            lines.Add(line);
        }
    }
}
