using System.Data.SqlClient;

public class ProjectManager
{
    private static ProjectManager _instance;
    private static readonly object _lock = new object();
    

    // Публичное свойство для хранения текущего проекта
    public int CurrentProject { get; set; }
    public int CurrentOperator { get; set; }


    // Метод для получения экземпляра класса
    public static ProjectManager Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new ProjectManager();
            }
        }
    }
}