using System;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WebSocketChatClient
{
    internal class Program
    {

        private static ClientWebSocket webSocket;
        private static CancellationTokenSource cts;
        private static string username;

        static async Task Main(string[] args)
        {
            Console.WriteLine("WebSocket Chat");
            Console.Write("Введите ваше имя: ");
            username = Console.ReadLine();

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Имя не может быть пустым!");
                return;
            }

            await ConnectToServer();
        }

        static async Task ConnectToServer()
        {
            webSocket = new ClientWebSocket();
            cts = new CancellationTokenSource();

            try
            {
                Console.WriteLine("Подключение к серверу...");
                await webSocket.ConnectAsync(new Uri("ws://localhost:8080/ws"), cts.Token);

                Console.WriteLine("Подключено к серверу");

                await SendJoinMessage();
                var receiveTask = ReceiveMessages();
                var sendTask = SendMessages();

                await Task.WhenAny(receiveTask, sendTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
            }
            finally
            {
                await Cleanup();
            }
        }
    
        static async Task SendJoinMessage()
        {
            var joinMessage = new
            {
                type = "join",
                username = username
            };

            await SendMessage(JsonConvert.SerializeObject(joinMessage));
        }

        static async Task ReceiveMessages()
        {
            var buffer = new byte[4096];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cts.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Сервер закрыл соединение");
                        break;
                    }

                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    HandleMessage(messageJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения сообщения: {ex.Message}");
            }
        }

        static void HandleMessage(string messageJson)
        {
            try
            {
                var message = JObject.Parse(messageJson);
                var type = message["type"].ToString();
                var timestamp = message["timestamp"].ToString();

                switch (type)
                {
                    case "system":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[СИСТЕМА] {message["message"]}");
                        Console.ResetColor();

                        break;

                    case "message":
                        var msgUsername = message["username"].ToString();
                        var text = message["text"].ToString();

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"[{msgUsername}]: {text}");
                        Console.ResetColor();

                        break;

                    case "users":
                        var users = message["users"].ToObject<string[]>();

                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine($"Онлайн пользователи ({users.Length}): ");

                        if (users != null)
                        {
                            foreach (var user in users)
                            {
                                Console.WriteLine($" - {user}");
                            }
                        }

                        Console.ResetColor();

                        break;
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки сообщения: {ex.Message}");
            }
        }

        static async Task SendMessages()
        {
            try
            {
                Console.WriteLine("Команды: ");

                Console.WriteLine("/users - показать список пользователей");
                Console.WriteLine("/exit - выход из чата");

                while (webSocket.State == WebSocketState.Open)
                {
                    var input = Console.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }

                    if (input.ToLower() == "/exit")
                    {
                        Console.WriteLine("Выход из чата...");
                        cts.Cancel();
                        break;
                    }

                    if (input.ToLower() == "/users")
                    {
                        var usersRequest = new { type = "users" };
                        await SendMessage(JsonConvert.SerializeObject(usersRequest));
                        continue;
                    }

                    var chatMessage = new
                    {
                        type = "message",
                        text = input,
                    };

                    await SendMessage(JsonConvert.SerializeObject(chatMessage));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки: {ex.Message}");
            }
        }

        static async Task SendMessage(string message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
            }
        }

        static async Task Cleanup()
        {
            if (webSocket != null)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Клиент закрыл соединение", CancellationToken.None);
                }

                webSocket.Dispose();
            }

            cts.Dispose();
            Console.WriteLine("Соединение закрыто");
        }
    }
}
