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

        //���ݿ������ַ���
        string? str;
        public Form1()
        {
            InitializeComponent();
        }
        string? m_filename;

        private void ��ˮ׼�۲��ļ�ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            this.dataGridView2.Rows.Clear();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//��ֵȷ���Ƿ����ѡ�����ļ�
            dialog.Title = "��ѡ���ļ���";
            dialog.Filter = "ˮ׼�۲��ļ�(*.in1)|*.in1";
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
                        //˵������֪������
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
                        //˵����ˮ׼��·����
                        int index = this.dataGridView2.Rows.Add();
                        this.dataGridView2.Rows[index].Cells[0].Value = index;
                        this.dataGridView2.Rows[index].Cells[1].Value = item[0];
                        this.dataGridView2.Rows[index].Cells[2].Value = item[1];
                        this.dataGridView2.Rows[index].Cells[3].Value = item[2];
                        this.dataGridView2.Rows[index].Cells[4].Value = item[3];
                        //��δ֪����ӵ������
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
                //���δ֪��
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

        private void �պϲ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //��ȡ����й۲��ļ�
            int n = this.dataGridView1.RowCount - 1;//�ܵ���
            int N = this.dataGridView2.RowCount - 1;//�۲ⷽ������
            List<LevelingPoint> levelingPoints_known = new List<LevelingPoint>();
            List<LevelingPoint> levelingPoints_unknown = new List<LevelingPoint>();
            if (n <= 1) { MessageBox.Show("��������ȷ�������ٽ��бպϲ����"); return; }
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
            if (T == 0) { MessageBox.Show("��ˮ׼�������ƽ�����"); return; }
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
            //���������Ϣ
            LevelingAdjust.StrLoopClosure.Clear();
            //���պϲ����
            LevelingAdjust.LoopClosure();
            //������·�պϲ����
            LevelingAdjust.LineClosure();
            string[] jieguo = LevelingAdjust.StrLoopClosure.ToArray();
            string _result = string.Join("", jieguo);
            this.richTextBox1.Text = _result;
            MessageBox.Show("���պϲ����ɹ�!");
        }

        private void ���Ƶ�̼߳���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LevelingAdjust.LevelingPoints.Count == 0 || LevelingAdjust.LevelingLines.Count == 0)
            {
                MessageBox.Show("���ȵ�������������ٽ��н��Ƹ̼߳��㣡");
                return;
            }
            LevelingAdjust.Cal_H0();//���ø߳̽���ֵ���㷽����
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

        private void ����ƽ���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string str = "******����ƽ���******\r\n";
            int n = this.dataGridView1.RowCount - 1;//�ܵ���
            List<LevelingPoint> levelingPoints_known = new List<LevelingPoint>();
            List<LevelingPoint> levelingPoints_unknown = new List<LevelingPoint>();
            if (n <= 1) { MessageBox.Show("��������ȷ�������ٽ��бպϲ����"); return; }
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
            str += "��֪�����:" + levelingPoints_known.Count + "\r\n";
            str += "δ֪�����:" + levelingPoints_unknown.Count + "\r\n";
            str += "��֪��:\r\n";
            foreach (var v in levelingPoints_known)
            {
                str += v.PointName + "," + v.Height + "\r\n";
            }
            str += "δ֪��:\r\n";
            foreach (var v in levelingPoints_unknown)
            {
                str += v.PointName + "," + v.Height + "\r\n";
            }
            this.richTextBox2.Text = str;
            MessageBox.Show("���ױ������ɳɹ�");
        }

        private void ���ݿ��¼ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            form.ShowDialog();
            if (form.ISOK())
            {
                str = form.connectStr();
                SqlConnectionManager scm = new SqlConnectionManager(str);
                // ִ�в�ѯ���
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

                // ִ�в�ѯ���
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
                MessageBox.Show("�����µ�¼!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnectionManager scm = new SqlConnectionManager(str);
            // ִ�в�ѯ���
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
                        MessageBox.Show("��ѯ�ɹ�!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("��ѯʧ��!");
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
                        MessageBox.Show("��ѯ�ɹ�!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("��ѯʧ��!");
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
                MessageBox.Show("��ӳɹ�!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("���ʧ��!");
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
                MessageBox.Show("ȷ��ɾ��?");
                scm.ExecuteNonQuery($"Delete from {comboBox1.Text} where ID = \'{str2}\'");
                MessageBox.Show("ɾ���ɹ�!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ɾ��ʧ��!");
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
                MessageBox.Show("ȷ�ϸ���?");
                try
                {
                    scm.ExecuteNonQuery($"update {comboBox1.Text} set Name = " +
                        $"\'{str3}\', Evevation = {str4},Nature = \'{str5}\',FileName = \'{str6}\'  where ID = \'{str2}\'");
                    MessageBox.Show("���ĳɹ�!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("����ʧ��!");
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
                MessageBox.Show("ȷ�ϸ���?");
                try
                {
                    scm.ExecuteNonQuery($"update {comboBox1.Text} set SID = " +
                    $"\'{str3}\', EID = \'{str4}\',Height = {str5},Length = {str6}  where ID = \'{str2}\'");
                    MessageBox.Show("���ĳɹ�!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("����ʧ��!");
                }
            }
        }

        private void ����ƽ�����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //��ȡ����й۲��ļ�
            int n = LevelingAdjust.MPnumber;//�ܵ���
            int N = LevelingAdjust.MLnumber;//�۲ⷽ������
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
            MessageBox.Show("����ɹ�!");
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
            LevelingAdjust.StrAdjust.Add("                                  �߳���ƽ����(����)\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("                              ��֪�̵߳㣺" + LevelingAdjust.MKownPnumber + "\r\n");
            LevelingAdjust.StrAdjust.Add("                              δ֪�̵߳㣺" + t + "\r\n");
            LevelingAdjust.StrAdjust.Add("                              �߲�������" + N + "\r\n");
            LevelingAdjust.StrAdjust.Add("                                  ���ɶ�: " + (N - t) + "\r\n");
            LevelingAdjust.StrAdjust.Add("                        ���λȨ�����: " + sigma.ToString("F2") + "(mm)\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("                                    ʵ��߲�����ͳ��\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("    ���        ���        �յ�        �߲�(m)       ����(km)         Ȩ\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            int ip = 0;
            foreach (var value in LevelingAdjust.LevelingLines)
            {
                LevelingAdjust.StrAdjust.Add($"    {value.LeveingLineNum,-6}      {value.StartPoint.PointName,-6}     {value.EndPoint.PointName,-6}      {value.LeveingHeightDifferent,-6}         {value.LeveingRoadLength,-6}        {P[ip, ip].ToString("F2")}\r\n");
                ip++;
            }
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("                                   �߳�ƽ��ֵ���侫��\r\n");
            LevelingAdjust.StrAdjust.Add("--------------------------------------------------------------------------------\r\n");
            LevelingAdjust.StrAdjust.Add("          ���           ���             �߳�(m)          �����(mm)\r\n");
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

        private void ����ƽ���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            foreach (var value in LevelingAdjust.StrAdjust)
            {
                richTextBox2.Text += value;
            }
        }

        private void �����ݿ⵼��ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // ��������Ի���
            SaveFileDialog saveDataSend = new SaveFileDialog();
            // Environment.SpecialFolder.MyDocuments ��ʾ���ҵ��ĵ���
            saveDataSend.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);   // ��ȡ�ļ�·��
            saveDataSend.Filter = "*.ou1|ˮ׼ƽ����ļ�";   // �����ļ�����Ϊ�ı��ļ�
            saveDataSend.DefaultExt = ".ou1";   // Ĭ���ļ�����չ��
            saveDataSend.FileName = "Data.ou1";   // �ļ�Ĭ����
            if (saveDataSend.ShowDialog() == DialogResult.OK)   // ��ʾ�ļ��򣬲���ѡ���ļ�
            {
                string fName = saveDataSend.FileName;   // ��ȡ�ļ���
                                                        // ����1��д���ļ����ļ���������2��д���ļ�������
                                                        // �ַ���"Hello"���ļ���������ݣ����Ը�����������޸�
                string txt = "";
                foreach (var mystr in LevelingAdjust.StrAdjust)
                {
                    txt += mystr;
                }
                System.IO.File.WriteAllText(fName, txt);   // ���ļ���д������
            }
            MessageBox.Show("�����ɹ�!");
        }

        private void in1�ļ��������ݿ�ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlConnectionManager scm = new SqlConnectionManager(str);
            if (!scm.TestConnection())
            {
                MessageBox.Show("���ݿ�����ʧ��,�����µ�¼!");
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
                MessageBox.Show("����ɹ�!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("����ʧ��!");
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

        private void ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ˮ׼��ƽ����������\n���Ͻ�ͨ��ѧ�����ѧ�빤��ѧԺ\n2022114444\n�ľ��\n917700519@qq.com\n");       
        }
    }
}
