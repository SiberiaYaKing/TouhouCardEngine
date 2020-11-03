﻿using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TouhouCardEngine;
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Reflection;
using NitoriNetwork.Common;
using TouhouCardEngine.Shared;
using ILogger = TouhouCardEngine.Shared.ILogger;
namespace Tests
{
    public class RoomTest
    {
        [Test]
        public void localRoomCreateTest()
        {
            createLocalRoomAndAssert(createRoomAssert);
        }
        void createLocalRoomAndAssert(Action<Room> onAssert)
        {
            using (ClientLogic client = new ClientLogic(new UnityLogger("Room")))
            {
                client.createLocalRoom();
                onAssert(client.room);
            }
        }
        private static void createRoomAssert(Room room)
        {
            Assert.AreEqual(1, room.getPlayers().Length);
            Assert.IsInstanceOf<LocalRoomPlayer>(room.getPlayers()[0]);
            Assert.AreNotEqual(0, room.getPlayers()[0].id);
        }
        [Test]
        public void localRoomAddAIPlayerTest()
        {
            createLocalRoomAndAssert(addAIPlayerAssert);
        }
        private static void addAIPlayerAssert(Room room)
        {
            room.addPlayer(new AIRoomPlayer(), new RoomPlayerData("AI", RoomPlayerType.ai));
            Assert.AreEqual(2, room.getPlayers().Length);
            Assert.IsInstanceOf<LocalRoomPlayer>(room.getPlayers()[0]);
            Assert.AreNotEqual(0, room.getPlayers()[0].id);
            Assert.IsInstanceOf<AIRoomPlayer>(room.getPlayers()[1]);
            Assert.AreNotEqual(0, room.getPlayers()[1].id);
        }
        [Test]
        public void localRoomSetPropTest()
        {
            createLocalRoomAndAssert(setPropAssert);
        }

        private static void setPropAssert(Room room)
        {
            room.setProp("key", "value");
            Assert.AreEqual("value", room.getProp<string>("key"));
        }

        [Test]
        public void localRoomSetPlayerPropTest()
        {
            createLocalRoomAndAssert(setPlayerPropAssert);
        }

        private static void setPlayerPropAssert(Room room)
        {
            room.setPlayerProp(room.data.ownerId, "key", "value");
            Assert.AreEqual("value", room.getPlayerProp<string>(room.data.ownerId, "key"));
        }

        [Test]
        public void localRoomRemovePlayerTest()
        {
            createLocalRoomAndAssert(removePlayerAssert);
        }

        private static void removePlayerAssert(Room room)
        {
            var player = new AIRoomPlayer();
            room.addPlayer(player, new RoomPlayerData("AI", RoomPlayerType.ai));
            room.removePlayer(player.id);
            Assert.Null(room.data.getPlayerData(player.id));
        }

        [Test]
        public void serializeTest()
        {
            RoomData data = new RoomData { ownerId = 1 };
            data.propDict.Add("randomSeed", 42);
            data.playerDataList.Add(new RoomPlayerData(1, "玩家", RoomPlayerType.human));
            data.playerDataList[0].propDict.Add("name", "you know who");

            string typeName = data.GetType().FullName;
            BsonDocument bsonDoc = data.ToBsonDocument();

            data = BsonSerializer.Deserialize(bsonDoc, TypeHelper.getType(typeName)) as RoomData;
            Assert.AreEqual(1, data.ownerId);
            Assert.AreEqual(42, data.propDict["randomSeed"]);
            Assert.AreEqual(1, data.playerDataList[0].id);
            Assert.AreEqual("玩家", data.playerDataList[0].name);
            Assert.AreEqual(RoomPlayerType.human, data.playerDataList[0].type);
            Assert.AreEqual("you know who", data.playerDataList[0].propDict["name"]);
        }
        [UnityTest]
        public IEnumerator LANRoomCreateTest()
        {
            UnityLogger logger = new UnityLogger("RoomLocal");
            using (ClientLogic client = new ClientLogic(logger))
            {
                yield return client.createOnlineRoom(new RoomPlayerData("本地玩家", RoomPlayerType.human)).wait();
            }
        }
        //[Test]
        //public void rpcLocalTest()
        //{
        //    ServerNetworking server = new ServerNetworking(null);
        //    MethodInfo method = GetType().GetMethod(nameof(testRPCMethod));
        //    Assert.NotNull(method);
        //    server.addRPCMethod(method);
        //    var rpcMethod = server.getRPCMethod(nameof(testRPCMethod), 1, null, new RPCArg(), new RPCArg());
        //    Assert.NotNull(rpcMethod);
        //    Assert.AreEqual(method, rpcMethod);
        //}
        //public void testRPCMethod(int primArg, object objArg, RPCArgBase varArg, IRPCArg iArg)
        //{
        //    Debug.Log("参数：" + primArg + objArg + varArg + iArg);
        //}
        //public class RPCArgBase { }
        //public class RPCArg : RPCArgBase, IRPCArg { }
        //public interface IRPCArg { }
        ///// <summary>
        ///// 创建房间测试。
        ///// </summary>
        ///// <returns></returns>
        //[UnityTest]
        //public IEnumerator clientRoomCreateTest()
        //{
        //    yield return createClientRoomAndAssert(onAssert);
        //    IEnumerator onAssert(Room room)
        //    {
        //        createRoomAssert(room);
        //        yield break;
        //    }
        //}
        //IEnumerator createClientRoomAndAssert(Func<Room, IEnumerator> onAssert)
        //{
        //    UnityLogger logger = new UnityLogger("Room");
        //    using (ClientLogic client = new ClientLogic())
        //    {
        //        client.networking.start();
        //        new GameObject(nameof(client)).AddComponent<Updater>().action = () => client.networking.update();
        //        using (ServerLogic server = new ServerLogic(logger))
        //        {
        //            server.networking.start();
        //            new GameObject(nameof(server)).AddComponent<Updater>().action = () => server.networking.update();
        //            //连接到服务器
        //            yield return client.networking.connect(server.networking.ip, server.networking.port).wait();
        //            //创建房间
        //            yield return client.createOnlineRoom(new RoomPlayerData("玩家1", RoomPlayerType.human)).wait();
        //            yield return onAssert?.Invoke(client.room);
        //        }
        //    }
        //}
        //[UnityTest]
        //public IEnumerator clientRoomJoinTest()
        //{
        //    yield return createClient2RoomAndAssert(onAssert);
        //    IEnumerator onAssert(Room room1, Room room2)
        //    {
        //        Assert.AreEqual(2, room1.getPlayers().Length);
        //        Assert.AreEqual(2, room2.getPlayers().Length);
        //        yield break;
        //    }
        //}
        //IEnumerator createClient2RoomAndAssert(Func<Room, Room, IEnumerator> onAssert)
        //{
        //    UnityLogger logger = new UnityLogger("Room");
        //    using (HostNetworking server = new ServerNetworking(null, logger))
        //    {
        //        server.start();
        //        new GameObject(nameof(server)).AddComponent<Updater>().action = () => server.update();
        //        using (ClientNetworking client1 = new ClientNetworking(logger))
        //        {
        //            client1.start();
        //            new GameObject(nameof(client1)).AddComponent<Updater>().action = () => client1.update();
        //            using (ClientNetworking client2 = new ClientNetworking(logger))
        //            {
        //                client2.start();
        //                new GameObject(nameof(client2)).AddComponent<Updater>().action = () => client2.update();
        //                //连接到服务器
        //                yield return client1.connect(server.ip, server.port).wait();
        //                yield return client2.connect(server.ip, server.port).wait();
        //                //创建房间
        //                var task = client1.reqCreateRoom(new RoomPlayerData(0, "玩家1", RoomPlayerType.human));
        //                yield return task.wait();
        //                RoomData roomData = task.Result;
        //                OnlineRoom room1 = new OnlineRoom(client1, roomData);
        //                //加入房间
        //                task = client2.reqJoinRoom(roomData.id, new RoomPlayerData(0, "玩家2", RoomPlayerType.human));
        //                yield return task.wait();
        //                roomData = task.Result;
        //                OnlineRoom room2 = new OnlineRoom(client2, roomData);
        //                yield return onAssert?.Invoke(room1, room2);
        //            }
        //        }
        //    }
        //}
        //[UnityTest]
        //public IEnumerator clientRoomAddAIPlayerTest()
        //{
        //    yield return createClientRoomAndAssert(onAssert);
        //    IEnumerator onAssert(Room room)
        //    {
        //        yield return room.addAIPlayer().wait();
        //        addAIPlayerAssert(room);
        //    }
        //}
        class Updater : MonoBehaviour, IDisposable
        {
            public Action action;
            protected void Update()
            {
                action?.Invoke();
            }
            public void Dispose()
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}
