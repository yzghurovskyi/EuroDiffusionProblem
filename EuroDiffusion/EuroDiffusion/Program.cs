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
            var output = string.Empty;

            try
            {
                var cases = Case.Parse(input);
                output = string.Join(Environment.NewLine, cases.Select(diffusionCase => diffusionCase.Process()));
            }
            catch (ArgumentException ex)
            {
                output = ex.Message;
            }
            finally
            {
                File.WriteAllText(outputPath, output);
            }
        }
    }
}
