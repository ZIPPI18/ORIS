using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace MyHTTPServer
{
    public class ContestEntry
    {
        public int Id { get; set; }
        public string Participant { get; set; }
        public string Nomination { get; set; }
        public string Status { get; set; }
    }

    public class ContestController : IController
    {
        private static List<ContestEntry> _entries = new List<ContestEntry>();
        private static int _nextId = 1;

        public async System.Threading.Tasks.Task HandleAsync(HttpListenerContext context, string cleanPath)
        {
            var request = context.Request;
            var response = context.Response;
            string method = request.HttpMethod.ToUpper();

            if (method == "GET" && cleanPath == "/entries")
            {
                string nom = request.QueryString["nomination"];
                var list = _entries;

                if (!string.IsNullOrEmpty(nom))
                {
                    list = list.Where(x => x.Nomination.ToLower() == nom.ToLower()).ToList();
                }

                int sci = _entries.Count(x => x.Status == "accepted" && x.Nomination == "science");
                int art = _entries.Count(x => x.Status == "accepted" && x.Nomination == "art");
                int spr = _entries.Count(x => x.Status == "accepted" && x.Nomination == "sport");

                string html = $@"
                <html>
                <body>
                    <h1>Заявки</h1>
    
                    <div>
                        <h3>Stats:</h3>
                        <p>Science: {sci} | Art: {art} | Sport: {spr}</p>
                    </div>

                    <form method='GET' action='/contest/entries'>
                        <input type='text' name='nomination' placeholder=Номинация...'>
                        <button type='submit'>Фильтр</button>
                    </form>
                    <br>

                    <table border='1'>
                        <tr>
                            <td>ID</td>
                            <td>Имя</td>
                            <td>Номинация</td>
                            <td>Статус</td>
                            <td>Link</td>
                        </tr>";


                foreach (var item in list)
                {
                    html += $@"<tr>
                        <td>{item.Id}</td>
                        <td>{HttpUtility.HtmlEncode(item.Participant)}</td>
                        <td>{item.Nomination}</td>
                        <td>{item.Status}</td>
                        <td><a href='/contest/entries/details?id={item.Id}'>Open</a></td>
                    </tr>";
                }

                html += $@"</table>

                <h2>Create new</h2>
                <form method='POST' action='/contest/entries/create'>
                    ФИО: <input type='text' name='participant'><br>
                    Номинация: <input type='text' name='nomination'><br>
                    <input type='submit' value='Отправить POST'>
                </form>
                </body>
                </html>";

                SendHtml(response, 200, html);
            }

            else if (method == "GET" && cleanPath == "/entries/details")
            {
                int id = int.Parse(request.QueryString["id"]);
                var item = _entries.FirstOrDefault(x => x.Id == id);

                if (item == null)
                {
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }

                string html = $@"<html>
                <body>
                    <h1>Заявка #{item.Id}</h1>
                    <p>Пользователь: {HttpUtility.HtmlEncode(item.Participant)}</p>
                    <p>Номинация: {item.Nomination}</p>
                    <p>Статус: {item.Status}</p>
                    <a href='/contest/entries'>Back</a>
                </body>
                </html>";

                SendHtml(response, 200, html);
            }

            else if (method == "POST" && cleanPath == "/entries/create")
            {
                var stream = request.InputStream;
                var reader = new StreamReader(stream);
                string body = reader.ReadToEnd();

                var data = HttpUtility.ParseQueryString(body);
                string part = data["participant"];
                string nom = data["nomination"];

                if (string.IsNullOrEmpty(part) || part.Length < 2 || part.Length > 60 ||
                    (nom != "science" && nom != "art" && nom != "sport"))
                {
                    response.StatusCode = 400;
                    response.Close();
                    return;
                }

                var item = new ContestEntry { Id = _nextId++, Participant = part, Nomination = nom, Status = "pending" };
                _entries.Add(item);

                response.StatusCode = 303;
                response.Headers.Add("Location", "/contest/entries");
                response.Close();
            }
            else if (method == "PUT" && cleanPath == "/entries/update")
            {
                int id = int.Parse(request.QueryString["id"]);

                var stream = request.InputStream;
                var reader = new StreamReader(stream);
                string body = reader.ReadToEnd();

                var data = HttpUtility.ParseQueryString(body);

                var item = _entries.FirstOrDefault(x => x.Id == id);
                if (item == null)
                {
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }

                item.Participant = data["participant"];
                item.Nomination = data["nomination"];
                item.Status = data["status"];

                response.StatusCode = 204;
                response.Close();
            }
        }

        private void SendHtml(HttpListenerResponse response, int code, string html)
        {
            response.StatusCode = code;
            response.ContentType = "text/html; charset=utf-8";
            byte[] buf = Encoding.UTF8.GetBytes(html);
            response.OutputStream.Write(buf, 0, buf.Length);
            response.OutputStream.Close();
        }
    }
}
