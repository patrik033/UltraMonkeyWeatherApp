using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMonkeyEFLibrary
{
  
    public class WeatherData
    {


        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public float Temp { get; set; }
        public int? AirMoisture { get; set; }
        public int? MoldIndex { get; set; }
        public double? OpenTime { get; set; }
        public double Diff { get; set; }


        public override string ToString()
        {
            return $"{Date} {Location} {Temp}";
        }
    }
}
