import express from 'express';
import http from 'http';
import { WebSocketServer } from 'ws';

const app = express();
const server = http.createServer(app);
const wss = new WebSocketServer({ server, path: '/ws' });

const clients = new Map();

app.get('/', (req, res) => {
  res.json({
    message: "WebSocket Chat Server",
    endpoints: {
      websocket: "ws://localhost:8080",
      users: 'GET /api/users',
    }
  })
})

app.get('/api/users', (req, res) => {
  const userList = Array.from(clients.values());
  res.json({
    count: userList.length,
    users: userList
  })
})

app.get('/api/status', (req, res) => {
  res.json({
    active: true,
    connections: clients.size,
  })
})

wss.on('connection', (ws) => {
  let username = null;

  ws.on('message', (message) => {
    try {
      const data = JSON.parse(message);

      switch(data.type) {
        case 'join':
          username = data.username;
          clients.set(ws, username);
          console.log(`Пользователь ${username} присоединился к чату`);

          broadcast({
            type: 'system',
            message: `${username} присоединился к чату`,
            timestamp: new Date().toISOString()
          }, ws)

          ws.send(JSON.stringify({
            type: 'system',
            message: 'Добро пожаловать в чат!',
            timestamp: new Date().toISOString()
          }))

          break;

        case 'message':
          if (username) {
            console.log(`${username}: ${data.text}`);
          }

          broadcast({
            type: 'message',
            username,
            text: data.text,
            timestamp: new Date().toISOString()
          })

          break;

        case 'users':
          const userList = Array.from(clients.values());

          ws.send(JSON.stringify({
            type: 'users',
            users: userList,
            timestamp: new Date().toISOString()
          }));

          break;
      }

    } catch(e) {
      console.error(`Ошибка обработки сообщения: ${e}`);
    }
  })

  ws.on('close', () => {
    if (username) {
      console.log(`Пользователь ${username} покинул чат`);
      clients.delete(ws);

      broadcast({
        type: 'system',
        message: `${username} покинул чат`,
        timestamp: new Date().toISOString()
      })
    }
  })

  ws.on('error', (error) => {
    console.error(`WebSocket ошибка: ${error}`);
  })
})

function broadcast(message, exclude = null) {
  const messageStr = JSON.stringify(message);

  wss.clients.forEach((client) => {
    if (client !== exclude && client.readyState === WebSocket.OPEN) {
      client.send(messageStr);
    }
  })
}

const PORT = 8080;
server.listen(PORT, () => {
  console.log(`Express сервер запущен на порту ${PORT}`);
  console.log(`HTTP: http://localhost:${PORT}`);
  console.log(`WebSocket: ws://localhost:${PORT}`);
})

process.on('SIGINT', () => {
  console.log('Закрытие сервера...');

  wss.clients.forEach((client) => {
    client.close();
  })

  server.close(() => {
    console.log('Сервер остановлен');
    process.exit(0);
  })
})