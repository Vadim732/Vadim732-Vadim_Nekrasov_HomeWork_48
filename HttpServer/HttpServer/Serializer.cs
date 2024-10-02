using System.Text.Json;

namespace HttpServer;

public class Serializer
{
    private readonly string path = "employees.json";

    public List<Emploeeys> LoadEmployees()
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
        {
            var newEmployees = new List<Emploeeys>
            {
                new Emploeeys { Id = 1, Name = "John Smith", Age = 50 },
                new Emploeeys { Id = 2, Name = "Anny Jackson", Age = 94 },
                new Emploeeys { Id = 3, Name = "Freddy Jackson", Age = 19 },
                new Emploeeys { Id = 4, Name = "Rick Doe", Age = 34 }
            };

            var json = JsonSerializer.Serialize(newEmployees, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            return newEmployees;
        }
        var fileContent = File.ReadAllText(path);
        
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            var newEmployees = new List<Emploeeys>
            {
                new Emploeeys { Id = 1, Name = "John Smith", Age = 50 },
                new Emploeeys { Id = 2, Name = "Anny Jackson", Age = 94 },
                new Emploeeys { Id = 3, Name = "Freddy Jackson", Age = 19 },
                new Emploeeys { Id = 4, Name = "Rick Doe", Age = 34 }
            };

            var json = JsonSerializer.Serialize(newEmployees, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            return newEmployees;
        }
        var employees = JsonSerializer.Deserialize<List<Emploeeys>>(fileContent);

        return employees;
    }
}