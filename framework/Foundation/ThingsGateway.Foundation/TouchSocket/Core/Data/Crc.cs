#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// Crc相关。
    /// <para>该代码来源于网络</para>
    /// </summary>
    public static class Crc
    {
        /// **********************************************************************
        /// Name: CRC-4/ITU    x4+x+1
        /// Poly: 0x03
        /// Init: 0x00
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc1(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (byte)((crc >> 1) ^ 0x0C);//0x0C = (reverse 0x03)>>(8-4)
                    else
                        crc = (byte)(crc >> 1);
                }
            }
            return new byte[] { crc };
        }

        /// **********************************************************************
        /// Name: CRC-5/EPC    x5+x3+1
        /// Poly: 0x09
        /// Init: 0x09
        /// Refin: false
        /// Refout: false
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc2(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0x48;// Initial value: 0x48 = 0x09<<(8-5)
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 0x80) > 0)
                        crc = (byte)((crc << 1) ^ 0x48);// 0x48 = 0x09<<(8-5)
                    else
                        crc = (byte)(crc << 1);
                }
            }
            return new byte[] { (byte)(crc >> 3) };
        }

        /// **********************************************************************
        /// Name: CRC-5/ITU    x5+x4+x2+1
        /// Poly: 0x15
        /// Init: 0x00
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc3(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (byte)((crc >> 1) ^ 0x15);// 0x15 = (reverse 0x15)>>(8-5)
                    else
                        crc = (byte)(crc >> 1);
                }
            }
            return new byte[] { crc };
        }

        /// **********************************************************************
        /// Name: CRC-5/USB    x5+x2+1
        /// Poly: 0x05
        /// Init: 0x1F
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x1F
        ///*************************************************************************
        public static byte[] Crc4(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0x1F;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (byte)((crc >> 1) ^ 0x14);// 0x14 = (reverse 0x05)>>(8-5)
                    else
                        crc = (byte)(crc >> 1);
                }
            }
            return new byte[] { (byte)(crc ^ 0x1F) };
        }

        /// **********************************************************************
        /// Name: CRC-6/ITU    x6+x+1
        /// Poly: 0x03
        /// Init: 0x00
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc5(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (byte)((crc >> 1) ^ 0x30);// 0x30 = (reverse 0x03)>>(8-6)
                    else
                        crc = (byte)(crc >> 1);
                }
            }
            return new byte[] { crc };
        }

        /// **********************************************************************
        /// Name: CRC-7/MMC    x7+x3+1
        /// Poly: 0x09
        /// Init: 0x00
        /// Refin: false
        /// Refout: false
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc6(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 0x80) > 0)
                        crc = (byte)((crc << 1) ^ 0x12);// 0x12 = 0x09<<(8-7)
                    else
                        crc = (byte)(crc << 1);
                }
            }
            return new byte[] { (byte)(crc >> 1) };
        }

        /// **********************************************************************
        /// Name: CRC8    x8+x2+x+1
        /// Poly: 0x07
        /// Init: 0x00
        /// Refin: false
        /// Refout: false
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc7(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    crc = (crc & 0x80) > 0 ? (byte)((crc << 1) ^ 0x07) : (byte)(crc << 1);
                }
            }
            return new byte[] { crc };
        }

        /// **********************************************************************
        /// Name: CRC-8/ITU    x8+x2+x+1
        /// Poly: 0x07
        /// Init: 0x00
        /// Refin: false
        /// Refout: false
        /// Xorout: 0x55
        ///*************************************************************************
        public static byte[] Crc8(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    crc = (crc & 0x80) > 0 ? (byte)((crc << 1) ^ 0x07) : (byte)(crc << 1);
                }
            }
            return new byte[] { (byte)(crc ^ 0x55) };
        }

        /// **********************************************************************
        /// Name: CRC-8/MAXIM    x8+x5+x4+1
        /// Poly: 0x31
        /// Init: 0x00
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc9(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (byte)((crc >> 1) ^ 0x8C);// 0x8C = reverse 0x31
                    else
                        crc = (byte)(crc >> 1);
                }
            }
            return new byte[] { crc };
        }

        /// **********************************************************************
        /// Name: CRC-8/ROHC    x8+x2+x+1
        /// Poly: 0x07
        /// Init: 0xFF
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x00
        ///*************************************************************************
        public static byte[] Crc10(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            byte crc = 0xFF;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (byte)((crc >> 1) ^ 0xE0);// 0xE0 = reverse 0x07
                    else
                        crc = (byte)(crc >> 1);
                }
            }
            return new byte[] { crc };
        }

        /// Z1协议校验码计算
        private static readonly byte[] table = { 0x00, 0x1C, 0x38, 0x24, 0x70, 0x6C, 0x48, 0x54, 0xE0, 0xFC,
                                0xD8, 0xC4, 0x90, 0x8C, 0xA8, 0xB4, 0xDC, 0xC0, 0xE4, 0xF8,
                                0xAC, 0xB0, 0x94, 0x88, 0x3C, 0x20, 0x04, 0x18, 0x4C, 0x50,
                                0x74, 0x68, 0xA4, 0xB8, 0x9C, 0x80, 0xD4, 0xC8, 0xEC, 0xF0,
                                0x44, 0x58, 0x7C, 0x60, 0x34, 0x28, 0x0C, 0x10, 0x78, 0x64,
                                0x40, 0x5C, 0x08, 0x14, 0x30, 0x2C, 0x98, 0x84, 0xA0, 0xBC,
                                0xE8, 0xF4, 0xD0, 0xCC, 0x54, 0x48, 0x6C, 0x70, 0x24, 0x38,
                                0x1C, 0x00, 0xB4, 0xA8, 0x8C, 0x90, 0xC4, 0xD8, 0xFC, 0xE0,
                                0x88, 0x94, 0xB0, 0xAC, 0xF8, 0xE4, 0xC0, 0xDC, 0x68, 0x74,
                                0x50, 0x4C, 0x18, 0x04, 0x20, 0x3C, 0xF0, 0xEC, 0xC8, 0xD4,
                                0x80, 0x9C, 0xB8, 0xA4, 0x10, 0x0C, 0x28, 0x34, 0x60, 0x7C,
                                0x58, 0x44, 0x2C, 0x30, 0x14, 0x08, 0x5C, 0x40, 0x64, 0x78,
                                0xCC, 0xD0, 0xF4, 0xE8, 0xBC, 0xA0, 0x84, 0x98, 0xA8, 0xB4,
                                0x90, 0x8C, 0xD8, 0xC4, 0xE0, 0xFC, 0x48, 0x54, 0x70, 0x6C,
                                0x38, 0x24, 0x00, 0x1C, 0x74, 0x68, 0x4C, 0x50, 0x04, 0x18,
                                0x3C, 0x20, 0x94, 0x88, 0xAC, 0xB0, 0xE4, 0xF8, 0xDC, 0xC0,
                                0x0C, 0x10, 0x34, 0x28, 0x7C, 0x60, 0x44, 0x58, 0xEC, 0xF0,
                                0xD4, 0xC8, 0x9C, 0x80, 0xA4, 0xB8, 0xD0, 0xCC, 0xE8, 0xF4,
                                0xA0, 0xBC, 0x98, 0x84, 0x30, 0x2C, 0x08, 0x14, 0x40, 0x5C,
                                0x78, 0x64, 0xFC, 0xE0, 0xC4, 0xD8, 0x8C, 0x90, 0xB4, 0xA8,
                                0x1C, 0x00, 0x24, 0x38, 0x6C, 0x70, 0x54, 0x48, 0x20, 0x3C,
                                0x18, 0x04, 0x50, 0x4C, 0x68, 0x74, 0xC0, 0xDC, 0xF8, 0xE4,
                                0xB0, 0xAC, 0x88, 0x94, 0x58, 0x44, 0x60, 0x7C, 0x28, 0x34,
                                0x10, 0x0C, 0xB8, 0xA4, 0x80, 0x9C, 0xC8, 0xD4, 0xF0, 0xEC,
                                0x84, 0x98, 0xBC, 0xA0, 0xF4, 0xE8, 0xCC, 0xD0, 0x64, 0x78,
                                0x5C, 0x40, 0x14, 0x08, 0x2C, 0x30
                              };

        /// <summary>
        /// Crc11
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] Crc11(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            int i;
            byte crc = 0x00;
            int tableIndex;
            for (i = start; i < length; i++)
            {
                tableIndex = crc ^ (buffer[i] & 0xFF);
                crc = table[tableIndex];
            }
            return new byte[] { crc };
        }

        /// **********************************************************************
        /// Name: CRC-12    x16+x12+x5+1
        /// Poly: 0x80
        /// Init: 0x0000
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x0000
        ///*************************************************************************
        public static byte[] Crc12(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0;// Initial value
            short iQ = 0, iR = 0;
            for (var i = start; i < length; i++)
            {
                // 多项式除法
                // 如果该位为1
                if ((buffer[i] & (0x80 >> iR)) > 0)
                {
                    // 则在余数尾部添1否则添0
                    crc |= 0x01;
                }
                // 如果12位除数中的最高位为1，则够除
                if (crc >= 0x1000)
                {
                    crc ^= 0x180D;
                }
                crc <<= 1;
                iR++;
                if (8 == iR)
                {
                    iR = 0;
                    iQ++;
                }
            }
            // 对后面添加的12个0做处理
            for (var i = 0; i < 12; i++)
            {
                if (crc >= 0x1000)
                {
                    crc ^= 0x180D;
                }
                crc <<= 1;
            }
            crc >>= 1;
            var ret = BitConverter.GetBytes(crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/CCITT    x16+x12+x5+1
        /// Poly: 0x1021
        /// Init: 0x0000
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x0000
        ///*************************************************************************
        public static byte[] Crc13(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (ushort)((crc >> 1) ^ 0x8408);// 0x8408 = reverse 0x1021
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            var ret = BitConverter.GetBytes(crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/CCITT FALSE    x16+x12+x5+1
        /// Poly: 0x1021
        /// Init: 0xFFFF
        /// Refin: false
        /// Refout: false
        /// Xorout: 0x0000
        ///*************************************************************************
        public static byte[] Crc14(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0xFFFF;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= (ushort)(buffer[i] << 8);
                for (var j = 0; j < 8; j++)
                {
                    crc = (crc & 0x8000) > 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
                }
            }
            var ret = BitConverter.GetBytes(crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/DNP    x16+x13+x12+x11+x10+x8+x6+x5+x2+1
        /// Poly: 0x3D65
        /// Init: 0x0000
        /// Refin: true
        /// Refout: true
        /// Xorout: 0xFFFF
        ///*************************************************************************
        public static byte[] Crc15(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (ushort)((crc >> 1) ^ 0xA6BC);// 0xA6BC = reverse 0x3D65
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            var ret = BitConverter.GetBytes((ushort)~crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/IBM    x16+x15+x2+1
        /// Poly: 0x8005
        /// Init: 0x0000
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x0000
        ///*************************************************************************
        public static byte[] Crc16(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);// 0xA001 = reverse 0x8005
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            var ret = BitConverter.GetBytes(crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/MAXIM    x16+x15+x2+1
        /// Poly: 0x8005
        /// Init: 0x0000
        /// Refin: true
        /// Refout: true
        /// Xorout: 0xFFFF
        ///*************************************************************************
        public static byte[] Crc17(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);// 0xA001 = reverse 0x8005
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            var ret = BitConverter.GetBytes((ushort)~crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/MODBUS    x16+x15+x2+1
        /// Poly: 0x8005
        /// Init: 0xFFFF
        /// Refin: true
        /// Refout: true
        /// Xorout: 0x0000
        ///*************************************************************************
        public static byte[] Crc18(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0xFFFF;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);// 0xA001 = reverse 0x8005
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            var ret = BitConverter.GetBytes(crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/USB    x16+x15+x2+1
        /// Poly: 0x8005
        /// Init: 0xFFFF
        /// Refin: true
        /// Refout: true
        /// Xorout: 0xFFFF
        ///*************************************************************************
        public static byte[] Crc19(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0xFFFF;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);// 0xA001 = reverse 0x8005
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            var ret = BitConverter.GetBytes((ushort)~crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/X25    x16+x12+x5+1
        /// Poly: 0x1021
        /// Init: 0xFFFF
        /// Refin: true
        /// Refout: true
        /// Xorout: 0xFFFF
        ///*************************************************************************
        public static byte[] Crc20(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0xFFFF;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (ushort)((crc >> 1) ^ 0x8408);// 0x8408 = reverse 0x1021
                    else
                        crc = (ushort)(crc >> 1);
                }
            }
            var ret = BitConverter.GetBytes((ushort)~crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC-16/XMODEM    x16+x12+x5+1
        /// Poly: 0x1021
        /// Init: 0x0000
        /// Refin: false
        /// Refout: false
        /// Xorout: 0x0000
        ///*************************************************************************
        public static byte[] Crc21(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            ushort crc = 0;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= (ushort)(buffer[i] << 8);
                for (var j = 0; j < 8; j++)
                {
                    crc = (crc & 0x8000) > 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
                }
            }
            var ret = BitConverter.GetBytes(crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC32    x32+x26+x23+x22+x16+x12+x11+x10+x8+x7+x5+x4+x2+x+1
        /// Poly: 0x04C11DB7
        /// Init: 0xFFFFFFFF
        /// Refin: true
        /// Refout: true
        /// Xorout: 0xFFFFFFFF
        ///*************************************************************************
        public static byte[] Crc22(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            var crc = 0xFFFFFFFF;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (var j = 0; j < 8; j++)
                {
                    if ((crc & 1) > 0)
                        crc = (crc >> 1) ^ 0xEDB88320;// 0xEDB88320= reverse 0x04C11DB7
                    else
                        crc = crc >> 1;
                }
            }
            var ret = BitConverter.GetBytes(~crc);
            Array.Reverse(ret);
            return ret;
        }

        /// **********************************************************************
        /// Name: CRC32/MPEG-2    x32+x26+x23+x22+x16+x12+x11+x10+x8+x7+x5+x4+x2+x+1
        /// Poly: 0x04C11DB7
        /// Init: 0xFFFFFFFF
        /// Refin: false
        /// Refout: false
        /// Xorout: 0x00000000
        ///*************************************************************************
        public static byte[] Crc23(byte[] buffer, int start = 0, int len = 0)
        {
            if (buffer == null || buffer.Length == 0) return null;
            if (start < 0) return null;
            if (len == 0) len = buffer.Length - start;
            var length = start + len;
            if (length > buffer.Length) return null;
            var crc = 0xFFFFFFFF;// Initial value
            for (var i = start; i < length; i++)
            {
                crc ^= (uint)(buffer[i] << 24);
                for (var j = 0; j < 8; j++)
                {
                    crc = (crc & 0x80000000) > 0 ? (crc << 1) ^ 0x04C11DB7 : crc << 1;
                }
            }
            var ret = BitConverter.GetBytes(crc);
            Array.Reverse(ret);
            return ret;
        }
    }
}