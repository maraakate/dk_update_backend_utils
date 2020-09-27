using System;
using System.Xml;

namespace DK_Upd_Push_To_SQL
{
   public class clsConfigReader
   {
      private string ConfigFileName;

      public clsConfigReader(string configFileName)
      {
         ConfigFileName = configFileName;
      }

      public string GetSetting(string setting)
      {
         try
         {
            XmlDocument oDoc = new XmlDocument();
            oDoc.Load(ConfigFileName);
            string xPath = "//add[@key='" + setting + "']";
            XmlElement oElm = (XmlElement)oDoc.SelectSingleNode(xPath);
            if (oElm != null) //attribute exists
            {
               return oElm.GetAttribute("value").ToString();
            }
            else
            {
               return null;
            }
         }
         catch (Exception InnerEx)
         {
            string message = "Unable to retrieve application Setting: " + setting;
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
}
