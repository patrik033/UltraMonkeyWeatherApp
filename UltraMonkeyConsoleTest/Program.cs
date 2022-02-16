using CsvHelper;
using System.Globalization;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;



//await WriteToEF();
string output = AVGtemp("Ute");
Console.WriteLine(output);


Console.ReadLine();

async Task WriteToEF()
{
    //laddar databasen med allt på filen
    WriteDataToDb write = new WriteDataToDb();




    await write.WriteToDb();



    Temps temp = new Temps();
    Seasons seasons = new Seasons();
    Humid humid = new Humid();
    Mold mold = new Mold();

    //skriver ut genomsnittstemperaturen på vald plats i vald ordning
    List<string> list = new List<string>();
    list = await temp.ReturnResult(true, "Ute");
    foreach (var item in list)
        Console.WriteLine(item);


    //skriver ut torrast/kallast
    Console.WriteLine();
    List<string> list2 = new List<string>();
    list2 = await humid.ReturnResult(true, "Inne");
    foreach (var item in list2)
        Console.WriteLine(item);


    //skriver ut moldindex
    Console.WriteLine();
    List<string> list3 = new List<string>();
    list3 = await mold.ReturnResult(true, "Ute");
    foreach (var item in list3)
        Console.WriteLine(item);

    Console.WriteLine();



    //Skriver ut första höstdagen
    List<WeatherData> autumnList = new List<WeatherData>();
    string finalValue = await seasons.LoopForAutumn();
    Console.WriteLine(finalValue);


}




string AVGtemp(string locationPlace)
{
    //Ask for Date 

    Console.WriteLine("Input a date(MM-DD): ");
    var input = DateOnly.Parse("2016-" + Console.ReadLine());
    string output = "";

    //Search for date in database
    using (var dbcontext = new UltraMonkeyContext())
    {
        var searcher = from d in dbcontext.WeatherDatas
                       where d.Date.Day == input.Day && d.Date.Month == input.Month && d.Location == locationPlace
                       group d by new
                       {
                           d.Date.Date,
                           d.Location
                       } into g
                       select new
                       {
                           Date = g.Key.Date,
                           Temp = g.Average(x => x.Temp),
                           Loc = g.Key.Location,
                       };

        foreach (var a in searcher)
        {
            output = $"{a.Date.Year}-{a.Date.Month}-{a.Date.Day} {Math.Round(a.Temp, 1)} {a.Loc} ";
            if (searcher  == null)
                output = "No data found";

        }
    }
    return output;
}