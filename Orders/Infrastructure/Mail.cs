using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace Orders.Infrastructure
{
    internal class Mail
    {



        public Mail()
        {

        }

        public void SendMail(string Email, string Message)
        {
            if (string.IsNullOrEmpty(Email))
                return;

            MailAddress from = new MailAddress("orders@ngk-ehz.ru", "Order PO");
            MailAddress to = new MailAddress(Email);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Оповещение ПО Заказы";
            m.Body = Message;
            SmtpClient smtp = new SmtpClient("zimbra.lancloud.ru", 587);

            smtp.Credentials = new NetworkCredential("a.scherbakov@ngk-ehz.ru", "Jjbr9uxa");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }

    }
}
