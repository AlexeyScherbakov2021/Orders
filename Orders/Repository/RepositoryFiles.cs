using Orders.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Orders.Repository
{

    internal class RepositoryFiles
    {
#if DEBUG
        static string FileStorage = @"\\ngk-dc-07\FileStore$\MoveOrdersD\";
#else
        static string FileStorage = @"\\ngk-dc-07\FileStore$\MoveOrders\";
#endif

        //--------------------------------------------------------------------------------------------
        // получение нужного пути и созздание диреткории года
        //--------------------------------------------------------------------------------------------
        private string CurrentPath(int Year)
        {
            string NewPath = FileStorage + Year.ToString() + "\\";
            if (!Directory.Exists(NewPath))
                Directory.CreateDirectory(NewPath);

            return NewPath;
        }

        //--------------------------------------------------------------------------------------------
        // Добавление файлов
        //--------------------------------------------------------------------------------------------
        public void AddFiles( RouteOrder CurrentStep)
        {
            string NewName;
            string NewPath = CurrentPath(CurrentStep.Order.o_date_created.Year);

            foreach(RouteAdding item in CurrentStep.RouteAddings)
            {
                if(item.FullName != null)
                {
                    NewName = NewPath + CurrentStep.id.ToString() + "." +  item.ad_text;
                    File.Copy(item.FullName, NewName, true );

                    // записано, обнуляем
                    item.FullName = null;
                }
            }
        }

        //--------------------------------------------------------------------------------------------
        // Удаление файлов
        //--------------------------------------------------------------------------------------------
        public void DeleteFiles(Order order)
        {
            string NewPath = CurrentPath(order.o_date_created.Year);
            string NewName;

            foreach(var CurrentStep in order.RouteOrders)
            {
                foreach (RouteAdding item in CurrentStep.RouteAddings)
                {
                    NewName = NewPath + CurrentStep.id.ToString() + "." + item.ad_text;
                    try
                    {
                        File.Delete(NewName);
                    }
                    catch { }
                }
            }
        }

        //--------------------------------------------------------------------------------------------
        // Получение файла в TEMP папку
        //--------------------------------------------------------------------------------------------
        public string GetFile(RouteAdding raFile)
        {
            string NewPath = CurrentPath(raFile.RouteOrder.Order.o_date_created.Year);

            //string NewPath = FileStorage + raFile.RouteOrder.Order.o_date_created.Year.ToString() + "\\";
            string NewName = NewPath + raFile.RouteOrder.id.ToString() + "." + raFile.ad_text;

            string TempFileName = Path.GetTempPath() + raFile.ad_text;

            try
            {
                File.Copy(NewName, TempFileName, true);
            }
            catch 
            {
                return null;
            }

            return TempFileName;
        }

    }
}
