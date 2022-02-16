using CsvHelper;
using System.Globalization;
using System.Linq;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;



//WriteToEF();

async Task WriteToEF()
{
    //laddar databasen med allt på filen
    WriteDataToDb write = new WriteDataToDb();
    await write.WriteToDb();
    Temps temp = new Temps();
    Seasons seasons = new Seasons();


    //skriver ut genomsnittstemperaturen på vald plats i vald ordning
    List<string> list = new List<string>();
    list = await temp.ReturnResult(true, "Inne");

    foreach (var item in list)
        Console.WriteLine(item);



    //Skriver ut första höstdagen
    List<WeatherData> autumnList = new List<WeatherData>();
    string finalValue = await seasons.LoopForAutumn();
    Console.WriteLine(finalValue);

   
}


AVGtemp("Ute");

void AVGtemp(string location)
{
    float AverageTemp = 0;
    //Ask for Date 
    Console.WriteLine("Input a date(MM-DD): ");
    var input = DateTime.Parse("2016-" + Console.ReadLine());
    input.ToString("yyyy-MM-dd");
    //input.ToString("yyyy-MM-dd");

    //Search for date in database
    using (var dbcontext = new UltraMonkeyContext())
    {
        //    var searcher = from d in dbcontext.WeatherDatas
        //                   where d.Date == input && d.Location == location
        //                   select new
        //                   {
        //                       temp = d.Temp,
        //                       date = d.Date.ToString("yyyy-MM-dd"),
        //                       location = d.Location
        //                   };
        var searcher = from d in dbcontext.WeatherDatas
                   where d.Location == location && d.Date == input
                   group d by new
                   {
                       Date = d.Date,
                       Temp = d.Temp,
                       Loc = d.Location
                   } into g
                   select new
                   {
                       date = g.Key,
                       temp = g.Average(x => x.Temp),
                       loc = g.Key.Loc

                    };

    foreach (var a in searcher)
        {
            AverageTemp += a.temp;
            Console.WriteLine($"{a.date} {a.temp} {a.loc}");
            
        }
        Console.WriteLine($"{AverageTemp}");
        Console.ReadKey();
    }
}
    
    //Take all with date
    //Count the Average temperature of that date
    //Output Date + AVG temp (Do it as Return type)
