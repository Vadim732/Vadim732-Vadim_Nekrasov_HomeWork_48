using System.Text.Json;

namespace HttpServer;

public class Serializer
{
    private readonly string path = "employees.json";
    
    public List<Emploeeys> LoadEmployees()
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0 || string.IsNullOrWhiteSpace(File.ReadAllText(path)))
        {
            return newEmployees();
        }
        var fileContent = File.ReadAllText(path);
        var employees = JsonSerializer.Deserialize<List<Emploeeys>>(fileContent);

        return employees;
    }
    
    public void AddEmployee(Emploeeys newEmployee)
    {
        var employees = LoadEmployees();
        employees.Add(newEmployee);
        var json = JsonSerializer.Serialize(employees, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
    
    private List<Emploeeys> newEmployees()
    {
        var newEmployees = new List<Emploeeys>
        {
            new Emploeeys { Id = "1", Name = "John", Surname = "Smith", Age = "50", About = "I am a hard worker" },
            new Emploeeys { Id = "2", Name = "Anny", Surname = "Jackson", Age = "94", About = "I am a cashier" },
            new Emploeeys { Id = "3", Name = "Freddy", Surname = "Jackson", Age = "19", About = "I am working part-time" },
            new Emploeeys { Id = "4", Name = "Rick", Surname = "Doe", Age = "34", About = "I like drinking tea" }
        };
        var json = JsonSerializer.Serialize(newEmployees, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);

        return newEmployees;
    }
}

