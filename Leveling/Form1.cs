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
                        _points.Add(point);
                    }
                    else if (item.Length == 4)
                    {
                        //˵����ˮ׼��·����
                        int index = this.dataGridView2.Rows.Add();
                        this.dataGridView2.Rows[index].Cells[0].Value = index;
                        this.dataGridView2.Rows[index].Cells[1].Value= item[0];
                        this.dataGridView2.Rows[index].Cells[2].Value= item[1];
                        this.dataGridView2.Rows[index].Cells[3].Value= item[2];
                        this.dataGridView2.Rows[index].Cells[4].Value= item[3];
                        //��δ֪����ӵ������
                        LevelingPoint point1= new LevelingPoint();
                        LevelingPoint point2= new LevelingPoint();
                        point1.PointName = item[0];
                        point2.PointName = item[1];
                        point1.Nature = LevelingPoint.PointNature.Known;
                        point2.Nature = LevelingPoint.PointNature.Known;
                        if (!_points.Contains(point1)) {
                            point1.Nature = LevelingPoint.PointNature.UnKnown;
                            _points.Add(point1);
                        }
                        if (!_points.Contains(point2))
                        {
                            point1.Nature = LevelingPoint.PointNature.UnKnown;
                            _points.Add(point2);
                        }
                    }
                }
                //
                //���δ֪��
                var p = _points.Distinct();
                List<LevelingPoint> levelingPoints = p.ToList();
                foreach (LevelingPoint point in levelingPoints) {
                    int index = this.dataGridView1.Rows.Add();
                    this.dataGridView1.Rows[index].Cells[0].Value = index;
                    this.dataGridView1.Rows[index].Cells[1].Value = point.PointName;
                    this.dataGridView1.Rows[index].Cells[2].Value = 0;
                    this.dataGridView1.Rows[index].Cells[3].Value = LevelingPoint.PointNature.UnKnown;
                }
            }
        }
    }
}
