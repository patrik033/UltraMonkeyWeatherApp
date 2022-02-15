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
        public async Task WriteToDb()
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

            await SaveToDb(uniques);

            //AddToFile(uniques, @"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\testtext.csv");

            //using (var context = new UltraMonkeyContext())
            //{
            //    context.WeatherDatas.AddRange(uniques);
            //    context.SaveChanges();
            //}

            AddToFile(uniques);

            Console.WriteLine("klar");
            Console.ReadLine();
        }



        private async Task SaveToDb(List<WeatherData> uniques)
        {
            using (var context = new UltraMonkeyContext())
            {
                if (context.Database.CanConnect())
                    return;
                else
                {
                    Console.WriteLine("Creating");
                    context.WeatherDatas.AddRange(uniques);
                    await context.SaveChangesAsync();
                }
            }
        }


        private void AddToFile(List<WeatherData> uniques, string path)
        {
            using (var writer = new StreamWriter(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\testtext.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(uniques);
            }
        }

        private void AddToClass(List<WeatherData> uniques, string[] splitedLine)
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

        private void CheckForBadCharacters(string[] splitedLine)
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

        private int CalculateMoldIndex(float temp, int humidity)
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
