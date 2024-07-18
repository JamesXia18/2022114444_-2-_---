using System.Diagnostics;
using System.IO;

namespace Leveling
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

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
            string? str;
            if (form.ISOK())
            {
                str = form.connectStr();
            }
            else {
                MessageBox.Show("�����µ�¼!");
            }
        }
    }
}
