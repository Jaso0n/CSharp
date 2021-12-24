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
        static bool flag = true;
        static double RREF = 8.431;
        static double IFULL = 74.999407;
        string EEPValue = String.Empty;
        private Form1 fm1;
        static bool singeladdr = true;

        public static string[] EEPaddress = new string[40] { "128", "129", "130", "131", "132", "133", "134", "135", "136", "137", "138", "139",
                                                              "160", "161", "162", "163", "164", "165", "166", "167", "168", "169", "170", "171",
                                                              "192", "193", "194", "195", "196", "197", "198", "199", "200", "201", "202", "203",
                                                              "204", "205", "206", "207"};
        public static string[] EEPvalue = new string[45];
        private static int[] EEPcombobox = new int[60];
                                                                                                                                        

        /****************************************************************************************************************
         * CRC Calculation Begin
         * *************************************************************************************************************/
        private static byte crr_calculation(byte crc_init, byte input_data)
        {
            byte temp_bit, input_bit;
            byte bit0, bit1, bit2, bit3, bit4, bit5, bit6, bit7;
            byte crc;
            byte i = 0;

            bit0 = (byte)(crc_init & 0x01);
            bit1 = (byte)((crc_init & 0x02) >> 1);
            bit2 = (byte)((crc_init & 0x04) >> 2);
            bit3 = (byte)((crc_init & 0x08) >> 3);
            bit4 = (byte)((crc_init & 0x10) >> 4);
            bit5 = (byte)((crc_init & 0x20) >> 5);
            bit6 = (byte)((crc_init & 0x40) >> 6);
            bit7 = (byte)((crc_init & 0x80) >> 7);

            while (i < 8)
            {
                input_bit = (byte)((input_data >> i) & 0x01);
                temp_bit = (byte)(input_bit ^ bit7);
                bit7 = bit6;
                bit6 = bit5;
                bit4 = (byte)(bit4 ^ temp_bit);
                bit5 = bit4;
                bit3 = (byte)(bit3 ^ temp_bit);
                bit4 = bit3;
                bit3 = bit2;
                bit2 = bit1;
                bit1 = bit0;
                bit0 = temp_bit;
                i++;
            }

            crc = (byte)((bit7 << 7) | (bit6 << 6) | (bit5 << 5) | (bit4 << 4) | (bit3 << 3) | (bit2 << 2) | (bit1 << 1) | (bit0));

            return crc;
        }
        private static byte CRC(byte[] commandframe, byte length)
        {
            byte j;
            byte crctemp = 0;
            for (j = 0; j < length; j++)
            {
                if (j == 0)
                    crctemp = crr_calculation(0xff, commandframe[j]);
                else
                    crctemp = crr_calculation(crctemp, commandframe[j]);
            }
            return crctemp;
        }

        private void CRC_Check()
        {
            byte[] temp = new byte[39];
            for (int i = 0; i < 39; i++)
            {
                NumericUpDown nud = (NumericUpDown)this.panel2.Controls["value" + (i + 1).ToString()];
                temp[i] = Convert.ToByte(nud.Value);
            }
            this.value40.Value = CRC(temp, 39);
        }
        /****************************************************************************************************************
         * CRC Calculation Over
         * *************************************************************************************************************/

        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Form1 f)
        {
            InitializeComponent();
            fm1 = f;
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            if (!flag)
            {
                for (int i = 1; i < 41; i++)
                {
                    NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];
                    nud.Value = Convert.ToInt16(EEPvalue[i]);
                }
                for (int i = 1; i < 58; i++)
                {
                    ComboBox cb = (ComboBox)this.panel2.Controls["comboBox" + i.ToString()];
                    cb.SelectedIndex = EEPcombobox[i];
                }
            }

           
            CRC_Check();
            this.path.Text = path_input;
            this.path2.Text = path_output;

            this.Rref.Text = RREF.ToString();
            this.I_Full.Text =  IFULL.ToString(); 
            this.checkBox1.Checked = singeladdr;
        }
        /***************************************************************************************************
         * Sure & Cancel Button Begin
         *************************************************************************************************/
        private void Sure_Click(object sender, EventArgs e)
        {
            if(!flag)
            {
                for (int i = 1; i < 41; i++)
                {
                    NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];
                    EEPvalue[i] = nud.Value.ToString();
                    EEPValue += EEPaddress[i - 1] + EEPvalue[i] + " ";
                }

                for (int i = 1;i < 58; i++)
                {
                    ComboBox cb = (ComboBox)this.panel2.Controls["comboBox" + i.ToString()];
                    EEPcombobox[i] = cb.SelectedIndex;
                }
            }
            this.Close();

            fm1.textBox2.Clear();
            fm1.textBox2.Text = EEPValue;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***************************************************************************************************
         * Sure Button Over
         *************************************************************************************************/


        /**************************************************************************************************
         * Input & Output Function Begin
         * ***********************************************************************************************/
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
                    CRC_Check();
                    Data_Refresh();

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
                string row_txt;
                NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];
                row_txt = EEPaddress[i-1] + "," + nud.Value.ToString();
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

            for (int i = 1; i < 40; i++)
            {
                NumericUpDown nud = (NumericUpDown)this.panel2.Controls["Value" + i.ToString()];
                EEPvalue[i] = sw.ReadLine();
                nud.Value = Convert.ToInt16(EEPvalue[i].Split(',')[1]);
                EEPvalue[i] = nud.Value.ToString();
            }
            sw.Close();
            fs.Close();

            return true;
        }
        /**************************************************************************************************
         * Input & Output Function Over
         * ***********************************************************************************************/


        /**************************************************************************************************
         * I_FULL & Data Refresh Begin
         * ***********************************************************************************************/
        private void RREF_Change(object sender, EventArgs e)
        {
            flag = false;
            if (this.comboBox43.Text != String.Empty && this.Rref.Text != String.Empty)
            {
                RREF = Convert.ToDouble(Rref.Text);
                IFULL = 1.235 / Convert.ToDouble(Rref.Text) * Convert.ToDouble(comboBox43.Text);
                I_Full.Text = IFULL.ToString("0.000000");
            }

            for (int i = 1; i < 13; i++)
            {
                NumericUpDown nud = (NumericUpDown)(this.panel2.Controls["value" + i.ToString()]);
                TextBox tb = (TextBox)this.panel2.Controls["value" + i.ToString() + "d"];
                tb.Text = (nud.Value / nud.Maximum * Convert.ToDecimal(IFULL)).ToString("0.000000");
            }

            for (int i = 13; i < 25; i++)
            {
                NumericUpDown nud = (NumericUpDown)(this.panel2.Controls["value" + i.ToString()]);
                TextBox tb = (TextBox)this.panel2.Controls["value" + i.ToString() + "p"];
                tb.Text = (nud.Value / nud.Maximum * 100).ToString("#0.0");
            }
        }

        private void IOUT_Change(object sender, EventArgs e)
        {
            NumericUpDown nud = sender as NumericUpDown;
            TextBox tb = (TextBox)this.panel2.Controls[nud.Name + "d"];
            tb.Text = (nud.Value / nud.Maximum * Convert.ToDecimal(IFULL)).ToString("0.000000");
            CRC_Check();
        }

        private void PWMOUT_Change(object sender, EventArgs e)
        {
            NumericUpDown nud = sender as NumericUpDown;
            TextBox tb = (TextBox)this.panel2.Controls[nud.Name + "p"];
            tb.Text = ((float)(nud.Value / nud.Maximum * 100)).ToString("#0.0");
            CRC_Check();
        }
        /**************************************************************************************************
         * I_FULL & Data Refresh Begin
         * ***********************************************************************************************/

        /**************************************************************************************************
        * FS0 ComboBox & NumericUpDown Begin
        **************************************************************************************************/
        private void FS0_Change(object sender, EventArgs e)
        {
            FS0_Change_CB();
            CRC_Check();
        }

        private void FS0_CB_Change(object sender, EventArgs e)
        {
            int bit0, bit1, bit2, bit3, bit4, bit5, bit6, bit7, bit8, bit9, bit10, bit11;
            ComboBox CB = sender as ComboBox;

            if (CB.Text == "Enable")
                CB.ForeColor = Color.ForestGreen;
            else
                CB.ForeColor = Color.Red;

            bit0 = this.comboBox12.Text == "Enable" ? 1 : 0;
            bit1 = this.comboBox11.Text == "Enable" ? 1 : 0;
            bit2 = this.comboBox10.Text == "Enable" ? 1 : 0;
            bit3 = this.comboBox9.Text == "Enable" ? 1 : 0;
            bit4 = this.comboBox8.Text == "Enable" ? 1 : 0;
            bit5 = this.comboBox7.Text == "Enable" ? 1 : 0;
            bit6 = this.comboBox6.Text == "Enable" ? 1 : 0;
            bit7 = this.comboBox5.Text == "Enable" ? 1 : 0;
            bit8 = this.comboBox4.Text == "Enable" ? 1 : 0;
            bit9 = this.comboBox3.Text == "Enable" ? 1 : 0;
            bit10 = this.comboBox2.Text == "Enable" ? 1 : 0;
            bit11 = this.comboBox1.Text == "Enable" ? 1 : 0;

            this.value25.Value = bit7 << 7 | bit6 << 6 | bit5 << 5 | bit4 << 4 | bit3 << 3 | bit2 << 2 | bit1 << 1 | bit0;
            this.value26.Value = bit11 << 3 | bit10 << 2 | bit9 << 1 | bit8;
        }

        private void FS0_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value25.Value);
            this.comboBox12.SelectedIndex = (temp & 1) == 1 ? 0 : 1;
            this.comboBox11.SelectedIndex = (temp & 2) == 2 ? 0 : 1;
            this.comboBox10.SelectedIndex = (temp & 4) == 4 ? 0 : 1;
            this.comboBox9.SelectedIndex = (temp & 8) == 8 ? 0 : 1;
            this.comboBox8.SelectedIndex = (temp & 16) == 16 ? 0 : 1;
            this.comboBox7.SelectedIndex = (temp & 32) == 32 ? 0 : 1;
            this.comboBox6.SelectedIndex = (temp & 64) == 64 ? 0 : 1;
            this.comboBox5.SelectedIndex = (temp & 128) == 128 ? 0 : 1;
            temp = Convert.ToByte(this.value26.Value);
            this.comboBox4.SelectedIndex = (temp & 1) == 1 ? 0 : 1;
            this.comboBox3.SelectedIndex = (temp & 2) == 2 ? 0 : 1;
            this.comboBox2.SelectedIndex = (temp & 4) == 4 ? 0 : 1;
            this.comboBox1.SelectedIndex = (temp & 8) == 8 ? 0 : 1;
        }
        /********************************************************************************************************************************
        * FS0 ComboBox & NumericUpDown Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * FS1 ComboBox & NumericUpDown  Begin
        ********************************************************************************************************************************/
        private void FS1_Change(object sender, EventArgs e)
        {
            FS1_Change_CB();
            CRC_Check();
        }

        private void FS1_CB_Change(object sender, EventArgs e)
        {
            int bit0, bit1, bit2, bit3, bit4, bit5, bit6, bit7, bit8, bit9, bit10, bit11;
            ComboBox CB = sender as ComboBox;

            if (CB.Text == "Enable")
                CB.ForeColor = Color.ForestGreen;
            else
                CB.ForeColor = Color.Red;

            bit0 = this.comboBox13.Text == "Enable" ? 1 : 0;
            bit1 = this.comboBox14.Text == "Enable" ? 1 : 0;
            bit2 = this.comboBox15.Text == "Enable" ? 1 : 0;
            bit3 = this.comboBox16.Text == "Enable" ? 1 : 0;
            bit4 = this.comboBox17.Text == "Enable" ? 1 : 0;
            bit5 = this.comboBox18.Text == "Enable" ? 1 : 0;
            bit6 = this.comboBox19.Text == "Enable" ? 1 : 0;
            bit7 = this.comboBox20.Text == "Enable" ? 1 : 0;
            bit8 = this.comboBox21.Text == "Enable" ? 1 : 0;
            bit9 = this.comboBox22.Text == "Enable" ? 1 : 0;
            bit10 = this.comboBox23.Text == "Enable" ? 1 : 0;
            bit11 = this.comboBox24.Text == "Enable" ? 1 : 0;

            this.value27.Value = bit7 << 7 | bit6 << 6 | bit5 << 5 | bit4 << 4 | bit3 << 3 | bit2 << 2 | bit1 << 1 | bit0;
            this.value28.Value = bit11 << 3 | bit10 << 2 | bit9 << 1 | bit8;
        }

        private void FS1_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value27.Value);
            this.comboBox13.SelectedIndex = (temp & 1) == 1 ? 0 : 1;
            this.comboBox14.SelectedIndex = (temp & 2) == 2 ? 0 : 1;
            this.comboBox15.SelectedIndex = (temp & 4) == 4 ? 0 : 1;
            this.comboBox16.SelectedIndex = (temp & 8) == 8 ? 0 : 1;
            this.comboBox17.SelectedIndex = (temp & 16) == 16 ? 0 : 1;
            this.comboBox18.SelectedIndex = (temp & 32) == 32 ? 0 : 1;
            this.comboBox19.SelectedIndex = (temp & 64) == 64 ? 0 : 1;
            this.comboBox20.SelectedIndex = (temp & 128) == 128 ? 0 : 1;
            temp = Convert.ToByte(this.value28.Value);
            this.comboBox21.SelectedIndex = (temp & 1) == 1 ? 0 : 1;
            this.comboBox22.SelectedIndex = (temp & 2) == 2 ? 0 : 1;
            this.comboBox23.SelectedIndex = (temp & 4) == 4 ? 0 : 1;
            this.comboBox24.SelectedIndex = (temp & 8) == 8 ? 0 : 1;
        }
        /********************************************************************************************************************************
        * FS1 ComboBox & NumericUpDown  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * Diagnostic ComboBox & NumericUpDown  Begin
        ********************************************************************************************************************************/
        private void Diagnostic_Change(object sender, EventArgs e)
        {
            Diagnostic_Change_CB();
            CRC_Check();
        }

        private void Diagnostic_CB_Change(object sender, EventArgs e)
        {
            int bit0, bit1, bit2, bit3, bit4, bit5, bit6, bit7, bit8, bit9, bit10, bit11;
            ComboBox CB = sender as ComboBox;

            if (CB.Text == "Enable")
                CB.ForeColor = Color.ForestGreen;
            else
                CB.ForeColor = Color.Red;

            bit0 = this.comboBox25.Text == "Enable" ? 1 : 0;
            bit1 = this.comboBox26.Text == "Enable" ? 1 : 0;
            bit2 = this.comboBox27.Text == "Enable" ? 1 : 0;
            bit3 = this.comboBox28.Text == "Enable" ? 1 : 0;
            bit4 = this.comboBox29.Text == "Enable" ? 1 : 0;
            bit5 = this.comboBox30.Text == "Enable" ? 1 : 0;
            bit6 = this.comboBox31.Text == "Enable" ? 1 : 0;
            bit7 = this.comboBox32.Text == "Enable" ? 1 : 0;
            bit8 = this.comboBox33.Text == "Enable" ? 1 : 0;
            bit9 = this.comboBox34.Text == "Enable" ? 1 : 0;
            bit10 = this.comboBox35.Text == "Enable" ? 1 : 0;
            bit11 = this.comboBox36.Text == "Enable" ? 1 : 0;

            this.value29.Value = bit7 << 7 | bit6 << 6 | bit5 << 5 | bit4 << 4 | bit3 << 3 | bit2 << 2 | bit1 << 1 | bit0;
            this.value30.Value = bit11 << 3 | bit10 << 2 | bit9 << 1 | bit8;
        }

        private void Diagnostic_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value29.Value);
            this.comboBox25.SelectedIndex = (temp & 1) == 1 ? 0 : 1;
            this.comboBox26.SelectedIndex = (temp & 2) == 2 ? 0 : 1;
            this.comboBox27.SelectedIndex = (temp & 4) == 4 ? 0 : 1;
            this.comboBox28.SelectedIndex = (temp & 8) == 8 ? 0 : 1;
            this.comboBox29.SelectedIndex = (temp & 16) == 16 ? 0 : 1;
            this.comboBox30.SelectedIndex = (temp & 32) == 32 ? 0 : 1;
            this.comboBox31.SelectedIndex = (temp & 64) == 64 ? 0 : 1;
            this.comboBox32.SelectedIndex = (temp & 128) == 128 ? 0 : 1;
            temp = Convert.ToByte(this.value30.Value);
            this.comboBox33.SelectedIndex = (temp & 1) == 1 ? 0 : 1;
            this.comboBox34.SelectedIndex = (temp & 2) == 2 ? 0 : 1;
            this.comboBox35.SelectedIndex = (temp & 4) == 4 ? 0 : 1;
            this.comboBox36.SelectedIndex = (temp & 8) == 8 ? 0 : 1;
        }
        /********************************************************************************************************************************
        * Diagnostic ComboBox & NumericUpDown  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * EEPM6 Change  Begin
        ********************************************************************************************************************************/
        private void EEPM6_Change(object sender, EventArgs e)
        {
            EEPM6_Change_CB();
            CRC_Check();
        }
        private void EEPM6_CB_Change(object sender, EventArgs e)
        {
            int bit0_3, bit4, bit6;

            if(this.comboBox39.SelectedIndex == -1)
                bit0_3 = 0;
            else
                bit0_3 = this.comboBox39.SelectedIndex;

            bit4 = this.comboBox38.Text == "Enable" ? 1 : 0;
            bit6 = this.comboBox37.Text == "4.4" ? 1 : 0;

            this.value31.Value = bit6 << 6 | bit4 << 4 | bit0_3;
        }

        private void EEPM6_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value31.Value);

            this.comboBox37.SelectedIndex = (temp & 64) == 64 ? 1 : 0;
            this.comboBox38.SelectedIndex = (temp & 16) == 16 ? 0 : 1;
            this.comboBox39.SelectedIndex = (temp & 15);
        }
        /********************************************************************************************************************************
        * EEPM6 Change  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * EEPM7 Change  Begin
        ********************************************************************************************************************************/
        private void EEPM7_Change(object sender, EventArgs e)
        {
            EEPM7_Change_CB();
            CRC_Check();
        }
        private void EEPM7_CB_Change(object sender, EventArgs e)
        {
            int bit0_1, bit2, bit3, bit4_7;

            if (this.comboBox43.SelectedIndex == -1)
                bit0_1 = 0;
            else
                bit0_1 = this.comboBox43.SelectedIndex;

            bit2 = this.comboBox42.Text == "One Fail All Fail" ? 1 : 0;
            bit3 = this.comboBox41.Text == "Enable" ? 1 : 0;

            if (this.comboBox40.SelectedIndex == -1)
                bit4_7 = 0;
            else
                bit4_7 = this.comboBox40.SelectedIndex;

            this.value32.Value = bit4_7 << 4 | bit3 << 3 | bit2 << 2 | bit0_1;

            ComboBox cb = sender as ComboBox;
            if (cb.Name == "comboBox43")
            {
                if (this.comboBox43.Text != String.Empty && this.Rref.Text != String.Empty)
                {
                    RREF = Convert.ToDouble(Rref.Text);
                    IFULL = 1.235 / Convert.ToDouble(Rref.Text) * Convert.ToDouble(comboBox43.Text);
                    I_Full.Text = IFULL.ToString("0.000000");
                }
            }
        }

        private void EEPM7_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value32.Value);

            this.comboBox40.SelectedIndex = (temp & 0xf0) >> 4;
            this.comboBox41.SelectedIndex = (temp & 8) == 8 ? 0 : 1;
            this.comboBox42.SelectedIndex = (temp & 4) == 4 ? 0 : 1;
            this.comboBox43.SelectedIndex = temp & 3;
        }
        /********************************************************************************************************************************
        * EEPM7 Change  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * EEPM8 Change  Begin
        ********************************************************************************************************************************/
        private void EEPM8_Change(object sender, EventArgs e)
        {
            EEPM8_Change_CB();
            CRC_Check();
        }
        private void EEPM8_CB_Change(object sender, EventArgs e)
        {
            int bit0_3, bit4_6;

            if (this.comboBox45.SelectedIndex == -1)
                bit0_3 = 0;
            else
                bit0_3 = this.comboBox45.SelectedIndex;

            if (this.comboBox44.SelectedIndex == -1)
                bit4_6 = 0;
            else
                bit4_6 = this.comboBox44.SelectedIndex;

            this.value33.Value = bit4_6 << 4 | bit0_3;
        }

        private void EEPM8_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value33.Value);

            this.comboBox44.SelectedIndex = (temp & 0x70) >> 4;
            this.comboBox45.SelectedIndex = temp & 15;
        }
        /********************************************************************************************************************************
        * EEPM8 Change  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * EEPM9 Change  Begin
        ********************************************************************************************************************************/
        private void EEPM9_Change(object sender, EventArgs e)
        {
            EEPM9_Change_CB();
            CRC_Check();
        }
        private void EEPM9_CB_Change(object sender, EventArgs e)
        {
            int bit0_3, bit4_7;

            if (this.comboBox47.SelectedIndex == -1)
                bit0_3 = 0;
            else
                bit0_3 = this.comboBox47.SelectedIndex;

            if (this.comboBox46.SelectedIndex == -1)
                bit4_7 = 0;
            else
                bit4_7 = this.comboBox46.SelectedIndex;

            this.value34.Value = bit4_7 << 4 | bit0_3;
        }

        private void EEPM9_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value34.Value);

            this.comboBox46.SelectedIndex = (temp & 0xF0) >> 4;
            this.comboBox47.SelectedIndex = temp & 15;
        }
        /********************************************************************************************************************************
        * EEPM9 Change  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * EEPM10 Change  Begin
        ********************************************************************************************************************************/
        private void EEPM10_Change(object sender, EventArgs e)
        {
            EEPM10_Change_CB();
            CRC_Check();
        }
        private void EEPM10_CB_Change(object sender, EventArgs e)
        {
            int bit0_3, bit4_7;

            if (this.comboBox49.SelectedIndex == -1)
                bit0_3 = 0;
            else
                bit0_3 = this.comboBox49.SelectedIndex;

            if (this.comboBox48.SelectedIndex == -1)
                bit4_7 = 0;
            else
                bit4_7 = this.comboBox48.SelectedIndex;

            this.value35.Value = bit4_7 << 4 | bit0_3;
        }

        private void EEPM10_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value35.Value);

            this.comboBox48.SelectedIndex = (temp & 0xF0) >> 4;
            this.comboBox49.SelectedIndex = temp & 15;
        }
        /********************************************************************************************************************************
        * EEPM10 Change  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * EEPM11 Change  Begin
        ********************************************************************************************************************************/
        private void EEPM11_Change(object sender, EventArgs e)
        {
            EEPM11_Change_CB();
            CRC_Check();
        }
        private void EEPM11_CB_Change(object sender, EventArgs e)
        {
            int bit0, bit1, bit2, bit3, bit4, bit5, bit6, bit7;
            ComboBox CB = sender as ComboBox;

            if (CB.Text == "1")
                CB.ForeColor = Color.ForestGreen;
            else
                CB.ForeColor = Color.Red;

            bit0 = this.comboBox54.Text == "1" ? 1 : 0;
            bit1 = this.comboBox55.Text == "1" ? 1 : 0;
            bit2 = this.comboBox56.Text == "1" ? 1 : 0;
            bit3 = this.comboBox57.Text == "1" ? 1 : 0;
            bit4 = this.comboBox50.Text == "1" ? 1 : 0;
            bit5 = this.comboBox51.Text == "1" ? 1 : 0;
            bit6 = this.comboBox52.Text == "1" ? 1 : 0;
            bit7 = this.comboBox53.Text == "1" ? 1 : 0;

            this.value36.Value = bit7 << 7 | bit6 << 6 | bit5 << 5 | bit4 << 4 | bit3 << 3 | bit2 << 2 | bit1 << 1 | bit0;
        }

        private void EEPM11_Change_CB()
        {
            byte temp;
            temp = Convert.ToByte(this.value36.Value);

            this.comboBox54.SelectedIndex = (temp & 1) == 1 ? 1 : 0;
            this.comboBox55.SelectedIndex = (temp & 2) == 2 ? 1 : 0;
            this.comboBox56.SelectedIndex = (temp & 4) == 4 ? 1 : 0;
            this.comboBox57.SelectedIndex = (temp & 8) == 8 ? 1 : 0;
            this.comboBox50.SelectedIndex = (temp & 16) == 16 ? 1 : 0;
            this.comboBox51.SelectedIndex = (temp & 32) == 32 ? 1 : 0;
            this.comboBox52.SelectedIndex = (temp & 64) == 64 ? 1 : 0;
            this.comboBox53.SelectedIndex = (temp & 128) == 128 ? 1 : 0;
        }
        /********************************************************************************************************************************
        * EEPM11 Change  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
        * EEPM12_14 Change  Begin
        ********************************************************************************************************************************/
        private void EEPM12_14_Change(object sender, EventArgs e)
        {
            CRC_Check();
        }
        /********************************************************************************************************************************
        * EEPM12_14 Change  Over
        ********************************************************************************************************************************/


        /********************************************************************************************************************************
         * Data Refresh Begin
         * *****************************************************************************************************************************/
        private void Data_Refresh()
        {
            FS0_Change_CB();
            FS1_Change_CB();
            Diagnostic_Change_CB();
            EEPM6_Change_CB();
            EEPM7_Change_CB();
            EEPM8_Change_CB();
            EEPM9_Change_CB();
            EEPM10_Change_CB();
            EEPM11_Change_CB();
        }
        /********************************************************************************************************************************
        * Data Refresh Over
        * *****************************************************************************************************************************/


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox58.Enabled = checkBox1.Checked;
            if(checkBox1.Checked)
            {
                checkBox2.Checked = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox2.Checked)
            {
                checkBox1.Checked = false;
            }
        }
    }

}
