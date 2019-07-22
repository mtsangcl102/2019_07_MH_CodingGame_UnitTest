import {assert, expect} from "chai";
import Debug from "debug";
import "mocha";
import {SocketClient} from "../src/SocketClient";
import {TestHelper} from "./Helpers/TestHelper";

const debug = Debug("app:Test");

// prepare variables
const host = (process.env.HOST || "").trim();
const socketClient0a: SocketClient = new SocketClient(host, "abc");
const socketClient1a: SocketClient = new SocketClient(host, "1");
const socketClient1b: SocketClient = new SocketClient(host, "1");
const socketClient2a: SocketClient = new SocketClient(host, "2");
const socketClient3a: SocketClient = new SocketClient(host, "3");

describe("Socket Client", () => {
  beforeEach(async () => {
    // runs before all tests in this block
  });

  afterEach(async () => {
    // runs after all tests in this block
    socketClient1a.disconnect();
    socketClient1b.disconnect();
    socketClient2a.disconnect();
    socketClient3a.disconnect();
    debug("----------------");
    await TestHelper.timeout(100);
  });

  it("invalid connection", async () => {
    await TestHelper.AssertInvalidConnect(socketClient0a);
  }).timeout(30 * 1000); // the first connection make take longer since the server may need cold start

  it("connect fight", async () => {
    // socketClient1a
    await TestHelper.AssertConnect(socketClient1a);
    const promise = new Promise(resolve => socketClient1a.on("disconnect", resolve));
    await TestHelper.AssertConnect(socketClient1b);

    // socketClient1a
    await promise;
    assert.isFalse(socketClient1a.isConnected);
  });

  it("resume room", async () => {
    // connect
    await TestHelper.AssertConnect(socketClient1a);
    await TestHelper.AssertLeaveIfHasRoom(socketClient1a);

    // create room
    await TestHelper.AssertCreateRoom(socketClient1a);

    // disconnect
    await TestHelper.AssertDisconnect(socketClient1a);

    // connect and check has room
    await TestHelper.AssertConnect(socketClient1a);
    await TestHelper.AssertHasRoom(socketClient1a);
    await TestHelper.AssertLeaveRoom(socketClient1a);
  });

  it("join room and chat", async () => {
    // socketClient1a
    await TestHelper.AssertConnect(socketClient1a);
    await TestHelper.AssertLeaveIfHasRoom(socketClient1a);
    await TestHelper.AssertCreateRoom(socketClient1a);

    // socketClient2a
    await TestHelper.AssertConnect(socketClient2a);
    await TestHelper.AssertLeaveIfHasRoom(socketClient2a);
    await TestHelper.AssertJoinRoom(socketClient2a, socketClient1a.roomId);

    // socketClient3a
    await TestHelper.AssertConnect(socketClient3a);
    await TestHelper.AssertInvalidJoinRoom(socketClient3a, socketClient1a.roomId);

    const promise1 = new Promise(resolve => socketClient1a.once("respondMessageInRoom", resolve));
    const promise2 = new Promise(resolve => socketClient2a.once("respondMessageInRoom", resolve));

    // socketClient: send chat
    const newMessage = "Hello World!";
    await TestHelper.AssertChatInRoom(socketClient2a, newMessage);
    await Promise.all([promise1, promise2]);
    assert.isAtLeast(socketClient1a.chats.length, 1);
    assert.isAtLeast(socketClient2a.chats.length, 1);
    assert.equal(socketClient1a.chats[0], newMessage);
  });
});