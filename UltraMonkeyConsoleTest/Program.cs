using CsvHelper;
using System.Globalization;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;



WriteToEF();


async void WriteToEF()
{
    //WriteDataToDb write = new WriteDataToDb();
    //write.WriteToDb();
    Temps temp = new Temps();
    Seasons seasons = new Seasons();


    //skriver ut genomsnittstemperaturen på vald plats i vald ordning
    List<string> list = new List<string>();
    list = temp.OrderTemps(true, "Inne");

    foreach (var item in list)
        Console.WriteLine(item);



    //Skriver ut första höstdagen
    List<WeatherData> autumnList = new List<WeatherData>();
    string finalValue = await seasons.LoopForAutumn();
    Console.WriteLine(finalValue);

    Console.ReadLine();
}


//AVGtemp();

void AVGtemp()
{
    //Ask for Date 
    Console.WriteLine("Input a date(MM-DD): ");
    var input = DateTime.Parse("2016-" + Console.ReadLine());
    //Search for date in database
    using (var dbcontext = new UltraMonkeyContext())
    {
        var searcher = from d in dbcontext.WeatherDatas
                       where d.Date == input
                       select new
                       {
                           Date = input
                       };
    }
    //Take all with date
    //Count the Average temperature of that date
    //Output Date + AVG temp (Do it as Return type)
}