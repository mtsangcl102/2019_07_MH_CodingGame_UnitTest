import { assert, expect } from "chai";
import {SocketClient} from "../../src/SocketClient";

export class TestHelper {
    public static timeout = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

    public static async AssertInvalidConnect(socketClient: SocketClient) {
        try {
            await socketClient.connect();
            assert.isTrue(false);
        } catch (err) {
            assert.isTrue(true);
        }

    }

    public static async AssertConnect(socketClient: SocketClient) {
        const promise = socketClient.connect();
        assert.isTrue(socketClient.isConnecting);
        await promise;
        assert.isTrue(socketClient.isConnected);
    }

    public static async AssertDisconnect(socketClient: SocketClient) {
        assert.isTrue(socketClient.isConnected);
        socketClient.disconnect();
        assert.isFalse(socketClient.isConnected);
        assert.isFalse(socketClient.hasRoom);
    }

    public static async AssertHasRoom(socketClient: SocketClient) {
        await socketClient.getRoom();
        assert.isTrue(socketClient.hasRoom);
    }

    public static async AssertCreateRoom(socketClient: SocketClient) {
        await socketClient.createRoom();
        assert.isTrue(socketClient.hasRoom);
    }

    public static async AssertJoinRoom(socketClient: SocketClient, roomId: string) {
        await socketClient.joinRoom(roomId);
        assert.isTrue(socketClient.hasRoom);
    }

    public static async AssertInvalidJoinRoom(socketClient: SocketClient, roomId: string) {
        await socketClient.joinRoom(roomId);
        assert.isFalse(socketClient.hasRoom);
    }

    public static async AssertLeaveRoom(socketClient: SocketClient) {
        assert.isTrue(socketClient.hasRoom);
        await socketClient.leaveRoom();
        assert.isFalse(socketClient.hasRoom);
    }

    public static async AssertLeaveIfHasRoom(socketClient: SocketClient) {
        await socketClient.getRoom();
        await socketClient.leaveRoom();
    }

    public static async AssertChatInRoom(socketClient: SocketClient, message: string) {
        assert.isTrue(socketClient.hasRoom);
        await socketClient.chatInRoom(message);
    }
}