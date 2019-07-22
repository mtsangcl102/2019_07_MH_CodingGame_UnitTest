let app = require('express')();
let server = require('http').Server(app);
let io = require('socket.io')(server);

server.listen(8000,function(){
    console.log("listening on port 8000...");
});

// WARNING: app.listen(80) will NOT work here!
let roomInfo = {};
let pplInRoom={};
let dummy = 1;


io.on('connection', function (socket) {
    const comingUserId = socket.handshake.query.userId;
    console.log(socket.id+" coming with uid "+comingUserId+"...");
    //Check if id is number
    if(!/^\d+$/.test(comingUserId)){
        console.log('Its uid is not a number, disconnecting...');
        socket.emit('connect_error');
    } else{
        console.log('Its uid is a number, checking connection fight...');
        //Check if anyone with same userId already connect
        const sids = io.sockets.adapter.sids;
        for(let sid in sids){
            const existingUserId = io.sockets.connected[sid].handshake.query.userId;
            if(existingUserId===comingUserId&&sid!==socket.id){
                console.log(socket.id+" fighting with "+sid);
                io.sockets.connected[sid].disconnect();
            }
        }
        console.log(socket.id+" has connected.");
        socket.emit('connect');
        if(roomInfo[comingUserId]){
            socket.join(roomInfo[comingUserId]);
            socket.emit('respondCreateRoom', {
                isSuccess: true,
                roomId: dummy,
            });
        }
    }

    socket.on('getRoom', ()=>{
        if(!roomInfo[socket.handshake.query.userId]){
            socket.emit('respondGetRoom', {
                isSuccess: false,
            });
        } else{
            socket.emit('respondGetRoom', {
                isSuccess: true,
                roomId: roomInfo[socket.handshake.query.userId],
            });
        }
    })

    socket.on('leaveRoom', ()=>{
        if(!roomInfo[socket.handshake.query.userId]){
            socket.emit('respondLeaveRoom', {
                isSuccess: false,
            });
        } else {
            socket.leave(roomInfo[socket.handshake.query.userId]);
            pplInRoom[roomInfo[socket.handshake.query.userId]] -= 1;
            socket.emit('respondLeaveRoom', {
                isSuccess: true,
            });
        }
    });

    socket.on('createRoom',()=>{
        if(roomInfo[socket.handshake.query.userId]){
            socket.leave(roomInfo[socket.handshake.query.userId]);
        }
        socket.join(dummy);
        pplInRoom[dummy] = 1;
        roomInfo[socket.handshake.query.userId] = dummy;
        socket.emit('respondCreateRoom', {
            isSuccess: true,
            roomId: dummy,
        });
        dummy++;
    });

    socket.on('disconnect',()=>{
        socket.disconnect();
    });

    socket.on('joinRoom',(e)=>{
        if(pplInRoom[e]>=2){
            socket.emit('respondJoinRoom',{
                isSuccess: false,
            });
        } else{
            socket.join(e);
            pplInRoom[e] += 1;
            roomInfo[socket.handshake.query.userId] = e;
            socket.emit('respondJoinRoom',{
                isSuccess: true,
                roomId: e,
            });
        }
    });

    socket.on('chatInRoom',(message)=>{
        socket.emit('respondChatInRoom',{
            isSuccess: true,
        });
        io.to(roomInfo[socket.handshake.query.userId]).emit('respondMessageInRoom',{
            isSuccess: true,
            message: message,
        });
    });
});