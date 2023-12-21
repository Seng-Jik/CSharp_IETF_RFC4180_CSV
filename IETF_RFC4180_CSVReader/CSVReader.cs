#nullable enable

using System.Text;

namespace IETF_RFC4180_CSV
{
    /// <summary>
    /// CSV阅读器
    /// 其中CSV格式遵守IETF RFC 4180中所描述的格式，同时也允许修改分隔符以读取TSV和DSV
    /// https://www.ietf.org/rfc/rfc4180.txt
    /// </summary>
    public class CSVReader : IDisposable
    {
        /// <summary>
        /// 解析csv文件时所使用的分隔符，修改此值以解析tsv、dsv等文件
        /// </summary>
        /// <value>分隔符</value>
        public char Separator { get; set; } = ',';

        /// <summary>
        /// 从读取csv文件的TextReader构造IETF_RFC4180_CSVReader
        /// </summary>
        /// <param name="csv">读取csv文件的TextReader</param>
        public CSVReader(TextReader csv)
        { this.csv = csv; }

        /// <summary>
        /// 从csv的字符串中构造IETF_RFC4180_CSVReader
        /// </summary>
        /// <param name="csv">csv字符串</param>
        /// <param name="separator">分隔符</param>
        public CSVReader(string csv)
        { this.csv = new StringReader(csv); }

        public void Dispose() => csv.Dispose();

        /// <summary>
        /// 从当前行弹出一个字段
        /// </summary>
        /// <returns>当前字段，若不能弹出字段则返回null</returns>
        public string? PopField()
        {
            if (noMoreFields) return null;

            bool isQuoted = csv.Peek() == '\"';
            if (isQuoted) csv.Read();

            while (true)
            {
                var peek = csv.Peek();

                if (isQuoted)
                {
                    if (peek == -1)
                        throw new InvalidDataException("字段双引号未闭合");

                    if (peek == '\"')
                    {
                        csv.Read();
                        var afterDoubleQuote = csv.Peek();

                        if (afterDoubleQuote == Separator ||
                            afterDoubleQuote == -1 ||
                            afterDoubleQuote == '\r' ||
                            afterDoubleQuote == '\n')
                        {
                            isQuoted = false;
                            break;
                        }
                        else if (afterDoubleQuote == '\"')
                            csv.Read();
                        else
                        {
                            csv.Read();
                            throw new InvalidDataException(
                                $"在字段闭合后发现意外字符{(char)afterDoubleQuote}");
                        }
                    }
                    else csv.Read();
                }
                else if (
                    peek == Separator ||
                    peek == '\n' ||
                    peek == '\r' ||
                    peek == -1)
                    break;
                else csv.Read();

                fieldSb.Append((char)peek);
            }

            var field = fieldSb.ToString();
            fieldSb.Clear();

            var peek2 = csv.Peek();
            if (peek2 == '\n' || peek2 == '\r' || peek2 == -1)
                noMoreFields = true;
            else if (peek2 == Separator)
                csv.Read();
            else throw new InvalidProgramException();

            return field;
        }

        /// <summary>
        /// 将光标移动到下一行
        /// </summary>
        /// <returns>如果文件结束，则返回false，否则返回true</returns>
        public bool NextRecord()
        {
            while (!noMoreFields) PopField();

            if (csv.Peek() == -1) return false;

            if (csv.Peek() == '\r') csv.Read();
            if (csv.Peek() == '\n') csv.Read();
            if (csv.Peek() == -1) return false;
            noMoreFields = false;
            return true;
        }

        readonly TextReader csv;

        readonly StringBuilder fieldSb = new();
        bool noMoreFields = false;
    }
}
