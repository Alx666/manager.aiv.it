using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace manager.aiv.it
{
    public static class Emailer
    {
        private const int Port          = 25;
        private const string Smtp       = "authsmtp.aiv01.it";
        private const string Username   = "amministrazione@aiv01.it";
        private const string Password   = "amministrazioneaiv01";

        public static void Send(string sSender, string sRcpt, string sSubject, string sBody)
        {
            MailMessage mail    = new MailMessage(sSender, sRcpt);
            mail.Subject        = sSubject;
            mail.Body           = sBody;

            SmtpClient client               = new SmtpClient();
            client.EnableSsl                = false;
            client.Port                     = Port;
            client.Host                     = Smtp;
            client.DeliveryMethod           = SmtpDeliveryMethod.Network;
            client.DeliveryFormat           = SmtpDeliveryFormat.International;
            client.UseDefaultCredentials    = false;
            client.Credentials              = new NetworkCredential(Username, Password);

            client.Send(mail);
        }
    }
}




//SmtpClient client = new SmtpClient();
//client.Port = 25;
//client.DeliveryMethod = SmtpDeliveryMethod.Network;
//client.UseDefaultCredentials = false;
//client.Host = "smtp.google.com";
//mail.Subject = "this is a test email.";
//mail.Body = "this is my test email body";
//client.Send(mail);