using MongoDB.Driver;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DayShift_Overview_Kaufland;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IMongoDatabase Database { get; private set; }
    
    static App()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        Database = client.GetDatabase("ShiftsDatabase");
    }
}



