using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using LgdLogo;


namespace LogoSelector
{
  /// <summary>
  /// lgdファイル、paramファイル取得
  /// </summary>
  class FileCollector
  {
    public List<LgdFile> LgdList { get; private set; }
    public List<string> Lgd_NameList { get { return LgdList.Select(lgd => lgd.Name).ToList(); } }

    public List<FileInfo> ParamList { get; private set; }
    public List<string> Param_NameList { get { return ParamList.Select(param => param.Name).ToList(); } }


    /// <summary>
    /// ファイル収集
    /// </summary>
    public void Collect(List<string> dirlist)
    {
      //initialize
      LgdList = new List<LgdFile>();
      ParamList = new List<FileInfo>();


      //collect file from dirlist
      var fi_lgd = new List<FileInfo>();
      var fi_ldp = new List<FileInfo>();
      var fi_param = new List<FileInfo>();

      foreach (string dir in dirlist)
      {
        if (Directory.Exists(dir) == false) continue;

        // DirectoryInfo().GetFiles("*.ldp")だと *.ldp に加えて *.ldp2 も取得され、
        //拡張子が上手く処理できない。
        //*.ldp* でないのに *.ldp2 も取得してしまう。
        //全ファイル取得してから自前で判定する。
        var files = new DirectoryInfo(dir).GetFiles();

        //  lgd  lgd2
        fi_lgd.AddRange(
          files.Where((fi) => fi.Extension.ToLower() == @".lgd")
          );
        fi_lgd.AddRange(
          files.Where((fi) => fi.Extension.ToLower() == @".lgd2")
          );

        //  ldp  ldp2
        fi_ldp.AddRange(
          files.Where((fi) => fi.Extension.ToLower() == @".ldp")
          );
        fi_ldp.AddRange(
          files.Where((fi) => fi.Extension.ToLower() == @".ldp2")
          );

        //  param
        fi_param.AddRange(
          files.Where((fi) => fi.Extension.ToLower() == @".param")
          );
      }

      //register
      //  lgd  lgd2
      fi_lgd.ForEach((fi) =>
      {
        LgdList.Add(new LgdFile(fi.FullName, fi.Name));
      });

      //register
      //  ldp  ldp2
      var filemanager = new LogoFileManager();

      foreach (var ldp in fi_ldp)
      {
        var logofile = filemanager.Load(ldp.FullName);
        if (logofile == null) continue;

        var logo_namelist = LogoFileUtil.GetNameList(logofile);

        logo_namelist.ForEach((logoname) =>
        {
          LgdList.Add(new LgdFile(ldp.FullName, logoname));
        });
      }

      //  param
      ParamList = fi_param;

    }


    /// <summary>
    /// Lgd_NameListからフルパス取得
    /// </summary>
    public string GetFullName_Lgd(string target)
    {
      var lgdlist = LgdList.Where(lgd => lgd.Name == target)
                           .ToList();
      if (lgdlist.Count == 0)
        return "";

      //found
      return lgdlist[0].GetFullName_Lgd();
    }


    /// <summary>
    /// Param_NameListからフルパス取得
    /// </summary>
    public string GetFullName_Param(string target)
    {
      var paramlist = ParamList.Where(param => param.Name == target)
                               .ToList();
      if (paramlist.Count == 0)
        return "";

      //found
      return paramlist[0].FullName;
    }

  }



  /// <summary>
  /// lgdファイル, ldp内のlgdファイル
  /// </summary>
  class LgdFile
  {
    // lgd or ldp  のパス
    public string Path { get; private set; }

    private string Ext { get { return System.IO.Path.GetExtension(Path).ToLower(); } }
    private bool IsLgd { get { return Ext == ".lgd"; } }
    private bool IsLgd2 { get { return Ext == ".lgd2"; } }
    private bool IsLdp { get { return Ext == ".ldp"; } }
    private bool IsLdp2 { get { return Ext == ".ldp2"; } }


    //ファイル名　or  ldp内のロゴ名
    //  WithoutExtension
    //  paramの検索に使用する
    public string Name { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    public LgdFile(string path, string name)
    {
      Path = path;
      Name = System.IO.Path.GetFileNameWithoutExtension(name);
    }

    /// <summary>
    /// lgdファイルのフルパス取得
    /// </summary>
    public string GetFullName_Lgd()
    {
      CleanTmpDir();

      if (IsLgd)
        return Path;
      else if (IsLgd2)          // lgd以外なら lgdを作成してフルパス取得
        return Save_AsLgd();
      else if (IsLdp || IsLdp2)
        return Save_AsLgd();
      else
        return "unknown ext";
    }


    /// <summary>
    /// lgdファイル保存
    /// </summary>
    public string Save_AsLgd()
    {
      var filemanager = new LogoFileManager();

      List<LogoData> logodata;
      {
        var logofile = filemanager.Load(Path);
        if (logofile == null)
          return "logofile == null";

        if (IsLgd2)
          logodata = logofile.LogoData;
        else if (IsLdp || IsLdp2)
          logodata = LogoFileUtil.GetLogoData(Name, logofile);
        else
          return "unknown ext";

        if (logodata.Count == 0)
          return "logodata.Count == 0";
      }

      string savepath;
      {
        string tmpDir;
        tmpDir = System.IO.Path.GetTempPath();
        tmpDir = System.IO.Path.Combine(tmpDir, "LogoSelector");
        if (Directory.Exists(tmpDir) == false)
          Directory.CreateDirectory(tmpDir);

        string savename;
        int PID = Process.GetCurrentProcess().Id;
        string timecode = DateTime.Now.ToString("ddHHmmssff");
        savename = Name + "_" + timecode + PID + ".LogoSelector.tmp.lgd";
        savename = Text.ValidateFileName(savename);

        savepath = System.IO.Path.Combine(tmpDir, savename);
      }

      //save lgd file
      filemanager.Save(savepath, logodata);

      return savepath;
    }


    /// <summary>
    /// win tempのLgdファイル削除
    /// </summary>
    static void CleanTmpDir()
    {
      string winTmp = System.IO.Path.GetTempPath();
      string logoTmpDir = System.IO.Path.Combine(winTmp, "LogoSelector");

      FileCleaner.Delete_File(1.0, logoTmpDir, "*.LogoSelector.tmp.lgd");
    }

  }//  class LgdFile



  static class Text
  {
    /// <summary>
    /// ファイル名に使えない文字を置換
    /// </summary>
    public static string ValidateFileName(string filename)
    {
      foreach (char invalid in Path.GetInvalidFileNameChars())
        filename = filename.Replace(invalid, '_');

      return filename;
    }
  }



  /// <summary>
  /// 削除処理　実行部
  /// </summary>
  static class FileCleaner
  {
    /// <summary>
    /// ファイル削除
    /// </summary>
    /// <param name="nDaysBefore">Ｎ日前のファイルを削除対象にする</param>
    /// <param name="directory">ファイルを探すフォルダ。　サブフォルダ内も対象</param>
    /// <param name="searchKey">ファイル名に含まれる文字。ワイルドカード可 * </param>
    /// <param name="ignoreKey">除外するファイルに含まれる文字。ワイルドカード不可 × </param>
    public static void Delete_File(double nDaysBefore, string directory,
      string searchKey, string ignoreKey = null)
    {
      if (Directory.Exists(directory) == false) return;
      System.Threading.Thread.Sleep(500);

      //ファイル取得
      var files = new FileInfo[] { };
      try
      {
        var dirInfo = new DirectoryInfo(directory);
        files = dirInfo.GetFiles(searchKey, SearchOption.AllDirectories);
      }
      catch (System.UnauthorizedAccessException)
      {
        /* Java  jre-8u73-windows-i586.exeを実行してインストール用のウィンドウを表示させると、
         * Tempフォルダにjds262768703.tmpがReadOnlyで作成される。
         * 
         * アクセス権限の無いファイルが含まれているフォルダに
         * files = dirInfo.GetFiles();
         * を実行すると System.UnauthorizedAccessExceptionが発生する。
         */
        return;
      }

      foreach (var onefile in files)
      {
        if (onefile.Exists == false) continue;
        if (ignoreKey != null && 0 <= onefile.Name.IndexOf(ignoreKey)) continue;

        //古いファイル？
        bool over_creation = nDaysBefore < (DateTime.Now - onefile.CreationTime).TotalDays;
        bool over_lastwrite = nDaysBefore < (DateTime.Now - onefile.LastWriteTime).TotalDays;
        if (over_creation && over_lastwrite)
        {
          try { onefile.Delete(); }
          catch { /*ファイル使用中*/ }
        }
      }
    }

    /// <summary>
    /// 空フォルダ削除
    /// </summary>
    /// <param name="parent_directory">親フォルダを指定。空のサブフォルダが削除対象、親フォルダ自身は削除されない。</param>
    public static void Delete_EmptyDir(string parent_directory)
    {
      if (Directory.Exists(parent_directory) == false) return;

      var dirs = new DirectoryInfo[] { };
      try
      {
        var dirInfo = new DirectoryInfo(parent_directory);
        dirs = dirInfo.GetDirectories("*", SearchOption.AllDirectories);
      }
      catch (System.UnauthorizedAccessException)
      {
        return;
      }

      foreach (var onedir in dirs)
      {
        if (onedir.Exists == false) continue;

        //空フォルダ？
        var files = onedir.GetFiles();
        if (files.Count() == 0)
        {
          try { onedir.Delete(); }
          catch { /*フォルダ使用中*/ }
        }
      }
    }
  }


}//namespace
