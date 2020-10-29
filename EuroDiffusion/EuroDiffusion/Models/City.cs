using System.Collections.Generic;
using System.Linq;

namespace EuroDiffusion.Models
{
    class City
    {
        public Coordinate Coordinate { get; }

        private readonly int[] _totalBalance;
        private readonly int[] _dailyIncome;
        private readonly int[] _dailyExpenses;
        private readonly List<City> _neighbours;
        private readonly Country _country;

        private const int InitialBudget = 1000000;
        private const int RepresentativeCount = 1000;

        public City(Country country, int countriesCount, Coordinate coordinate)
        {
            _totalBalance = new int[countriesCount];
            _dailyIncome = new int[countriesCount];
            _dailyExpenses = new int[countriesCount];

            _totalBalance[country.Id] = InitialBudget;

            Coordinate = coordinate;
            _neighbours = new List<City>();
            _country = country;
        }

        public bool IsComplete => _totalBalance.All(motifMonets => motifMonets != 0);

        public void StartDay()
        {
            for(var motifIndex = 0; motifIndex < _totalBalance.Length; motifIndex++)
            {
                var monetsCount = _totalBalance[motifIndex] / RepresentativeCount;

                foreach (var city in _neighbours)
                {
                    _dailyExpenses[motifIndex] += monetsCount;
                    city.Fill(motifIndex, monetsCount);
                }
            }
        }

        public void EndDay()
        {
            for (var motifIndex = 0; motifIndex < _totalBalance.Length; motifIndex++)
            {
                _totalBalance[motifIndex] += _dailyIncome[motifIndex] - _dailyExpenses[motifIndex];

                _dailyIncome[motifIndex] = 0;
                _dailyExpenses[motifIndex] = 0;
            }
        }

        public void DefineNeighbours(Dictionary<Coordinate, City> citiesMap)
        {
            if (citiesMap.TryGetValue(new Coordinate(Coordinate.X + 1, Coordinate.Y), out var neighbour))
                _neighbours.Add(neighbour);

            if (citiesMap.TryGetValue(new Coordinate(Coordinate.X - 1, Coordinate.Y), out neighbour))
                _neighbours.Add(neighbour);

            if (citiesMap.TryGetValue(new Coordinate(Coordinate.X, Coordinate.Y + 1), out neighbour))
                _neighbours.Add(neighbour);

            if (citiesMap.TryGetValue(new Coordinate(Coordinate.X, Coordinate.Y - 1), out neighbour))
                _neighbours.Add(neighbour);
        }

        public bool HasForeignNeighbour() => _neighbours.Any(n => n._country.Id != _country.Id);

        private void Fill(int countryId, int monetsCount) => _dailyIncome[countryId] += monetsCount;
    }
}
