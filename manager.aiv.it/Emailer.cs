using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Concurrent;
using System.Threading;

namespace manager.aiv.it
{
    public static class Emailer
    {
        private const int    Port          = 25;
        private const string Smtp       = "authsmtp.aiv01.it";
        private const string Username   = "amministrazione@aiv01.it";
        private const string Password   = "amministrazioneaiv01";
        private const string Sender     = "didattica@aiv01.it";
        private const string SYSAdMail  = "alxeyesoul@live.com";
        

        private static SmtpClient                        m_hClient;
        private static BlockingCollection<MailMessage>   m_hQueue;
        private static Thread                            m_hSenderThread;        

        public static EmailerState State        { get; private set; }

        static Emailer()
        {
            State = EmailerState.NotStarted;

            Initialize();

            m_hQueue                        = new BlockingCollection<MailMessage>();
            m_hSenderThread                 = new Thread(ThreadSendRoutine);
            m_hSenderThread.Start();            
        }

        private static void Initialize()
        {
            m_hClient                       = new SmtpClient(Smtp, Port);
            m_hClient.EnableSsl             = false;
            m_hClient.DeliveryMethod        = SmtpDeliveryMethod.Network;
            m_hClient.DeliveryFormat        = SmtpDeliveryFormat.International;
            m_hClient.UseDefaultCredentials = false;
            m_hClient.Credentials           = new NetworkCredential(Username, Password);
        }

        public static void Send(string sRcpt, string sSubject, string sBody)
        {
            m_hQueue.Add(new MailMessage(Sender, sRcpt, sSubject, sBody));
        }

        internal static void Send(List<User> hUsers, string sSubject, string sBody)
        {
            hUsers.Select(u => new MailMessage(Sender, u.Email, sSubject, sBody)).ToList().ForEach(m => m_hQueue.Add(m));
        }

        private static void ThreadSendRoutine()
        {
            State = EmailerState.Online;

            while (true)
            {
                MailMessage hCurrent = null;

                try
                {
                    hCurrent = m_hQueue.Take();
                    m_hClient.Send(hCurrent);
                }
                catch (ArgumentNullException)
                {
                    //Colpa nostra la mail e formattata male
                }
                catch (ObjectDisposedException)
                {
                    Initialize();
                }
                catch (SmtpFailedRecipientsException hEx)
                {
                    using (AivEntities db = new AivEntities())
                    {
                        User hBadMailUser = (from u in db.Users where u.Email == hEx.FailedRecipient select u).FirstOrDefault();
                        User hAdmin       = (from u in db.Users where u.Email == SYSAdMail select u).FirstOrDefault();

                        if (hBadMailUser != null && hAdmin != null)
                        {
                            Note hNewNote = new Note();
                            hNewNote.Text = $"(Automatic Message) User was unable to receive emails, please check the address {Environment.NewLine}Subject:{Environment.NewLine}{hCurrent.Subject}" ;
                            hNewNote.Subject = hBadMailUser;
                            hNewNote.Author = hAdmin;
                            db.Notes.Add(hNewNote);
                            db.SaveChanges();
                        }
                    }
                }
                catch (SmtpException)
                {
                    m_hClient.Dispose();
                    Initialize();
                }
                catch (Exception)
                {
                    m_hClient.Dispose();
                    State = EmailerState.Offline;
                    break;
                }
            }
        }



        public enum EmailerState
        {
            NotStarted,
            Online,
            Offline
        }
    }
}
