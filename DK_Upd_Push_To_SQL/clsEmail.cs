using System;
using System.Net;
using System.Net.Mail;

public class clsEmail
{
   public static void Email(MailAddress fromAddr, string toAddr, string body, string subject, string host, string user, string pass)
   {
      try
      {
         MailMessage message = new MailMessage();
         SmtpClient smtp = new SmtpClient();
         message.From = fromAddr;
         message.To.Add(new MailAddress(toAddr));
         message.Subject = subject;
         message.IsBodyHtml = false; //to make message body as html  
         message.Body = body;
         smtp.Port = 587;
         smtp.Host = host;
         smtp.EnableSsl = true;
         smtp.UseDefaultCredentials = false;
         smtp.Credentials = new NetworkCredential(user, pass);
         smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
         smtp.Send(message);
      }
      catch (Exception InnerEx)
      {
         string message = "Unable to send email: ";
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
