using System.Net;
using System.Text.Json;
using RazorEngine;
using RazorEngine.Templating;

namespace HttpServer;

public class MyServer
{
    private string _siteDirectory;
    private HttpListener _listener;
    private int _port;

    public async Task RunServerAsync(string path, int port)
    {
        _siteDirectory = path;
        _port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{_port.ToString()}/");
        _listener.Start();
        Console.WriteLine($"Server started on {_port} \nFiles in {_siteDirectory}");
        await ListenAsync();
    }

    private async Task ListenAsync()
    {
        try
        {
            while (true)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                Process(context);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void Process(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath;

        if (context.Request.HttpMethod == "POST" && filename == "/showText.html")
        {
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                string requestBody = reader.ReadToEnd();
                var jsonDocument = JsonDocument.Parse(requestBody);
                string text = jsonDocument.RootElement.GetProperty("text").GetString() ?? "No text provided";

                string layoutPath = _siteDirectory + "/layout.html";
                var razorService = Engine.Razor;
                if (!razorService.IsTemplateCached("layout", null))
                    razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
                string html = razorService.RunCompile("<h1>@Model.Text</h1>", "templateKey", null,
                    new { Text = text });

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            }
        }
        else
        {
            filename = _siteDirectory + filename;

            if (File.Exists(filename))
            {
                try
                {
                    string content = BuildHtml(filename, context.Request.QueryString);
                    context.Response.ContentType = GetContentType(filename);
                    context.Response.ContentLength64 = System.Text.Encoding.UTF8.GetBytes(content).Length;
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.OutputStream.Write(new byte[0]);
                }
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.OutputStream.Write(new byte[0]);
            }

            context.Response.OutputStream.Close();
        }
    }

    private string BuildHtml(string filename, System.Collections.Specialized.NameValueCollection query)
    {
        var serializer = new Serializer();
        List<Emploeeys> employees = serializer.LoadEmployees();

        if (query["IdFrom"] != null && query["IdTo"] != null)
        {
            string idFrom = query["IdFrom"] ?? "0";
            string idTo = query["IdTo"] ?? int.MaxValue.ToString();
            employees = employees
                .Where(e => String.Compare(e.Id, idFrom) >= 0 && String.Compare(e.Id, idTo) <= 0)
                .OrderBy(e => e.Id)
                .ToList();
        }

        string layoutPath = _siteDirectory + "/layout.html";
        var razorService = Engine.Razor;
        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
        if (!razorService.IsTemplateCached(filename, null))
        {
            razorService.AddTemplate(filename, File.ReadAllText(filename));
            razorService.Compile(filename);
        }
        string html = razorService.Run(filename, null, new
        {
            Employees = employees
        });

        return html;
    }


    private string? GetContentType(string filename)
    {
        var Dictionary = new Dictionary<string, string>()
        {
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".gif", "image/gif" },
            { ".html", "text/html" },
            { ".json", "application/json" }
        };
        string contentype = "";
        string extension = Path.GetExtension(filename);
        Dictionary.TryGetValue(extension, out contentype);
        return contentype;
    }

    public void Stop()
    {
        _listener.Abort();
        _listener.Stop();
    }
}