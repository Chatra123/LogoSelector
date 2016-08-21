using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;


namespace OctNov.Ini
{
  static class WinApi_Ini
  {
    [DllImport("KERNEL32.DLL")]
    public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

    [DllImport("KERNEL32.DLL")]
    public static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);
  }


  static class IniHelper
  {
    static readonly string AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location,
                           AppDir = Path.GetDirectoryName(AppPath),
                           AppName = Path.GetFileNameWithoutExtension(AppPath);
    public static readonly string IniPath = Path.Combine(AppDir, AppName + ".ini");


    /// <summary>
    /// ini  -->  string
    /// </summary>
    public static string GetString(string section, string key, string defaultValue = "")
    {
      var text = new StringBuilder(512);
      WinApi_Ini.GetPrivateProfileString(section, key, defaultValue, text, (uint)text.Capacity, IniPath);
      return text.ToString();
    }

    /// <summary>
    /// ini  -->  int
    /// </summary>
    public static int GetInt(string section, string key, int defaultValue = 0)
    {
      uint ret = WinApi_Ini.GetPrivateProfileInt(section, key, defaultValue, IniPath);
      return (int)ret;
    }

    /// <summary>
    /// ini  -->  bool
    /// </summary>
    public static bool GetBool(string section, string key, int defaultValue = 0)
    {
      uint ret = WinApi_Ini.GetPrivateProfileInt(section, key, defaultValue, IniPath);
      return (int)ret != 0;
    }


  }

}




