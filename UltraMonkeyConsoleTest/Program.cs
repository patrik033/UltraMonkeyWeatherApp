using CsvHelper;
using System.Globalization;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;

Console.SetBufferSize(800, 32000);
List<WeatherData> testData = new List<WeatherData>();
List<string> list = new List<string>();

var uniques = testData.Distinct().ToList();

using(StreamReader sr = new StreamReader(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\TempFuktData.csv"))
{
    string headerLine = sr.ReadLine();
    string line;
    while ((line = sr.ReadLine()) != null)
    {

        var splitedLine = line.Split(',');
        splitedLine[2].Trim();


        if (splitedLine[2].Contains('.'))
        {
            splitedLine[2] = splitedLine[2].Replace('.', ',');
        }

        if (splitedLine[2].Contains('−'))
        {
            splitedLine[2] = splitedLine[2].Replace('−', '-');
        }

        float myTemp = float.Parse(splitedLine[2]);

        if (splitedLine[2].Contains('^') || splitedLine[2].Contains('’') || splitedLine[2].Contains('â'))
        {
            for (int i = 0; i < splitedLine[2].Length; i++)
            {
                if (char.IsDigit(splitedLine[2][i]))
                {
                    splitedLine[2] = splitedLine[2].Substring(i);
                }
            }
        }


        splitedLine[2].Trim();

        var dates = DateTime.Parse(splitedLine[0]);
        dates.GetDateTimeFormats();
        var location = splitedLine[1];
        float temp = float.Parse(splitedLine[2]);
        var humid = int.Parse(splitedLine[3]);


        var myData = new WeatherData
        {
            Date = dates,
            Location = location,
            Temp = temp,
            AirMoisture = humid
        };

        uniques.Add(myData);
    }
}

using(var context = new UltraMonkeyContext())
{
    context.WeatherDatas.AddRange(uniques);
    context.SaveChanges();
}



//using(var writer = new StreamWriter(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\testtext.csv"))
//using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
//{
//    csv.WriteRecords(uniques);
//}



Console.WriteLine("klar");
Console.ReadLine();