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
            //var uniqueItems = new List<WeatherData>();
            List<DateTime> counter = new List<DateTime>();
            int myCounter = 0;
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
                    await AddToClass(uniques, splitedLine);
                }

                RemoveDuplicates();
                AddOpenTime();

     











            }
             SaveToDb(uniques);
           
            Console.WriteLine("klar");
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
            using (var writer = new StreamWriter(@"C:\Users\zn_19\Downloads\testtext.csv"))
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



        List<double> AvgPerQuarter(List<WeatherData> data)
        {
            DateTime time = data[0].Date;
            double avgTemp = 0;
            int points = 0;
            int count = 0;
            List<double> tempsPerQuarter = new List<double>();
            // double[] tempsPerQuarter = new double[96]; //96 är pga att det alltid bara finns 96 kvartar på ett dygn, kommer aldrig att ändras!

            for (int i = 0; i < data.Count; i++)
            {
                if (time.AddMinutes(15) > data[i].Date)
                {
                    avgTemp += data[i].Temp;
                    points++;
                }
                if (time.AddMinutes(15) <= data[i].Date)
                {
                    tempsPerQuarter.Add(Math.Round(avgTemp / points, 1)); // Lägger in medeltemperatur i listan.
                    count++;
                    points = 0; // Nollställer dessa inför nästa varv i loopen.
                    avgTemp = 0;
                    time = data[i].Date;
                }
            }
            return tempsPerQuarter;
        }



        void DoorOpen()
        {

            DateTime myDate = new DateTime(2016, 10, 01);
            DateTime myDate2 = new DateTime(2016, 10, 03);
            var outside = uniques.Where(x => x.Location == "Ute" && x.Date >= myDate && x.Date <= myDate2).ToList();
            var inside = uniques.Where(x => x.Location == "Inne" && x.Date >= myDate && x.Date <= myDate2).ToList();

            if (outside.Count == 0 || inside.Count == 0) return;
            List<double> outsideTemps = AvgPerQuarter(outside);
            List<double> insideTemps = AvgPerQuarter(inside);
            var doubleList = new List<string>();

            for (int i = 0; i + 1 < insideTemps.Count - 1; i++)
            {

                if (outsideTemps[i] < outsideTemps[i + 1] && insideTemps[i] > insideTemps[i + 1])
                {
                    doubleList.Add($"{outsideTemps[i]}/{outsideTemps[i + 1]}  {String.Format("{0:t}", inside[0].Date.AddMinutes(i * 15))}=>{String.Format("{0:t}", inside[0].Date.AddMinutes(i * 15 + 30))}   {insideTemps[i]}/{insideTemps[i + 1]}");
                }
            }
            foreach (var item in doubleList)
            {
                Console.WriteLine(item);
            }
        }
    }
}
