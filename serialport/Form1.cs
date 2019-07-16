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
    public partial class Form1 : Form
    {
        private byte[] ReadyToSend, RTS;
        private int SDNumb;
        private int RENumb;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] baud = { "300","600","1200","2400","4800","9600","19200","38400",
                "43000", "56000", "57600","115200", "128000", "230400", "256000", "460800" };
            comboBox2.Items.AddRange(baud);
            comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            //串口默认设置
            comboBox1.Text = "COM1";
            comboBox2.Text = "9600";
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "1";
            button1.BackColor = Color.ForestGreen;

        }

        private void Button1_Click(object sender, EventArgs e)
            //串口初始化
        {
            try {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    label7.Text = "串口已关闭";
                    label7.ForeColor = Color.Red;
                    button1.Text = "打开串口";
                    button1.BackColor = Color.ForestGreen;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;

                }
                else
                {
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;

                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.DataBits = Convert.ToInt16(comboBox3.Text);
                    serialPort1.Parity = System.IO.Ports.Parity.None;

                    if (comboBox5.Text.Equals("1"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBox5.Text.Equals("1.5"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBox5.Text.Equals("2"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.Two;
                    serialPort1.Open();
                    label7.Text = "串口已打开";
                    label7.ForeColor = Color.Green;
                    button1.Text = "关闭串口";
                    button1.BackColor = Color.Firebrick;
                }
            }
            catch (Exception ex)
            {
                serialPort1 = new System.IO.Ports.SerialPort();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show(ex.Message);
                label7.Text = "串口已关闭";
                label7.ForeColor = Color.Red;
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        //串口发送
        {
            try
            {
                int num = 0;
                if (serialPort1.IsOpen & checkBox3.Checked)
                {
                    //16进制发送
                    string rds = textBox2.Text.Replace(" ","");//去除输入的空格
                    num = (rds.Length - rds.Length % 2) / 2;
                    if (rds.Length % 2 == 0)
                    //判断用户输入是否为偶数
                    {
                        ReadyToSend = new byte[num];
                        for (int i = 0; i < num; i++)
                        {
                            ReadyToSend[i] = Convert.ToByte(rds.Substring(i * 2, 2), 16);
                        }
                    }
                    else
                    {
                        int k = num + 1;
                        ReadyToSend = new byte[k];
                        for (int i = 0; i < k -1; i++)
                        {
                            ReadyToSend[i] = Convert.ToByte(rds.Substring(i * 2, 2), 16);
                        }
                        ReadyToSend[k-1] = Convert.ToByte(rds.Substring(rds.Length - 1, 1), 16);
                    }
                    //新行发送
                    if (checkBox1.Checked)
                    {
                        byte[] changeLine = { 0x0d, 0x0a };
                        RTS = ReadyToSend.Concat(changeLine).ToArray(); 
                        serialPort1.Write(RTS, 0, RTS.Length);
                        SDNumb += RTS.Length;
                    }
                    else
                    {
                        RTS = ReadyToSend;
                        serialPort1.Write(RTS, 0,RTS.Length );
                        SDNumb += RTS.Length;
                    }
                }
                else if (serialPort1.IsOpen & !checkBox3.Checked)
                {
                    //ASCII 发送
                    if (checkBox1.Checked)
                    //新行发送
                    {
                        serialPort1.Write(textBox2.Text + "\r" + "\n");
                        SDNumb += textBox2.Text.Length + 2;
                    }

                    else
                    {
                        serialPort1.Write(textBox2.Text);
                        SDNumb += textBox2.Text.Length;
                    }
                }
                label8.Text = "TX:" + SDNumb.ToString() + "Bytes";
            }
            catch (Exception ex)
            {
                serialPort1.Close();
                serialPort1 = new System.IO.Ports.SerialPort();
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show(ex.Message);
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }
        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //数据接收
            int RecvNum = serialPort1.BytesToRead;
            byte[] RecvBuff = new byte[RecvNum];
            serialPort1.Read(RecvBuff, 0, RecvNum);
            RENumb += RecvNum;
            label9.Text = "RX:" +RENumb.ToString() + "Bytes";
            try
            {
                this.Invoke(new EventHandler(delegate
             {
                 if (checkBox2.Checked)
                 {
                     //16 进制显示
                     foreach (byte b in RecvBuff)
                     {
                         textBox1.AppendText(b.ToString("X2")+' ');
                     }

                 }
                 else
                 {
                     textBox1.AppendText(Encoding.ASCII.GetString(RecvBuff));
                 }
             }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
            //清除接受缓冲区
        {
            textBox1.Text = null;
            RENumb = 0;
            label9.Text = "RX:0Bytes";
        }


        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            //当勾选"16进制发送" 时，将发送缓冲区中的字符串转换成16进制的ASCII码
            if (checkBox3.Checked && (textBox2.Text != null))
            {
                byte[] array = Encoding.ASCII.GetBytes(textBox2.Text);
                string str2asc = null;
                foreach (byte b in array)
                {
                    //str2asc = str2asc + Convert.ToString(b, 16) + " ";
                    str2asc = str2asc + b.ToString("X2") + ' ';
                }
                textBox2.Text = str2asc;

            }
            else if ((textBox2.Text != null) && (!checkBox3.Checked))
            {
                string[] hex = textBox2.Text.Split(' ');
                byte[] intarry = new byte[hex.Length];
                for (int i = 0; i < hex.Length - 1; i++)
                {
                    intarry[i] = Convert.ToByte(hex[i], 16);
                }
                textBox2.Text = Encoding.ASCII.GetString(intarry);
            }
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (checkBox3.Checked)
            {
                bool IsNum = (e.KeyChar >= 48) && (e.KeyChar <= 57);//判断输入是不是数字
                bool IsAplha = (e.KeyChar >= 'A') && (e.KeyChar <= 'F');//判断输入是不是大写字母
                bool Isalpha = (e.KeyChar >= 'a') && (e.KeyChar <= 'f');//判断输入是不是小写字母
                bool Ispermit = e.KeyChar == 8 || e.KeyChar == 32;//判断输入是不是退格和空格
                if (IsNum || IsAplha || Isalpha || Ispermit)
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                    MessageBox.Show("合法的输入为0-9，a-f，A-F，空格和退格");
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)//清除发送缓冲区
        {
            textBox2.Text = null;
            SDNumb = 0;
            label8.Text = "TX:0Bytes";
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
        
        }
    }
}
