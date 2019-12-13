using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebtoonDB
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        MySqlConnection conn;
        MySqlDataAdapter dataAdapter;
        DataSet dataSet;
        string selectTable;

        public string getWriterid { get; set; }

        private void Form2_Load(object sender, EventArgs e)
        {
            string connStr = "server=localhost;port=3305;database=webtoon;uid=root;pwd=taranfu35";
            conn = new MySqlConnection(connStr);

            ClearGV();
            SetSearchComboBox();

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

        // 완성
        private void SetSearchComboBox()
        {
            writerCB.Items.Clear();
            string sql;
            MySqlCommand cmd;
            sql = "SELECT * FROM writer";

            if (selectTable == "webtoon")
                sql = "SELECT wr.nick FROM webtoon.webtoon wt, webtoon.writer wr where wr.writerid=wt.we_writerid";

            if (selectTable == "challenge")
                sql = "SELECT wr.nick FROM webtoon.challenge ch, webtoon.writer wr where wr.writerid=ch.ch_writerid";

            cmd = new MySqlCommand(sql, conn);

            try
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    writerCB.Items.Add(reader.GetString("nick"));
                }
                reader.Close();
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

        // 미완성
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string sql = $"UPDATE writer SET writerid=@{getWriterid}, nick=@nick, writername=@writername, email=@eamil WHERE writerid=@{getWriterid}";
            dataAdapter.UpdateCommand = new MySqlCommand(sql, conn);
            dataAdapter.UpdateCommand.Parameters.AddWithValue("@nick", nick.Text);
            dataAdapter.UpdateCommand.Parameters.AddWithValue("@writername", writername.Text);
            dataAdapter.UpdateCommand.Parameters.AddWithValue("@email", email.Text);

            SetSearchComboBox();

            try
            {
                conn.Open();
                dataAdapter.UpdateCommand.ExecuteNonQuery();

                dataSet.Clear();
                dataAdapter.Fill(dataSet, "writer");
                writerGV.DataSource = dataSet.Tables["writer"];
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

        // 완성
        private void select_Click(object sender, EventArgs e)
        {
            selectWebtoon();
        }

        // 미완성(날짜)
        private void selectWebtoon()
        {
            string queryStr;

            string[] conditions = new string[7];

            if (selectTable == "webtoon")
            {
                conditions[0] = (wtoonid.Text != "") ? "* FROM webtoon WHERE wtoonid=@wtoonid" : null;
                conditions[1] = (wtoonname.Text != "") ? "* FROM webtoon WHERE wtoonname=@wtoonname" : null;
                //conditions[2] = (date.Text != "") ? "1st_date=@1st_date" : null;
                conditions[2] = (writerCB.Text != "") ? $"wt.* FROM webtoon.webtoon wt, webtoon.writer wr where wr.writerid=wt.we_writerid and wr.nick=@wtnick" : null;
            }

            if (selectTable == "challenge")
            {
                conditions[0] = (wtoonid.Text != "") ? "* FROM challenge WHERE chalid=@chalid" : null;
                conditions[1] = (wtoonname.Text != "") ? "* FROM challenge WHERE chalname=@chalname" : null;
                conditions[2] = (writerCB.Text != "") ? "ch.* FROM webtoon.challenge ch, webtoon.writer wr where wr.writerid=ch.ch_writerid and wr.nick=@chnick" : null;
            }

            if (conditions[0] != null || conditions[1] != null || conditions[2] != null)
            {
                queryStr = $"SELECT ";
                bool firstCondition = true;
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (conditions[i] != null)
                        if (firstCondition)
                        {
                            queryStr += conditions[i];
                            firstCondition = false;
                        }
                        else
                        {
                            queryStr += " and " + conditions[i];
                        }
                }
            }
            else
            {
                queryStr = $"SELECT * FROM {selectTable}";
            }

            if (selectTable == "webtoon")
            {
                dataAdapter.SelectCommand = new MySqlCommand(queryStr, conn);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@wtoonid", wtoonid.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@wtoonname", wtoonname.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@wtnick", writerCB.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@1st_date", date.Text);
            }

            if (selectTable == "challenge")
            {
                dataAdapter.SelectCommand = new MySqlCommand(queryStr, conn);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@chalid", wtoonid.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@chalname", wtoonname.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@chnick", writerCB.Text);
            }

            SetSearchComboBox();

            try
            {
                conn.Open();
                dataSet.Clear();
                if (dataAdapter.Fill(dataSet, $"{selectTable}") > 0)
                    wtoonGV.DataSource = dataSet.Tables[$"{selectTable}"];
                else
                    MessageBox.Show("찾는 데이터가 없습니다.");
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

        // 완성--
        private void wtoonGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SetSearchComboBox();
        }

        private void chalGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SetSearchComboBox();
        }

        private void writerGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SetSearchComboBox();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Tclear();
            ClearGV();
        }

        private void clear_Click(object sender, EventArgs e)
        {
            Tclear();
            ClearGV();
        }
        // --

        // 미완성
        private void insert_Click(object sender, EventArgs e)
        {
            string sql;
            if (selectTable == "webtoon")
            {
                // wtoonid wtoonname we_writerid 1st_date compl
                sql = $"INSERT INTO webtoon (wtoonid, wtoonname, 1st_date) " + $"VALUES(@wtoonid, @wtoonname, @1st_date)";
                dataAdapter.InsertCommand = new MySqlCommand(sql, conn);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@wtoonid", wtoonid.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@wtoonname", wtoonname.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@1st_date", date.Text);
            }

            if (selectTable == "challenge")
            {
                // chalid chalname ch_writerid
                sql = $"INSERT INTO challenge (wtoonid, wtoonname, 1st_date) " + $"VALUES(@wtoonid, @wtoonname, @1st_date)";
                dataAdapter.InsertCommand = new MySqlCommand(sql, conn);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@chalid", wtoonid.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@chalname", wtoonname.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@ch_writerid", date.Text);
            }

            SetSearchComboBox();
        }

        // 미완성
        private void delete_Click(object sender, EventArgs e)
        {
            if (selectTable == "webtoon")
            {
                // wtoonid wtoonname we_writerid 1st_date compl
                string sql = $"DELETE FROM webtoon WHERE wtoonid=@wtoonid";
                dataAdapter.DeleteCommand = new MySqlCommand(sql, conn);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@wtoonid", wtoonid.Text);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@wtoonname", wtoonname.Text);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@1st_date", date.Text);
            }

            if (selectTable == "challenge")
            {
                // chalid chalname ch_writerid
                string sql = $"DELETE FROM challenge WHERE chalid=@chalid";
                dataAdapter.DeleteCommand = new MySqlCommand(sql, conn);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@chalid", wtoonid.Text);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@chalname", wtoonname.Text);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@ch_writerid", date.Text);
            }
            SetSearchComboBox();

            try
            {
                conn.Open();
                dataAdapter.DeleteCommand.ExecuteNonQuery();

                dataSet.Clear();
                dataAdapter.Fill(dataSet, $"{selectTable}");
                if (selectTable == "webtoon")
                    wtoonGV.DataSource = dataSet.Tables["webtoon"];
                if (selectTable == "challenge")
                    chalGV.DataSource = dataSet.Tables["challenge"];
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

        // 미완성
        private void update_Click(object sender, EventArgs e)
        {
            // writerid nick writername email
            string sql = $"UPDATE webtoon SET wtoonid=@writerid WHERE writerid=@writerid";
            dataAdapter.UpdateCommand = new MySqlCommand(sql, conn);
            dataAdapter.UpdateCommand.Parameters.AddWithValue("@writerid", writerid.Text);

            SetSearchComboBox();

            try
            {
                conn.Open();
                dataAdapter.UpdateCommand.ExecuteNonQuery();

                dataSet.Clear();
                dataAdapter.Fill(dataSet, "writer");
                writerGV.DataSource = dataSet.Tables["writer"];
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


        // 이름을 바꾸면 gv의 width 길이 부족으로 헤더 이름이 두줄로 출력되서 미관상 보기 좋지 않음 -> 개선중
        /*private void changeHeaderText()
        {
            string[] writerColHeader = { "작가ID", "닉네임", "이름", "Email" };
            string[] wtoonColHeader = { "웹툰ID", "웹툰 제목", "웹툰작가", "첫연재일", "완결여부" };
            string[] chalColHeader = { "웹툰ID", "웹툰제목", "웹툰작가" };

            if (selectTable == "writer")
            {
                foreach (DataGridViewColumn column in writerGV.Columns)
                {
                    column.HeaderText = string.Concat($"{writerColHeader[column.Index]}");
                }
            }

            if (selectTable == "webtoon")
            {
                foreach (DataGridViewColumn column in wtoonGV.Columns)
                {
                    column.HeaderText = string.Concat($"{wtoonColHeader[column.Index]}");
                }
            }

            if (selectTable == "challenge")
            {
                foreach (DataGridViewColumn column in chalGV.Columns)
                {
                    column.HeaderText = string.Concat($"{chalColHeader[column.Index]}");
                }
            }
        }*/

        // 완성--
        private void wtoonGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int selectedRowIndex = e.RowIndex;
            DataGridViewRow row = wtoonGV.Rows[selectedRowIndex];
            wtoonid.Text = row.Cells[0].Value.ToString();
            wtoonname.Text = row.Cells[1].Value.ToString();
            writerCB.Text = row.Cells[2].Value.ToString();

            if (row.Cells[4].Value.ToString() == "0")
                compl.Text = "false";
            else compl.Text = "true";

            SetSearchComboBox();
        }

        private void chalGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int selectedRowIndex = e.RowIndex;
            DataGridViewRow row = chalGV.Rows[selectedRowIndex];
            wtoonid.Text = row.Cells[0].Value.ToString();
            wtoonname.Text = row.Cells[1].Value.ToString();

            SetSearchComboBox();
        }

        private void writerGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int selectedRowIndex = e.RowIndex;
            DataGridViewRow row = writerGV.Rows[selectedRowIndex];
            if (writerid.Text == row.Cells[0].Value.ToString())
            {
                nick.Text = row.Cells[1].Value.ToString();
                writername.Text = row.Cells[2].Value.ToString();
                email.Text = row.Cells[3].Value.ToString();
            }
            else
            {
                nick.Clear();
                writername.Clear();
                email.Clear();
            }

            SetSearchComboBox();
        }

        private void Tclear()
        {
            nick.Clear();
            writername.Clear();
            email.Clear();
            wtoonid.Clear();
            wtoonname.Clear();
            date.Text = $"{DateTime.Now}";
            compl.Text = "";
            writerCB.Text = "";
            complCB.Checked = false;
        }

        private void complCB_CheckedChanged(object sender, EventArgs e)
        {
            if (complCB.Checked == true)
                dataAdapter = new MySqlDataAdapter("SELECT * FROM webtoon WHERE compl=true", conn);
            else
                dataAdapter = new MySqlDataAdapter("SELECT * FROM webtoon", conn);

            dataSet = new DataSet();
            dataAdapter.Fill(dataSet, "webtoon");
            wtoonGV.DataSource = dataSet.Tables["webtoon"];
        }

        private void wtoonid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                selectWebtoon();
        }

        private void wtoonname_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                selectWebtoon();
        }

        private void ClearGV()
        {
            if (bigTab.SelectedTab == writerTab)
            {
                selectTable = "writer";
                writerid.Text = getWriterid;
                writerid.ReadOnly = true;
            }

            else if (smallTab.SelectedTab == wtoonTab)
            {
                complCB.Enabled = true;
                compl.Enabled = true;
                selectTable = "webtoon";
            }

            else if (smallTab.SelectedTab == chalTab)
            {
                complCB.Enabled = false;
                compl.Enabled = false;
                selectTable = "challenge";
            }
            Tclear();

            dataAdapter = new MySqlDataAdapter("SELECT * FROM writer", conn);
            dataSet = new DataSet();
            dataAdapter.Fill(dataSet, "writer");
            writerGV.DataSource = dataSet.Tables["writer"];

            dataAdapter = new MySqlDataAdapter("SELECT * FROM webtoon", conn);
            dataSet = new DataSet();
            dataAdapter.Fill(dataSet, "webtoon");
            wtoonGV.DataSource = dataSet.Tables["webtoon"];

            dataAdapter = new MySqlDataAdapter("SELECT * FROM challenge", conn);
            dataSet = new DataSet();
            dataAdapter.Fill(dataSet, "challenge");
            chalGV.DataSource = dataSet.Tables["challenge"];

            //changeHeaderText();
            SetSearchComboBox();
        }

        private void bigTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearGV();
        }

        private void smallTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearGV();
        }
        // --
    }
}
