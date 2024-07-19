using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using System.Diagnostics;

namespace Leveling
{
    public partial class Form1 : Form
    {

        //数据库连接字符串
        string? str;
        public Form1()
        {
            InitializeComponent();
        }
        string? m_filename;

        private void 打开水准观测文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            this.dataGridView2.Rows.Clear();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "水准观测文件(*.in1)|*.in1";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = dialog.FileName;
                m_filename = file;
                using StreamReader reader = new StreamReader(file);
                string? line;
                List<LevelingPoint> _points = new List<LevelingPoint>();
                List<LevelingPoint> _Kpoints = new List<LevelingPoint>();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] item = line.Split(',');
                    if (item.Length == 2)
                    {
                        //说明是已知点数据
                        int index = this.dataGridView1.Rows.Add();
                        this.dataGridView1.Rows[index].Cells[0].Value = index;
                        this.dataGridView1.Rows[index].Cells[1].Value = item[0];
                        this.dataGridView1.Rows[index].Cells[2].Value = item[1];
                        this.dataGridView1.Rows[index].Cells[3].Value = LevelingPoint.PointNature.Known;
                        LevelingPoint point = new LevelingPoint();
                        point.Nature = LevelingPoint.PointNature.Known;
                        point.PointName = item[0];
                        _Kpoints.Add(point);
                    }
                    else if (item.Length == 4)
                    {
                        //说明是水准线路部分
                        int index = this.dataGridView2.Rows.Add();
                        this.dataGridView2.Rows[index].Cells[0].Value = index;
                        this.dataGridView2.Rows[index].Cells[1].Value = item[0];
                        this.dataGridView2.Rows[index].Cells[2].Value = item[1];
                        this.dataGridView2.Rows[index].Cells[3].Value = item[2];
                        this.dataGridView2.Rows[index].Cells[4].Value = item[3];
                        //将未知点添加到表格中
                        LevelingPoint point1 = new LevelingPoint();
                        LevelingPoint point2 = new LevelingPoint();
                        point1.PointName = item[0];
                        point2.PointName = item[1];
                        bool isOK = true;
                        if (!_points.Contains(point1))
                        {
                            foreach (var point in _Kpoints)
                            {
                                if (point1.PointName == point.PointName)
                                {
                                    isOK = false; break;
                                }
                            }
                            if (isOK)
                            {
                                point1.Nature = LevelingPoint.PointNature.UnKnown;
                                _points.Add(point1);
                            }
                        }
                        isOK = true;
                        if (!_points.Contains(point2))
                        {
                            foreach (var point in _Kpoints)
                            {
                                if (point2.PointName == point.PointName)
                                {
                                    isOK = false; break;
                                }
                            }
                            if (isOK)
                            {
                                point2.Nature = LevelingPoint.PointNature.UnKnown;
                                _points.Add(point2);
                            }
                        }
                    }
                }
                //
                //添加未知点
                var p = _points.Distinct();
                List<LevelingPoint> levelingPoints = p.ToList();
                foreach (LevelingPoint point in levelingPoints)
                {
                    int index = this.dataGridView1.Rows.Add();
                    this.dataGridView1.Rows[index].Cells[0].Value = index;
                    this.dataGridView1.Rows[index].Cells[1].Value = point.PointName;
                    this.dataGridView1.Rows[index].Cells[2].Value = 0;
                    this.dataGridView1.Rows[index].Cells[3].Value = LevelingPoint.PointNature.UnKnown;
                }
            }
        }

        private void 闭合差计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //读取表格中观测文件
            int n = this.dataGridView1.RowCount - 1;//总点数
            int N = this.dataGridView2.RowCount - 1;//观测方程总数
            List<LevelingPoint> levelingPoints_known = new List<LevelingPoint>();
            List<LevelingPoint> levelingPoints_unknown = new List<LevelingPoint>();
            if (n <= 1) { MessageBox.Show("请输入正确的数据再进行闭合差计算"); return; }
            int count = 0;
            for (int i = 0; i < n; i++)
            {
                string? name = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                double height = double.Parse(this.dataGridView1.Rows[i].Cells[2].Value.ToString());
                string? prop = this.dataGridView1.Rows[i].Cells[3].Value.ToString();
                if (prop == "Known")
                {
                    LevelingPoint levelingPoint = new LevelingPoint();
                    levelingPoint.PointName = name;
                    levelingPoint.Height = height;
                    levelingPoint.Nature = LevelingPoint.PointNature.Known;
                    levelingPoint.UKnownPointNum = -1;
                    levelingPoints_known.Add(levelingPoint);
                }
                else
                {
                    LevelingPoint levelingPoint = new LevelingPoint();
                    levelingPoint.PointName = name;
                    levelingPoint.Height = height;
                    levelingPoint.Nature = LevelingPoint.PointNature.UnKnown;
                    levelingPoint.UKnownPointNum = count++;
                    levelingPoints_unknown.Add(levelingPoint);
                }
            }
            int T = levelingPoints_unknown.Count;
            int R = N - T;
            if (T == 0) { MessageBox.Show("无水准点需进行平差计算"); return; }
            List<LevelingLine> lines = new List<LevelingLine>();
            for (int i = 0; i < N; ++i)
            {
                LevelingPoint startPoint = new LevelingPoint();
                LevelingPoint endPoint = new LevelingPoint();
                startPoint.PointName = this.dataGridView2.Rows[i].Cells[1].Value.ToString();
                endPoint.PointName = this.dataGridView2.Rows[i].Cells[2].Value.ToString();
                double h = double.Parse(this.dataGridView2.Rows[i].Cells[3].Value.ToString());
                double s = double.Parse(this.dataGridView2.Rows[i].Cells[4].Value.ToString());
                LevelingLine line = new LevelingLine();
                line.StartPoint = startPoint;
                line.EndPoint = endPoint;
                line.LeveingHeightDifferent = h;
                line.LeveingRoadLength = s;
                lines.Add(line);
            }
            LevelingAdjust.MPnumber = n;
            LevelingAdjust.MLnumber = N;
            LevelingAdjust.MKownPnumber = levelingPoints_known.Count;
            LevelingAdjust.LevelingPoints = levelingPoints_known;
            foreach (var v in levelingPoints_unknown)
            {
                LevelingAdjust.LevelingPoints.Add(v);
            }
            count = 0;
            foreach (var v in LevelingAdjust.LevelingPoints)
            {
                v.PointNum = count++;
            }
            foreach (var points in lines)
            {
                foreach (var p in LevelingAdjust.LevelingPoints)
                {
                    if (points.StartPoint.PointName == p.PointName)
                    {
                        points.StartPoint = p;
                    }
                }
                foreach (var p in LevelingAdjust.LevelingPoints)
                {
                    if (points.EndPoint.PointName == p.PointName)
                    {
                        points.EndPoint = p;
                    }
                }
            }
            LevelingAdjust.LevelingLines = lines;
            count = 0;
            foreach (var v in LevelingAdjust.LevelingLines)
            {
                v.LeveingLineNum = count++;
            }
            //清除多余信息
            LevelingAdjust.StrLoopClosure.Clear();
            //环闭合差计算
            LevelingAdjust.LoopClosure();
            //附和线路闭合差计算
            LevelingAdjust.LineClosure();
            string[] jieguo = LevelingAdjust.StrLoopClosure.ToArray();
            string _result = string.Join("", jieguo);
            this.richTextBox1.Text = _result;
            MessageBox.Show("环闭合差计算成功!");
        }

        private void 近似点高程计算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LevelingAdjust.LevelingPoints.Count == 0 || LevelingAdjust.LevelingLines.Count == 0)
            {
                MessageBox.Show("请先导入或输入数据再进行近似高程计算！");
                return;
            }
            LevelingAdjust.Cal_H0();//调用高程近似值计算方法。
            this.dataGridView1.Rows.Clear();
            for (int i = 0; i < LevelingAdjust.LevelingPoints.Count; ++i)
            {
                int index = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[i].Cells[0].Value = index;
                this.dataGridView1.Rows[i].Cells[1].Value = LevelingAdjust.LevelingPoints[i].PointName;
                this.dataGridView1.Rows[i].Cells[2].Value = LevelingAdjust.LevelingPoints[i].Height;
                this.dataGridView1.Rows[i].Cells[3].Value = LevelingAdjust.LevelingPoints[i].Nature;
            }
        }

        private void 简易平差报告ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string str = "******简易平差报告******\r\n";
            int n = this.dataGridView1.RowCount - 1;//总点数
            List<LevelingPoint> levelingPoints_known = new List<LevelingPoint>();
            List<LevelingPoint> levelingPoints_unknown = new List<LevelingPoint>();
            if (n <= 1) { MessageBox.Show("请输入正确的数据再进行闭合差计算"); return; }
            int count = 0;
            for (int i = 0; i < n; i++)
            {
                string? name = this.dataGridView1.Rows[i].Cells[1].Value.ToString();
                double height = double.Parse(this.dataGridView1.Rows[i].Cells[2].Value.ToString());
                string? prop = this.dataGridView1.Rows[i].Cells[3].Value.ToString();
                if (prop == "Known")
                {
                    LevelingPoint levelingPoint = new LevelingPoint();
                    levelingPoint.PointName = name;
                    levelingPoint.Height = height;
                    levelingPoint.Nature = LevelingPoint.PointNature.Known;
                    levelingPoint.UKnownPointNum = -1;
                    levelingPoints_known.Add(levelingPoint);
                }
                else
                {
                    LevelingPoint levelingPoint = new LevelingPoint();
                    levelingPoint.PointName = name;
                    levelingPoint.Height = height;
                    levelingPoint.Nature = LevelingPoint.PointNature.UnKnown;
                    levelingPoint.UKnownPointNum = count++;
                    levelingPoints_unknown.Add(levelingPoint);
                }
            }
            str += "已知点个数:" + levelingPoints_known.Count + "\r\n";
            str += "未知点个数:" + levelingPoints_unknown.Count + "\r\n";
            str += "已知点:\r\n";
            foreach (var v in levelingPoints_known)
            {
                str += v.PointName + "," + v.Height + "\r\n";
            }
            str += "未知点:\r\n";
            foreach (var v in levelingPoints_unknown)
            {
                str += v.PointName + "," + v.Height + "\r\n";
            }
            this.richTextBox2.Text = str;
            MessageBox.Show("简易报告生成成功");
        }

        private void 数据库登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            form.ShowDialog();
            if (form.ISOK())
            {
                str = form.connectStr();
                SqlConnectionManager scm = new SqlConnectionManager(str);
                // 执行查询语句
                using (SqlDataReader reader = scm.ExecuteReader("SELECT * FROM dbo.LevelingPoint"))
                {

                    while (reader.Read())
                    {
                        int count = dataGridView3.Rows.Add();
                        dataGridView3.Rows[count].Cells[0].Value = reader["ID"];
                        dataGridView3.Rows[count].Cells[1].Value = reader["Name"];
                        dataGridView3.Rows[count].Cells[2].Value = reader["Evevation"]; ;
                        dataGridView3.Rows[count].Cells[3].Value = reader["Nature"];
                        dataGridView3.Rows[count].Cells[4].Value = reader["FileName"];

                    }
                }

                // 执行查询语句
                using (SqlDataReader reader = scm.ExecuteReader("SELECT * FROM dbo.LevelingLine"))
                {

                    while (reader.Read())
                    {
                        int count = dataGridView5.Rows.Add();
                        dataGridView5.Rows[count].Cells[0].Value = reader["ID"];
                        dataGridView5.Rows[count].Cells[1].Value = reader["SID"];
                        dataGridView5.Rows[count].Cells[2].Value = reader["EID"]; ;
                        dataGridView5.Rows[count].Cells[3].Value = reader["Height"];
                        dataGridView5.Rows[count].Cells[4].Value = reader["Length"];
                        count++;
                    }

                }
            }
            else
            {
                MessageBox.Show("请重新登录!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnectionManager scm = new SqlConnectionManager(str);
            // 执行查询语句
            string? str1 = "select * from" + $" {comboBox1.Text} " + richTextBox3.Text;
            using (SqlDataReader reader = scm.ExecuteReader(str1))
            {
                if (comboBox1.Text == "LevelingPoint")
                {
                    try
                    {

                        dataGridView3.Rows.Clear();
                        while (reader.Read())
                        {
                            int count = dataGridView3.Rows.Add();
                            dataGridView3.Rows[count].Cells[0].Value = reader["ID"];
                            dataGridView3.Rows[count].Cells[1].Value = reader["Name"];
                            dataGridView3.Rows[count].Cells[2].Value = reader["Evevation"]; ;
                            dataGridView3.Rows[count].Cells[3].Value = reader["Nature"];
                            dataGridView3.Rows[count].Cells[4].Value = reader["FileName"];
                            count++;
                        }
                        MessageBox.Show("查询成功!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("查询失败!");
                    }
                }
                else if (comboBox1.Text == "LevelingLine")
                {

                    dataGridView5.Rows.Clear();
                    try
                    {
                        while (reader.Read())
                        {
                            int count = dataGridView5.Rows.Add();
                            dataGridView5.Rows[count].Cells[0].Value = reader["ID"];
                            dataGridView5.Rows[count].Cells[1].Value = reader["SID"];
                            dataGridView5.Rows[count].Cells[2].Value = reader["EID"]; ;
                            dataGridView5.Rows[count].Cells[3].Value = reader["Height"];
                            dataGridView5.Rows[count].Cells[4].Value = reader["Length"];
                            count++;
                        }
                        MessageBox.Show("查询成功!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("查询失败!");
                    }
                }
            }
        }
        private int GetSelectedRowIndex(DataGridView dgv)
        {
            if (dgv.Rows.Count == 0)
            {
                return 0;
            }
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Selected)
                {
                    return row.Index;
                }
            }
            return 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int index = 0;
            SqlConnectionManager scm = new SqlConnectionManager(str);
            string str2 = "";
            if (comboBox1.Text == "LevelingPoint")
            {
                index = GetSelectedRowIndex(dataGridView3);
                str2 += "\'" + dataGridView3.Rows[index].Cells[0].Value + "\',";
                str2 += "\'" + dataGridView3.Rows[index].Cells[1].Value + "\',";
                str2 += dataGridView3.Rows[index].Cells[2].Value + ",";
                str2 += "\'" + dataGridView3.Rows[index].Cells[3].Value + "\',";
                str2 += "\'" + dataGridView3.Rows[index].Cells[4].Value.ToString() + "\'";
            }
            else if (comboBox1.Text == "LevelingLine")
            {
                index = GetSelectedRowIndex(dataGridView5);
                str2 += "\'" + dataGridView5.Rows[index].Cells[0].Value + "\',";
                str2 += "\'" + dataGridView5.Rows[index].Cells[1].Value + "\',";
                str2 += "\'" + dataGridView5.Rows[index].Cells[2].Value + "\',";
                str2 += dataGridView5.Rows[index].Cells[3].Value + ",";
                str2 += dataGridView5.Rows[index].Cells[4].Value.ToString();
            }
            try
            {

                scm.ExecuteNonQuery($"Insert into {comboBox1.Text} values ({str2})");
                MessageBox.Show("添加成功!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败!");
            }
        }



        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "LevelingPoint")
            {
                dataGridView3.Rows.Add();
            }
            if (comboBox1.Text == "LevelingLine")
            {
                dataGridView5.Rows.Add();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int index = 0;
            SqlConnectionManager scm = new SqlConnectionManager(str);
            string str2 = "";
            if (comboBox1.Text == "LevelingPoint")
            {
                index = GetSelectedRowIndex(dataGridView3);
                str2 = dataGridView3.Rows[index].Cells[0].Value.ToString();
            }
            else if (comboBox1.Text == "LevelingLine")
            {
                index = GetSelectedRowIndex(dataGridView5);
                str2 = dataGridView5.Rows[index].Cells[0].Value.ToString();
            }
            try
            {
                MessageBox.Show("确认删除?");
                scm.ExecuteNonQuery($"Delete from {comboBox1.Text} where ID = \'{str2}\'");
                MessageBox.Show("删除成功!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int index = 0;
            SqlConnectionManager scm = new SqlConnectionManager(str);
            string str2 = "";
            if (comboBox1.Text == "LevelingPoint")
            {
                index = GetSelectedRowIndex(dataGridView3);
                str2 = dataGridView3.Rows[index].Cells[0].Value.ToString();
                string str3 = dataGridView3.Rows[index].Cells[1].Value.ToString();
                string str4 = dataGridView3.Rows[index].Cells[2].Value.ToString();
                string str5 = dataGridView3.Rows[index].Cells[3].Value.ToString();
                string str6 = dataGridView3.Rows[index].Cells[4].Value.ToString();
                MessageBox.Show("确认更改?");
                try
                {
                    scm.ExecuteNonQuery($"update {comboBox1.Text} set Name = " +
                        $"\'{str3}\', Evevation = {str4},Nature = \'{str5}\',FileName = \'{str6}\'  where ID = \'{str2}\'");
                    MessageBox.Show("更改成功!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("更改失败!");
                }
            }
            else if (comboBox1.Text == "LevelingLine")
            {
                index = GetSelectedRowIndex(dataGridView5);
                str2 = dataGridView5.Rows[index].Cells[0].Value.ToString();
                string str3 = dataGridView5.Rows[index].Cells[1].Value.ToString();
                string str4 = dataGridView5.Rows[index].Cells[2].Value.ToString();
                string str5 = dataGridView5.Rows[index].Cells[3].Value.ToString();
                string str6 = dataGridView5.Rows[index].Cells[4].Value.ToString();
                MessageBox.Show("确认更改?");
                try
                {
                    scm.ExecuteNonQuery($"update {comboBox1.Text} set SID = " +
                    $"\'{str3}\', EID = \'{str4}\',Height = {str5},Length = {str6}  where ID = \'{str2}\'");
                    MessageBox.Show("更改成功!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("更改失败!");
                }
            }
        }

        private void 严密平差解算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //读取表格中观测文件
            int n = LevelingAdjust.MPnumber;//总点数
            int N = LevelingAdjust.MLnumber;//观测方程总数
            int t = LevelingAdjust.MPnumber - LevelingAdjust.MKownPnumber;
            //V = BX - L
            var B = DenseMatrix.Create(N, t, 0.0);
            var L = DenseMatrix.Create(N, 1, 0.0);
            var P = DenseMatrix.Create(N, N, 0.0);
            for (int i = 0; i < N; ++i)
            {
                LevelingLine levelingLine = LevelingAdjust.LevelingLines[i];
                P[i, i] = 10 / levelingLine.LeveingRoadLength;
                L[i, 0] = levelingLine.LeveingHeightDifferent + levelingLine.StartPoint.Height - levelingLine.EndPoint.Height;
                if (levelingLine.StartPoint.UKnownPointNum != -1)
                {
                    B[i, levelingLine.StartPoint.UKnownPointNum] = -1;
                }
                if (levelingLine.EndPoint.UKnownPointNum != -1)
                {
                    B[i, levelingLine.EndPoint.UKnownPointNum] = 1;
                }
            }
            var N_ = B.Transpose() * P * B;
            var x = N_.Inverse() * B.Transpose() * P * L;
            for (int i = 0; i < t; ++i)
            {
                LevelingAdjust.LevelingPoints.FirstOrDefault(p => p.UKnownPointNum == i).Height += x[i, 0];
            }
            MessageBox.Show("解算成功!");
            this.dataGridView1.Rows.Clear();
            for (int i = 0; i < LevelingAdjust.LevelingPoints.Count; ++i)
            {
                int index = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[i].Cells[0].Value = index;
                this.dataGridView1.Rows[i].Cells[1].Value = LevelingAdjust.LevelingPoints[i].PointName;
                this.dataGridView1.Rows[i].Cells[2].Value = LevelingAdjust.LevelingPoints[i].Height;
                this.dataGridView1.Rows[i].Cells[3].Value = LevelingAdjust.LevelingPoints[i].Nature;
            }
            var V = B * x - L;
            var temp = V.Transpose() * P * V;
            var sigma = Math.Sqrt(temp[0, 0] / (N - t)) * 1000;
            var result = N_.Inverse() * sigma * sigma;
            LevelingAdjust.StrAdjust.Clear();
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("                                  高程网平差结果(常规)\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("                              已知高程点：" + LevelingAdjust.MKownPnumber + "\r\n");
            LevelingAdjust.StrAdjust.Add("                              未知高程点：" + t + "\r\n");
            LevelingAdjust.StrAdjust.Add("                              高差测段数：" + N + "\r\n");
            LevelingAdjust.StrAdjust.Add("                                  自由度: " + (N - t) + "\r\n");
            LevelingAdjust.StrAdjust.Add("                        验后单位权中误差: " + sigma.ToString("F2") + "(mm)\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("                                    实测高差数据统计\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("    序号        起点        终点        高差(m)       距离(km)         权\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            int ip = 0;
            foreach (var value in LevelingAdjust.LevelingLines)
            {
                LevelingAdjust.StrAdjust.Add($"    {value.LeveingLineNum,-6}      {value.StartPoint.PointName,-6}     {value.EndPoint.PointName,-6}      {value.LeveingHeightDifferent,-6}         {value.LeveingRoadLength,-6}        {P[ip, ip].ToString("F2")}\r\n");
                ip++;
            }
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("                                   高程平差值及其精度\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("          序号           点号             高程(m)          中误差(mm)\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            ip = 0;
            foreach (var value in LevelingAdjust.LevelingPoints)
            {
                if (value.UKnownPointNum != -1)
                {
                    LevelingAdjust.StrAdjust.Add($"          {value.PointNum,-6}        {value.PointName,-6}             {value.Height.ToString("F3"),-6}          {result[ip, ip].ToString("F3")}\r\n");
                    ip++;
                }
                else
                {
                    LevelingAdjust.StrAdjust.Add($"          {value.PointNum,-6}        {value.PointName,-6}             {value.Height.ToString("F3"),-6}          \r\n");
                }
            }
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
        }

        private void 严密平差报告ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            foreach (var value in LevelingAdjust.StrAdjust)
            {
                richTextBox2.Text += value;
            }
        }

        private void 从数据库导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // 创建保存对话框
            SaveFileDialog saveDataSend = new SaveFileDialog();
            // Environment.SpecialFolder.MyDocuments 表示在我的文档中
            saveDataSend.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);   // 获取文件路径
            saveDataSend.Filter = "*.ou1|水准平差报告文件";   // 设置文件类型为文本文件
            saveDataSend.DefaultExt = ".ou1";   // 默认文件的拓展名
            saveDataSend.FileName = "Data.ou1";   // 文件默认名
            if (saveDataSend.ShowDialog() == DialogResult.OK)   // 显示文件框，并且选择文件
            {
                string fName = saveDataSend.FileName;   // 获取文件名
                                                        // 参数1：写入文件的文件名；参数2：写入文件的内容
                                                        // 字符串"Hello"是文件保存的内容，可以根据需求进行修改
                string txt = "";
                foreach (var mystr in LevelingAdjust.StrAdjust)
                {
                    txt += mystr;
                }
                System.IO.File.WriteAllText(fName, txt);   // 向文件中写入内容
            }
            MessageBox.Show("导出成功!");
        }

        private void in1文件导入数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlConnectionManager scm = new SqlConnectionManager(str);
            if (!scm.TestConnection())
            {
                MessageBox.Show("数据库连接失败,请重新登录!");
            }
            string str1, str2;
            str1 = "Insert into LevelingPoint values\r\n";
            str2 = "Insert into LevelingLine  values\r\n";
            Dictionary<int, string> pointID = new Dictionary<int, string>();
            foreach (var value in LevelingAdjust.LevelingPoints)
            {
                int k;
                if (value.Nature == LevelingPoint.PointNature.Known)
                {
                    k = 1;
                }
                else
                {
                    k = 0;
                }
                pointID.Add(value.PointNum, System.DateTime.Now.ToString("G") + $"No.{value.PointNum}");
                str1 += $"(\'{pointID[value.PointNum]}\',\'{value.PointName}\'," +
                    $"{value.Height},{k},\'{m_filename}\'),";
            }
            foreach (var value in LevelingAdjust.LevelingLines)
            {
                str2 += $"(\'{System.DateTime.Now.ToString("G") + $"No.{value.LeveingLineNum}"}\'," +
                    $"\'{pointID[value.StartPoint.PointNum]}\'," +
                    $"\'{pointID[value.EndPoint.PointNum]}\'," +
                    $"{value.LeveingHeightDifferent},{value.LeveingRoadLength}),";
            }
            try
            {
                str1 = RemoveTrailingComma(str1);
                str2 = RemoveTrailingComma(str2);
                scm.ExecuteNonQuery(str1);
                scm.ExecuteNonQuery(str2);
                MessageBox.Show("导入成功!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入失败!");
            }
        }

        static string RemoveTrailingComma(string str)
        {
            if (str.EndsWith(","))
            {
                return str.Substring(0, str.Length - 1);
            }
            return str;
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("水准网平差测绘程序设计\n西南交通大学地球科学与工程学院\n2022114444\n夏靖皓\n917700519@qq.com\n");       
        }
    }
}
