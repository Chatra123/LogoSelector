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
  /// lgd、paramファイル収集
  /// </summary>
  class LgdFiler
  {
    private List<string> DirList;
    public List<LgdFile> LgdList { get; private set; }
    public List<string> Lgd_NameList { get { return LgdList.Select(lgd => lgd.Name).ToList(); } }
    public List<string> Lgd_ChList { get { return LgdList.SelectMany(lgd => lgd.ChList).ToList(); } }
    public List<FileInfo> ParamList { get; private set; }
    public List<string> Param_NameList { get { return ParamList.Select(param => param.Name).ToList(); } }

    /// <summary>
    /// constructor
    /// </summary>
    public LgdFiler(List<string> dirlist)
    {
      DirList = new List<string>(dirlist);
      LgdList = new List<LgdFile>();
      ParamList = new List<FileInfo>();
    }

    /// <summary>
    /// ファイル収集
    /// </summary>
    public void Collect()
    {
      var lgd = new List<FileInfo>();
      var ldp = new List<FileInfo>();
      var param = new List<FileInfo>();

      //Collect file
      // DirectoryInfo().GetFiles("*.ldp")だと *.ldp に加えて *.ldp2 も取得される。
      // "*.ldp*" でないのに *.ldp2 も取得してしまうので自前で判定する。
      foreach (string dir in DirList)
      {
        if (Directory.Exists(dir) == false)
          continue;
        var files = new DirectoryInfo(dir).GetFiles();
        //  lgd  lgd2
        lgd.AddRange(files.Where((fi) => fi.Extension.ToLower() == ".lgd"));
        lgd.AddRange(files.Where((fi) => fi.Extension.ToLower() == ".lgd2"));
        //  ldp  ldp2
        ldp.AddRange(files.Where((fi) => fi.Extension.ToLower() == ".ldp"));
        ldp.AddRange(files.Where((fi) => fi.Extension.ToLower() == ".ldp2"));
        //  param
        param.AddRange(files.Where((fi) => fi.Extension.ToLower() == ".param"));
      }

      //Register
      //  lgd  lgd2
      lgd.ForEach((fi) => { LgdList.Add(new LgdFile(fi.FullName)); });
      //  ldp  ldp2
      var filemanager = new LogoFileManager();
      foreach (var one in ldp)
      {
        var logofile = filemanager.Load(one.FullName);
        if (logofile == null)
          continue;

        var logo_namelist = LogoFileUtil.GetNameList(logofile);
        logo_namelist.ForEach((logoname) =>
        {
          LgdList.Add(new LgdFile(one.FullName, logoname));
        });
      }
      //  param
      ParamList = param;
    }


    /// <summary>
    /// Lgdのフルパス取得
    /// </summary>
    public string GetLgd_FullName(string key)
    {
      var lgdlist = LgdList.Where(lgd => lgd.Name.Contains(key)).ToList();
      if (lgdlist.Count == 0)
        return "";
      return lgdlist[0].GetLgd_FullName();
    }


    /// <summary>
    /// Paramのフルパス取得
    /// </summary>
    public string GetParam_FullName(string key)
    {
      var paramlist = ParamList.Where(fi => fi.Name.Contains(key) ).ToList();
      if (paramlist.Count == 0)
        return "";
      return paramlist[0].FullName;
    }
  }



  /// <summary>
  /// LgdFile
  /// </summary>
  class LgdFile
  {
    // lgd path or ldp path
    //  ex.
    //    C:\LodoData1\Dlife ディーライフ.2016.lgd
    public string Path { get; private set; }

    private string Ext { get { return System.IO.Path.GetExtension(Path).ToLower(); } }
    private bool IsLgd { get { return Ext == ".lgd"; } }
    private bool IsLgd2 { get { return Ext == ".lgd2"; } }
    private bool IsLdp { get { return Ext == ".ldp"; } }
    private bool IsLdp2 { get { return Ext == ".ldp2"; } }
    //ファイル名　or  ldp内のロゴ名
    //  ex.
    //    Dlife ディーライフ.2016
    public string Name { get; private set; }
    //チャンネル名
    // ex.
    //   Dlife
    //   ディーライフ
    public List<string> ChList { get; private set; }


    /// <summary>
    /// constructor
    /// </summary>
    public LgdFile(string path, string name = null)
    {
      //C:\LodoData1\Dlife ディーライフ.2016.lgd
      Path = path;
      //Dlife ディーライフ.2016 
      Name = name ?? System.IO.Path.GetFileNameWithoutExtension(path);
      //Dlife ディーライフ
      string ch_liner = new Regex(@"^(.*)\.(.*)").Replace(Name, "$1");
      //Dlife
      //ディーライフ
      ChList = ch_liner.Split()
                       .Where((ch) => ch != string.Empty)
                       .ToList();
    }


    /// <summary>
    /// lgdファイルのフルパス取得
    /// </summary>
    public string GetLgd_FullName()
    {
      //古いファイル削除
      string tmpDir;
      tmpDir = System.IO.Path.GetTempPath();
      tmpDir = System.IO.Path.Combine(tmpDir, "LogoSelector");
      var cleaner = new LGLauncher.FileCleaner();
      cleaner.Delete_File(2.0, tmpDir, "*.tmp.lgd");

      if (IsLgd)
        return Path;
      else if (IsLgd2)          // lgd以外なら lgdを作成してフルパス取得
        return Save_AsLgd();
      else if (IsLdp || IsLdp2)
        return Save_AsLgd();
      else
        return "";
    }


    /// <summary>
    /// lgdファイルとしてロゴデータを保存
    /// </summary>
    public string Save_AsLgd()
    {
      var filemanager = new LogoFileManager();

      string savepath;
      {
        string tmpDir;
        tmpDir = System.IO.Path.GetTempPath();
        tmpDir = System.IO.Path.Combine(tmpDir, "LogoSelector");
        if (Directory.Exists(tmpDir) == false)
          Directory.CreateDirectory(tmpDir);
        string name;
        string timecode = DateTime.Now.ToString("dd");
        name = Name + "." + timecode  + ".tmp.lgd";
        name = ValidFileName.Validate(name);
        savepath = System.IO.Path.Combine(tmpDir, name);
      }
      if (File.Exists(savepath))
        return savepath;

      List<LogoData> logodata;
      {
        var logofile = filemanager.Load(Path);
        if (logofile == null)
          return "";

        if (IsLgd2)
          logodata = logofile.LogoData;
        else if (IsLdp || IsLdp2)
          logodata = LogoFileUtil.GetLogoData(Name, logofile);
        else
          return "";
        if (logodata.Count == 0)
          return "";
      }

      filemanager.Save(savepath, logodata);
      return savepath;
    }

  }//class LgdFile



  static class ValidFileName
  {
    /// <summary>
    /// ファイル名に使えない文字を置換
    /// </summary>
    public static string Validate(string filename)
    {
      foreach (char invalid in Path.GetInvalidFileNameChars())
        filename = filename.Replace(invalid, '_');
      return filename;
    }
  }





}//namespace
