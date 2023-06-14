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
                // �������� �������� ���������� a, b, c, d, eq �� ��������� �����
                double a = double.Parse(textBox1.Text);
                double b = double.Parse(textBox2.Text);
                double c = double.Parse(textBox3.Text);
                double d = double.Parse(textBox4.Text);
                double eq = double.Parse(textBox5.Text);

                
                // ���������� ������� ��� ������� ���������
                Func<double, double> function = x =>
                   a* Math.Pow(x,2) + b * x +  c*Math.Pow(eq, d * x) - eq;

                // ������ ��������� ������� �������
                double solution = equationSolver.NewtonMethod(function);

                // ������� ���������
                label1.Text = $"�������: x = {solution}";

                // ������ ������ �������
                functionPlotter.PlotFunction(function);
            }
            catch (FormatException)
            {
                MessageBox.Show("������ �����. ��������� ������������ ��������� ��������.");
            }
        }
    }

    public class EquationSolver
    {
        public double NewtonMethod(Func<double, double> function)
        {
            const double epsilon = 1e-10; // ������ ��������
            const int maxIterations = 1000; // ������������ ���������� ��������
            double x0 = 0.42; // ��������� �����������

            double x = x0;
            double fx = function(x);
            double dfx = Differentiate(function, x);

            int iterations = 0;
            while (Math.Abs(fx) > epsilon && iterations < maxIterations)
            {
                if (Math.Abs(dfx) < epsilon)
                {
                    // �������� ������� �� ����
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
            const double dx = 0.001; // ������ ��� ������������� ���������� �����������

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

            double xMin = -10; // ����������� �������� x ��� �������
            double xMax = 10; // ������������ �������� x ��� �������
            double yMin = -10; // ����������� �������� y ��� �������
            double yMax = 10; // ������������ �������� y ��� �������

            // ������� ������� ������� � ��������
            int plotWidth = plotPictureBox.Width;
            int plotHeight = plotPictureBox.Height;

            // ��������������� �������� x � y � �������� �������� �������
            Func<double, int> xToPixel = x =>
                (int)((x - xMin) / (xMax - xMin) * plotWidth);
            Func<double, int> yToPixel = y =>
                (int)((yMax - y) / (yMax - yMin) * plotHeight);

            // ������� �������
            graphics.Clear(Color.White);

            // ������ ��� ���������
            graphics.DrawLine(Pens.Black, xToPixel(xMin), yToPixel(0), xToPixel(xMax), yToPixel(0));
            graphics.DrawLine(Pens.Black, xToPixel(0), yToPixel(yMin), xToPixel(0), yToPixel(yMax));

            // ������ ������ �������
            double step = 0.01; // ��� ����� �������
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


            // ���������� ������ � PictureBox
            plotPictureBox.Image = bitmap;
        }
    }
}
