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
    private IniFile IniFile;

    public List<string> LogoDir { get; private set; }
    public List<List<string>> SearchSet_byKeyword { get; private set; }
    public List<List<string>> SearchSet_AddComment { get; private set; }
    public string Comment_NotFoundLogo { get; private set; }
    public string Comment_NotFoundParam { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    public Setting_File()
    {
      IniFile = new IniFile();

      //initialize value
      LogoDir = new List<string>();
      SearchSet_byKeyword = new List<List<string>>();
      SearchSet_AddComment = new List<List<string>>();
      Comment_NotFoundLogo = "";
      Comment_NotFoundParam = "";

      //開発時の利便性のためにアプリフォルダを追加する
      string AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string AppDir = System.IO.Path.GetDirectoryName(AppPath);
      LogoDir.Add(AppDir);

      //create ini
      if (IniFile.IsExist == false)
      {
        File.WriteAllText(IniFile.IniPath, IniText.Default, Encoding.GetEncoding("Shift_JIS"));
        File.AppendAllText(IniFile.IniPath, IniText.Readme, Encoding.GetEncoding("Shift_JIS"));
      }
      Read();
    }


    /// <summary>
    /// ReadIni
    /// </summary>
    public void Read()
    {
      for (int no = 1; no <= 9; no++)
      {
        const string section = "LogoDir";
        var path = IniFile.GetString(section, "Path_" + no);
        if (path != "")
          LogoDir.Add(path);
      }
      for (int no = 1; no <= 9; no++)
      {
        const string section = "SearchKeyword";
        var keyword = IniFile.GetString(section, "Keyword_" + no);
        var logo = IniFile.GetString(section, "Logo_" + no);
        var param = IniFile.GetString(section, "Param_" + no);
        if (keyword != "")
          SearchSet_byKeyword.Add(new List<string> { keyword, logo, param });
      }
      for (int no = 1; no <= 9; no++)
      {
        const string section = "AddComment_Keyword";
        var keyword = IniFile.GetString(section, "Keyword_" + no);
        var comment = IniFile.GetString(section, "Comment_" + no);
        if (keyword != "")
          SearchSet_AddComment.Add(new List<string> { keyword, comment });
      }
      {
        const string section = "AddComment_NotFound";
        var comm_logo = IniFile.GetString(section, "NotFoundLogo");
        if (comm_logo != "")
          Comment_NotFoundLogo =  comm_logo;
        var comm_param = IniFile.GetString(section, "NotFoundParam");
        if (comm_param != "")
          Comment_NotFoundParam = comm_param;
      }
    }



    /// <summary>
    /// 結果表示
    /// </summary>
    public void Show()
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
      result.AppendLine("  NotFoundLogo  = " + Comment_NotFoundLogo);
      result.AppendLine("  NotFoundParam = " + Comment_NotFoundParam);
      result.AppendLine();
      Console.Error.WriteLine(result.ToString());
    }





  }
}



