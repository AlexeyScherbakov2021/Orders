using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;


namespace Orders
{
    public class LogFile
    {
        StreamWriter file;
        string sFileName;
        int iWrite;
        static readonly object _locker = new object();

        public LogFile(int iW = 1)
        {
            iWrite = iW;
            // если лог писать не надо, то выход
            if (iWrite == 0)
                return;

            // открытие файла для записи
            sFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Orders.log";
            //sFileName = sFileName.Substring(0, sFileName.Length - 3) + "\\SimStend.log";
            file = new StreamWriter(sFileName, true);
        }

        //---------------------------------------------------------
        // Вывод строки журнала в файл
        //---------------------------------------------------------
        public void WriteLineLog(string sLine, params object[] vs)
        {
            // если записывать не надо, то выход
            if (iWrite == 0)
                return;
            
            // получаем текущее время
            DateTime dt = DateTime.Now;
            string s;

            // формрование строки записи по входным параметрам
            sLine = string.Format(sLine, vs);

            //  Должен быть вход в критическую секцию !!!!!!!!!!!!!!!!!!!!!
            lock (_locker)
            {
                FileInfo fi = new FileInfo(sFileName);
                if (fi.Length > 2000000)
                {
                    // если размер файла превысил 2 МБ, то создаем новый
                    file.Close();
                    file.Dispose();
                    s = sFileName.Substring(0, sFileName.Length - 4);
                    s += DateTime.Now.ToString(" dd-MM-yyyy HH-mm") + ".log";
                    // старый файл переименовать
                    fi.MoveTo(s);
                    // создание нового файла
                    file = new StreamWriter(sFileName, true);
                }
                // запись строки и сброс на диск
                file.WriteLine(dt.ToString("dd.MM.yyyy HH:mm:ss.ffff ") + sLine);
                file.Flush();
            }

            // Должен быть выход из критической секции !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        }
    }
}
