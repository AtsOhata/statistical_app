using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StatisticalApp
{
    public partial class Form1 : Form
    {
        /// <summary>Folder path of the analised file</summary>
        string FolderPath;
        /// <summary>File name of the analised file</summary>
        string FileName;

        /// <summary>The float value added when there is a missing datum</summary>
        readonly double DATA_MISSING = -9999.9999f;

        public Form1()
        {
            InitializeComponent();
        }
        void Form1_Load(object sender, EventArgs e){}

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i];
                textBox1.Text = fileName;
            }
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }


        /// <summary>Read a file</summary>
        List<string> ReadText(string path)
        {
            List<string> list = new();
            FolderPath = Path.GetDirectoryName(path);
            FileName = Path.GetFileNameWithoutExtension(path);
            using (StreamReader reader = new(path))
            {
                while (reader.Peek() != -1) list.Add(reader.ReadLine());
                return list;
            }
        }
        
        /// <summary>Read a text file and make the values double line by line</summary>
        List<double> ReadTextAsDouble()
        {
            List<string> list = ReadText(textBox1.Text);
            if (list == null) return null;
            List<double> values = new();
            list.ForEach(x => values.Add(double.Parse(x)));
            return values;
        }

        /// <summary>Read a text file and make the values double list</summary>
        List<List<double>> ReadTextFloatList()
        {
            List<string> list = ReadText(textBox1.Text);
            List<List<string>> stringValues = new List<List<string>>(list.Select(x => x.Split(',').ToList()));
            List<List<double>> values = new List<List<double>>();
            double dummy = 0f;
            stringValues.ForEach(x =>
            {
                for (int i = 0; i < x.Count; i++) if (!double.TryParse(x[i], out dummy)) x[i] = DATA_MISSING.ToString();
                values.Add(x.ConvertAll(double.Parse));
            });
            return values;
        }

        void WriteText(string text)
        {
            SaveFileDialog sfd = new();
            sfd.FileName = FileName + "_Result.txt";
            sfd.InitialDirectory = FolderPath;
            sfd.Filter = "txt file(*.txt)|*.txt";
            sfd.Title = "Choose where to save";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using StreamWriter sw = new(sfd.FileName);
                sw.WriteLine(text);
            }
        }

        void WriteText(List<string> text)
        {
            SaveFileDialog sfd = new();
            sfd.FileName = FileName + "_Result.txt";
            sfd.InitialDirectory = FolderPath;
            sfd.Filter = "txt file(*.txt)|*.txt";
            sfd.Title = "Choose where to save";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using StreamWriter sw = new(sfd.FileName);
                text.ForEach(x => sw.WriteLine(x));
            }
        }

        /// <summary>Calculate percentile</summary>
        /// <param name="percent">0～100</param>
        double CalculatePercentile(double percent, List<double> data)
        {
            if (percent < 0 || percent > 100) return -1f;
            data.Sort();
            int t = (int)Math.Floor((data.Count + 1) * (percent / 100));  // The integer of (amount of data + 1) * (N / 100)
            double q = (data.Count + 1) * (percent / 100) % 1;  // The decimal of (amount of data + 1) * (N / 100)
            return data[t - 1] + (data[t] - data[t - 1]) * q;
        }

        /// <summary>Calculate SD</summary>
        /// <param name="data">Data</param>
        /// <returns>Standard Deviations</returns>
        List<double> CaluclateStandardDeviation(List<double> data)
        {
            double average = data.Average();
            List<double> deviations = data.Select(x => x - average).ToList();
            List<double> deviationSquare = deviations.Select(x => x * x).ToList();
            double variance = deviationSquare.Sum() / data.Count();
            double standardDeviation = (double) Math.Sqrt(variance);
            double variance2 = deviationSquare.Sum() / (data.Count() - 1);
            double standardDeviation2 = (double) Math.Sqrt(variance2);
            return new List<double>() {standardDeviation, standardDeviation2 };
        }


        /// <summary>
        /// Arithmetic Mean<br></br>
        /// Explanation:　Add all values and divide by the amount of values<br></br>
        /// Example Usage:　For many use<br></br>
        /// Formula:　(x1+x2+x3+...+xn) / n
        /// </summary>
        void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }
            
            double result = values.Average();

            WriteText("Arithmetic mean = " + result);
        }


        /// <summary>
        /// Geometric Mean<br></br>
        /// Explanation: The average of rate of change<br></br>
        /// Example Usage: To calculate the rate of growth in sales<br></br>
        /// Formula:　ⁿ√x1*n2*n3*...*xn
        /// </summary>
        void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            int i = values.Count;
            double result = Math.Pow(values[i - 1] / values[0], 1.0 / (i - 1));

            WriteText("Geometric mean = " + result);
        }

        /// <summary>
        /// Harmonic Mean<br></br>
        /// Explanation: The reciprocal of the average of the reciprocals of values<br></br>
        /// Example Usage: To calculate the average speed of round trip. Baically when there is any meaning in calculating the reciprocal of data<br></br>
        /// Formula:　n / (1/x1 + 2/x2+...+1/xn)
        /// </summary>
        void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            int i = values.Count;
            double result = values.Count / (values.Select(x => 1 / x).Sum());

            WriteText("Harmonic mean = " + result);
        }


        /// <summary>
        /// Median<br></br>
        /// Explanation: The middle value of data sorted in ascending/descending order<br></br>
        /// Example Usage: To evaluate the salary by households. It is better to use with the average value to remove the effect of outlier<br></br>
        /// Formula: If amount of data is even, calculate the average of two middle data
        /// </summary>
        void button4_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            values.Sort();
            int i = values.Count;
            double result;
            if (i % 2 == 1) result = values[i / 2];
            else result = (values[i / 2 - 1] + values[i / 2]) / 2.0;
            
            WriteText("Median = " + result);
        }


        /// <summary>
        /// Proportion<br></br>
        /// Explanation: The amount of part in whole<br></br>
        /// Example Usage:　For many use<br></br>
        /// Formula:　c(part) / n(whole)
        /// </summary>
        void button5_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double sum = values.Sum();
            List<string> result = new();
            values.ForEach(x => result.Add(x + " - " + x / sum * 100 + "%"));
            WriteText(result);
        }

        /// <summary>
        /// Percentile<br></br>
        /// Explanation: The position of a datum from data cut into 100 pieces and sorted in ascending order<br></br>
        /// Example Usage: To evaluate a company's yearly sales in the whole industry<br></br>
        /// The first quartile 25%  Median50%　The third quartile75%<br></br>
        /// Formula:　To calculate N percentile　Given that the integer of (Amount of data + 1) * (N / 100) is T and decimal is Q,  The Tth + ((T+1)th - Tth) * Q<br></br>
        /// Example: 20 data　75percentile => 21 * 0.75 = 15.75 => (15th datum) + (16th datum - 15th datum) * 0.75
        /// </summary>
        void button6_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double n = -1;
            Form2 f2 = new();
            f2.LabelText = "The percentile of what value do you need? Input a value from 0 to 100";
            f2.Input = "0";
            while( n < 0 || n > 100)
            {
                f2.ShowDialog();
                if (f2.QuitFlag) return;
                try { n = double.Parse(f2.Input); }
                catch (FormatException) { MessageBox.Show("Input a value from 0 to 100", "Error", MessageBoxButtons.OK); continue; }
                if (n < 0 || n > 100) { MessageBox.Show("Input a value from 0 to 100", "Error", MessageBoxButtons.OK); }
            }
            double result = CalculatePercentile(n, values);
            WriteText(n + "percentile = " + result);
        }


        /// <summary>
        /// Mode<br></br>
        /// Explanation: The most frequent value in data<br></br>
        /// Example Usage: To calculate the most frequent amount of money in allowance<br></br>
        /// Formula: N/A
        /// </summary>
        void button7_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            List<double> elements = values.Distinct().ToList();
            int counts = 0;
            List<double> results = new();
            elements.ForEach(x =>
            {
                int a = values.Where(y => x == y).Count(); 
                if (a > counts)
                {
                    counts = a;
                    results.Clear();
                    results.Add(x);
                }
                else if (a == counts)
                {
                    results.Add(x);
                }
            });
            string s = string.Join('・', results);
            WriteText(new List<string>()
            {
                "Mode = " + s,
                "Quantity of the value = " + counts
            });
        }

        /// <summary>
        /// Standard deviation(SD) and unbiased SD<br></br>
        /// Explanation: The variability of data<br></br>
        /// Example Usage:　For many use<br></br>
        /// Formula:　Calculate average, deviation, deviation square, sum of deviation square and variation<br></br>
        /// deviation = Each value - average<br></br>
        /// deviation square = deviation * deviation<br></br>
        /// variation = The average of sum of deviation square<br></br>
        /// unbiased SD = SD - √variation
        /// </summary>
        void button8_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            List<double> standardDeviations = CaluclateStandardDeviation(values);

            WriteText(new List<string>()
            {
                "Standard deviation = " + standardDeviations[0],
                "Unbiased standard deviation = " + standardDeviations[1]
            });
        }


        /// <summary>
        /// Coefficient of variation<br></br>
        /// Explanation: The value of SD / average<br></br>
        /// Example Usage: To compare variations of data of different unit like height and weight<br></br>
        /// Formula: SD / average<br></br>
        /// Coefficient of variation &gt;= 1 Large(With outliers)   0.5 &lt;= Coefficient of variation &lt; 1 A little large
        /// </summary>
        void button9_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double average = values.Average();
            List<double> deviations = values.Select(x => x - average).ToList();
            List<double> deviationSquare = deviations.Select(x => x * x).ToList();
            double variance = deviationSquare.Sum() / values.Count;
            double standardDeviation = Math.Sqrt(variance);
            double result = (double) standardDeviation / average;
            WriteText("Coefficient of variation = " + result);
        }


        /// <summary>
        /// Quartile deviation<br></br>
        /// Explanation: Interquartile range = The delta between Q1(25 percentile) and Q3(75 percentile)<br></br>
        /// Quartile deviation = The half value of the interquartile range<br></br>
        /// Example Usage: To evaluate the variation of data without being affected by extreme values (since it evaluates the value 25～75%)<br></br>
        /// Formula: Interquartile range = Q3 - Q1<br></br>
        /// Quartile deviation = Interquartile range / 2
        /// </summary>
        void button10_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double Q1 = CalculatePercentile(25, values);
            double Q3 = CalculatePercentile(75, values);
            double interquartileRange = Q3 - Q1;
            double quartileDeviation = interquartileRange / 2;

            WriteText("Quartile deviation = " + quartileDeviation);
        }

        /// <summary>
        /// Outliers<br></br>
        /// Explanation: Extreme values<br></br>
        /// Example Usage: For many use<br></br>
        /// Formula: Non-normal distribution => Use the lower point and upper point<br></br>
        /// Normal distribution => Smirnov-Grubbs test<br></br>
        /// Lower point = Q1 - interquartile range * 1.5<br></br>
        /// Upper point = Q3 + interquartile range * 1.5
        /// </summary>
        void button11_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double Q1 = CalculatePercentile(25, values);
            double Q3 = CalculatePercentile(75, values);
            double interquartileRange = Q3 - Q1;
            double lowerPoint = Q1 - interquartileRange * 1.5f;
            double upperPoint = Q3 + interquartileRange * 1.5f;
            List<double> outliers = new();
            values.ForEach(x => { if (x < lowerPoint || x > upperPoint) outliers.Add(x); });

            WriteText(new List<string>()
            {
                "Lower point = " + lowerPoint + " Upper point = " + upperPoint,
                "Outliers = " + string.Join('・', outliers)
            });
        }

        /// <summary>
        /// Normalized score<br></br>
        /// Explanation: It shows the position of a datum in the whole data set<br></br>
        /// Example Usage: To evaluate values of different types of data set with different averages and SDs like scores in maths and English<br></br>
        /// You can how good your scores are<br></br>
        /// Formula: Normalized score = (datum - average) / SD
        /// </summary>
        void button12_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double average = values.Average();
            double standardDeviation = CaluclateStandardDeviation(values)[0];
            List<double> normalizedScore = new(values.Select(x => (x - average) / standardDeviation));
            WriteText(normalizedScore.Select(x => x.ToString()).ToList());
        }


        /// <summary>
        /// Deviation value<br></br>
        /// Explanation: The evaluation of a datum considering the deviation of data<br></br>
        /// Example Usage: To compare scores of different subjects, to compare quality of different items<br></br>
        /// Formula: 10 * normalized score + 50
        /// </summary>
        void button13_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double average = values.Average();
            double standardDeviation = CaluclateStandardDeviation(values)[0];
            List<double> normalizedScore = new(values.Select(x => (x - average) / standardDeviation));
            List<double> deviationValues = new(normalizedScore.Select(x => 10 * x + 50));
            WriteText(deviationValues.Select(x => x.ToString()).ToList());
        }

        /// <summary>
        /// Single correlation coefficient<br></br>
        /// Explanation: The degree of correlation coefficient<br></br>
        /// Example Usage: To evaluate the correlation between time amount for study and test score<br></br>
        /// Formula: Given that Time(X) Score(Y)<br></br>
        /// Calculate the sum of deviation square and sum of products<br></br>
        /// Single correlation coefficient = Sum of products / √(Sum of deviation of X * Sum of deviation of Y)
        /// </summary>
        void button14_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<List<double>> values = ReadTextFloatList();  // txtファイルList<double>で読み込み
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            List<double> xList = new(), yList = new();
            values.ForEach(p => { xList.Add(p[0]); yList.Add(p[1]); });
            double xAverage = xList.Average(), yAverage = yList.Average();
            List<double> xDeviations = new(xList.Select(p => p - xAverage));
            List<double> yDeviations = new(yList.Select(p => p - yAverage));
            double xSumOfSquaredResiduals = xDeviations.Select(p => p * p).Sum();
            double ySumOfSquaredResiduals = yDeviations.Select(p => p * p).Sum();
            double multiplyAccumulate = 0f;
            for (int i = 0; i < xDeviations.Count; i++) multiplyAccumulate += xDeviations[i] * yDeviations[i];  // xy積和
            double singleCorrelationCoefficient = multiplyAccumulate / (double)Math.Sqrt(xSumOfSquaredResiduals * ySumOfSquaredResiduals);  // 単相関係数
            WriteText("Single correlation coefficient = " + singleCorrelationCoefficient);
        }

        /// <summary>
        /// Single regression equation<br></br>
        /// Explanation: The equation calculated with single regression analysis<br></br>
        /// Example Usage: To see the relationship between the investment and return<br></br>
        /// Formula: Given that you got y = ax + b<br></br>
        /// a = Sum of products of xy / Sum of deviation square of x　　b = Average of y - a * Average of x
        /// </summary>
        void button15_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<List<double>> values = ReadTextFloatList();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            List<double> xList = new(), yList = new();
            values.ForEach(p => { xList.Add(p[0]); yList.Add(p[1]); });
            double xAverage = xList.Average(), yAverage = yList.Average();
            List<double> xDeviations = new(xList.Select(p => p - xAverage));
            List<double> yDeviations = new(yList.Select(p => p - yAverage));
            double xSumOfSquaredResiduals = xDeviations.Select(p => p * p).Sum();
            double multiplyAccumulate = 0f;
            for (int i = 0; i < xDeviations.Count; i++) multiplyAccumulate += xDeviations[i] * yDeviations[i];
            double a = multiplyAccumulate / xSumOfSquaredResiduals;
            double b = yAverage - a * xAverage;
            string singleRegressionEquation = b > 0 ? "y = " + a + "x + " + b : "y = " + a + "x - " + Math.Abs(b);
            WriteText("Single regression equation = " + singleRegressionEquation);
        }

        /// <summary>
        /// Crosstable / Cramer's coefficient of association<br></br>
        /// Explanation: To see causal relationship and correalation between two category data<br></br>
        /// Example Usage: The reason why item satisfied customers, correlation between character and hobby<br></br>
        /// Formula: Make a crosstable <br></br>
        /// Cramer's coefficient of association = √(Chi squared / n(k - 1))
        /// </summary>
        void button16_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<List<double>> values = ReadTextFloatList();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }
            for(int i = 0; i < values.Count; i++) if (values[i].Contains(DATA_MISSING)) { MessageBox.Show("There are missing data", "Error", MessageBoxButtons.OK); return; }

            Form3 f3 = new();
            for (int i = 0; i < values[0].Count; i++) f3.Columns += "Counted items" + i + ",";
            for (int i = 0; i < values.Count; i++) f3.Rows += "Classified items" + i + ",";
            f3.Columns = f3.Columns.TrimEnd(','); f3.Rows = f3.Rows.TrimEnd(',');
            values.ForEach(x => f3.Table += string.Join(' ', x) + "\r\n");
            f3.ShowDialog();
            if (f3.QuitFlag) return; 

            List<string> rows = new(f3.Rows.Split(','));
            int totalSum = (int) values.Select(x => x.Sum()).Sum();
            List<double> columnSums = new();
            for (int i = 0; i < values.Count; i++) columnSums.Add(values.Select(x => x[i]).Sum());
            List<double> rowSums = new(values.Select(x => x.Sum()));
            string totalSumText = totalSum + "(100%)";
            List<string> columnSumsText = new(columnSums.Select(x => x + "(" + (int)(x / totalSum * 100) + "%)"));
            List<string> rowSumsText = new(rowSums.Select(x => x + "(100%)"));
            List<List<string>> valuesText = new(values.Select(x => x.Select(y => y + "(" + (int)(y / x.Sum() * 100) + "%)").ToList()));
            // Calculate Cramer's coefficient of association
            List<List<double>> expectations = new List<List<double>>();
            for (int i = 0; i < values.Count(); i++) expectations.Add(new List<double>(columnSums.Select(x => x * rowSums[i] / totalSum)));
            double chiSquared = 0f;
            for (int i = 0; i < values.Count(); i++) for (int j = 0; j < values[i].Count(); j++) chiSquared += (double) Math.Pow(values[i][j] - expectations[i][j], 2) / expectations[i][j];
            double cramer = (double)Math.Sqrt(chiSquared / (totalSum * (Math.Min(columnSums.Count, rowSums.Count) - 1)));

            List<string> crosstable = new()
            {
                ",Whole," + f3.Columns,  // First line
                "Whole," + totalSumText + "," + string.Join(',', columnSumsText)  // Second line
            };
            for (int i = 0; i < rowSumsText.Count; i++) crosstable.Add(rows[i] + "," + rowSumsText[i] + "," + string.Join(',', valuesText[i]));
            crosstable.Add("Cramer's coefficient of association = " + cramer);

            WriteText(crosstable);
        }

        /// <summary>
        /// Correlation ratio<br></br>
        /// Explanation: The indicator of correlation between category data and number data<br></br>
        /// Example Usage: Correlation between favorite item and ages<br></br>
        /// Formula:　η² = Sb / (Sw + Sb)   Sb=between-subgroup variation Sw:within-subgroup variation<br></br>
        /// Sb = n1(U1 - U) + n2(U2 - U) + ...   nx = amount of data　Ux =within-subgroup average　U = whole average<br></br>
        /// Sw = S1 + S2 + ...　　the sum of each sum of deviation square
        /// </summary>
        void button17_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<List<double>> values = ReadTextFloatList();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }

            double Sw = 0;
            double Sb = 0;
            double totalAverage = values.SelectMany(x => x).Where(x => x != DATA_MISSING).Average();
            for (int i = 0; i < values[0].Count; i++)
            {
                double f = 0; int counter = 0;
                for (int j = 0; j < values.Count; j++) if (values[j][i] != DATA_MISSING) { f += values[j][i]; counter++; }
                double average = f / counter;
                for (int j = 0; j < values.Count; j++) if (values[j][i] != DATA_MISSING) { Sw += (double)Math.Pow(values[j][i] - average, 2); }
                Sb += counter * (double) Math.Pow(average - totalAverage, 2);
            }
            double correlationRatio = Sb / (Sw + Sb);

            WriteText("Correlation ratio = " + correlationRatio);
        }

        /// <summary>
        /// Spearman's rank correlation coefficient<br></br>
        /// Explanation: The indicator of correlation of ordinal scale<br></br>
        /// Example Usage: To evaluate colleration of satisfaction about hotel and satisfaction about bath<br></br>
        /// Formula:　γ = (Tx + Ty - Σd²) / 2√(TxTy)　　x,y=tie sum of each item Tx = (n³-n-x)/12, Ty=(n³-n-y)/12　n=sample size
        /// </summary>
        void button18_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<List<double>> values = ReadTextFloatList();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].Contains(DATA_MISSING)) { MessageBox.Show("There are missing data", "Error", MessageBoxButtons.OK); return; }
                if (values[i].Count > 2) { MessageBox.Show("Some lines have too big values", "Error", MessageBoxButtons.OK); return; }
                if (values[i].Count < 2) { MessageBox.Show("Some lines have too small values", "Error", MessageBoxButtons.OK); return; }
            }

            List<double> x = new(), y = new();
            for(int i = 0; i < values.Count; i++) { x.Add(values[i][0]); y.Add(values[i][1]); }
            double tieSumX = 0, tieSumY = 0;
            List<double> xRank = new(), yRank = new();
            Dictionary<double, double> xValueRank = new(), yValueRank = new();
            int rank = 1;
            foreach (var b in x.OrderBy(p => p).GroupBy(p => p))
            {
                List<int> c = new(); for (int i = 1; i <= b.Count(); i++) c.Add(i);
                tieSumX += Math.Pow(b.Count(), 3) - b.Count();
                xValueRank.Add(b.Key, (double)((rank - 1) * b.Count() + c.Sum()) / b.Count());
                rank += b.Count();
            }
            rank = 1;
            foreach (var b in y.OrderBy(p => p).GroupBy(p => p))
            {
                List<int> c = new(); for (int i = 1; i <= b.Count(); i++) c.Add(i);
                tieSumY += (double)Math.Pow(b.Count(), 3) - b.Count();
                yValueRank.Add(b.Key, (double)((rank - 1) * b.Count() + c.Sum()) / b.Count());
                rank += b.Count();
            }
            x.ForEach(p => xRank.Add(xValueRank.GetValueOrDefault(p)));
            y.ForEach(p => yRank.Add(yValueRank.GetValueOrDefault(p)));
            double dSquaredSum = 0;
            for (int i = 0; i < xRank.Count; i++) dSquaredSum += Math.Pow((xRank[i] - yRank[i]), 2);
            double Tx = (Math.Pow(x.Count(), 3) - x.Count() - tieSumX) / 12;  // Tx = (n³-n-x)/12
            double Ty = (Math.Pow(y.Count(), 3) - y.Count() - tieSumY) / 12;  // Ty = (n³-n-y)/12
            double spearman = (Tx + Ty - dSquaredSum) / (2 * Math.Sqrt(Tx * Ty));
            WriteText("Spearman's rank correlation coefficient = " + spearman);
        }


        /// <summary>
        /// Improvement Index<br></br>
        /// Explanation: Indicator for prioritizing each element on CS graph<br></br>
        /// Example Usage: To know priority of elements to be improved for customer satisfaction<br></br>
        /// Formula: Calculate from distance and degree on CS graph<br></br>
        /// Improvement index = Distance * degree   Degree = (90 - degree with the base line) / 90        
        /// </summary>
        void button19_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<List<double>> values = ReadTextFloatList();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].Contains(DATA_MISSING)) { MessageBox.Show("There are missing data", "Error", MessageBoxButtons.OK); return; }
                if (values[i].Count > 2) { MessageBox.Show("Some lines have too big value", "Error", MessageBoxButtons.OK); return; }
                if (values[i].Count < 2) { MessageBox.Show("Some lines have too small value", "Error", MessageBoxButtons.OK); return; }
            }

            List<double> x = new(), y = new();
            for (int i = 0; i < values.Count; i++) { x.Add(values[i][1]); y.Add(values[i][0]); }
            double xAverage = x.Average();
            double xStandardDeviation = CaluclateStandardDeviation(x)[0];
            List<double> xNormalizedScore = new(x.Select(p => (p - xAverage) / xStandardDeviation));
            List<double> xDeviationValues = new(xNormalizedScore.Select(p => 10 * p + 50));
            double yAverage = y.Average();
            double yStandardDeviation = CaluclateStandardDeviation(y)[0];
            List<double> yNormalizedScore = new(y.Select(p => (p - yAverage) / yStandardDeviation));
            List<double> yDeviationValues = new(yNormalizedScore.Select(p => (10 * p) + 50));
            List<double> distances = new();  // The distance from the origin
            List<double> degrees = new();  // The degree of base line
            for (int i = 0; i < x.Count; i++)
            {
                distances.Add(Math.Sqrt(Math.Pow(xDeviationValues[i] - 50.0, 2) + Math.Pow(yDeviationValues[i] - 50.0, 2)));
                degrees.Add(Math.Atan2(yDeviationValues[i] - 50.0, xDeviationValues[i] - 50.0) * 180 / Math.PI);
            }
            degrees = degrees.Select(p => Math.Abs((p + 45) % 180)).ToList();  // Converting degree to degree of base line
            List<double> correctedDegreeIndices = new(degrees.Select(p => (90 - p) / 90));
            List<double> improvementIndices = new();
            for(int i = 0; i < distances.Count; i++)
            {
                improvementIndices.Add(distances[i] * correctedDegreeIndices[i]);
            }

            WriteText(improvementIndices.Select(p => p.ToString()).ToList());
        }

        /// <summary>
        /// Skewness / Kurtosis<br></br>
        /// Explanation: How distorted the distribution from the standard distribution<br></br>
        /// There is no official basement, but if the range is -0.5～0.5, it can be regarded as standard distribution<br></br>
        /// Example Usage: To measure whether the distribution of times of travel abroad of women in 20 years is standard distribution<br></br>
        /// Formula: Skewness = n / (n-1)(n-2)Σ(i=1からn) ((Datum-average) / SD)³<br></br>
        /// Kurtosis = n(n+1) / (n-1)(n-2)(n-3)Σ(i=1からn)((Datum-average)⁴/SD) - 3(n-1)²/(n-2)(n-3)
        /// </summary>
        void button20_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }
            for (int i = 0; i < values.Count; i++)
            {
                if (values.Contains(DATA_MISSING)) { MessageBox.Show("There are missing data", "Error", MessageBoxButtons.OK); return; }
            }

            double Skewness = 0.0, Kurtosis = 0.0;
            double average = values.Average(), standardDeviation = CaluclateStandardDeviation(values)[1];
            values.ForEach(x => 
            {
                Skewness += Math.Pow((x - average) / standardDeviation, 3);
                Kurtosis += Math.Pow((x - average), 4) / Math.Pow(standardDeviation, 4);
            });
            double n = values.Count;
            Skewness = Skewness * n / ((n - 1) * (n - 2));
            Kurtosis = Kurtosis * n * (n + 1) / ((n - 1) * (n - 2) * (n - 3)) - 3 * (n-1) * (n-1) / ((n-2) * (n-3));

            WriteText(new List<string>()
            {
                "Skewness = " + Skewness,
                "Kurtosis = " + Kurtosis
            });
        }

        /// <summary>
        /// Normal probability plot<br></br>
        /// Explanation: Investigate whether data distribution follows the standard distribtion<br></br>
        /// Example Usage: To see whether the degree distribution follows the standard distribtion<br></br>
        /// Formula: The lower cumulative probability of the standard distribution
        /// </summary>
        void button21_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "") { MessageBox.Show("Drop a text file", "Error", MessageBoxButtons.OK); return; }
            List<double> values = ReadTextAsDouble();
            if (values == null) { MessageBox.Show("There are no data in the file", "Error", MessageBoxButtons.OK); return; }
            for (int i = 0; i < values.Count; i++)
            {
                if (values.Contains(DATA_MISSING)) { MessageBox.Show("There are missing data", "Error", MessageBoxButtons.OK); return; }
            }
            
            double d = 1-P_nor(1.01);

            WriteText(d + "");
        }


        /// <summary>
        /// The lower cumulative probability of the standard distribution
        /// </summary>
        /// <param name="z">The z value</param>
        double P_nor(double z)
        {
            int i;
            double z2, prev, p, t;

            z2 = z * z;
            t = p = z * Math.Exp(-0.5 * z2) / Math.Sqrt(2 * Math.PI);
            for (i = 3; i < 200; i += 2)
            {
                prev = p; t *= z2 / i; p += t;
                if (p == prev) return 0.5 + p;
            }
            return (z);
        }
    }
}
