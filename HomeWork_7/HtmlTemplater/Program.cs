using HtmlTemplater;

string template = "<p>Имя: ${user.Name}</p> <p>Возраст: ${user.Age}</p> <p>Группа: ${user.Group} </p> <p>Пол: ${user.Sex}</p>";
User user = new User("Ваня", "11-541", 22, true);

HtmlTemplateRenderer templateRenderer = new HtmlTemplateRenderer();
string result = templateRenderer.RenderFromString(template, user);

Console.WriteLine(result);

string template1 = "<p>Назвние книги: ${Book.Name}</p>" +
    "<p>Год издания: ${Book.Year}</p>" +
    "<p>Автор: ${book.Author}</p> ";

Book book = new Book("Солярис", 1961, "Станислав Лем");

string result1 = templateRenderer.RenderFromString(template1, book);

Console.WriteLine(result1);

string template2 = "<h1>${pen. Name} ${pen.Price}</h1>" +
"<p> Название ручки: ${ pen.Name}</ p > " +
"<р> Цвет: ${ pen.Color} мм </p>" +
"<р> Ширина: ${ pen.Width} мм </p> " +
"<р> Цена: ${ реп.Price} рублей </p> ";

Pen pen = new Pen("Berlingo", "Розовая", 1, 20);

string result2 = templateRenderer.RenderFromString(template2, pen);

Console.WriteLine(result2);

string testIfTemplate = "Привет! $if(user.Sex) Ты парень $else Ты девушка $endif Пока!";

User user1 = new User("Рома", "11-641", 22, true);

string result3 = TemplateParser.ProcessIfConditions(testIfTemplate, user1);

Console.WriteLine("Результат работы if:");
Console.WriteLine(result3);

List<User> listUser = new List<User>();

listUser.Add(user1);
listUser.Add(user);

string template4 = "<p> Список пользователей:\n$foreach(User)Пользователь: $item.Name, Группа: $item.Group $endforeach </p>";

string result4 = TemplateParser.ProcessForeach(template4, listUser);

Console.WriteLine(result4);

Console.ReadLine();
