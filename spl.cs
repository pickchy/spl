using System;
using System.Collections.Generic;
using System.IO;

public class SimpleSpline
{
    private double smoothParam;
    private List<double> x = new List<double>();
    private List<double> f = new List<double>();
    private List<double> alpha = new List<double>();
    private List<double> mainDiag = new List<double>();
    private List<double> offDiag = new List<double>();
    private List<double> rhs = new List<double>();

    public SimpleSpline(double smoothParam = 0.5)
    {
        this.smoothParam = smoothParam;
    }

    public void AddPoint(double xValue, double fValue)
    {
        x.Add(xValue);
        f.Add(fValue);
    }

    public void ComputeSpline()
    {
        double avgF = 0;
        foreach(var val in f) avgF += val;
        avgF /= f.Count;

        for (int i = 0; i < x.Count; i++)
        {
            mainDiag.Add(0);
            rhs.Add(0);
        }

        for (int i = 0; i < x.Count - 1; i++)
        {
            double dx = x[i + 1] - x[i];
            double baseWeight = 1 - smoothParam;

            mainDiag[i] += baseWeight / dx;
            mainDiag[i + 1] += baseWeight / dx;
            rhs[i] += baseWeight * f[i];
            rhs[i + 1] += baseWeight * f[i + 1];
            offDiag.Add(-baseWeight / (2 * dx));
        }

        rhs[0] += smoothParam * avgF;
        rhs[^ 1] += smoothParam * avgF;

        List<double> u = new List<double>(), v = new List<double>();
        double z = mainDiag[0];
        v.Add(offDiag[0] / z);
        u.Add(rhs[0] / z);

        for (int i = 1; i < mainDiag.Count - 1; i++)
        {
            z = mainDiag[i] + offDiag[i - 1] * v[i - 1];
            v.Add(offDiag[i - 1] / z);
            u.Add((rhs[i] - offDiag[i - 1] * u[i - 1]) / z);
        }

        z = mainDiag[^ 1] + offDiag[^ 1] * v[^ 2];
        u.Add((rhs[^ 1] - offDiag[^ 1] * u[^ 2]) / z);

        alpha.Add(u[^ 1]);
        for (int i = u.Count - 2; i >= 0; i--)
            alpha.Insert(0, u[i] + v[i] * alpha[0]);
    }

    public void SaveResult(string outputFile)
    {
        using StreamWriter writer = new StreamWriter(outputFile);
        for (int i = 0; i < x.Count; i++)
        {
            writer.WriteLine(alpha[i]);
        }
    }
}

public class Program
{
    public static void Main()
    {
        SimpleSpline spline = new SimpleSpline(0.5);
        using (StreamReader reader = new StreamReader("D:\Input.txt"))
        {
            int index = 1;
            string line;
            while ((line = reader.ReadLine()) != null && double.TryParse(line, out double value))
            {
                spline.AddPoint(index++, value);
            }
        }

        spline.ComputeSpline();
        spline.SaveResult("D:\Output.txt");
    }
}
