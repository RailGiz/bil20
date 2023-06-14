using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace bil20
{
    public partial class Form1 : Form
    {
        private readonly EquationSolver equationSolver;
        private readonly FunctionPlotter functionPlotter;
        private PictureBox plotPictureBox;

        public Form1()
        {
            InitializeComponent();

            equationSolver = new EquationSolver();
            plotPictureBox = pictureBox1;
            functionPlotter = new FunctionPlotter(plotPictureBox);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем значения параметров a, b, c, d, eq из текстовых полей
                double a = double.Parse(textBox1.Text);
                double b = double.Parse(textBox2.Text);
                double c = double.Parse(textBox3.Text);
                double d = double.Parse(textBox4.Text);
                double eq = double.Parse(textBox5.Text);

                
                // Определяем функцию для решения уравнения
                Func<double, double> function = x =>
                   a* Math.Pow(x,2) + b * x +  c*Math.Pow(eq, d * x) - eq;

                // Решаем уравнение методом Ньютона
                double solution = equationSolver.NewtonMethod(function);

                // Выводим результат
                label1.Text = $"Решение: x = {solution}";

                // Строим график функции
                functionPlotter.PlotFunction(function);
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка ввода. Проверьте правильность введенных значений.");
            }
        }
    }

    public class EquationSolver
    {
        public double NewtonMethod(Func<double, double> function)
        {
            const double epsilon = 1e-10; // Предел точности
            const int maxIterations = 1000; // Максимальное количество итераций
            double x0 = 0.42; // Начальное приближение

            double x = x0;
            double fx = function(x);
            double dfx = Differentiate(function, x);

            int iterations = 0;
            while (Math.Abs(fx) > epsilon && iterations < maxIterations)
            {
                if (Math.Abs(dfx) < epsilon)
                {
                    // Проверка деления на ноль
                    break;
                }

                x -= fx / dfx;
                fx = function(x);
                dfx = Differentiate(function, x);

                iterations++;
            }

            return x;
        }

        private double Differentiate(Func<double, double> function, double x)
        {
            const double dx = 0.001; // Дельта для приближенного вычисления производной

            double x1 = x - dx;
            double x2 = x + dx;

            double f1 = function(x1);
            double f2 = function(x2);

            return (f2 - f1) / (x2 - x1);
        }

    }

    public class FunctionPlotter
    {
        private readonly PictureBox plotPictureBox;

        public FunctionPlotter(PictureBox pictureBox)
        {
            plotPictureBox = pictureBox;
        }

        public void PlotFunction(Func<double, double> function)
        {
            Bitmap bitmap = new Bitmap(plotPictureBox.Width, plotPictureBox.Height);
            Graphics graphics = Graphics.FromImage(bitmap);

            double xMin = -10; // Минимальное значение x для графика
            double xMax = 10; // Максимальное значение x для графика
            double yMin = -10; // Минимальное значение y для графика
            double yMax = 10; // Максимальное значение y для графика

            // Размеры области графика в пикселях
            int plotWidth = plotPictureBox.Width;
            int plotHeight = plotPictureBox.Height;

            // Масштабирование значений x и y в пределах размеров графика
            Func<double, int> xToPixel = x =>
                (int)((x - xMin) / (xMax - xMin) * plotWidth);
            Func<double, int> yToPixel = y =>
                (int)((yMax - y) / (yMax - yMin) * plotHeight);

            // Очистка графика
            graphics.Clear(Color.White);

            // Рисуем оси координат
            graphics.DrawLine(Pens.Black, xToPixel(xMin), yToPixel(0), xToPixel(xMax), yToPixel(0));
            graphics.DrawLine(Pens.Black, xToPixel(0), yToPixel(yMin), xToPixel(0), yToPixel(yMax));

            // Рисуем график функции
            double step = 0.01; // Шаг между точками
            double prevX = xMin;
            double prevY = function(prevX);

            for (double x = xMin + step; x <= xMax; x += step)
            {
                double y = function(x);

                int prevXPixel = xToPixel(prevX);
                int prevYPixel = yToPixel(prevY);
                int xPixel = xToPixel(x);
                int yPixel = yToPixel(y);

                if (prevXPixel >= 0 && prevXPixel < plotWidth && prevYPixel >= 0 && prevYPixel < plotHeight &&
                    xPixel >= 0 && xPixel < plotWidth && yPixel >= 0 && yPixel < plotHeight)
                {
                    graphics.DrawLine(Pens.Black, prevXPixel, prevYPixel, xPixel, yPixel);
                }

                prevX = x;
                prevY = y;
            }


            // Отображаем график в PictureBox
            plotPictureBox.Image = bitmap;
        }
    }
}
