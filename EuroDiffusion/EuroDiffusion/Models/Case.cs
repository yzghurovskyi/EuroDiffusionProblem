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
        private const string CasesInputEndTerminator = "0";

        private readonly List<string> _caseInput;
        private readonly int _number;
        private readonly int _expectedCountriesCount;
        private List<Country> _countries;

        public Case(int number, List<string> caseInput, int expectedCountriesCount)
        {
            _number = number;
            _caseInput = caseInput;
            _expectedCountriesCount = expectedCountriesCount;
        }

        public string Process()
        {
            try
            {
                ValidateCountriesCount(_caseInput.Count);
                _countries = _caseInput.Select(InitCountry).ToList();
            }
            catch (Exception ex)
            {
                return GetResult(ex.Message);
            }
            finally
            {
                Country.CountryIndex = 0;
            }

            InitCities();

            var allCities = _countries.SelectMany(c => c.Cities).ToList();

            var crossedCoordinates = CrossedCoordinates(allCities);
            if (crossedCoordinates.Any())
            {
                var crossedCoordinatesMessage = $"Validation error: Cities crossed at: {string.Join(", ", crossedCoordinates)}";
                return GetResult(crossedCoordinatesMessage);
            }

            SetUpRelations(allCities);

            if (_countries.Count > 1)
            {
                var countriesWithoutNeighbours = CountriesWithoutNeighbours();
                if (countriesWithoutNeighbours.Any())
                {
                    var crossedCoordinatesMessage = $"Validation error: Countries without borders: {string.Join(", ", countriesWithoutNeighbours.Select(c => c.Name))}";
                    return GetResult(crossedCoordinatesMessage);
                }
            }

            var day = 0;
            while (true)
            {
                _countries.ForEach(c => c.CheckCompletion(day));

                if (_countries.TrueForAll(c => c.IsComplete))
                    break;

                day++;

                foreach (var country in _countries)
                    country.StartDay();

                foreach (var country in _countries)
                    country.EndDay();
            }

            return GetResult(
                $"{string.Join(Environment.NewLine, _countries.OrderBy(c => c.CompleteDay).ThenBy(c => c.Name))}");
        }

        private void ValidateCountriesCount(int actualCountriesCount)
        {
            if(actualCountriesCount != _expectedCountriesCount)
                throw new ArgumentException(
                    $"Validation error: Count of countries should be {_expectedCountriesCount}, but got {actualCountriesCount} country lines");

            if (actualCountriesCount < MinCountriesCount || actualCountriesCount > MaxCountriesCount)
                throw new ArgumentException(
                    $"Count of countries should be between {MinCountriesCount} and {MaxCountriesCount}, " +
                    $"but got {actualCountriesCount}");
        }

        private void ValidateCountryName(string countryName)
        {
            if (countryName.Length > 25)
                throw new ArgumentException($"Maximum length of country name: {MaxCountryNameLength}, " +
                                            $"but got {countryName} with length: {countryName.Length}");
        }

        private void ValidateCoordinates(int lower, int higher, string coordinate, string countryName)
        {
            if (lower < MinLowCoordinateValue)

                throw new ArgumentException(
                    $"Lower {coordinate} coordinate should be >= {MinLowCoordinateValue}, but got {lower} for {countryName}");

            if (lower > higher)
                throw new ArgumentException(
                    $"Lower {coordinate} coordinate value should be <= {coordinate} higher coordinate, but got {lower} <= {higher} for {countryName}");

            if (higher > MaxUpperCoordinateValue)
                throw new ArgumentException(
                    $"Higher {coordinate} coordinate should be <= {MaxUpperCoordinateValue}, but got {higher} for {countryName}");

        }

        private string GetResult(string message) => $"Case Number {_number}{Environment.NewLine}{message}";

        private List<Coordinate> CrossedCoordinates(List<City> cities)
        {
            var citiesGroups = cities.GroupBy(c => c.Coordinate)
                .ToDictionary(group => group.Key, group => group.ToList());

            return citiesGroups.Where(group => group.Value.Count > 1).Select(group => group.Key).ToList();
        }

        private Country InitCountry(string countryLine)
        {
            var countryParams = countryLine.Split(SpaceSeparator);

            if(countryParams.Length != 5)
                throw new ArgumentException($"Invalid count of parameter for {countryLine}");

            var countryName = countryParams[0];
            ValidateCountryName(countryName);

            var xLowestValue = ParseCoordinateValue(countryParams[1]);
            var yLowestValue = ParseCoordinateValue(countryParams[2]);
            var xUpperValue = ParseCoordinateValue(countryParams[3]);
            var yUpperValue = ParseCoordinateValue(countryParams[4]);

            ValidateCoordinates(xLowestValue, xUpperValue, "x", countryName);
            ValidateCoordinates(yLowestValue, yUpperValue, "y", countryName);

            var leftLowestCoordinate = new Coordinate(xLowestValue, yLowestValue);
            var rightUpperCoordinate = new Coordinate(xUpperValue, yUpperValue);

            return new Country(leftLowestCoordinate, rightUpperCoordinate, countryName);
        }

        private void InitCities()
        {
            foreach (var country in _countries)
                country.InitCities(_countries.Count);
        }

        private void SetUpRelations(List<City> cities)
        {
            var citiesMap = cities.ToDictionary(c => c.Coordinate);

            foreach (var city in cities)
                city.DefineNeighbours(citiesMap);
        }

        private List<Country> CountriesWithoutNeighbours() => _countries.Where(c => !c.HasForeignBorder()).ToList();

        public static List<Case> Parse(string input)
        {
            var lines = input.Split(Environment.NewLine);

            var startLine = 0;
            var caseCounter = 1;
            var cases = new List<Case>();

            while (true)
            {
                var countryCountLine = lines[startLine];

                if (!int.TryParse(countryCountLine, out var expectedCountriesCount))
                    throw new ArgumentException(
                        $"Can not define count of countries for Case №{caseCounter}, got {countryCountLine}");

                if (expectedCountriesCount == 0)
                    if (countryCountLine.Trim().Equals(CasesInputEndTerminator))
                        break;
                    else
                        throw new ArgumentException(
                            $"Invalid end of cases: \"{countryCountLine}\"");

                var caseLines = lines
                    .Skip(startLine + 1)
                    .TakeWhile(line => !char.IsDigit(line[0])).ToList();

                cases.Add(new Case(caseCounter, caseLines, expectedCountriesCount));

                caseCounter++;
                startLine += caseLines.Count + 1;
            }

            return cases;
        }

        private int ParseCoordinateValue(string value)
        {
            if(!int.TryParse(value, out var result))
                throw new ArgumentException($"Can not parse \"{value}\" as coordinate value");

            return result;
        }
    }
}
