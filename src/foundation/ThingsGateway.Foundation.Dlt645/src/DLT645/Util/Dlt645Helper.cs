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

using System.Text;

using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// 解析参数
/// </summary>
internal class DataInfo
{
    /// <summary>
    /// 解析长度
    /// </summary>
    public int ByteLength { get; set; }

    /// <summary>
    /// 小数位
    /// </summary>
    public int Digtal { get; set; }

    /// <summary>
    /// 有符号解析
    /// </summary>
    public bool IsSigned { get; set; }
}

internal static class Dlt645Helper
{
    internal static string Get2007ErrorMessage(byte buffer)
    {
        string error = buffer switch
        {
            0x80 => "其他错误",
            0x40 => "费率数超",
            0x20 => "日时段数超",
            0x10 => "年时区数超",
            0x08 => "通信速率不能更改",
            0x04 => "密码错/未授权",
            0x02 => "无请求数据",
            _ => "其他错误",
        };
        return error;
    }

    /// <summary>
    /// 获取返回的解析信息
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    internal static List<DataInfo> GetDataInfos(this byte[] buffer)
    {
        //详细见文档，由GPT转换C#代码而来（手动验证并注释）
        //在dlt645协议中，已规定每个数据标识对应的返回字节数量，并且个别数据需要转换正负符合以及ASCII码
        List<DataInfo> dataInfos = new();
        switch (buffer[3])
        {
            case 0://电能量数据标识
                switch (buffer[2])
                {
                    case 0:     //组合有功总电能
                    case 3:     //组合无功1总电能
                    case 4:     //组合无功2总电能
                        dataInfos.AddRange(new DataInfo[]
                        {
                            new(){ ByteLength=4,Digtal=2,IsSigned=true},
                        });
                        break;

                    default:
                        //正向有功总电能
                        //反向有功总电能
                        dataInfos.AddRange(new DataInfo[]
                        {
                            new(){ ByteLength=4,Digtal=2,IsSigned=false},
                        });
                        break;
                }

                break;

            case 1://最大需量及发生时间数据标识
                switch (buffer[2])
                {
                    case 3:     //组合无功1
                    case 4:     //组合无功2
                        dataInfos.AddRange(new DataInfo[]
                        {
                            new(){ ByteLength=3,Digtal=5,IsSigned=true},
                            new(){ ByteLength=5,Digtal=0,IsSigned=false},
                        });
                        break;

                    default:
                        dataInfos.AddRange(new DataInfo[]
                        {
                           new(){ ByteLength=3,Digtal=5,IsSigned=false},
                           new(){ ByteLength=5,Digtal=0,IsSigned=false},
                        });
                        break;
                }

                break;

            case 2://变量数据返回
                switch (buffer[2])
                {
                    case 1://电压

                        dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=2,Digtal=1,IsSigned=false},
});

                        break;

                    case 2://电流
                        dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=3,IsSigned=true},
});
                        break;

                    case < 6:
                        //瞬时总有功功率
                        //瞬时A相有功功率
                        //瞬时B相有功功率
                        //瞬时C相有功功率
                        //瞬时有功功率数据块
                        //瞬时总无功功率
                        //瞬时A相无功功率
                        //瞬时B相无功功率
                        //瞬时C相无功功率
                        //瞬时无功功率数据块
                        //瞬时总视在功率
                        //瞬时A相视在功率
                        //瞬时B相视在功率
                        //瞬时C相视在功率
                        //瞬时视在功率数据块
                        dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=4,IsSigned=true},
});

                        break;

                    case 6://功率因数
                        dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=2,Digtal=3,IsSigned=true},
});
                        break;

                    case 7://相角

                        dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=2,Digtal=1,IsSigned=false},
});

                        break;

                    case < 0x80:
                        {
                            //A相电压波形失真度
                            //B相电压波形失真度
                            //C相电压波形失真度
                            //电压波形失真度数据块
                            //A相电流波形失真度
                            //B相电流波形失真度
                            //C相电流波形失真度
                            //电流波形失真度数据块
                            //A相电压1次谐波含量
                            //…
                            //A相电压21次谐波含量
                            //A相电压谐波含量数据块
                            //B相电压1次谐波含量
                            //…
                            //B相电压21次谐波含量
                            //B相电压谐波含量数据块
                            //C相电压1次谐波含量
                            //…
                            //C相电压21次谐波含量
                            //C相电压谐波含量数据块
                            //A相电流1次谐波含量
                            //…
                            //A相电流21次谐波含量
                            //A相电流谐波含量数据块
                            //B相电流1次谐波含量
                            //…
                            //B相电流21次谐波含量
                            //B相电流谐波含量数据块
                            //C相电流1次谐波含量
                            //…
                            //C相电流21次谐波含量
                            //C相电流谐波含量数据块
                        }

                        dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=2,Digtal=2,IsSigned=false},
});

                        break;

                    case 0x80:
                        switch (buffer[0])
                        {
                            case 1://零线电流
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=3,IsSigned=true},
});
                                break;

                            case 2://电网频率
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=2,Digtal=2,IsSigned=false},
});
                                break;

                            case 3://一分钟有功总平均功率
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=4,IsSigned=false},
});
                                break;

                            case 4://当前有功需量
                            case 5://当前无功需量
                            case 6://当前视在需量
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=4,IsSigned=true},
});
                                break;

                            case 7://表内温度
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=2,Digtal=1,IsSigned=false},
});
                                break;

                            case 8://时钟电池电压(内部)
                            case 9://停电抄表电池电压(外部)
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=2,Digtal=2,IsSigned=false},
});
                                break;

                            case 10://内部电池工作时间
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=4,Digtal=0,IsSigned=false},
});
                                break;

                            case 11://当前阶梯电价
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=4,Digtal=4,IsSigned=false},
});
                                break;
                        }
                        break;

                    default:
                        break;
                }
                break;

            case 3://事件记录数据标识
                switch (buffer[2])
                {
                    case < 5://ABC数据+累计时间
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=0,IsSigned=false},
});
                            }

                            break;
                        }
                        else
                        {
                            dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=6,Digtal=0},
                            new(){ ByteLength=6,Digtal=0},

                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2 },
                            new DataInfo() { ByteLength = 2, Digtal = 1 },
                            new DataInfo() { ByteLength = 3, Digtal = 3 },
                            new DataInfo() { ByteLength = 3, Digtal = 4},
                            new DataInfo() { ByteLength = 3, Digtal = 4},
                            new DataInfo() { ByteLength = 2, Digtal = 3 },
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 2, Digtal = 1 },
                            new DataInfo() { ByteLength = 3, Digtal = 3 },
                            new DataInfo() { ByteLength = 3, Digtal = 4 },
                            new DataInfo() { ByteLength = 3, Digtal = 4 },
                            new DataInfo() { ByteLength = 2, Digtal = 3 },
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 2, Digtal = 1 },
                            new DataInfo() { ByteLength = 3, Digtal = 3 },
                            new DataInfo() { ByteLength = 3, Digtal = 4 },
                            new DataInfo() { ByteLength = 3, Digtal = 4 },
                            new DataInfo() { ByteLength = 2, Digtal = 3 },
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
                            new DataInfo() { ByteLength = 4, Digtal = 2},
});
                            break;
                        }

                    case 5://全失压总次数，总累计时间
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=0,IsSigned=false},
                           new(){ ByteLength=3,Digtal=0,IsSigned=false},
});
                            break;
                        }
                        else if (buffer[1] == 0)
                        {
                            dataInfos.AddRange(new DataInfo[]
{
                             new(){ ByteLength=6,Digtal=0},
                             new DataInfo(){ByteLength=3, Digtal=3},
                             new DataInfo(){ByteLength=6, Digtal=0},
});

                            break;
                        }

                        break;

                    case 6://辅助电源失电总次数，总累计时间

                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=0,IsSigned=false},
                           new(){ ByteLength=3,Digtal=0,IsSigned=false},
});

                            break;
                        }
                        else if (buffer[1] == 0)
                        {
                            dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=6,Digtal=0,IsSigned=false},
                           new(){ ByteLength=6,Digtal=0,IsSigned=false},
});
                            break;
                        }

                        break;

                    case 7://电压逆相序总次数，总累计时间
                    case 8://电流逆相序总次数，总累计时间
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new DataInfo[]
{
                           new(){ ByteLength=3,Digtal=0,IsSigned=false},
                           new(){ ByteLength=3,Digtal=0,IsSigned=false},
});

                            break;
                        }
                        else if (buffer[1] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=6, Digtal=0},
                            });
                            for (int i = 0; i < 16; i++)
                            {
                                dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=4, Digtal=2},
                            });
                            }
                            break;
                        }

                        break;

                    case 9://电压不平衡总次数，总累计时间
                    case 0x0A://电流不平衡总次数，总累计时间
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                            });
                            break;
                        }
                        else if (buffer[1] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=2, Digtal=2},
                                new DataInfo(){ByteLength=2, Digtal=2},
                            });
                            for (int i = 0; i < 16; i++)
                            {
                                dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=4, Digtal=2},
                            });
                            }
                            break;
                        }

                        break;

                    case 0x0B:
                    case 0x0C:
                    case 0x0D:
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                            });
                            break;
                        }
                        else
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=2, Digtal=1},
                                new DataInfo(){ByteLength=3, Digtal=3},
                                new DataInfo(){ByteLength=3, Digtal=4},
                                new DataInfo(){ByteLength=3, Digtal=4},
                                new DataInfo(){ByteLength=2, Digtal=3},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=2, Digtal=1},
                                new DataInfo(){ByteLength=3, Digtal=3},
                                new DataInfo(){ByteLength=3, Digtal=4},
                                new DataInfo(){ByteLength=3, Digtal=4},
                                new DataInfo(){ByteLength=2, Digtal=3},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=4, Digtal=2},
                                new DataInfo(){ByteLength=2, Digtal=1},
                                new DataInfo(){ByteLength=3, Digtal=3},
                                new DataInfo(){ByteLength=3, Digtal=4},
                                new DataInfo(){ByteLength=3, Digtal=4},
                                new DataInfo(){ByteLength=2, Digtal=3},
                            });
                            break;
                        }

                    case 0x0E:
                    case 0x0F:
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                            });
                            break;
                        }
                        else
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=6, Digtal=0},
                            });
                            for (int i = 0; i < 16; i++)
                            {
                                dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=4, Digtal=2},
                            });
                            }
                            break;
                        }

                    case 0x10:
                        dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=2},
                                new DataInfo(){ByteLength=3, Digtal=2},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=2, Digtal=1},
                                new DataInfo(){ByteLength=4, Digtal=0},
                                new DataInfo(){ByteLength=2, Digtal=1},
                                new DataInfo(){ByteLength=4, Digtal=0},
                            });
                        break;

                    case 0x11:
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                            });
                            break;
                        }
                        else if (buffer[1] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=6, Digtal=0},
                            });
                            break;
                        }
                        break;

                    case 0x12:
                        if (buffer[1] == 0 && buffer[0] == 0)
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=0},
                            });
                            break;
                        }
                        else
                        {
                            dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=3, Digtal=4},
                                new DataInfo(){ByteLength=5, Digtal=0},
                            });
                            break;
                        }

                    case 0x30:

                        switch (buffer[1])
                        {
                            case 0://编程记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                            });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=4, Digtal=0},
                            });
                                    for (int i = 0; i < 10; i++)
                                    {
                                        dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=4, Digtal=0},
                            });
                                    }
                                }

                                break;

                            case 1://电表清零记录

                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=3, Digtal=0},
                            });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=6, Digtal=0},
                                new DataInfo(){ByteLength=4, Digtal=0},
                            });
                                    for (int i = 0; i < 24; i++)
                                    {
                                        dataInfos.AddRange(new List<DataInfo>()
                            {
                                new DataInfo(){ByteLength=4, Digtal=2},
                            });
                                    }
                                }
                                break;

                            case 2://编程记录

                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 }
                                    });

                                    for (int i = 0; i < 24; i++)
                                    {
                                        dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 4 },
                                        new DataInfo { ByteLength = 5, Digtal = 0 }
                                    });
                                    }
                                }
                                break;

                            case 3://事件清零记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 }
                                    });
                                }
                                break;

                            case 4://校时记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 6, Digtal = 0 }
                                    });
                                }
                                break;

                            case 5://时段表编程记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                    });
                                    for (int i = 0; i < 14; i++)
                                    {
                                        dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 },
                                    });
                                    }
                                }
                                break;

                            case 6://时区表编程记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                    });
                                    for (int i = 0; i < 28; i++)
                                    {
                                        dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 },
                                    });
                                    }
                                }
                                break;

                            case 7://周休日编程记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                        new DataInfo { ByteLength = 1, Digtal = 0 },
                                    });
                                }
                                break;

                            case 8://节假日编程记录

                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                    });
                                    for (int i = 0; i < 255; i++)
                                    {
                                        dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                    });
                                    }
                                }
                                break;

                            case 9:
                            case 10:
                            case 11://有功组合方式编程记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                        new DataInfo { ByteLength = 1, Digtal = 0 },
                                    });
                                }
                                break;

                            case 12://结算日编程记录
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 4, Digtal = 0 },
                                        new DataInfo { ByteLength = 2, Digtal = 0 },
                                        new DataInfo { ByteLength = 2, Digtal = 0 },
                                        new DataInfo { ByteLength = 2, Digtal = 0 },
                                    });
                                }
                                break;

                            case 13:
                            case 14:
                                if (buffer[0] == 0)
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 3, Digtal = 0 }
                                    });
                                }
                                else
                                {
                                    dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                        new DataInfo { ByteLength = 6, Digtal = 0 },
                                    });
                                    for (int i = 0; i < 12; i++)
                                    {
                                        dataInfos.AddRange(new List<DataInfo>()
                                    {
                                        new DataInfo { ByteLength = 4, Digtal = 2 },
                                    });
                                    }
                                }
                                break;
                        }
                        break;
                }
                break;

            case 4:

                switch (buffer[2])
                {
                    case 0:
                        switch (buffer[1])
                        {
                            case 1:
                                switch (buffer[0])
                                {
                                    case 1:
                                        dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=4,Digtal=0,IsSigned=false},
});
                                        break;

                                    case 2:
                                        dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=3,Digtal=0,IsSigned=false},
});
                                        break;

                                    case 3:
                                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 1, Digtal = 0, IsSigned = false } });
                                        break;

                                    case 4:
                                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 1, Digtal = 0, IsSigned = false } });
                                        break;

                                    case 5:
                                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 2, Digtal = 0, IsSigned = false } });
                                        break;

                                    case 6:
                                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 5, Digtal = 0, IsSigned = false } });
                                        break;

                                    case 7:
                                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 5, Digtal = 0, IsSigned = false } });
                                        break;

                                    default:
                                        break;
                                }
                                break;

                            case 2:
                                switch (buffer[0])
                                {
                                    case 5:
                                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 2, Digtal = 0, IsSigned = false } });
                                        break;

                                    default:
                                        break;
                                }
                                break;

                            case 3:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 1, Digtal = 0, IsSigned = false } });
                                break;

                            case 4:
                                if (buffer[0] <= 2)
                                    dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=6,Digtal=0,IsSigned=false},
});
                                else if (buffer[0] == 3)
                                    dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=32,Digtal=-1,IsSigned=false},
});
                                else if (buffer[0] <= 6)
                                    dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 6, Digtal = -1, IsSigned = false } });
                                else if (buffer[0] <= 8)
                                    dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 4, Digtal = -1, IsSigned = false } });
                                else if (buffer[0] <= 10)
                                    dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 3, Digtal = 0, IsSigned = false } });
                                else if (buffer[0] <= 12)
                                    dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 10, Digtal = -1, IsSigned = false } });
                                else if (buffer[0] == 13)
                                    dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 16, Digtal = -1, IsSigned = false } });
                                break;

                            case 5:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 2, Digtal = 0, IsSigned = false } });
                                break;

                            case 6:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 1, Digtal = 0, IsSigned = false } });
                                break;

                            case 7:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 1, Digtal = 0, IsSigned = false } });
                                break;

                            case 8:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 1, Digtal = 0, IsSigned = false } });
                                break;

                            case 9:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 1, Digtal = 0, IsSigned = false } });
                                break;

                            case 10:
                                switch (buffer[0])
                                {
                                    case 1:
                                        dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=4,Digtal=0,IsSigned=false},
});
                                        break;

                                    default:
                                        dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=2,Digtal=0,IsSigned=false},
});
                                        break;
                                }
                                break;

                            case 11:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 2, Digtal = 0, IsSigned = false } });
                                break;

                            case 12:
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 4, Digtal = 0, IsSigned = false } });
                                break;

                            case 13:
                                dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=2,Digtal=3,IsSigned=false},
});
                                break;

                            case 14:
                                if (buffer[0] < 3)
                                    dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=3,Digtal=4,IsSigned=false},
});
                                else
                                    dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=2,Digtal=1,IsSigned=false},
});
                                break;
                        }
                        break;

                    case 1:
                    case 2:
                        if (buffer[1] == 0)
                        {
                            for (int i = 0; i < 14; i++)
                            {
                                dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 3, Digtal = 0, IsSigned = false } });
                            }
                        }
                        break;

                    case 3:
                    case 4:
                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 4, Digtal = 0, IsSigned = false } });
                        break;

                    case 0x80:
                        dataInfos.AddRange(new DataInfo[] { new() { ByteLength = 32, Digtal = -1, IsSigned = false } });
                        break;
                }

                break;

            case 5:
                switch (buffer[2])
                {
                    case 0:
                        switch (buffer[1])
                        {
                            case 0:
                                switch (buffer[0])
                                {
                                    case 1:
                                        dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=5,Digtal=0,IsSigned=false},
});
                                        break;
                                }
                                break;

                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=4,Digtal=2,IsSigned=false},
});
                                break;

                            case 9:
                            case 10:
                                dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=3,Digtal=4,IsSigned=false},
                            new(){ ByteLength=5,Digtal=0,IsSigned=false},
});
                                break;

                            case 16:
                                for (int i = 0; i < 8; i++)
                                {
                                    dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=3,Digtal=4,IsSigned=false},
});
                                }
                                break;
                        }
                        break;
                }
                break;

            case 6:
                switch (buffer[1])
                {
                    case 0:
                        switch (buffer[0])
                        {
                            case 0:
                                dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=1,Digtal=0,IsSigned=false},
});
                                break;

                            case 1:
                                dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=6,Digtal=0,IsSigned=false},
});
                                break;
                        }
                        break;

                    default:
                        if (buffer[0] == 2)
                            dataInfos.AddRange(new DataInfo[]
{
                            new(){ ByteLength=1,Digtal=0,IsSigned=false},
});
                        break;
                }

                break;
        }
        return dataInfos;
    }

    /// <summary>
    /// 获取Dlt645报文
    /// </summary>
    internal static byte[] GetDlt645_2007Command(
      string address,
      byte control,
      string defaultStation,
      byte[] codes = null,
      string[] datas = null
        )
    {
        codes ??= new byte[0];
        datas ??= new string[0];
        var operResult = Dlt645_2007AddressHelper.ParseFrom(address);
        var buffer = operResult.DataId;
        if (buffer.Length > 0)
        {
            var dataInfos = GetDataInfos(buffer);
            List<byte> buffers = new();
            if (datas.Length > 0)
            {
                if (datas.Length != dataInfos.Count)
                {
                    throw new("写入参数数量不符合地址要求");
                }

                for (int i = 0; i < datas.Length; i++)
                {
                    var dataInfo = dataInfos[i];
                    var data = datas[i];
                    byte[] buffer1;
                    if (dataInfo.IsSigned)//可能为负数
                    {
                        var doubleValue = Convert.ToDouble(data);
                        if (dataInfo.Digtal == 0)//无小数点
                        {
                        }
                        else
                        {
                            doubleValue *= Math.Pow(10.0, dataInfo.Digtal);
                        }
                        buffer1 = doubleValue.ToString().ByHexStringToBytes().Reverse().ToArray();
                        if (doubleValue < 0)
                        {
                            buffer1[0] = (byte)(buffer1[0] & 0x80);
                        }
                    }
                    else
                    {
                        if (dataInfo.Digtal < 0)
                        {
                            buffer1 = Encoding.ASCII.GetBytes(data).Reverse().ToArray();
                        }
                        else if (dataInfo.Digtal == 0)//无小数点
                        {
                            buffer1 = data.ByHexStringToBytes().Reverse().ToArray();
                        }
                        else
                        {
                            buffer1 = (Convert.ToDouble(data) * Math.Pow(10.0, dataInfo.Digtal)).ToString().ByHexStringToBytes().Reverse().ToArray();
                        }
                    }

                    buffers.AddRange(buffer1);
                }
            }

            buffer = buffer.SpliceArray(codes, buffers.ToArray());
        }

        byte[] stationBytes;
        if (operResult.Station.Length == 0)
        {
            if (defaultStation.IsNullOrEmpty()) defaultStation = string.Empty;
            if (defaultStation.Length < 12)
                defaultStation = defaultStation.PadLeft(12, '0');
            stationBytes = defaultStation.ByHexStringToBytes().Reverse().ToArray();
        }
        else
        {
            stationBytes = operResult.Station;
        }

        return GetDlt645_2007Command(control, buffer, stationBytes);
    }

    internal static byte[] GetDlt645_2007Command(byte control, byte[] buffer, byte[] stationBytes)
    {
        buffer ??= new byte[0];
        byte[] array = new byte[12 + buffer.Length];
        array[0] = 0x68;//帧起始符
        stationBytes.CopyTo(array, 1);//6个字节地址域
        array[7] = 0x68;//帧起始符
        array[8] = control;//控制码
        array[9] = (byte)buffer.Length;//数据域长度
        if (buffer.Length != 0)//数据域标识DI3、DI2、DI1、DI0
        {
            buffer.CopyTo(array, 10);
            for (int index = 0; index < buffer.Length; ++index)
                array[index + 10] += 0x33;
            //传输时发送方按字节进行加33H处理，接收方按字节进行减33H处理
        }
        int num = 0;
        for (int index = 0; index < array.Length - 2; ++index)
            num += array[index];
        array[array.Length - 2] = (byte)num;//校验码,总加和
        array[array.Length - 1] = 0x16;//	结束符
        return array;
    }
}