#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace ThingsGateway.Foundation.Demo;

public partial class MainLayout
{

    private List<NavItem> Navs { get; set; } = new();
    private List<PageTabItem> pageTabItems { get; set; } = new();
    protected override void OnInitialized()
    {
        var dataString =
"""
[
  {
    "Href": "/index",
    "Title": "��ҳ"
  },
  {
    "Title": "Modbus",
    "Children": [
      {
        "Href": "/ModbusRtu",
        "Title": "ModbusRtu"
      },
      {
        "Href": "/ModbusTcp",
        "Title": "ModbusTcp"
      },
      {
        "Href": "/ModbusRtuOverTcp",
        "Title": "ModbusRtuOverTcp"
      },
      {
        "Href": "/ModbusRtuOverUdp",
        "Title": "ModbusRtuOverUdp"
      },
      {
        "Href": "/ModbusUdp",
        "Title": "ModbusUdp"
      },
      {
        "Href": "/ModbusTcpDtu",
        "Title": "ModbusTcpDtu"
      },
      {
        "Href": "/ModbusTcpServer",
        "Title": "ModbusTcpServer"
      },
      {
        "Href": "/ModbusSerialServer",
        "Title": "ModbusSerialServer"
      }
    ]
  },
  {
    "Title": "Siemens",
    "Children": [
      {
        "Href": "/S7_1500",
        "Title": "S7_1500"
      },
      {
        "Href": "/S7_1200",
        "Title": "S7_1200"
      },
      {
        "Href": "/S7_200",
        "Title": "S7_200"
      },
      {
        "Href": "/S7_200SMART",
        "Title": "S7_200SMART"
      },
      {
        "Href": "/S7_300",
        "Title": "S7_400"
      },
      {
        "Href": "/S7_400",
        "Title": "S7_400"
      }
    ]
  },
  {
    "Title": "DLT645",
    "Children": [
      {
        "Href": "/DLT645_2007",
        "Title": "DLT645_2007"
      },
      {
        "Href": "/DLT645_2007OverTcp",
        "Title": "DLT645_2007OverTcp"
      }
    ]
  },
  {
    "Title": "OPCDA",
    "Children": [
      {
        "Href": "/OPCDAClient",
        "Title": "OPCDAClient"
      }
    ]
  },
  {
    "Title": "OPCUA",
    "Children": [
      {
        "Href": "/OPCUAClient",
        "Title": "OPCUAClient"
      }
    ]
  },
  {
    "Title": "Mqtt",
    "Children": [
      {
        "Href": "/MqttClient",
        "Title": "MqttClient"
      }
    ]
  }
]


""";
        Navs = dataString.FromJsonString<List<NavItem>>();

#if Pro
        var dataStringPro =
"""
[
  {
    "Title": "ABCIP",
    "Children": [
      {
        "Href": "/ABCIPTCP",
        "Title": "ABCIPTCP"
      }
    ]
  },
  {
    "Title": "Omron",
    "Children": [
      {
        "Href": "/OmronFinsTcp",
        "Title": "OmronFinsTcp"
      },
      {
        "Href": "/OmronFinsUdp",
        "Title": "OmronFinsUdp"
      }
    ]
  },
  {
    "Title": "Secs",
    "Children": [
      {
        "Href": "/SecsTcp",
        "Title": "SecsTcp"
      }
    ]
  },
  {
    "Title": "TS550",
    "Children": [
      {
        "Href": "/TS550",
        "Title": "TS550"
      }
    ]
  },
  {
    "Title": "Vigor",
    "Children": [
      {
        "Href": "/VigorSerial",
        "Title": "VigorSerial"
      },
      {
        "Href": "/VigorSerialOverTcp",
        "Title": "VigorSerialOverTcp"
      }
    ]
  },
  {
    "Title": "GasCustom",
    "Children": [
      {
        "Href": "/GasCustomSerial",
        "Title": "GasCustomSerial"
      },
      {
        "Href": "/GasCustomSerialOverTcp",
        "Title": "GasCustomSerialOverTcp"
      }
    ]
  }

]


""";
        Navs.AddRange(dataStringPro.FromJsonString<List<NavItem>>());
#endif
        pageTabItems = Navs.PasePageTabItem();
        base.OnInitialized();
    }
}