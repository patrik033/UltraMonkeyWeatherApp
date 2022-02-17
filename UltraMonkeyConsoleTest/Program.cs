using CsvHelper;
using System.Globalization;
using System.Linq;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;
using Spectre.Console;

Temps temp = new Temps();
Seasons seasons = new Seasons();
Humid humid = new Humid();
Mold mold = new Mold();
bool loop = true;

while (loop)
{
Console.Clear();
var menu = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Visa väderdata")
        .PageSize(10)
        .MoreChoicesText("Utomhus")
        .AddChoices(new[] {
            "s. Mata in Data",
            "1. Medeltemp för valt datum",
            "2. Sortering av varmast/kallast dag",
            "3. Sortering av torrast/fuktigast dag",
            "4. Sortering av minst/högst risk för mögel",
            "5. Datum för metreologisk höst",
            "6. Datum för metreologisk vinter"
        }));
        AnsiConsole.WriteLine($"{menu[0]}");
    switch (menu[0])
    {
    case 's': WriteToEF();
        break;
    
    case '1':
            PromptMetod();
            //PromptDateList();
            //AVGtemp();
            Console.ReadKey();
            Console.Clear();
            break;

        case '2':
            Console.WriteLine();
            List<string> list2 = new List<string>();
            list2 = await humid.ReturnResult(true, "Inne");
            foreach (var item in list2)
                Console.WriteLine(item);
                Console.ReadKey();
            break;

        case '3':
        
            break;

        case '4':
            Console.WriteLine();
            List<string> list3 = new List<string>();
            list3 = await mold.ReturnResult(true, "Ute");
            foreach (var item in list3)
                Console.WriteLine(item);
            Console.ReadKey();
            break;

        case '5':
            List<WeatherData> autumnList = new List<WeatherData>();
            string finalValue = await seasons.LoopForAutumn();
            Console.WriteLine(finalValue);
            Console.ReadKey();
            break;

        case '6':
            break;
    
    default:
        
            break;
    }
}

//string PromptDateList(string input)
//{
//    //Kallar på Query 
//    //Var queryn kommer adderas i Addchoices
    
    
//    var menu = AnsiConsole.Prompt(
//    new SelectionPrompt<string>()
//        .Title("Visa väderdata?")
//        .PageSize(10)
//        .MoreChoicesText("Utomhus")
//        .AddChoices(QueryTable {
//            "placeholder"
//        }));
//AnsiConsole.Write(menu);
//return menu;
//}

await WriteToEF();
//string output = AVGtemp("Ute");
PromptMetod();

string PromptMetod()
{
    var menu = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Visa väderdata?")
        .PageSize(10)
        .MoreChoicesText("Utomhus")
        .AddChoices(new[]
        {
            "1. Inne",
            "2. Ute"
        }));
    return menu;
}

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
    float AverageTemp = 0;
    //Ask for Date 

    Console.WriteLine("Input a date(MM-DD): ");
    var input = DateOnly.Parse("2016-" + Console.ReadLine());
    string output = "";

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
            if (searcher == null)
                output = "No data found";

        }
    }
    return output;
}