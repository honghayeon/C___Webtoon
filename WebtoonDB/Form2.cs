using MySql.Data.MySqlClient;
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
using Excel = Microsoft.Office.Interop.Excel;

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
        string selectTable, WNameToId, WIdToName, complIndex, endCell;

        public string getWriterid { get; set; }

        private void Form2_Load(object sender, EventArgs e)
        {
            // string connStr = "server=localhost;port=3305;database=webtoon;uid=root;pwd=taranfu35";
            string connStr = "server=localhost;port=3306;database=webtoon;uid=root;pwd=1010";
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

        private void SetSearchComboBox()
        {
            writerCB.Items.Clear();
            string sql = "SELECT * FROM writer";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

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

        private void writerUpdate_Click(object sender, EventArgs e)
        {
            string sql = $"UPDATE writer SET nick = @nick, writername = @writername, email = @email WHERE (writerid = {getWriterid})";
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

        private void select_Click(object sender, EventArgs e)
        {
            selectWebtoon();
        }

        // 미완성(날짜 -> 첫 연재일이니까 추가하는 것도 아닌 검색에는 필요없지 않을까...?)
        private void selectWebtoon()
        {
            writerIdName();
            string queryStr;
            string[] conditions = new string[7];

            if (selectTable == "webtoon")
            {
                conditions[0] = (wtoonid.Text != "") ? "wtoonid=@wtoonid" : null;
                conditions[1] = (wtoonname.Text != "") ? "wtoonname=@wtoonname" : null;
                //conditions[2] = (date.Text != "") ? "1st_date=@1st_date" : null;
                conditions[2] = (writerCB.Text != "") ? $"we_writerid=@we_writerid" : null;
            }

            if (selectTable == "challenge")
            {
                conditions[0] = (wtoonid.Text != "") ? "chalid=@chalid" : null;
                conditions[1] = (wtoonname.Text != "") ? "chalname=@chalname" : null;
                conditions[2] = (writerCB.Text != "") ? "ch_writerid=@ch_writerid" : null;
            }

            if (conditions[0] != null || conditions[1] != null || conditions[2] != null)
            {
                queryStr = $"SELECT distinct webtoon.* FROM webtoon, writer WHERE ";
                if (selectTable == "challenge")
                    queryStr = $"SELECT distinct challenge.* FROM challenge, writer WHERE ";
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
                dataAdapter.SelectCommand.Parameters.AddWithValue("@we_writerid", WNameToId);
                //dataAdapter.SelectCommand.Parameters.AddWithValue("@1st_date", date.Text);
            }

            if (selectTable == "challenge")
            {
                dataAdapter.SelectCommand = new MySqlCommand(queryStr, conn);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@chalid", wtoonid.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@chalname", wtoonname.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@ch_writerid", WNameToId);
            }

            SetSearchComboBox();

            try
            {
                conn.Open();
                dataSet.Clear();
                if (dataAdapter.Fill(dataSet, $"{selectTable}") > 0)
                {
                    if (selectTable == "webtoon")
                        wtoonGV.DataSource = dataSet.Tables["webtoon"];
                    if (selectTable == "challenge")
                        chalGV.DataSource = dataSet.Tables["challenge"];
                }
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

        private void insert_Click(object sender, EventArgs e)
        {
            string sql;
            if (selectTable == "webtoon")
            {
                writerIdName();
                changeIdName(WNameToId);
                sql = $"INSERT INTO webtoon.webtoon (wtoonid, wtoonname, we_writerid, 1st_date, compl)" + $" VALUES (@wtoonid, @wtoonname, @we_writer, @1st_date, @compl)";
                dataAdapter.InsertCommand = new MySqlCommand(sql, conn);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@wtoonid", wtoonid.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@wtoonname", wtoonname.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@we_writer", WNameToId);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@1st_date", date.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@compl", complIndex);
            }

            if (selectTable == "challenge")
            {
                sql = $"INSERT INTO challenge (chalid, chalname, ch_writerid)" + $"VALUES (@chalid, @chalname, @ch_writerid)";
                dataAdapter.InsertCommand = new MySqlCommand(sql, conn);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@chalid", wtoonid.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@chalname", wtoonname.Text);
                dataAdapter.InsertCommand.Parameters.AddWithValue("@ch_writerid", WNameToId);
            }

            SetSearchComboBox();

            try
            {
                conn.Open();
                dataAdapter.InsertCommand.ExecuteNonQuery();

                dataSet.Clear();
                dataAdapter.Fill(dataSet, $"{selectTable}");
                if (selectTable == "webtoon")
                    wtoonGV.DataSource = dataSet.Tables["webtoon"];
                if (selectTable == "challenge")
                    chalGV.DataSource = dataSet.Tables["challenge"];
                complIndex = "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
                ClearGV();
            }
        }

        private void delete_Click(object sender, EventArgs e)
        {
            if (selectTable == "webtoon")
            {
                string sql = $"DELETE FROM webtoon WHERE wtoonid=@wtoonid";
                dataAdapter.DeleteCommand = new MySqlCommand(sql, conn);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@wtoonid", wtoonid.Text);
            }

            if (selectTable == "challenge")
            {
                string sql = $"DELETE FROM challenge WHERE chalid=@chalid";
                dataAdapter.DeleteCommand = new MySqlCommand(sql, conn);
                dataAdapter.DeleteCommand.Parameters.AddWithValue("@chalid", wtoonid.Text);
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
                ClearGV();
            }
        }

        private void update_Click(object sender, EventArgs e)
        {
            writerIdName();
            changeIdName(WNameToId);
            if (selectTable == "webtoon")
            {
                string sql = "UPDATE webtoon SET wtoonname = @wtoonname, we_writerid = @we_writerid, 1st_date = @1st_date, compl = @compl WHERE (wtoonid = @wtoonid)";
                dataAdapter.UpdateCommand = new MySqlCommand(sql, conn);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@wtoonid", wtoonid.Text);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@wtoonname", wtoonname.Text);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@we_writerid", WNameToId);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@1st_date", date.Value);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@compl", complIndex);
            }

            if (selectTable == "challenge")
            {
                string sql = "UPDATE challenge SET chalname = @chalname, ch_writerid = @ch_writerid WHERE (chalid = @chalid)";
                dataAdapter.UpdateCommand = new MySqlCommand(sql, conn);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@chalid", wtoonid.Text);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@chalname", wtoonname.Text);
                dataAdapter.UpdateCommand.Parameters.AddWithValue("@ch_writerid", WNameToId);
            }

            SetSearchComboBox();

            try
            {
                conn.Open();
                dataAdapter.UpdateCommand.ExecuteNonQuery();

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
                ClearGV();
            }
        }

        private void changeHeaderText()
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
        }

        private void wtoonGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int selectedRowIndex = e.RowIndex;
            DataGridViewRow row = wtoonGV.Rows[selectedRowIndex];
            wtoonid.Text = row.Cells[0].Value.ToString();
            wtoonname.Text = row.Cells[1].Value.ToString();
            changeIdName(row.Cells[2].Value.ToString());
            writerCB.Text = WIdToName;
            date.Value = Convert.ToDateTime(row.Cells[3].Value);
            if (row.Cells[4].Value.ToString() == "0")
                compl.Text = "연재 중";
            else compl.Text = "완결";

            SetSearchComboBox();
        }
        
        private void chalGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int selectedRowIndex = e.RowIndex;
            DataGridViewRow row = chalGV.Rows[selectedRowIndex];
            wtoonid.Text = row.Cells[0].Value.ToString();
            wtoonname.Text = row.Cells[1].Value.ToString();
            changeIdName(row.Cells[2].Value.ToString());
            writerCB.Text = WIdToName.ToString();

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
                txtBtn.Visible = false;
                excelBtn.Visible = false;

                dataAdapter = new MySqlDataAdapter("SELECT * FROM writer", conn);
                dataSet = new DataSet();
                dataAdapter.Fill(dataSet, "writer");
                writerGV.DataSource = dataSet.Tables["writer"];
            }

            else if (smallTab.SelectedTab == wtoonTab)
            {
                selectTable = "webtoon";
                complCB.Enabled = true;
                compl.Enabled = true;
                date.Enabled = true;
                txtBtn.Visible = true;
                excelBtn.Visible = true;

                dataAdapter = new MySqlDataAdapter("SELECT * FROM webtoon", conn);
                dataSet = new DataSet();
                dataAdapter.Fill(dataSet, "webtoon");
                wtoonGV.DataSource = dataSet.Tables["webtoon"];
            }

            else if (smallTab.SelectedTab == chalTab)
            {
                selectTable = "challenge";
                complCB.Enabled = false;
                compl.Enabled = false;
                date.Enabled = false;
                txtBtn.Visible = true;
                excelBtn.Visible = true;

                dataAdapter = new MySqlDataAdapter("SELECT * FROM challenge", conn);
                dataSet = new DataSet();
                dataAdapter.Fill(dataSet, "challenge");
                chalGV.DataSource = dataSet.Tables["challenge"];
            }
            complIndex = "0";
            WNameToId = "";
            WIdToName = "";

            Tclear();
            changeHeaderText();
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

        private void compl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (compl.Text == "완결")
                complIndex = "1";
            else complIndex = "0";
        }
        
        private void writerCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            writerIdName();
        }

        private void writerIdName()
        {
            string WName = writerCB.Text.ToString();
            string sql = $"SELECT * FROM writer where nick=\"{WName}\"";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            try
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    WNameToId = reader.GetString("writerid");
                    WIdToName = reader.GetString("nick");
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

        private void changeIdName(string id)
        {
            string sql = $"SELECT distinct * FROM writer, webtoon where writerid={id} AND we_writerid={id}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            try
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    WIdToName = reader.GetString("nick");
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

        /* Excel/txt 파일로 저장하기 */

        private void txtBtn_Click(object sender, EventArgs e)
        {
            DataInGV();

            saveFileDialog1.Filter = "텍스트 파일(*.txt)|*.txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveTextFile(saveFileDialog1.FileName);
            }
        }

        private void excelBtn_Click(object sender, EventArgs e)
        {
            DataInGV();

            saveFileDialog1.Filter = "엑셀 파일(*.xlsx)|*.xlsx";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveExcelFile(saveFileDialog1.FileName);
            }
        }

        // dataGridView에 데이터가 존재하는지 체크
        private void DataInGV()
        {
            if (selectTable == "webtoon")
            {
                if (wtoonGV.RowCount == 0)
                {
                    MessageBox.Show("저장할 데이터가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                endCell = "E";
            }
            else if (selectTable == "challenge")
            {
                if (chalGV.RowCount == 0)
                {
                    MessageBox.Show("저장할 데이터가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                endCell = "C";
            }
        }

        private void SaveExcelFile(string filePath)
        {
            // 참조- COM - excel 검색 - Microsoft~ 추가
            // 1. 엑셀 사용에 필요한 객체 생성
            Excel.Application eapp; // 엑셀 프로그램
            Excel.Workbook eworkbook; // 엑셀 시트를 여러개 포함하는 단위
            Excel.Worksheet eworksheet; // 엑셀 워크시트

            eapp = new Excel.Application();
            eworkbook = eapp.Workbooks.Add();
            eworksheet = eworkbook.Sheets[1]; // 엑셀 워크시트는 index가 1부터 시작한다.

            // 2. 엑셀에 저장할 데이터를 2차원 배열 형태로 준비
            string[,] dataArr;
            int colCount = dataSet.Tables[selectTable].Columns.Count + 1;
            int rowCount = dataSet.Tables[selectTable].Rows.Count + 1;
            dataArr = new string[rowCount, colCount];

            // 2-1 Column 이름 저장
            for (int i = 0; i < colCount - 1; i++)
            {
                dataArr[0, i] = dataSet.Tables[selectTable].Columns[i].ColumnName; // 첫 행에 컬럼 이름 저장
            }

            // 2-2 행 데이터 저장
            for (int i = 0; i < rowCount - 1; i++)
            {
                for (int j = 0; j < colCount - 1; j++)
                {
                    dataArr[i + 1, j] = dataSet.Tables[selectTable].Rows[i].ItemArray[j].ToString();
                }
            }

            // 3. 준비된 데이터를 엑셀파일에 저장
            endCell += rowCount; // 데이터가 저장이 끝나는 셀의 주소
            eworksheet.get_Range("A1:" + endCell).Value = dataArr; // 데이터가 저장될 셀의 범위에 2차원 배열 저장

            eworkbook.SaveAs(filePath, Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                false, false, Excel.XlSaveAsAccessMode.xlShared, false, false, Type.Missing, Type.Missing, Type.Missing);
            eworkbook.Close(false, Type.Missing, Type.Missing);
            eapp.Quit();
        }

        private void SaveTextFile(string filePath)
        {
            // SaveFileDialog에서 지정한 파일 경로에 Stream 생성 -> 저장
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                // Column 이름을 저장
                foreach (DataColumn col in dataSet.Tables[selectTable].Columns)
                {
                    sw.Write($"{col.ColumnName}\t");
                }
                sw.WriteLine();

                // DataSet의 데이터 행 저장
                foreach (DataRow row in dataSet.Tables[selectTable].Rows)
                {
                    string rowString = "";
                    foreach (var item in row.ItemArray)
                    {
                        rowString += $"{item.ToString()}\t";
                    }
                    sw.WriteLine(rowString);
                }
            }
        }
    }
}
