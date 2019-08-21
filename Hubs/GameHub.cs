using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Sudoku.Classes;
using Sudoku.Models;

namespace Sudoku.Hubs
{
    public class GameHub : Hub
    {
        static List<PlayerModel> players = new List<PlayerModel>();

        //Подключение нового пользователя
        public void Connect()
        {
            var id = Context.ConnectionId;

            if (!players.Any(x => x.ConnectionId == id))
            {
                var userName = string.Format("Игрок №{0}", players.Count() + 1);

                players.Add(new PlayerModel { ConnectionId = id, Name = userName, WinsCount = 0 });

                //Посылаем сообщение текущему пользователю
                Clients.Caller.onConnected(userName, id);
            }
        }

        //Новая игра
        public void StartNewGame(int level)
        {
            var matrix = Matrix.GenerateNew(level);

            //Посылаем игровую матрицу всем пользователям
            Clients.All.onNewGameStarted(matrix);
        }

        //Возвращает текущую матрицу
        public void GetPlayingMatrix()
        {
            var matrix = Matrix.GetPlayingMatrix();

            //Посылаем игровую матрицу текущему пользователю
            Clients.Caller.onGetPlayingMatrix(matrix);
        }

        //Проверяем переданное значение. Если оно правильное, то записываем его в текущую матрицу
        public void AddNumber(int number, int row, int col)
        {
            //Пробуем добавить в текущую матрицу новый номер
            var result = Matrix.AddNumber(number, row, col);

            if (result.IsNumberCorrect)
            {
                if (result.IsNumberAdded)
                {
                    //У всех пользователей устанавливаем правильное значение
                    Clients.All.onCorrentNumberAdded(number, row, col);

                    //Проверяем закончена ли игра
                    if (result.IsGameOver)
                    {
                        var player = players.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                        if (player != null)
                        {
                            player.WinsCount++;
                        }

                        //Сообщаем всем, что игра закончена
                        Clients.All.onGameOver(Context.ConnectionId);
                    }
                }
                else
                {
                    //Кто-то чуть раньше успел добавить правильный номер. Ничего не делаем
                }
            }
            else
            {
                //Сообщаем текущему пользователю, что но ввел неверное значение
                Clients.Caller.onWrongNumberAdded(number, row, col);
            }
        }

        //Изменить имя игрока
        public void ChangeName(string connectionId, string name)
        {
            var player = players.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (player != null)
            {
                player.Name = name;
                Clients.Caller.onNameChanged(name);
            }
        }

        //Получить подсказку
        public void GetHint(int row, int col)
        {
            //Получаем правильное значение для указанной ячейки
            var number = Matrix.GetHint(row, col);

            //У всех пользователей устанавливаем правильное значение
            Clients.All.onGetHint(number, row, col);
        }

        //Показать 10 лучших результатов
        public void GetResults()
        {
            Clients.Caller.onGetResults(players.OrderByDescending(x => x.WinsCount).Take(10).ToList());
        }

        //Отключение пользователя
        public override Task OnDisconnected(bool stopCalled)
        {
            //var item = players.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            //if (item != null)
            //{
            //    //Players.Remove(item);
            //    Clients.All.onUserDisconnected(Context.ConnectionId, item.Name);
            //}

            return base.OnDisconnected(stopCalled);
        }
    }
}