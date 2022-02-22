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

                    string path = @"..\..\..\..\TempFuktData.csv";
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

            //AddToFile(uniques, @"..\..\..\..\testtext.csv");
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
                for (int j = i + 1; j < uniques.Count; j++)
                {
                    if (uniques[i].Date == uniques[j].Date && uniques[i].Location == uniques[j].Location && uniques[i].Temp == uniques[j].Temp && uniques[i].AirMoisture == uniques[j].AirMoisture)
                    {
                        uniques.RemoveAt(j);
                    }
                    else if (uniques[i].Date != uniques[j].Date)
                        break;
                }
            }

            for (int i = 0; i < uniques.Count; i++)
            {
                for (int j = i + 1; j < uniques.Count; j++)
                {
                    if (uniques[i].Date == uniques[j].Date && uniques[i].Location == uniques[j].Location)
                    {
                        uniques.RemoveAt(j);
                    }
                    else if (uniques[i].Date != uniques[j].Date)
                        break;
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
                value = 7;
            if (humidity > 80 && humidity <= 90 && temp >= 10 && temp <= 50)
                value = 6;
            if (humidity > 70 && humidity <= 80 && temp >= 10 && temp <= 50)
                value = 5;
            if (humidity > 60 && humidity <= 70 && temp >= 10 && temp <= 50)
                value = 4;
            if (humidity > 50 && humidity <= 60 && temp >= 10 && temp <= 50)
                value = 3;
            if (humidity > 40 && humidity <= 50 && temp >= 10 && temp <= 50)
                value = 2;
            if (humidity > 30 && humidity <= 40 && temp >= 10 && temp <= 50)
                value = 1;
            //if (humidity > 20 && humidity <= 60 && temp >= 10 && temp <= 50)
            //    value = 2;
            //if (humidity > 10 && humidity <= 50 && temp >= 10 && temp <= 50)
            //    value = 1;
            return value;
        }
    }
}

