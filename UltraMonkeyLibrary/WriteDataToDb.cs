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

            using (var context = new UltraMonkeyContext())
            {
                if (context.WeatherDatas.Count() == 0)
                {
                    HashSet<WeatherData> testData = new HashSet<WeatherData>();
                    uniques = testData.DistinctBy(x => x.Date).DistinctBy(c => c.Location).ToList();

                    string path = @"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\TempFuktData.csv";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string headerLine = sr.ReadLine();
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var splitedLine = line.Split(',');
                            splitedLine[2].Trim();
                            splitedLine[1].Trim();
                            float res = CheckForBadCharacters(splitedLine);
                            await AddToClass(uniques, splitedLine, res);
                        }

                        RemoveDuplicates();
                        AddOpenTime();
                    }
                    SaveToDb(uniques);
                }
                else
                {
                    Console.WriteLine("Database allready exists");
                    await Task.Delay(1000);
                }

            }

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
                    //kollar 30 platser framåt, dvs varje kvart om temperaturen har ändrats något
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
            int num = 0;
            //if (context.WeatherDatas.Count() == 0)
            //{
            //    Console.WriteLine("Creating");
            //    context.WeatherDatas.AddRange(uniques);
            //    context.SaveChanges();
            //    Console.WriteLine("Finished");
            //}
            //else
            //    Console.WriteLine("Database already exists");
            for (int i = 0; i < uniques.Count; i++)
            {
                try
                {
                    using (var context = new UltraMonkeyContext())
                    {
                        context.Add(uniques[i]);
                        context.SaveChanges();
                    }
                }
                catch
                {

                }
                num++;
            }
            Console.WriteLine(num);
        }

        //skriv alla data till en fil
        //bara testad med .csv filer
        private void AddToFile(List<WeatherData> uniques, string pathTest)
        {
            using (var writer = new StreamWriter(pathTest))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(uniques);
            }
        }

        private async Task AddToClass(List<WeatherData> uniques, string[] splitedLine, float res)
        {
            var dates = DateTime.Parse(splitedLine[0]);
            dates.GetDateTimeFormats();
            var location = splitedLine[1];
            //float temp = float.Parse(splitedLine[2]);
            var humid = int.Parse(splitedLine[3]);
            int moldIndex = CalculateMoldIndex(res, humid);

            var myData = new WeatherData
            {
                Date = dates,
                Location = location,
                Temp = res,
                AirMoisture = humid,
                MoldIndex = moldIndex,
            };

            uniques.Add(myData);

            await Task.FromResult(uniques);
        }


        //TODO fixa fulhacket - kultur
        private float CheckForBadCharacters(string[] splitedLine)
        {
            float.TryParse(splitedLine[2], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float result);
            return result;
        }


        //TODO ge mer spridning
        private int CalculateMoldIndex(float temp, int humidity)
        {
            int value = 0;

            if (humidity > 90 && humidity <= 100 && temp >= 10 && temp <= 50)
                value = 3;
            else if (humidity > 80 && humidity <= 90 && temp >= 10 && temp <= 50)
                value = 2;
            else if (humidity >= 73 && humidity <= 80 && temp >= 10 && temp <= 50)
                value = 1;
            else if (temp > 50 || humidity < 73 && temp < 10 || temp < 10)
                value = 0;
            return value;
        }
    }
}
