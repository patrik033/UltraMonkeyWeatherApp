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
            List<WeatherData> listTest = new List<WeatherData>();
            HashSet<WeatherData> testData = new HashSet<WeatherData>();
            uniques = testData.DistinctBy(x => x.Date).DistinctBy(d => d.Temp).DistinctBy(c => c.Location).ToList();
            string path = @"C:\Users\zn_19\Downloads\TempFuktData.csv";
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
                RemoveDuplicates();
                AddOpenTime();
            }
             SaveToDb(uniques);
            //AddToFile(uniques, @"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\testtext.csv");
        }

        private void AddOpenTime()
        {
            for (int i = 0; i + 31 < uniques.Count; i += 30)
            {
                var uteLocation = uniques[i];
                var inneLocation = uniques[(i + 1)];

                if (uniques[i].Location == uniques[i + 1].Location)
                {
                    i++;
                }
                else if (uniques[i].Location == uniques[i + 30].Location && uniques[i + 1].Location == uniques[(i + 1) + 30].Location && uniques[i].Date == uniques[i + 1].Date)
                {

                    if (uniques[i + 30].Temp > uniques[i].Temp && uniques[(i + 1) + 30].Temp < uniques[i + 1].Temp)
                    {
                        float outDiff = uniques[i + 30].Temp - uniques[(i)].Temp;
                        float innDiff = uniques[(i + 1)].Temp - uniques[(i + 30) + 1].Temp;
                        double result = Math.Round((outDiff + innDiff), 1);


                        uniques[(i + 30) + 1].OpenTime = 15;
                        uniques[(i + 30) + 1].Diff = result;
                    }
                }
            }
        }

        private void RemoveDuplicates()
        {
            for (int i = 0; i < uniques.Count; i++)
            {
                for (int j = i + 1; j + 1 < uniques.Count; j++)
                {
                    if (uniques[i].Date == uniques[j].Date && uniques[i].Location == uniques[j].Location && uniques[i].Temp == uniques[j].Temp)
                        uniques.RemoveAt(j);


                    if (uniques[i].Date == uniques[j + 1].Date && uniques[i].Location == uniques[j + 1].Location && uniques[i].Temp == uniques[j + 1].Temp)
                        uniques.RemoveAt(j);
                    else
                        break;
                }
            }

            // only to find the last duplicate from i which could in some cases be located at 2 spots ahead from i
            for (int i = 0; i + 2 < uniques.Count; i++)
            {
                if (uniques[i].Date == uniques[i + 2].Date && uniques[i].Location == uniques[i + 2].Location && uniques[i].Temp == uniques[i + 2].Temp)
                {
                    uniques.RemoveAt(i + 2);
                }
            }
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
            using (var writer = new StreamWriter(pathTest))
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
