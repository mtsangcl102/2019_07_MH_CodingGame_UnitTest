module.exports = {
  apps: [{
    name          : "SocketServer Watch",
    script        : "node SocketServer.ts",
    watch         : ["./"],
    ignore_watch  : [""],
    env           : {
      "NODE_ENV": "development",
    },
    env_production: {
      "NODE_ENV": "production"
    },
    watch_options : {
      "followSymlinks": true
    }
  }]
};
