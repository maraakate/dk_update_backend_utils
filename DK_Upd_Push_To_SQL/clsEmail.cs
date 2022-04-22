using System;
using System.Net;
using System.Net.Mail;
using System.Security;

public class clsEmail
{
    public static void Email(MailAddress fromAddr, string toAddr, string body, string subject, string[] attachments, string host, string user, string pass)
    {
        try
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = fromAddr;
            message.To.Add(new MailAddress(toAddr));
            message.Subject = subject;
            message.IsBodyHtml = false; //to make message body as html
            message.Priority = MailPriority.High;
            message.Body = body;
            try
            {
                int i = 0;
                foreach (string attachment in attachments)
                {
                    message.Attachments.Insert(i, new Attachment(attachment));
                    i++;
                }
            }
            catch
            {

            }
            smtp.UseDefaultCredentials = false;
            smtp.Port = 587;
            smtp.Host = host;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(user, pass);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            smtp.Send(message);
        }
        catch (Exception InnerEx)
        {
            string message = String.Empty;
            if (InnerEx.InnerException != null)
            {
                message = String.Format("Unable to send email: {0}\n", InnerEx.InnerException.Message);
            }
            else
            {
                message = String.Format("Unable to send email: {0}\n", InnerEx.Message);
            }
            SettingsException OuterEx = new SettingsException(message, InnerEx);
            throw OuterEx;
        }
    }

    public class SettingsException : System.Exception
    {
        public SettingsException(string message) : base(message) { }
        public SettingsException(string message, Exception inner) : base(message, inner) { }
    }
}
