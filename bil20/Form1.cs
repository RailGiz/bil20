using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace bil20
{
    public partial class Form1 : Form
    {
        private EquationSolver equationSolver;
        private FunctionPlotter functionPlotter;
        private GraphExporter graphExporter;
        private DatabaseManager databaseManager;

        public Form1()
        {
            InitializeComponent();

            // Инициализация объектов и подключение к базе данных
            equationSolver = new EquationSolver();
            functionPlotter = new FunctionPlotter(pictureBox1);
            graphExporter = new GraphExporter(pictureBox1);

            databaseManager = new DatabaseManager("Server=localhost;Database=bil20;Uid=root;Pwd=;");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Получение значений параметров a, b, c, d, eq из текстовых полей
                double a = double.Parse(textBox1.Text);
                double b = double.Parse(textBox2.Text);
                double c = double.Parse(textBox3.Text);
                double d = double.Parse(textBox4.Text);
                double eq = double.Parse(textBox5.Text);

                // Определение функции для решения уравнения
                Func<double, double> function = x =>
                   a * Math.Pow(x, 2) + b * x + c * Math.Pow(eq, d * x) - eq;

                // Решение уравнения методом Ньютона
                double solution = equationSolver.NewtonMethod(function);

                // Вывод результата
                label1.Text = $"Решение: x = {solution}";

                // Построение графика функции
                functionPlotter.PlotFunction(function);

                // Сохранение графика в базе данных
                byte[] graphImageData = graphExporter.ExportToByteArray();
                databaseManager.SaveImageToDatabase(graphImageData);
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

            // Рисование осей координат
            graphics.DrawLine(Pens.Black, xToPixel(xMin), yToPixel(0), xToPixel(xMax), yToPixel(0));
            graphics.DrawLine(Pens.Black, xToPixel(0), yToPixel(yMin), xToPixel(0), yToPixel(yMax));

            // Рисование графика функции
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

            // Отображение графика в PictureBox
            plotPictureBox.Image = bitmap;
        }
    }

    public class GraphExporter
    {
        private readonly PictureBox pictureBox;

        public GraphExporter(PictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
        }

        public byte[] ExportToByteArray()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                pictureBox.Image.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }

    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void SaveImageToDatabase(byte[] imageBytes)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand("INSERT INTO Images (image) VALUES (@image)", connection);
                command.Parameters.AddWithValue("@image", imageBytes);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}