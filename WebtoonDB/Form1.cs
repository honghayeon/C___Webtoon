using MySql.Data.MySqlClient;
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

        MySqlConnection conn;
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 100;
            timer1.Start();

            // string connStr = "server=localhost;port=3305;database=webtoon;uid=root;pwd=taranfu35";
            string connStr = "server=localhost;port=3306;database=webtoon;uid=root;pwd=1010";
            conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
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
            openForm2Dialog();
        }

        private void openForm2Dialog()
        {
            if (string.IsNullOrEmpty(loginID.Text))
            {
                MessageBox.Show(this, "Please enter your id.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                loginID.Focus();
                return;
            }
            try
            {
                conn.Open();

                string str = "SELECT * FROM login WHERE id=" + $"\"{loginID.Text}\"";
                MySqlCommand cmd = new MySqlCommand(str, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    if (reader["pwd"].ToString() == loginPW.Text)
                    {
                        Form2 form2 = new Form2();
                        form2.getWriterid = reader["l_writerid"].ToString();
                        form2.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show(this, "Your id and password don't match.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void loginPW_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                openForm2Dialog();
        }

        private void loginID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                openForm2Dialog();
        }
    }
}
