using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace serialport
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    static class Data
    {
        public static string path_input = string.Empty;  //输入路径
        public static string path_output = string.Empty; //输出路径
        public static bool flag = true; //是否有值发生改变
        public static double RREF = 8.431; //参考电阻默认值
        public static double IFULL = 74.999407;//参考电流默认值
        public static byte addr;//芯片地址

        public static byte[] EEPaddress = new byte[40] { 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139,
                                                         160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171,
                                                         192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203,
                                                         204, 205, 206, 207}; //EEPROM ADDR

        public static byte[] EEPvalue = new byte[40];//EEPROM Data

        /****************************************************************************************************************
         * CRC Calculation Begin @ShuGuang
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
        public static byte CRC(byte[] commandframe, byte length)
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
        /****************************************************************************************************************
         * CRC Calculation Over
         * *************************************************************************************************************/

        /****************************************************************************
        * TPS Frame Begin  @ShuGuang
        * ************************************************************************/
        enum cmd_len_t
        {
            Single_1byte = 0,
            Burst_2byte = 1,
            Burst_4byte = 2,
            Burst_8byte = 3,
        };

        /* 生成DEV_ADDR结构 */
        static byte tps_dev_get(byte addr, cmd_len_t len, byte broascast, byte readwrite)
        {
            byte tps_dev;

            tps_dev = (byte)((addr & 0x0f) | ((broascast & 0x01) << 6) | ((readwrite & 0x01) << 7));

            switch (len)
            {
                case cmd_len_t.Single_1byte: tps_dev |= (0 << 4); break;
                case cmd_len_t.Burst_2byte: tps_dev |= (1 << 4); break;
                case cmd_len_t.Burst_4byte: tps_dev |= (2 << 4); break;
                case cmd_len_t.Burst_8byte: tps_dev |= (3 << 4); break;
            }

            return tps_dev;
        }

        /* 写1个字节 */
        public static byte tps_write_1byte(byte addr, byte reg, byte data, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Single_1byte, 0, 1);
            buf[2] = reg;
            buf[3] = data;

            for (int i = 0; i < 4; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 3);
            buf[4] = v;

            return 5;
        }

#if false

        /* 写2个字节 */
        static byte tps_write_2byte(byte addr, byte reg, byte[] data, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Burst_2byte, 0, 1);
            buf[2] = reg;
            buf[3] = data[0];
            buf[4] = data[1];

            for (int i = 0; i < 5; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 4);
            buf[5] = v;

            return 6;
        }
        /* 写4个字节 */
        static byte tps_write_4byte(byte addr, byte reg, byte[] data, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Burst_4byte, 0, 1);
            buf[2] = reg;
            buf[3] = data[0];
            buf[4] = data[1];
            buf[5] = data[2];
            buf[6] = data[3];

            for (int i = 0; i < 7; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 6);
            buf[7] = v;

            return 8;
        }

        /* 写8个字节 */
        static byte tps_write_8byte(byte addr, byte reg, byte[] data, ref byte[] buf)
        {
            byte[] temp = new byte[15];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Burst_8byte, 0, 1);
            buf[2] = reg;
            buf[3] = data[0];
            buf[4] = data[1];
            buf[5] = data[2];
            buf[6] = data[3];
            buf[7] = data[4];
            buf[8] = data[5];
            buf[9] = data[6];
            buf[10] = data[7];

            for (int i = 0; i < 11; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 10);
            buf[11] = v;

            return 12;
        }

        /* 广播写1个字节 */
        static byte tps_write_1byte_broadcast(byte reg, byte data, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(0, cmd_len_t.Single_1byte, 1, 1);
            buf[2] = reg;
            buf[3] = data;

            for (int i = 0; i < 4; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 3);
            buf[4] = v;

            return 5;
        }
        /* 广播写2个字节 */
        static byte tps_write_2byte_broadcast(byte reg, byte[] data, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(0, cmd_len_t.Burst_2byte, 1, 1);
            buf[2] = reg;
            buf[3] = data[0];
            buf[4] = data[1];

            for (int i = 0; i < 5; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 4);
            buf[5] = v;

            return 6;
        }
        /* 广播写4个字节 */
        static byte tps_write_4byte_broadcast(byte reg, byte[] data, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(0, cmd_len_t.Burst_4byte, 1, 1);
            buf[2] = reg;
            buf[3] = data[0];
            buf[4] = data[1];
            buf[5] = data[2];
            buf[6] = data[3];

            for (int i = 0; i < 7; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 6);
            buf[7] = v;

            return 8;
        }
        /* 广播写8个字节 */
        static byte tps_write_8byte_broadcast(byte reg, byte[] data, ref byte[] buf)
        {
            byte[] temp = new byte[15];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(0, cmd_len_t.Burst_8byte, 1, 1);
            buf[2] = reg;
            buf[3] = data[0];
            buf[4] = data[1];
            buf[5] = data[2];
            buf[6] = data[3];
            buf[7] = data[4];
            buf[8] = data[5];
            buf[9] = data[6];
            buf[10] = data[7];

            for (int i = 0; i < 11; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 10);
            buf[11] = v;

            return 12;
        }
#endif

        /* 读1个字节 */
        public static byte tps_read_1byte(byte addr, byte reg, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Single_1byte, 0, 0);
            buf[2] = reg;

            for (int i = 0; i < 3; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 2);
            buf[3] = v;

            return 4;
        }

#if false
        /* 读2个字节 */
        static byte tps_read_2byte(byte addr, byte reg, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Burst_2byte, 0, 0);
            buf[2] = reg;

            for (int i = 0; i < 3; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 3);
            buf[3] = v;

            return 4;
        }
        /* 读4个字节 */
        static byte tps_read_4byte(byte addr, byte reg, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Burst_4byte, 0, 0);
            buf[2] = reg;

            for (int i = 0; i < 3; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 3);
            buf[3] = v;

            return 4;
        }
        /* 读8个字节 */
        static byte tps_read_8byte(byte addr, byte reg, ref byte[] buf)
        {
            byte[] temp = new byte[10];
            buf[0] = 0x55;
            buf[1] = tps_dev_get(addr, cmd_len_t.Burst_8byte, 0, 0);
            buf[2] = reg;

            for (int i = 0; i < 3; i++)
                temp[i] = buf[i + 1];

            byte v = CRC(temp, 3);
            buf[3] = v;

            return 4;
        }

#endif
        /****************************************************************************
         * TPS Frame Over
         * ************************************************************************/

    }
}
