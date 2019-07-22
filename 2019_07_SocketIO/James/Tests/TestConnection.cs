using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Tests
{
    [Serializable]
    public class SearchResult
    {
        public string name;
        public string latest;
    }

    public struct SearchJson
    {
        public SearchResult[] results;
        public int total;
    }
    
    public class TestConnection
    {
        private const string Host = "https://blooming-fjord-24277.herokuapp.com/socket.io/";

        public GameClient gameClient0a = new GameClient(Host, "abc");
        public GameClient gameClient1a = new GameClient(Host, "1");
        public GameClient gameClient1b = new GameClient(Host, "1");
        public GameClient gameClient2a = new GameClient(Host, "2");
        public GameClient gameClient3a = new GameClient(Host, "3");

        [SetUp]
        public void SetUp() {
            //SetUp runs before all test cases
            Debug.Log($"--- Setup --- ");
        }

        [TearDown]
        public void TearDown() {
            //SetUp runs after all test cases
            Debug.Log($"--- TearDown --- ");
            
            gameClient1a.Disconnect();
            gameClient1b.Disconnect();
            gameClient2a.Disconnect();
            gameClient3a.Disconnect();
        }
        
        [UnityTest]
        public IEnumerator TestSimpleJson()
        {
            string url = "https://api.cdnjs.com/libraries?search=mocha";
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                Assert.IsTrue(false);
            }
            else
            {
                yield return TestHelper.Timeout(1.0);
                
                // Show results as text
                var json = JsonUtility.FromJson<SearchJson>(www.downloadHandler.text);
                Debug.Log($"{www.downloadHandler.text}");
                Assert.GreaterOrEqual(json.total, 1);
                Assert.GreaterOrEqual(json.results.Length, 1);
                Assert.AreEqual(json.results[0].name, "mocha");
            }

            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestInvalidConnection()
        {
            yield return GameClientHelper.AssertInvalidConnect(gameClient0a);
        }

        [UnityTest]
        public IEnumerator TestConnectFight()
        {
            yield return GameClientHelper.AssertConnect(gameClient1a);
            yield return GameClientHelper.AssertConnect(gameClient1b);

            yield return TestHelper.Timeout(() => !gameClient1a.IsConnected);
            Assert.IsFalse(gameClient1a.IsConnected);
        }

        [UnityTest]
        public IEnumerator TestResumeRoom()
        {
            yield return GameClientHelper.AssertConnect(gameClient1a);
            yield return GameClientHelper.AssertLeaveIfHasRoom(gameClient1a);
            yield return GameClientHelper.AssertCreateRoom(gameClient1a);
            
            // disconnect
            yield return GameClientHelper.AssertDisconnect(gameClient1a);
            
            // connect and check has room
            yield return GameClientHelper.AssertConnect(gameClient1a);
            yield return GameClientHelper.AssertHasRoom(gameClient1a);
            yield return GameClientHelper.AssertLeaveRoom(gameClient1a);
        }

        [UnityTest]
        public IEnumerator TestJoinRoomAndChat()
        {
            // socketClient1a
            yield return GameClientHelper.AssertConnect(gameClient1a);
            yield return GameClientHelper.AssertLeaveIfHasRoom(gameClient1a);
            yield return GameClientHelper.AssertCreateRoom(gameClient1a);
            
            // socketClient2a
            yield return GameClientHelper.AssertConnect(gameClient2a);
            yield return GameClientHelper.AssertLeaveIfHasRoom(gameClient2a);
            yield return GameClientHelper.AssertJoinRoom(gameClient2a, gameClient1a.RoomId);
            
            // socketClient3a
            yield return GameClientHelper.AssertConnect(gameClient3a);
            yield return GameClientHelper.AssertInvalidJoinRoom(gameClient3a, gameClient1a.RoomId);
            
            // socketClient: send chat
            int total = 0;
            gameClient1a.OnEventMessage += (sender, message) =>
            {
                if (message.EventName == "respondMessageInRoom")
                    total++;
            };
            gameClient2a.OnEventMessage += (sender, message) =>
            {
                if (message.EventName == "respondMessageInRoom")
                    total++;
            };
            
            string newMessage = "Hello World!";
            yield return GameClientHelper.AssertChatInRoom(gameClient1a, newMessage);
            yield return TestHelper.Timeout(() => total == 2);

            Assert.AreEqual(gameClient1a.Chats.Count, 1);
            Assert.AreEqual(gameClient2a.Chats.Count, 1);
            Assert.AreEqual(gameClient1a.Chats[0], newMessage);
        }
    }
}