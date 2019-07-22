const server = require('http').createServer();
const io = require('socket.io')(server);

let connectedUsers = [];
let rooms = [];
let currRoomId = 1;

io.on('connection', client => {
    for (let i = 0; i < connectedUsers.length; i++) {
        if (connectedUsers[i].handshake.query.userId == client.handshake.query.userId) {
            connectedUsers[i].emit('disconnect');
            connectedUsers.splice(i);
            break;
        }
    }
    connectedUsers.push(client);

    client.on('getRoom', () => {
        console.log("getRoom");
        if (rooms.length > 0) {
            rooms[0].roommates.push(client.handshake.query.userId);
        }
        client.emit('respondGetRoom', {isSuccess: true, roomId: rooms.length > 0 ? rooms[0].roomId : ''});
    });

    client.on('createRoom', () => {
        console.log("createRoom");
        let myRoomId = currRoomId;
        currRoomId++;
        rooms.push({roomId: myRoomId, roommates: [client.handshake.query.userId]});
        client.emit('respondCreateRoom', {isSuccess: true, roomId: myRoomId});
    });

    client.on('joinRoom', data => {
        console.log("joinRoom");
        for (let i = 0; i < rooms.length; i++) {
            if (rooms[i].roomId == data && rooms[i].roommates.length < 2) {
                rooms[i].roommates.push(client.handshake.query.userId);
                client.emit('respondJoinRoom', {isSuccess: true, roomId: rooms[i].roomId});
                return;
            }
        }
        client.emit('respondJoinRoom', {isSuccess: false});
    });

    client.on('leaveRoom', () => {
        console.log("leaveRoom");
        for (let i = 0; i < rooms.length; i++) {
            for (let j = 0; j < rooms[i].roommates.length; j++) {
                if (rooms[i].roommates[j] == client.handshake.query.userId) {
                    rooms[i].roommates.splice(j);
                    client.emit('respondLeaveRoom', {isSuccess: true});
                    break;
                }
            }
        }
    });

    client.on('chatInRoom', data => {
        console.log("chatInRoom");
        client.emit('respondChatInRoom', {isSuccess: true});
        for (let i = 0; i < rooms.length; i++) {
            if (rooms[i].roommates.includes(client.handshake.query.userId)) {
                for (let j = 0; j < connectedUsers.length; j++) {
                    if (rooms[i].roommates.includes(connectedUsers[j].handshake.query.userId)) {
                        connectedUsers[j].emit('respondMessageInRoom', {isSuccess: true, message: data});
                    }
                }
            }
        }
    });

    client.on('respondMessageInRoom', data =>{
        console.log("respondMessageInRoom");
        client.emit('respondMessageInRoom', {isSuccess: true, message: data});
    });

    client.on('connect', data => {
        console.log("Connect");
        console.log(data);
    });

    client.on('disconnect', () => {
        console.log("Disconnect");

        for (let i = 0; i < connectedUsers.length; i++) {
            if (connectedUsers[i].handshake.query.userId == client.handshake.query.userId) {
                connectedUsers.splice(i);
                break;
            }
        }

        client.disconnect();
    });
});
server.listen(3000);