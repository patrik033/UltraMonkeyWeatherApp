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

            using (StreamReader sr = new StreamReader(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\TempFuktData.csv"))
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

            //ta ut inne & ute med samma tid,
            //ta ut nästa tid med inne o ute


            //using (var context = new UltraMonkeyContext())
            //{
            //    context.WeatherDatas.AddRange(uniques);
            //    context.SaveChanges();
            //}

            AddToFile(uniques);

            Console.WriteLine("klar");
            Console.ReadLine();
        }

        private static void RemoveDuplicated(List<WeatherData> uniques)
        {
            for (int i = 0; i < uniques.Count; i++)
            {
                for (int j = i + 1; j < uniques.Count; j++)
                {
                    if (uniques[i].Location.ToLower().Contains("inne") == uniques[j].Location.ToLower().Contains("inne"))
                        uniques.RemoveAt(j);
                    if (uniques[i].Location.ToLower().Contains("ute") == uniques[j].Location.ToLower().Contains("ute"))
                        uniques.RemoveAt(j);
                   
                    
                    else
                        break;
                }
            }
        }

        private static void AddToFile(List<WeatherData> uniques)
        {
            using (var writer = new StreamWriter(@"C:\Users\zn_19\Documents\Temptest.csv"))
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
