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


namespace serialport
{
    public partial class Form2 : Form
    {
        static string path_input = string.Empty;
        static string path_output = string.Empty;
        private static string[] KV = new string[45];
        private static bool flag = true;


        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            for (int i = 1; i < 41; i++)
            {
                TextBox tb = (TextBox)this.panel2.Controls["Address" + i.ToString()];
                NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];
                if (!flag)
                {
                    tb.Text = KV[i].Split(',')[0];
                    nud.Value = Convert.ToInt16(KV[i].Split(',')[1]);
                }
                else
                {
                    tb.Text = String.Empty;
                    nud.Value = 0;
                }
            }

            this.path.Text = path_input;
            this.path2.Text = path_output;
        }

        private void Sure_Click(object sender, EventArgs e)
        {
            if(!flag)
            {
                for (int i = 1; i < 41; i++)
                {
                    TextBox tb = (TextBox)this.panel2.Controls["Address" + i.ToString()];
                    NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];
                    KV[i] = tb.Text + "," + nud.Value.ToString();
                }
            }
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Output_Click(object sender, EventArgs e)
        {
            if(this.path2.Text != string.Empty)
            {
                if(Save_CSV(this.path2.Text))
                {
                    MessageBox.Show("导出完成");
                    flag = false;
                }
            }
            else
            {
                MessageBox.Show("导出失败");
            }
        }

        private void Input_Click(object sender, EventArgs e)
        {
            if (this.path.Text != string.Empty)
            {
                if (Input_CSV(this.path.Text))
                {
                    MessageBox.Show("导入完成");
                    flag=false;
                }
            }
            else
            {
                MessageBox.Show("导入失败");
            }
        }

        private void Path_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV文件|*.csv;*.CSV";
            ofd.ShowDialog();
            path_input = ofd.FileName;
            this.path.Text = path_input;
        }

        private void Path2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV文件 | *.csv; *.CSV";
            ofd.ShowDialog();
            path_output = ofd.FileName;
            this.path2.Text = path_output;
        }


        private bool Save_CSV(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Create,FileAccess.Write);

            StreamWriter sw = new StreamWriter(fs);

            string col_txt = "ADDRESS" + ","+ "VALUE";
            sw.WriteLine(col_txt);

            for (int i = 1; i < 41; i++)
            {
                string row_txt = string.Empty;
                
                TextBox tb = (TextBox)this.panel2.Controls["Address" + i.ToString()];
                NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];
                
                row_txt += tb.Text + "," + nud.Value.ToString();

                sw.WriteLine(row_txt);
            }
            sw.Flush();
            sw.Close();
            fs.Close();

            return true;
        }


        private bool Input_CSV(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader sw = new StreamReader(fs);

            sw.ReadLine();

            for (int i = 1; i < 41; i++)
            { 
                TextBox tb = (TextBox)this.panel2.Controls["Address" + i.ToString()];
                NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];

                KV[i] = sw.ReadLine();

                tb.Text = KV[i].Split(',')[0];
                nud.Value = Convert.ToInt16(KV[i].Split(',')[1]);
            }

            sw.Close();
            fs.Close();

            return true;
        }

        private void Text_Change(object sender, EventArgs e)
        {
            flag = false;
        }
    }
}
