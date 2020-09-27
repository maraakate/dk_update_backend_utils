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
      CfgReaderException,
      EmailToAddrNull,
      EmailFromAddrNull,
      EmailHostNull,
      EmailUserNull,
      EmailPassNull,
      DateTimeInvalidLen,
   }
}
