using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace LogoSelector
{
  using OctNov.Ini;

  class Setting_File
  {
    public List<string> LogoDir { get; private set; }
    public List<List<string>> SearchSet_byKeyword { get; private set; }
    public List<List<string>> SearchSet_AddComment { get; private set; }

    public string Mes_NotFoundLogo { get; private set; }
    public string Mes_NotFoundParam { get; private set; }
    public bool Enable_ShortCH { get; private set; }
    public bool Enable_NonNumCH { get; private set; }


    /// <summary>
    /// constructor
    /// </summary>
    public Setting_File()
    {
      LogoDir = new List<string>();
      SearchSet_byKeyword = new List<List<string>>();
      SearchSet_AddComment = new List<List<string>>();
      Mes_NotFoundLogo = "";
      Mes_NotFoundParam = "";
      Enable_ShortCH = false;
      Enable_NonNumCH = false;

      //開発時の利便性のためにアプリフォルダを追加する
      string AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string AppDir = System.IO.Path.GetDirectoryName(AppPath);
      LogoDir.Add(AppDir);
   }


    /// <summary>
    /// Iniファイル読み込み
    /// </summary>
    public void ReadIni()
    {
      //create ini
      if (File.Exists(IniHelper.IniPath) == false)
      {
        File.WriteAllText(IniHelper.IniPath, IniText.Default, Encoding.GetEncoding("Shift_JIS"));
        File.AppendAllText(IniHelper.IniPath, IniText.Readme, Encoding.GetEncoding("Shift_JIS"));
      }

      //Read
      for (int no = 1; no <= 9; no++)
      {
        const string section = "LogoDir";
        var path = IniHelper.GetString(section, "Path_" + no);
        if (path != "")
          LogoDir.Add(path);
      }

      for (int no = 1; no <= 9; no++)
      {
        const string section = "SearchKeyword";
        var keyword = IniHelper.GetString(section, "Keyword_" + no);
        var logo = IniHelper.GetString(section, "Logo_" + no);
        var param = IniHelper.GetString(section, "Param_" + no);
        if (keyword != "")
          SearchSet_byKeyword.Add(new List<string> { keyword, logo, param });
      }

      for (int no = 1; no <= 9; no++)
      {
        const string section = "AddComment_Keyword";
        var keyword = IniHelper.GetString(section, "Keyword_" + no);
        var comment = IniHelper.GetString(section, "Comment_" + no);
        if (keyword != "")
          SearchSet_AddComment.Add(new List<string> { keyword, comment });
      }


      {
        const string section = "AddComment_NotFound";
        var comm_logo = IniHelper.GetString(section, "NotFoundLogo");
        if (comm_logo != "")
          Mes_NotFoundLogo = comm_logo;

        var comm_param = IniHelper.GetString(section, "NotFoundParam");
        if (comm_param != "")
          Mes_NotFoundParam = comm_param;
      }
      {
        const string section = "Option";
        Enable_ShortCH = IniHelper.GetBool(section, "AppendSearch_ShortCH");
        Enable_NonNumCH = IniHelper.GetBool(section, "AppendSearch_NonNumCH");
      }

    }



    /// <summary>
    /// 結果表示
    /// </summary>
    public void ShowSetting()
    {
      var result = new StringBuilder();

      {
        int no = 0;                      //Path_0 = AppDir
        result.AppendLine("[LogoDir]");
        foreach (var path in LogoDir)
        {
          result.AppendLine("  Path_" + no + " = " + path);
          no++;
        }
        result.AppendLine();
      }

      {
        int no = 1;
        result.AppendLine("[SearchKeyword]");
        foreach (var set in SearchSet_byKeyword)
        {
          var keyword = set[0];
          var comment = set[1];
          result.AppendLine("  Keyword_" + no + " = " + keyword);
          result.AppendLine("  Comment_" + no + " = " + comment);
          no++;
        }
        result.AppendLine();
      }

      {
        int no = 1;
        result.AppendLine("[SearchSet_AddComment]");
        foreach (var set in SearchSet_AddComment)
        {
          var keyword = set[0];
          var comment = set[1];
          result.AppendLine("  Keyword_" + no + " = " + keyword);
          result.AppendLine("  Comment_" + no + " = " + comment);
          no++;
        }
        result.AppendLine();
      }

      result.AppendLine("[AddComment_NotFound]");
      result.AppendLine("  NotFoundLogo  = " + Mes_NotFoundLogo);
      result.AppendLine("  NotFoundParam = " + Mes_NotFoundParam);
      result.AppendLine();

      result.AppendLine("[Option]");
      result.AppendLine("  AppendSearch_ShortCH  = " + Enable_ShortCH);
      result.AppendLine("  AppendSearch_NonNumCH = " + Enable_NonNumCH);
      result.AppendLine();

      Console.Error.WriteLine(result.ToString());
    }





  }
}



