using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace StatisticalApp
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        public string Table { get { return textBox1.Text; } set { textBox1.Text = value; } }
        public string Columns { get { return textBox2.Text; } set { textBox2.Text = value; } }
        public string Rows { get { return textBox3.Text; } set { textBox3.Text = value; } }
        public string ComboRisk { get { if (comboBox1.Enabled && comboBox1.SelectedIndex >= 0) return comboBox1.Text; else return null; } 
            set { comboBox1.Text = value; } }
        public string ComboOdds { get { if (comboBox2.Enabled && comboBox2.SelectedIndex >= 0) return comboBox2.Text; else return null; }
            set { comboBox2.Text = value; } }

        public bool QuitFlag = false;  // 戻るボタンフラグ


        // 分析ボタン
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        // 戻るボタン
        private void button2_Click(object sender, EventArgs e)
        {
            QuitFlag = true;
            Close();
        }

        // 集計項目名(列)のテキストボックス
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // リスク比とオッズ比のコンボボックスの内容の更新
            int i1 = comboBox1.SelectedIndex, i2 = comboBox2.SelectedIndex;
            comboBox1.Items.Clear(); comboBox2.Items.Clear();
            string[] columns = Columns.Split(',');
            comboBox1.Items.AddRange(columns); comboBox2.Items.AddRange(columns);
            if (0 <= i1 && i1 < columns.Length) comboBox1.SelectedItem = columns[i1];
            if (0 <= i2 && i2 < columns.Length) comboBox2.SelectedItem = columns[i2];
        }

        // リスク比チェックボックス
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = checkBox1.Checked;
        }

        // オッズ比チェックボックス
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = checkBox2.Checked;
        }


    }
}
