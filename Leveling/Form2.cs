using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Leveling
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.textBox1.Text = "Localhost";
            this.textBox2.Text = "sa";
            this.textBox3.Text = "123456";
            this.pictureBox1.Image = Image.FromFile("..\\..\\..\\..\\资源文件\\tu.png");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectStr = 
            $"Data Source={textBox1.Text};Initial Catalog=level;User ID={textBox2.Text};Password={textBox3.Text}";
            SqlConnectionManager connector = new SqlConnectionManager(connectStr);

            if (connector.TestConnection())
            {
                MessageBox.Show("登录成功!");
                isOK = true;
                this.Close();
            }
            else {
                MessageBox.Show("登录失败!");
                isOK = false;
            }

        }

        private bool isOK = true;

        public bool ISOK() {
            return isOK;
        }

        public string connectStr() {
            return 
            $"Data Source={textBox1.Text};Initial Catalog=level;User ID={textBox2.Text};Password={textBox3.Text}";
        }

    }
}
