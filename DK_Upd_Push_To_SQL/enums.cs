namespace DK_Upd_Push_To_SQL
{
   enum ErrorCodes : int
   {
      OK = 0,
      NullFileNamePath,
      NullPDBFilePath,
      NullArch,
      ErrorSQLQuery,
      NullMD5Hash,
      DKExeMissing,
      NullSQLConnStr,
      NullDKExePathStr,
      NulldkPathStr,
      CfgReaderException,
      EmailToAddrNull,
      EmailFromAddrNull,
      EmailHostNull,
      EmailUserNull,
      EmailPassNull,
      FtpExeNull,
      FtpAddressNull,
      FtpPortNull,
      FtpUserNull,
      FtpPassNull,
      FtpDirectoryNull,
      DateTimeInvalidLen,
   }
}
