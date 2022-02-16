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
        public List<WeatherData> uniques { get; set; }

        public WriteDataToDb()
        {
            uniques = new List<WeatherData>();
        }
        public async Task WriteToDb()
        {

            List<WeatherData> testData = new List<WeatherData>();
            uniques = testData.DistinctBy(x => x.Date).DistinctBy(d => d.Temp).DistinctBy(c => c.Location).ToList();

            using (StreamReader sr = new StreamReader(path))
            {
                string headerLine = sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var splitedLine = line.Split(',');
                    splitedLine[2].Trim();
                    splitedLine[1].Trim();
                    CheckForBadCharacters(splitedLine);
                    await AddToClass(uniques, splitedLine);

                }

                //first =1 v2
                //second = 2
                /// 1 v1,2,1 v2,4,5,2

                for (int i = 0; i < uniques.Count; i++)
                {
                    for (int j = i + 1; j < uniques.Count; j++)
                    {
                        if (uniques[i].Location.Contains("Inne") && uniques[j].Location.Contains("Inne") || uniques[i].Location.Contains("Ute") && uniques[j].Location.Contains("Ute"))
                        {
                            if (uniques[i].Temp < uniques[j].Temp || uniques[i].Temp > uniques[j].Temp)
                                uniques[j].OpenTime = 1;
                            break;
                        }
                    }
                }
            }
            // SaveToDb(uniques);
            Console.WriteLine("klar");
            AddToFile(uniques, @"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\testtext.csv");
        }

        private void SaveToDb(List<WeatherData> uniques)
        {
            using (var context = new UltraMonkeyContext())
            {
                if (context.WeatherDatas.Count() == 0)
                {
                    Console.WriteLine("Creating");
                    context.WeatherDatas.AddRange(uniques);
                    context.SaveChanges();
                    Console.WriteLine("Finished");
                }
                else
                    Console.WriteLine("Database already exists");

            }
        }


        private void AddToFile(List<WeatherData> uniques, string pathTest)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(uniques);
            }
        }

        private async Task AddToClass(List<WeatherData> uniques, string[] splitedLine)
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
                MoldIndex = moldIndex,
                OpenTime = 0
            };

            uniques.Add(myData);



            await Task.FromResult(uniques);
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



            for (int i = 0; i < splitedLine[2].Length; i++)
            {
                if (char.IsDigit(splitedLine[2][i]))
                {
                    splitedLine[2] = splitedLine[2].Substring(i);
                    break;
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
