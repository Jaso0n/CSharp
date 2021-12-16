using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace serialport
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            uint add = 128;
            for (int i = 1; i < 41; i++)
            {
                TextBox tb = (TextBox)this.panel2.Controls["Address" + i.ToString()];
                tb.Text = "0x"+ add.ToString("X") + "(" + add.ToString() +")";
                add++;
            }
        }

        private void Sure_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Output_Click(object sender, EventArgs e)
        {

        }

        private void Input_Click(object sender, EventArgs e)
        {

        }

        private void Path_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel文件|*.xlsx;*.XLSX | CSV文件|*.csv;*.CSV";
            ofd.ShowDialog();
            string path = ofd.FileName;
            this.path.Text = path;
        }

        private void Address1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
