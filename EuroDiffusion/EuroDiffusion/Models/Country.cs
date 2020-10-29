using System.Collections.Generic;
using System.Linq;

namespace EuroDiffusion.Models
{
    class Country
    {
        public readonly int Id;
        public readonly string Name;
        public readonly List<City> Cities;

        public int CompleteDay { get; private set; }
        public bool IsComplete { get; private set; }
        public static int CountryIndex { get; set; }

        private readonly Coordinate _leftLowest;
        private readonly Coordinate _rightHighest;

        static Country()
        {
            CountryIndex = 0;
        }

        private Country()
        {
            Id = CountryIndex;
            CountryIndex++;
            Cities = new List<City>();
        }

        public Country(Coordinate leftLowest, Coordinate rightHighest, string name) : this()
        {
            Name = name;
            _leftLowest = leftLowest;
            _rightHighest = rightHighest;
        }

        public void InitCities(int countriesCount)
        {
            for (var x = _leftLowest.X; x <= _rightHighest.X; x++)
                for (var y = _leftLowest.Y; y <= _rightHighest.Y; y++)
                    Cities.Add(new City(this, countriesCount, new Coordinate(x, y)));
        }

        public void StartDay()
        {
            foreach (var city in Cities)
                city.StartDay();
        }

        public void CheckCompletion(int day)
        {
            if (!IsComplete && Cities.TrueForAll(city => city.IsComplete))
            {
                CompleteDay = day;
                IsComplete = true;
            }
        }

        public void EndDay()
        {
            foreach (var city in Cities)
                city.EndDay();
        }

        public bool HasForeignBorder() => Cities.Any(c => c.HasForeignNeighbour());

        public override string ToString() => $"{Name} {CompleteDay}";
    }
}
