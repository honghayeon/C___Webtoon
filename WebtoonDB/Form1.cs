using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebtoonDB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 200;
            timer1.Start();

        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (progressBar.Value == progressBar.Maximum)
            {
                timer1.Stop();

                panel1.Visible = false;
                panel3.Visible = true;
            }
            else
            {
                progressBar.PerformStep();
            }
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();               // 폼 띄우기(Modal)
        }
    }
}
