var userId = '';

$(function () {

    //Создаем пустую таблицу
    createTemplateGrid();

    //Ссылка на автоматически-сгенерированный прокси хаба
    var game = $.connection.gameHub;

    //onConnected - Функция, вызываемая при подключении нового пользователя
    game.client.onConnected = function (userName, connectionId) {
        //Устанавливаем имя по умолчанию
        $('#userName').val(userName);
        //Запоминаем текущий ID соединения
        userId = connectionId;
    };

    //onNewGameStarted - Вызывается после нажания Новая игра. Заполняем таблицу
    game.client.onNewGameStarted = function (matrix) {
        updateGrid(matrix);
    };

    //onCorrentNumberAdded - Вызывается после добавления правильного значения любым пользователем, в т.ч. и текущим
    game.client.onCorrentNumberAdded = function (number, row, col) {
        let cell = $('#c' + row + col).val(number);
        $(cell).prop('disabled', true);
        cell.removeClass('wrong-number').addClass('correct-number');
    };

    //onWrongNumberAdded - Вызывается после добавления неверного значения текущим пользователей
    game.client.onWrongNumberAdded = function (number, row, col) {
        let cell = $('#c' + row + col).val(number);
        cell.addClass('wrong-number');
    };

    //onGetHint - Вызывается после того как юзер нажал "Подсказка"
    game.client.onGetHint = function (number, row, col) {
        let cell = $('#c' + row + col).val(number);
        $(cell).prop('disabled', true);
        cell.removeClass('wrong-number');
    };

    //onGameOver - Вызывается после добавления неверного значения текущим пользователей
    game.client.onGameOver = function (winnerID) {
        if (userId === winnerID) {
            Swal.fire('Поздравляем!', 'Вы победили!', 'warning');
        } else {
            Swal.fire('Играя закончена.', 'Вы проиграли...', 'warning');
        }
    };

    //onGetPlayingMatrix - Вызывается при загрузке страницы. Заполняем таблицу
    game.client.onGetPlayingMatrix = function (matrix, connectionId) {
        updateGrid(matrix);
    };

    //onNameChanged - Вызывается после изменения имени
    game.client.onNameChanged = function (name) {
        Swal.fire('Привет, ' + name + '!', 'Выше имя изменено!', 'success');
    }

    //onGetResults - Вызывается при показе результатов
    game.client.onGetResults = function (results) {

        let html = '<table class="result-table">';
        html += '<tbody class="result-tbody"><tr><td>Имя игрока</td><td>Победы</td></tr>';
        for (var i = 0; i < results.length; i++) {
            html += '<tr><td>' + results[i].Name + '</td><td>' + results[i].WinsCount + '</td></tr>';
        }
        html += '</tbody></table>';

        Swal.fire({
            title: '<strong>Результаты</strong>',
            html: html,
            showCloseButton: true,
            showCancelButton: false,
            focusConfirm: false
        })
    }

    //ОТКРЫВАЕМ СОЕДИНЕНИЕ
    $.connection.hub.start().done(function () {

        //При изменении ячейки
        $(".cell").keydown(function (e) {
            //Только цифры от 1 до 9
            let val = String.fromCharCode(e.keyCode);
            if (val >= 1 && val <= 9) {
                let id = $(this).prop('id');
                let row = id.substring(1, 2);
                let col = id.substring(2, 3);

                game.server.addNumber(val, row, col);
            }
            else {
                return false;
            }
        });

        //Установлен фокус в ячейку
        $('.cell').focus(function () {
            $(".cell").each(function () {
                $(this).removeClass('focused');
            });
            $(this).addClass('focused');
        });

        //Кнопка: Изменить имя
        $('#btnChangeName').click(function () {
            var userName = $('#userName').val();
            if (userName === '') {
                Swal.fire('Ошибка!', 'Имя не указано', 'error');
            } else {
                game.server.changeName(userId, userName);
            }
        });

        //Кнопка: Начать новую игру
        $('#btnNewGame').click(function () {

            Swal.fire({
                title: 'Выберите уровень сложности',
                showConfirmButton: false,
                html:
                    '<button id="easy" class="btn btn-success">' +
                    'Легкий' +
                    '</button><br/><br/>' +
                    '<button id="middle" class="btn btn-warning">' +
                    'Средний' +
                    '</button><br/><br/>' +
                    '<button id="hard" class="btn btn-danger">' +
                    'Сложный' +
                    '</button>',
                onBeforeOpen: () => {
                    const content = Swal.getContent()
                    const $ = content.querySelector.bind(content)

                    $('#easy').addEventListener('click', () => {
                        game.server.startNewGame(3);
                        Swal.close();
                    })

                    $('#middle').addEventListener('click', () => {
                        game.server.startNewGame(10);
                        Swal.close();
                    })

                    $('#hard').addEventListener('click', () => {
                        game.server.startNewGame(25);
                        Swal.close();
                    })
                }
            })
        });

        //Кнопка: Подсказка
        $('#btnHint').click(function () {
            var focusedCell = $('.focused')[0];
            if (focusedCell === undefined) {
                Swal.fire('Ошибка!', 'Ячейка для подсказки не выбрана.', 'error');
            } else {
                let id = $(focusedCell).prop('id');
                let row = id.substring(1, 2);
                let col = id.substring(2, 3);

                game.server.getHint(row, col);
            }
        });

        //Кнопка: Результаты
        $('#btnGetResults').click(function () {
            game.server.getResults();
        });

        //Получаем текущую игру или создаем новую
        game.server.getPlayingMatrix();

        //Добавляем нового пользователя в список игроков
        game.server.connect();
    });

    //Рисуем пустую таблицу
    function createTemplateGrid() {
        let divClass = '';
        var table_body = '<table border="1">';
        for (var row = 0; row < 9; row++) {
            table_body += '<tr>';
            for (var col = 0; col < 9; col++) {

                if (row % 3 === 0) {
                    if (col % 3 === 0) {
                        divClass = 'g0';
                    } else {
                        divClass = 'f0';
                    }
                } else {
                    if (col % 3 === 0) {
                        divClass = 'e0';
                    } else {
                        divClass = 'c0';
                    }
                }

                table_body += '<td class="' + divClass + '">';
                table_body += '<input maxlength="1" class="cell" autocomplete="off" id="c' + row + col + '">';
                table_body += '</td>';
            }
            table_body += '</tr>';
        }
        table_body += '</table>';
        $('#tableDiv').html(table_body);
    }

    //Обновляет все значение в таблице
    function updateGrid(matrix) {

        for (var row = 0; row < 9; row++) {
            for (var col = 0; col < 9; col++) {

                let val = matrix[row][col];
                let cell = $('#c' + row + col);

                if (val === 0) {
                    cell.val('');
                    $(cell).prop('disabled', false);
                } else {
                    cell.val(val);
                    $(cell).prop('disabled', true);
                }

                $(cell).removeClass('wrong-number');
                $(cell).removeClass('correct-number');
            }
        }
    }

});

// Кодирование тегов
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}