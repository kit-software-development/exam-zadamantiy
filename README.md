# Sea Battle, Stoyanovskiy Lev, 33536/2
Экзаменационный проект.

## Описание
Игра морской бой. 
Сервер позволяет иметь множество игровых сессий между разными пользователями одновременно. Ведётся учёт рейтинга и многое другое.

## Что необходимо для работы
Возможность прямой связи по UDP. По умолчанию используются порты 7360-7365, настроить их можно в SeaBattle.Lib\Messaging\Ports.

## Запуск
1. Запускается сервер.
2. Далее можно запускать клиенты.
* В каждом клиенте вводится адрес сервера, логин и пароль. 
* В зависимости от того первый это вход или нет нажимается кнопка Register или Login
3. Готово

## Общее
Q: Как вызвать оппонента на дуэль?  
A: Нажать кнопку Дуэль и ввести ник оппонента

Q: Как расставлять корабли?  
A: Нужно выбрать корабль справа и кликать по ячейке, в которую Вы хотите его поставить. Если корабль не может встать в данную позицию вертикально, то он повернется автоматически. Если Вы хотите повернуть корабль, то необходимо нажать по нему. Чтобы удалить корабль необходимо кликнуть правой кнопкой мыши.

## Известные проблемы
Мелькание и пропадание графики, обусловленоe реализацией через класс Graphics, который плохо поддерживает прозрачность.
В итоге, если использовать стандартные способы отрисовки, то появляются проблемы с отрисовкой прозрачных элементов, иначе приходится мириться с морганием графики и её периодическим пропаданием.
Если графика исчезла, то необходимо нажать кнопку DebugRefresh
