using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Sudoku.Classes;
using Sudoku.Models;

namespace Sudoku.Hubs
{
    public class GameHub : Hub
    {
        static List<Player> players = new List<Player>();
        static object fakeLocker = new object();

        //Новая игра
        public void StartNewGame(int level)
        {
            var grid = Matrix.GenerateNew(level);

            Clients.All.onNewGameStarted(grid);
        }

        //Возвращает текущую матрицу
        public void GetCurrentGrid()
        {
            var grid = Matrix.GetCurrentMatrix();

            //Посылаем игровую матрицу текущему пользователю
            Clients.Caller.onGetCurrentGrid(grid);
        }

        //Подключение нового пользователя
        public void Connect()
        {
            var id = Context.ConnectionId;

            if (!players.Any(x => x.ConnectionId == id))
            {
                var userName = string.Format("Игрок №{0}", players.Count() + 1);

                players.Add(new Player { ConnectionId = id, Name = userName, WinsCount = 0 });

                //Посылаем сообщение текущему пользователю
                Clients.Caller.onConnected(userName, id);
            }
        }

        //Проверяем переданное значение. Если оно правильное, то записываем его в текущую матрицу
        public void AddNumber(int number, int row, int col)
        {
            var id = Context.ConnectionId;

            var numberIsCorrect = Matrix.CheckNumber(number, row, col);

            if (numberIsCorrect)
            {
                //Добавляем в текущую таблицу новый номер
                var numberAdded = Matrix.AddNumber(number, row, col);

                if (numberAdded)
                {
                    //У всех пользователей устанавливаем правильное значение
                    Clients.All.onCorrentNumberAdded(number, row, col);

                    //Проверяем закончена ли игра
                    if (Matrix.IsGameOver())
                    {
                        var player = players.FirstOrDefault(x => x.ConnectionId == id);
                        if (player != null)
                        {
                            player.WinsCount++;
                        }

                        //Сообщаем всем, что игра закончена
                        Clients.All.onGameOver(id);
                    }
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
            var number = Matrix.GetNumber(row, col);

            //У всех пользователей устанавливаем правильное значение
            Clients.All.onGetHint(number, row, col);
        }

        //Показать результаты
        public void GetResults()
        {
            Clients.Caller.onGetResults(players.OrderByDescending(x => x.WinsCount).Take(10).ToList());
        }

        //Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = players.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (item != null)
            {
                //Players.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.Name);
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}