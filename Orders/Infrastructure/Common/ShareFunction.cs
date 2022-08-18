using Orders.Models;
using Orders.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Orders.Infrastructure.Common
{
    internal class ShareFunction
    {
        private static readonly RepositoryFiles repoFiles = new RepositoryFiles();


        //--------------------------------------------------------------------------------
        // Прикрепление файлов к списку ListFiles
        //--------------------------------------------------------------------------------
        public static void AddFiles(string[] files, ICollection<RouteAdding> ListFiles)
        {
            if (files is null)
                return;

            foreach (var file in files)
            {
                FileInfo info = new FileInfo(file);

                //if (info.Length > 5000000)
                //{
                //    MessageBox.Show($"Файл \"{info.Name}\" имеет размер более 5 МБ.\r\n\r\nОн не будет добавлен.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    continue;
                //}

                RouteAdding ra = new RouteAdding();
                ra.ad_text = info.Name;
                ra.FullName = file;

                //try
                //{

                //    FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //    ra.ad_file = new byte[fs.Length];
                //    fs.Read(ra.ad_file, 0, (int)fs.Length);
                //    fs.Close();
                //}
                //catch(Exception e)
                //{
                //    MessageBox.Show(e.Message,"Ошибка добавления файла", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                ListFiles.Add(ra);

            }

        }

        //--------------------------------------------------------------------------------
        // Отправка на следующий этап маршрута
        //--------------------------------------------------------------------------------
        public static void SendToNextStep(Order order, RouteOrder CurrentStep, ICollection<RouteAdding> ListFiles)
        {
            CurrentStep.RouteAddings = ListFiles;
            CurrentStep.ro_check = EnumCheckedStatus.Checked;
            CurrentStep.ro_date_check = DateTime.Now;
            RouteOrder NextStep;

            // TODO Добавление прикрепленных файлов
            repoFiles.AddFiles(CurrentStep);

            if (order.RouteOrders.All(it => it.ro_check > EnumCheckedStatus.CheckedProcess))
            {
                // маршрут окончен
                NextStep = null;
                SendMail(order.RouteOrders.First().User.u_email, order.o_number);
            }
            else
            {
                NextStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute + 1);

                if (NextStep.ro_return_step != null && CurrentStep.ro_return_step is null)
                {
                    // следующий этап подчиненный после главного
                    if (NextStep.ro_check == EnumCheckedStatus.Checked)
                    {
                        // он уже был рассмотрен,  делаем прыжок
                        NextStep = order.RouteOrders
                            .FirstOrDefault(it => it.ro_step > NextStep.ro_step && it.ro_return_step is null);

                    }
                    else
                        CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;

                }
                else if (NextStep.ro_return_step is null && CurrentStep.ro_return_step != null)
                {
                    // следующий этап после подчиненного уже основной
                    // вохвращаемся на главный
                    NextStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == CurrentStep.ro_return_step);
                }

            }
            SetStatusStep(CurrentStep, NextStep, order);

            SendMail(NextStep?.User.u_email, order.o_number);
            //SendMail(NextStep?.User.u_email, $"Вам необходимо рассмотреть заказ № {order.o_number}. " +
            //    $"Ссылка на программу - \"file:///s:/Производство/01_Мавричев/ПО Движение заказов/Orders.exe\"");
        }


        //--------------------------------------------------------------------------------
        // переустановки статусов при отправке далее
        //--------------------------------------------------------------------------------
        public static void SetStatusStep(RouteOrder step, RouteOrder nextStep, Order order)
        {
            int selectStatus = (int)EnumStatus.None;

            if (step.ro_check > EnumCheckedStatus.CheckedProcess)
            {
                switch ((EnumTypesStep)step.ro_typeId)
                {
                    case EnumTypesStep.Coordinate:
                        selectStatus = (int)EnumStatus.Coordinated;
                        break;

                    case EnumTypesStep.Approve:
                        selectStatus = (int)EnumStatus.Approved;
                        break;

                    case EnumTypesStep.Review:
                        selectStatus = (int)EnumStatus.Coordinated;
                        break;

                    case EnumTypesStep.Notify:
                        selectStatus = (int)EnumStatus.Coordinated;
                        break;

                    case EnumTypesStep.Created:
                        selectStatus = (int)EnumStatus.Created;
                        break;

                }

                step.ro_statusId = selectStatus;
                order.o_statusId = selectStatus;
            }
            else
                step.ro_statusId = (int)EnumStatus.Waiting;

            order.o_stepRoute = step.ro_step;

            if (nextStep != null)
            {
                switch ((EnumTypesStep)nextStep.ro_typeId)
                {
                    case EnumTypesStep.Coordinate:
                        selectStatus = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Approve:
                        selectStatus = (int)EnumStatus.ApprovWork;
                        break;

                    case EnumTypesStep.Review:
                        selectStatus = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Notify:
                        selectStatus = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Created:
                        selectStatus = (int)EnumStatus.Created;
                        break;

                }

                nextStep.ro_statusId = selectStatus;
                nextStep.ro_check = EnumCheckedStatus.CheckedProcess;
                order.o_statusId = selectStatus;
                order.o_stepRoute = nextStep.ro_step;
            }

        }

        //--------------------------------------------------------------------------------
        // Отправка сообщения по почте
        //--------------------------------------------------------------------------------
        public static async void SendMail(string Email, string Text)
        {
            if (string.IsNullOrEmpty(Email))
                return;

            string Message = "<html><body>Вам необходимо рассмотреть заказ № " + Text + 
                ". Ссылка на программу - <a href=\"file:///s:/Производство/01_Мавричев/ПО Движение заказов/Orders.exe\">ПО Движение заказов</ф></body></html>";

            MailAddress from = new MailAddress("orders@ngk-ehz.ru", "Order PO");
            MailAddress to = new MailAddress(Email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Оповещение ПО Движение заказов";

            m.IsBodyHtml = true;
            m.Body = Message;
            
            
            
            SmtpClient smtp = new SmtpClient("zimbra.lancloud.ru", 587);

            smtp.Credentials = new NetworkCredential("a.scherbakov@ngk-ehz.ru", "Jjbr9uxa");
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(m);
        }


    }
}
