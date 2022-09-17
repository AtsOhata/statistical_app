using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace StatisticalApp
{
    /// <summary>値を一つ取得するフォーム</summary>
    public partial class Form2 : Form
    {
        public bool QuitFlag = false;
        public string LabelText { get { return label1.Text; } set { label1.Text = value; } }
        public string Input { get { return textBox1.Text; } set { textBox1.Text = value; } }

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            QuitFlag = false;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            QuitFlag = true;
            Close();
        }
    }
}
