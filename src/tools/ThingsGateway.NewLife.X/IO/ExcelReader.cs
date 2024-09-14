﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace ThingsGateway.NewLife.X.IO;

/// <summary>轻量级Excel读取器，仅用于导入数据</summary>
/// <remarks>
/// 文档 https://newlifex.com/core/excel_reader
/// 仅支持xlsx格式，本质上是压缩包，内部xml。
/// 可根据xml格式扩展读取自己想要的内容。
/// </remarks>
public class ExcelReader : DisposeBase
{
    #region 属性

    private IDictionary<String, ZipArchiveEntry>? _entries;

    private String[]? _sharedStrings;

    private String?[]? _styles;

    private ZipArchive _zip;

    /// <summary>文件名</summary>
    public String? FileName { get; }

    /// <summary>工作表</summary>
    public ICollection<String>? Sheets => _entries?.Keys;

    #endregion 属性

    #region 构造

    /// <summary>实例化读取器</summary>
    /// <param name="fileName"></param>
    public ExcelReader(String fileName)
    {
        if (fileName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(fileName));

        FileName = fileName;

        //_zip = ZipFile.OpenRead(fileName.GetFullPath());
        // 共享访问，避免文件被其它进程打开时再次访问抛出异常
        var fs = new FileStream(fileName.GetFullPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _zip = new ZipArchive(fs, ZipArchiveMode.Read, true);

        Parse();
    }

    /// <summary>实例化读取器</summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    public ExcelReader(Stream stream, Encoding encoding)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        if (stream is FileStream fs) FileName = fs.Name;

        _zip = new ZipArchive(stream, ZipArchiveMode.Read, true, encoding);

        Parse();
    }

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _entries?.Clear();
        _zip.TryDispose();
    }

    #endregion 构造

    #region 方法

    private static DateTime _1900 = new(1900, 1, 1);

    /// <summary>逐行读取数据，第一行很可能是表头</summary>
    /// <param name="sheet">工作表名。一般是sheet1/sheet2/sheet3，默认空，使用第一个数据表</param>
    /// <returns></returns>
    public IEnumerable<Object?[]> ReadRows(String? sheet = null)
    {
        if (Sheets == null || _entries == null) yield break;

        if (sheet.IsNullOrEmpty()) sheet = Sheets.FirstOrDefault();
        if (sheet.IsNullOrEmpty()) throw new ArgumentNullException(nameof(sheet));

        if (!_entries.TryGetValue(sheet, out var entry)) throw new ArgumentOutOfRangeException(nameof(sheet), "Unable to find worksheet");

        var doc = XDocument.Load(entry.Open());
        if (doc.Root == null) yield break;

        var data = doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName.EqualIgnoreCase("sheetData"));
        if (data == null) yield break;

        // 加快样式判断速度
        var styles = _styles;
        if (styles != null && styles.Length == 0) styles = null;

        foreach (var row in data.Elements())
        {
            var vs = new List<String?>();
            var c = 'A';
            foreach (var col in row.Elements())
            {
                // 值
                var val = col.Value;

                // 某些列没有数据，被跳过。r=CellReference
                var r = col.Attribute("r");
                if (r != null)
                {
                    // 按最后一个字母递增，最多支持25个空列
                    var c2 = r.Value.Last(Char.IsLetter);
                    while (c2 != c)
                    {
                        vs.Add(null);
                        if (c == 'Z')
                            c = 'A';
                        else
                            c++;
                    }
                }

                // t=DataType, s=SharedString, b=Boolean, n=Number, d=Date
                var t = col.Attribute("t");
                if (t != null && t.Value == "s")
                {
                    val = _sharedStrings?[val.ToInt()];
                }
                else if (styles != null)
                {
                    // 特殊支持时间日期，s=StyleIndex
                    var s = col.Attribute("s");
                    if (s != null)
                    {
                        var si = s.Value.ToInt();
                        if (si < styles.Length)
                        {
                            var st = styles[si];
                            if (st != null && st.StartsWith("yy"))
                            {
                                if (val.Contains('.'))
                                {
                                    var ss = val.Split('.');
                                    var dt = _1900.AddDays(ss[0].ToInt() - 2);
                                    dt = dt.AddSeconds(ss[1].ToLong() / 115740);
                                    val = dt.ToFullString();
                                }
                                else
                                {
                                    val = _1900.AddDays(val.ToInt() - 2).ToString("yyyy-MM-dd");
                                }
                            }
                        }
                        else
                        {
                            foreach (var colElement in col.Elements())
                            {
                                if (colElement.Name.LocalName.Equals("v"))
                                {
                                    val = colElement.Value;
                                }
                            }
                        }
                    }
                }

                vs.Add(val);

                // 循环判断，用最简单的办法兼容超过26列的表格
                if (c == 'Z')
                    c = 'A';
                else
                    c++;
            }

            yield return vs.ToArray();
        }
    }

    private void Parse()
    {
        // 读取共享字符串
        {
            var entry = _zip.GetEntry("xl/sharedStrings.xml");
            if (entry != null) _sharedStrings = ReadStrings(entry.Open());
        }

        // 读取样式
        {
            var entry = _zip.GetEntry("xl/styles.xml");
            if (entry != null) _styles = ReadStyles(entry.Open());
        }

        // 读取sheet
        {
            _entries = ReadSheets(_zip);
        }
    }

    private IDictionary<String, ZipArchiveEntry> ReadSheets(ZipArchive zip)
    {
        var dic = new Dictionary<String, String?>();

        var entry = _zip.GetEntry("xl/workbook.xml");
        if (entry != null)
        {
            var doc = XDocument.Load(entry.Open());
            if (doc?.Root != null)
            {
                //var list = new List<String>();
                var sheets = doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "sheets");
                if (sheets != null)
                {
                    foreach (var item in sheets.Elements())
                    {
                        var id = item.Attribute("sheetId");
                        var name = item.Attribute("name");
                        if (id != null) dic[id.Value] = name?.Value;
                    }
                }
            }
        }

        //_entries = _zip.Entries.Where(e =>
        //    e.FullName.StartsWithIgnoreCase("xl/worksheets/") &&
        //    e.Name.EndsWithIgnoreCase(".xml"))
        //    .ToDictionary(e => e.Name.TrimEnd(".xml"), e => e);

        var dic2 = new Dictionary<String, ZipArchiveEntry>();
        foreach (var item in zip.Entries)
        {
            if (item.FullName.StartsWithIgnoreCase("xl/worksheets/") && item.Name.EndsWithIgnoreCase(".xml"))
            {
                var name = item.Name.TrimEnd(".xml");
                if (dic.TryGetValue(name.TrimStart("sheet"), out var str)) name = str;
                name ??= String.Empty;

                dic2[name] = item;
            }
        }

        return dic2;
    }

    private String[]? ReadStrings(Stream ms)
    {
        var doc = XDocument.Load(ms);
        if (doc?.Root == null) return null;

        var list = new List<String>();
        foreach (var item in doc.Root.Elements())
        {
            list.Add(item.Value);
        }

        return list.ToArray();
    }

    private String?[]? ReadStyles(Stream ms)
    {
        var doc = XDocument.Load(ms);
        if (doc?.Root == null) return null;

        var fmts = new Dictionary<Int32, String?>();
        var numFmts = doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "numFmts");
        if (numFmts != null)
        {
            foreach (var item in numFmts.Elements())
            {
                var id = item.Attribute("numFmtId");
                var code = item.Attribute("formatCode");
                if (id != null) fmts.Add(id.Value.ToInt(), code?.Value);
            }
        }

        var list = new List<String?>();
        var xfs = doc.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "cellXfs");
        if (xfs != null)
        {
            foreach (var item in xfs.Elements())
            {
                var fid = item.Attribute("numFmtId");
                if (fid != null && fmts.TryGetValue(fid.Value.ToInt(), out var code))
                    list.Add(code);
                else
                    list.Add(null);
            }
        }

        return list.ToArray();
    }

    #endregion 方法
}
