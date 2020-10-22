using System;
using System.Collections.Generic;
using System.Linq;

namespace EuroDiffusion.Models
{
    class Case
    {
        private const int MinLowCoordinateValue = 1;
        private const int MaxUpperCoordinateValue = 10;
        private const int MaxCountryNameLength = 25;
        private const int MaxCountriesCount = 20;
        private const int MinCountriesCount = 1;
        private const string SpaceSeparator = " ";

        public int Number { get; set; }
        public List<Country> Countries { get; set; }

        public string Process()
        {
            foreach (var country in Countries)
                country.InitCities(Countries.Count);

            var cities = Countries.SelectMany(c => c.Cities).ToList();
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
                ValidateCountriesCount(countriesCount, caseCounter);
                var countries = new List<Country>();

                for (var lineIndex = startLine + 1; lineIndex <= startLine + countriesCount; lineIndex++)
                {
                    var countryLine = lines[lineIndex];
                    var countryParams = countryLine.Split(SpaceSeparator);

                    var countryName = countryParams[0];
                    ValidateCountryName(countryName);

                    var xLowestValue = int.Parse(countryParams[1]);
                    var yLowestValue = int.Parse(countryParams[2]);
                    var xUpperValue = int.Parse(countryParams[3]);
                    var yUpperValue = int.Parse(countryParams[4]);

                    ValidateCoordinates(xLowestValue, xUpperValue, "x", countryName);
                    ValidateCoordinates(yLowestValue, yUpperValue, "y", countryName);

                    var leftLowestCoordinate = new Coordinate(xLowestValue, yLowestValue);
                    var rightUpperCoordinate = new Coordinate(xUpperValue, yUpperValue);

                    countries.Add(new Country(leftLowestCoordinate, rightUpperCoordinate, countryName));
                }
                cases.Add(new Case { Countries = countries, Number = caseCounter });

                Country.CountryIndex = 0;
                caseCounter++;
                startLine += countriesCount + 1;
            }

            return cases;
        }

        private static void ValidateCountriesCount(int countriesCount, int caseNumber)
        {
            if (countriesCount < MinCountriesCount || countriesCount > MaxCountriesCount)
                throw new ArgumentException($"Count of countries should be between {MinCountriesCount} and {MaxCountriesCount}, " +
                                            $"but got {countriesCount} for Case №{caseNumber}");
        }

        private static void ValidateCountryName(string countryName)
        {
            if (countryName.Length > 25)
                throw new ArgumentException($"Maximum length of country name: {MaxCountryNameLength}, " +
                                            $"but got {countryName} with length: {countryName.Length}");
        }

        private static void ValidateCoordinates(int lower, int higher, string coordinate, string countryName)
        {
            if (lower < MinLowCoordinateValue)

                throw new ArgumentException($"Lower {coordinate} coordinate should be >= {MinLowCoordinateValue}, but got {lower} for {countryName}");

            if (lower > higher)
                throw new ArgumentException($"Lower {coordinate} coordinate value should be <= {coordinate} higher coordinate, but got {lower} <= {higher} for {countryName}");

            if (higher > MaxUpperCoordinateValue)
                throw new ArgumentException($"Higher {coordinate} coordinate should be <= {MaxUpperCoordinateValue}, but got {higher} for {countryName}");

        }
    }
}
