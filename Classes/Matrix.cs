using Sudoku.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sudoku.Classes
{
    public static class Matrix
    {
        static int[,] solvedMatrix = null;       //Полностью заполненная матрица. С ней будем сравнивать цифры, введенные пользователями
        static int[,] playingMatrix = null;      //Игровая матрица, в которой закрыты ячейки
        static object fakeLocker = new object(); //Фейковый объект для блокировки


        //GenerateNew - Генерирует новую матрицу по кнопке "Новая игра"
        public static int[,] GenerateNew(int level)
        {
            lock (fakeLocker)
            {
                Initialization(level);
            }

            return playingMatrix;
        }

        //GetPlayingMatrix - возвращает текущую игровую матрицу. Если ее нет, то сначала создает новую. Вызывается при загрузке страницы
        public static int[,] GetPlayingMatrix()
        {
            if (playingMatrix == null)
            {
                lock (fakeLocker)
                {
                    if (playingMatrix == null)
                    {
                        Initialization();
                    }
                }
            }

            return playingMatrix;
        }

        //Initialization - генерирует новую игровую матрицу
        private static void Initialization(int level = 25)
        {
            solvedMatrix = new int[9, 9];
            playingMatrix = new int[9, 9];

            //1. Базовое заполнение
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    solvedMatrix[i, j] = (i * 3 + i / 3 + j) % 9 + 1;
                }
            }

            //2. Перемешиваем
            for (int shuffle = 0; shuffle < 10; shuffle++)
            {
                int val1 = new Random(Guid.NewGuid().GetHashCode()).Next(1, 10);
                int val2 = new Random(Guid.NewGuid().GetHashCode()).Next(1, 10);

                int x1, y1, x2, y2;
                x1 = x2 = y1 = y2 = 0;

                for (int i = 0; i < 9; i += 3)
                {
                    for (int k = 0; k < 9; k += 3)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            for (int z = 0; z < 3; z++)
                            {
                                if (solvedMatrix[i + j, k + z] == val1)
                                {
                                    x1 = i + j;
                                    y1 = k + z;
                                }
                                if (solvedMatrix[i + j, k + z] == val2)
                                {
                                    x2 = i + j;
                                    y2 = k + z;
                                }
                            }
                        }

                        solvedMatrix[x1, y1] = val2;
                        solvedMatrix[x2, y2] = val1;
                    }
                }
            }

            //3. Копируем оригинальную таблицу в игровую (для отправки пользователям) и закрываем в ней N ячеек
            Array.Copy(solvedMatrix, playingMatrix, solvedMatrix.Length);

            for (int i = 0; i < level; i++)
            {
                int row = new Random(Guid.NewGuid().GetHashCode()).Next(0, 9);
                int col = new Random(Guid.NewGuid().GetHashCode()).Next(0, 9);

                playingMatrix[row, col] = 0;
            }
        }

        //AddNumber - Проверяем на правильность переданное значение и добавляем его в игровую матрицу, если оно верное. Вызывается, когда пользователь ввел какое-то число
        public static AddNumberModel AddNumber(int number, int row, int col)
        {
            var result = new AddNumberModel();

            lock (fakeLocker)
            {
                //Узнаем правильность значения. Сравние его со значением из заполненой таблицы
                if (solvedMatrix[row, col] == number)
                {
                    //Ячейка пока пустая
                    if (playingMatrix[row, col] == 0)
                    {
                        playingMatrix[row, col] = number;

                        result.IsNumberCorrect = true;
                        result.IsNumberAdded = true;
                        result.IsGameOver = !playingMatrix.ContainsValue(0);
                    }
                    //Кто-то уже успел чуть раньше заполнить эту ячейку
                    else
                    {
                        result.IsNumberCorrect = true;
                        result.IsNumberAdded = false;
                    }
                }
                else
                {
                    result.IsNumberCorrect = false;
                }
            }

            return result;
        }

        //GetNumber - возвращает значение по переданным координатам. Используется, когда пользователь нажал "Подсказка"
        public static int GetHint(int row, int col)
        {
            int number = 0;

            lock (fakeLocker)
            {
                //Получаем из заполненой матрицы нужное значение
                number = solvedMatrix[row, col];

                //Устанавлием в игровую матрицу правильное значение
                playingMatrix[row, col] = number;
            }

            return number;
        }
    }
}