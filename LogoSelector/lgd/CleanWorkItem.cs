using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace LGLauncher
{

  /// <summary>
  /// 削除処理　実行部
  /// </summary>
  class FileCleaner
  {
    /// <summary>
    /// ファイル削除
    /// </summary>
    /// <param name="nDaysBefore">Ｎ日前のファイルを削除対象にする。</param>
    /// <param name="directory">ファイルを探すフォルダ。　サブフォルダ内も対象</param>
    /// <param name="searchKey">ファイル名に含まれる文字。ワイルドカード可 * </param>
    /// <param name="ignoreKey">除外するファイルに含まれる文字。ワイルドカード不可 × </param>
    public void Delete_File(double nDaysBefore, string directory,
                            string searchKey, string ignoreKey = null)
    {
      if (Directory.Exists(directory) == false) return;
      Thread.Sleep(500);

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
    public void Delete_EmptyDir(string parent_directory)
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


}
