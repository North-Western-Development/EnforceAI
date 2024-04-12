// See https://aka.ms/new-console-template for more information

using System.Numerics;

(Vector2, Vector2)[] testValues = new (Vector2, Vector2)[1000000];
Random gen = new Random();

for (int i = 0; i < 1000000; i++)
{
    testValues[i] = (new Vector2(gen.Next(), gen.Next()), new Vector2(gen.Next(), gen.Next()));
}

Console.WriteLine("Test One: 1000000 iterations of \u221a((x\u2082-x\u2081)\u00b2 + (y\u2082-y\u2081)\u00b2)");

DateTime testOneStart = DateTime.Now;

for (int i = 0; i < 1000000; i++)
{
    float distance = MathF.Sqrt(MathF.Pow(testValues[i].Item2.X - testValues[i].Item1.X, 2) + MathF.Pow(testValues[i].Item2.Y - testValues[i].Item1.Y, 2));
}

DateTime testOneStop = DateTime.Now;

Console.WriteLine("Finished Test One");

Console.WriteLine("Starting Test Two: 1000000 iterations of (x\u2082-x\u2081)\u00b2 + (y\u2082-y\u2081)\u00b2");

DateTime testTwoStart = DateTime.Now;

for (int i = 0; i < 1000000; i++)
{
    float distance = (MathF.Pow(testValues[i].Item2.X - testValues[i].Item1.X, 2) + MathF.Pow(testValues[i].Item2.Y - testValues[i].Item1.Y, 2));
}

DateTime testTwoStop = DateTime.Now;

Console.WriteLine("Finished Test Two");

double differenceOne = (testOneStop - testOneStart).TotalMilliseconds;
double differenceTwo = (testTwoStop - testTwoStart).TotalMilliseconds;

Console.WriteLine($"First test took {differenceOne}ms");
Console.WriteLine($"Second test took {differenceTwo}ms");

Console.WriteLine($"Performance improvement {100-((differenceTwo / differenceOne) * 100)}%");