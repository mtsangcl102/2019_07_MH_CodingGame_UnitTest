const server = require('http').createServer();
const io = require('socket.io')(server);
global.allSockets = [];
global.users = [];
global.rooms = [];

io.on('connection', socket => {
    var userId = socket.handshake.query.userId;
    if(!global.users[userId]) global.users[userId] = {'userId': userId, 'roomId': ''};

    // Connect Fight
    if(global.allSockets[userId]) {
       global.allSockets[userId].disconnect();
    }
    global.allSockets[userId] = socket;

    // Resume Room
    socket.on('getRoom', function(){
        var res = new Object();
        res.isSuccess = global.users[userId].roomId ? true : false;
        res.roomId = global.users[userId].roomId;
        socket.emit('respondGetRoom', res);
    });

    socket.on('leaveRoom', function(){
        var res = new Object();
        res.isSuccess = false;
        if(global.users[userId].roomId){
            socket.leave(global.users[userId].roomId);
            global.rooms[global.users[userId].roomId].count--;
            global.users[userId].roomId = '';
            res.isSuccess = true;
        }
        socket.emit('respondLeaveRoom', res);
    });

    // Join room and chat
    socket.on('createRoom', function(){
        var res = new Object();
        var genRoomId = Math.random().toString(36).substring(7);
        socket.join(genRoomId);
        if(!global.rooms[genRoomId]) global.rooms[genRoomId] = {'roomId': genRoomId ,'count': 0};
        global.rooms[genRoomId].count++;
        global.users[userId].roomId = genRoomId;
        res.isSuccess = true;
        res.roomId = genRoomId;
        socket.emit('respondCreateRoom', res);
    });

    socket.on('joinRoom', function(roomId){
        var res = new Object();
        res.isSuccess = false;
        res.roomId = '';
        if(!global.users[userId].roomId && global.rooms[roomId].count<2){
            socket.join(roomId);
            global.rooms[roomId].count++;
            global.users[userId].roomId = roomId;
            res.isSuccess = true;
            res.roomId = roomId;
        }
        socket.emit('respondJoinRoom', res);
    });

    socket.on('chatInRoom', function(msg) {
        var res = new Object();
        res.isSuccess = false;
        if (global.users[userId].roomId){
            res.isSuccess = true;
            res.message = msg;
            socket.emit('respondChatInRoom', res);
            io.in(global.users[userId].roomId).emit('respondMessageInRoom', res);
        }
    });
});
server.listen(10003);