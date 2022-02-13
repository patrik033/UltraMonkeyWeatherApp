using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMonkeyEFLibrary;

namespace UltraMonkeyLibrary
{
    public class WriteDataToDb
    {
        public void WriteToDb()
        {
            List<WeatherData> testData = new List<WeatherData>();
            var uniques = testData.DistinctBy(x => x.Date).DistinctBy(d => d.Temp).DistinctBy(c => c.Location).ToList();

            using (StreamReader sr = new StreamReader(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\TempFuktData1.csv"))
            {
                string headerLine = sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var splitedLine = line.Split(',');
                    splitedLine[2].Trim();
                    splitedLine[1].Trim();
                    CheckForBadCharacters(splitedLine);
                    splitedLine[2].Trim();
                    AddToClass(uniques, splitedLine);
                }
            }

            //Visar varmast/kallast dag basserat på plats
            List<string> list = new List<string>();
            list = OrderTemps(uniques, true, "Ute");

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }

            //datum för meterologisk höst
            List<WeatherData> autumnList = new List<WeatherData>();

            string finalValue = LoopForAutumn(uniques, autumnList);
            Console.WriteLine(finalValue);
            

            //SaveToDb(uniques);

            //AddToFile(uniques);

            Console.WriteLine("klar");
            Console.ReadLine();
        }

        private string LoopForAutumn(List<WeatherData> uniques, List<WeatherData> autumnList)
        {
            int temp = 0;
            string returnValue = "";
            bool keepRunning = true;
            List<WeatherData> list = new List<WeatherData>();
            list = FirstDayForAutumn(uniques, list, temp);
            while (keepRunning)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Temp > 0 && list[i].Temp < 10 && list[0].Temp > list[1].Temp && list[1].Temp > list[2].Temp && list[2].Temp > list[3].Temp && list[3].Temp > list[4].Temp)
                    {
                        returnValue = $"Första höstdagen inföll den: {list[0].Date.ToString()}";
                        keepRunning = false;
                        break;
                    }
                    else
                    {
                        list.Clear();
                        temp++;
                        list = FirstDayForAutumn(uniques, list, temp);
                    }
                }
            }
            return returnValue;
        }

        private static void SaveToDb(List<WeatherData> uniques)
        {
            using (var context = new UltraMonkeyContext())
            {
                context.WeatherDatas.AddRange(uniques);
                context.SaveChanges();
            }
        }



        //TODO make it talk to the database instead of a list
        private List<string> OrderTemps(List<WeatherData> uniques, bool orderBy, string roomType)
        {
            List<string> temp = new List<string>();
            string items = "";
            if (orderBy)
            {
                return OrderByTemp(uniques, roomType, temp, ref items);
            }
            else
            {
                return OrderByDescendingTemp(uniques, roomType, temp, ref items);
            }
        }

        private static List<string> OrderByDescendingTemp(List<WeatherData> uniques, string roomType, List<string> temp, ref string items)
        {
            var newList = uniques.GroupBy(x => new
            {
                x.Date.Date,
                x.Location
            }).Select(g => new
            {
                Date = g.Key,
                AVG = g.Average(x => x.Temp),
                Loc = g.Key.Location
            }).Where(x => x.Loc == roomType).OrderByDescending(x => x.AVG);
            foreach (var item in newList)
            {
                items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, Average: {item.AVG}";
                temp.Add(items);
            }
            return temp;
        }

        private static List<string> OrderByTemp(List<WeatherData> uniques, string roomType, List<string> temp, ref string items)
        {
            var newList = uniques.GroupBy(x => new
            {
                x.Date.Date,
                x.Location
            }).Select(g => new
            {
                Date = g.Key,
                AVG = g.Average(x => x.Temp),
                Loc = g.Key.Location

            }).Where(x => x.Loc == roomType).OrderBy(x => x.Date.Date);
            foreach (var item in newList)
            {
                items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, {item.Loc} Average: {Math.Round(item.AVG, 1)}";
                temp.Add(items);
            }
            return temp;
        }


        private static List<WeatherData> FirstDayForAutumn(List<WeatherData> uniques, List<WeatherData> temp, int skip)
        {
            List<WeatherData> temps = new List<WeatherData>();
            var newList = uniques.GroupBy(x => new
            {
                x.Date.Date,
                x.Location
            }).Select(g => new
            {
                Date = g.Key,
                AVG = g.Average(x => x.Temp),
                Loc = g.Key.Location

            }).Where(x => x.Loc == "Ute").OrderBy(x => x.Date.Date).Skip(skip).Take(5);
            foreach (var item in newList)
            {
                WeatherData Rubin = new WeatherData()
                {
                    Date = item.Date.Date,
                    Location = item.Loc,
                    Temp = item.AVG
                };
                temps.Add(Rubin);
            }
            return temps;
        }

        private static void AddToFile(List<WeatherData> uniques)
        {
            using (var writer = new StreamWriter(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\testtext.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(uniques);
            }
        }

        private static void AddToClass(List<WeatherData> uniques, string[] splitedLine)
        {
            var dates = DateTime.Parse(splitedLine[0]);
            dates.GetDateTimeFormats();
            var location = splitedLine[1];
            float temp = float.Parse(splitedLine[2]);
            var humid = int.Parse(splitedLine[3]);
            int moldIndex = CalculateMoldIndex(temp, humid);

            var myData = new WeatherData
            {
                Date = dates,
                Location = location,
                Temp = temp,
                AirMoisture = humid,
                MoldIndex = moldIndex
            };
            uniques.Add(myData);
        }

        private static void CheckForBadCharacters(string[] splitedLine)
        {
            if (splitedLine[2].Contains('.'))
            {
                splitedLine[2] = splitedLine[2].Replace('.', ',');
            }

            if (splitedLine[2].Contains('−'))
            {
                splitedLine[2] = splitedLine[2].Replace('−', '-');
            }

            for (int i = 0; i < splitedLine[2].Length; i++)
            {
                if (char.IsDigit(splitedLine[2][i]))
                {
                    splitedLine[2] = splitedLine[2].Substring(i);
                    break;
                }
            }
            float myTemp = float.Parse(splitedLine[2]);
        }

        private static int CalculateMoldIndex(float temp, int humidity)
        {
            int value = 0;

            if (humidity > 90 && humidity <= 100 && temp >= 10 && temp <= 50)
                value = 3;
            if (humidity > 80 && humidity <= 90 && temp >= 10 && temp <= 50)
                value = 2;
            if (humidity >= 73 && humidity <= 80 && temp >= 10 && temp <= 50)
                value = 1;
            if (temp > 50 || humidity < 73 && temp < 10 || temp < 10)
                value = 0;
            return value;
        }
    }
}
