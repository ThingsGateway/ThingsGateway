//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.SiemensS7;

internal partial class SiemensHelper
{
    // S7变量多读Item
    internal static readonly byte[] S7_MULRD_ITEM = [
        0x12,            // Var 规范.
		0x0a,            // 剩余的字节长度
		0x10,            // Syntax ID
		(byte)S7WordLength.Byte,  // 相关的数据长度代码（注意:根据传入的变量更改）
		0x00,0x01,       // 数据长度						（注意:根据传入的变量更改）
		0x00,0x00,       // DB编号							（注意:根据传入的变量更改）
		0x84,            // 数据块类型					    （注意:根据传入的变量更改）
		0x00,0x00,0x00   // 数据块偏移量			    （注意:根据传入的变量更改）
	];

    /// <summary>
    /// S7连读写请求头(包含ISO头和COTP头)
    /// </summary>
    internal static readonly byte[] S7_MULRW_HEADER = [
        0x03,0x00,
        0x00,0x1f,       // 报文长度(item.len*12+19，注意:根据传入读取item数量更改)
		0x02,0xf0, 0x80, //COTP信息
		0x32,            // S7协议ID
		0x01,            // 类型，请求命令
		0x00,0x00,       // 冗余识别
		0x00,0x01,       // 序列号
		0x00,0x0e,       // parameter长度（item.len*12+2，注意:根据传入读取item数量更改）
		0x00,0x00,       // Data Length+4 ,写入时填写，读取时为0
		0x04,            //  4 Read Var, 5 Write Var  ，注意更改
		0x01,            // Item数量（item.len，注意:根据传入读取item数量更改）
	];

    // ISO连接请求报文(也包含ISO头和COTP头)
    internal static byte[] ISO_CR = [
		// TPKT (RFC1006 Header)
		0x03, // RFC 1006 ID (3)
		0x00, // 保留 0
		0x00, // 数据包长度 (整个框架、有效载荷和TPDU包括在内)
		0x16, // 数据包长度 (整个框架、有效载荷和TPDU包括在内)
		// COTP (ISO 8073 Header)
		0x11, // PDU Size Length
		0xE0, // CR -连接请求ID
		0x00, 0x00, // Dst Reference
		0x00, 0x01, // Src Reference
		0x00, // Class + Options Flags
		//对于S7200/Smart ，下面参数也需要重写
		0xC0, // PDU Max Length ID
		0x01, 0x0A, // PDU Max Length
		0xC1, // Src TSAP Identifier
		0x02, // Src TSAP Length (2 bytes)
		0x01, 0x02, // Src TSAP  (需重写)
		0xC2, // Dst TSAP Identifier
		0x02, // Dst TSAP Length (2 bytes)
		0x01, 0x00  // Dst TSAP  (需重写)
	];

    // ISO连接请求报文(也包含ISO头和COTP头)
    internal static byte[] ISO_CR200 = [
		// TPKT (RFC1006 Header)
		0x03, // RFC 1006 ID (3)
		0x00, // 保留 0
		0x00, // 数据包长度 (整个框架、有效载荷和TPDU包括在内)
		0x16, // 数据包长度 (整个框架、有效载荷和TPDU包括在内)
		// COTP (ISO 8073 Header)
		0x11, // PDU Size Length
		0xE0, // CR -连接请求ID
		0x00, 0x00, // Dst Reference
		0x00, 0x01, // Src Reference
		0x00, // Class + Options Flags

		//对于S7200/Smart
		0xC1,0x02,
        0x4D,0x57, //LOCALTASP
		0xC2,
        0x02,
        0x4D,0x57, //DESTTASP
		0xC0,
        0x01,0x09
        ];

    // ISO连接请求报文(也包含ISO头和COTP头)
    internal static byte[] ISO_CR200SMART = [
		// TPKT (RFC1006 Header)
		0x03, // RFC 1006 ID (3)
		0x00, // 保留 0
		0x00, // 数据包长度 (整个框架、有效载荷和TPDU包括在内)
		0x16, // 数据包长度 (整个框架、有效载荷和TPDU包括在内)
		// COTP (ISO 8073 Header)
		0x11, // PDU Size Length
		0xE0, // CR -连接请求ID
		0x00, 0x00, // Dst Reference
		0x00, 0x01, // Src Reference
		0x00, // Class + Options Flags

		//对于S7200/Smart
		0xC1,0x02,
        0x10,0x00,//LOCALTASP
		0xC2,
        0x02,
        0x03,0x00,//DESTTASP
		0xC0,
        0x01,0x0A
        ];

    // PDU获取报文(也包含ISO头和COTP头)
    internal static byte[] S7_PN = [
            0x03, 0x00, 0x00, 0x19,
            0x02, 0xf0, 0x80, // TPKT + COTP
			0x32, 0x01, 0x00, 0x00,
			//这里对于S7200/Smart需要重写
            0x04, 0x00, 0x00, 0x08,
            0x00, 0x00, 0xf0, 0x00,
            0x00, 0x01, 0x00, 0x01,
            0x01,0xE0        // PDU Length Requested  这里默认480字节，对于S7200/Smart 960字节
	];

    // PDU获取报文(也包含ISO头和COTP头)
    internal static byte[] S7200_PN = [
            0x03, 0x00, 0x00, 0x19,
            0x02, 0xf0, 0x80, // TPKT + COTP
			0x32, 0x01, 0x00, 0x00,
            0x00,0x00,0x00,0x08,
            0x00, 0x00, 0xf0, 0x00,
            0x00, 0x01, 0x00, 0x01,
            0x01,0xE0        // PDU Length Requested  这里默认960字节
	];

    internal static byte[] S7200SMART_PN = [
            0x03, 0x00, 0x00, 0x19,
            0x02, 0xf0, 0x80, // TPKT + COTP
			0x32, 0x01, 0x00, 0x00,
            0xCC,0xC1,0x00,0x08,
            0x00, 0x00, 0xf0, 0x00,
            0x00, 0x01, 0x00, 0x01,
            0x01,0xE0        // PDU Length Requested  这里默认960字节
	];
}
