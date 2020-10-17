using System;
using System.Collections.Generic;
using System.Linq;

namespace EuroDiffusion.Models
{
    class Case
    {
        public int Number { get; set; }
        public List<Country> Countries { get; set; }

        public string Process()
        {
            foreach (var country in Countries)
                country.InitCities(Countries.Count);

            var cities = Countries.SelectMany(c => c.Cities);
            var citiesMap = cities.ToDictionary(c => c.Coordinate);

            foreach (var city in cities)
                city.DefineNeighbours(citiesMap);

            var day = 0;

            while (true)
            {
                Countries.ForEach(c => c.CheckCompletion(day));

                if (Countries.TrueForAll(c => c.IsComplete))
                    break;

                day++;

                foreach (var country in Countries)
                    country.StartDay();

                foreach (var country in Countries)
                    country.EndDay();
            }

            return $"Case Number {Number}{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, Countries.OrderBy(c => c.CompleteDay).ThenBy(c => c.Name))}"; 
        }

        public static List<Case> Parse(string input)
        {
            var lines = input.Split(Environment.NewLine);

            var startLine = 0;
            var caseCounter = 1;
            var cases = new List<Case>();

            while (int.TryParse(lines[startLine], out var countriesCount) && countriesCount != 0)
            {
                var countries = new List<Country>();

                for (var lineIndex = startLine + 1; lineIndex <= startLine + countriesCount; lineIndex++)
                {
                    var countryLine = lines[lineIndex];
                    var countryParams = countryLine.Split(" ");

                    var countryName = countryParams[0];
                    var leftLowestCoord = new Coordinate(int.Parse(countryParams[1]), int.Parse(countryParams[2]));
                    var rightHighestCoord = new Coordinate(int.Parse(countryParams[3]), int.Parse(countryParams[4]));

                    countries.Add(new Country(leftLowestCoord, rightHighestCoord, countryName));
                }
                cases.Add(new Case { Countries = countries, Number = caseCounter });

                Country.CountryIndex = 0;
                caseCounter++;
                startLine += countriesCount + 1;
            }

            return cases;
        }
    }
}
