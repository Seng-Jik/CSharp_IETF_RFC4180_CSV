#nullable enable

using System.Text;

namespace IETF_RFC4180_CSV
{
    /// <summary>
    /// CSV写入器
    /// 其中CSV格式遵守IETF RFC 4180中所描述的格式，同时也允许修改分隔符以写入TSV和DSV
    /// https://www.ietf.org/rfc/rfc4180.txt
    /// </summary>
    public class CSVWriter
    {
        /// <summary>
        /// 写入csv文件时所使用的分隔符，修改此值以写入tsv、dsv等文件
        /// 你需要自己保证每一条记录的字段数相同
        /// </summary>
        /// <value>分隔符</value>
        public char Separator = ',';

        /// <summary>
        /// 是否强制每个字段均被双引号包裹
        /// </summary>
        public bool AlwaysWrap = false;

        /// <summary>
        /// 写入csv时所使用的换行符
        /// </summary>
        public string NewLine = System.Environment.NewLine;

        /// <summary>
        /// 在当前记录写入字段
        /// </summary>
        /// <param name="field">字段值</param>
        public void WriteField(string field)
        {
            if (!isFirstFieldOfCurrentLine)
                sb.Append(Separator);
            isFirstFieldOfCurrentLine = false;

            bool wrap =
                AlwaysWrap ||
                field.Contains(Separator) ||
                field.Contains('\"') ||
                field.Contains('\n') ||
                field.Contains('\r') ||
                field.Contains(',') ||
                field.Contains('\t');

            if (wrap) sb.Append('\"');
            sb.Append(wrap ? field.Replace("\"", "\"\"") : field);
            if (wrap) sb.Append('\"');
        }

        /// <summary>
        /// 切换到下一条记录
        /// </summary>
        public void NextRecord()
        {
            isFirstFieldOfCurrentLine = true;
            sb.Append(NewLine);
        }

        /// <summary>
        /// 导出当前的csv
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return sb.ToString();
        }

        /// <summary>
        /// 清空已经写入的csv
        /// </summary>
        public void Clear()
        {
            sb.Clear();
            isFirstFieldOfCurrentLine = true;
        }

        readonly StringBuilder sb = new();
        bool isFirstFieldOfCurrentLine = true;
    }
}
