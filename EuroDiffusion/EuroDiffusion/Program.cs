using EuroDiffusion.Models;
using System;
using System.IO;
using System.Linq;

namespace EuroDiffusion
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            var inputPath = Path.Combine(currentDirectory, args[0]);
            var outputPath = Path.Combine(currentDirectory, args[1]);

            var input = File.ReadAllText(inputPath);
            var cases = Case.Parse(input);
            var output = string.Join(Environment.NewLine, cases.Select(diffusionCase => diffusionCase.Process()));

            File.WriteAllText(outputPath, output);
        }
    }
}
