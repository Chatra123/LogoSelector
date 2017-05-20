/*
 *  LogoSelector
 *    ロゴ、パラメーターを検索してフルパスを返す。
 * 
 * 
 *  引数で
 *    - args[0] = チャンネル名
 *    - args[1] = プログラム名
 *    - args[2] = ＴＳフルパス
 *   として受けとり、
 * 
 *   標準出力の
 *    - １行目  ロゴパス
 *    - ２行目  パラメーターパス
 *    - ３行目  コメント
 *    を出力 
 */

/*
 *  リリース出力の exeは dllをマージ済み
 * 　　プロジェクトのプロパティ　→　ビルドイベント
 *  あらかじめ ILMerge.exeが必要。
 *  ILMergeの処理に時間がかかるためデバッグ出力ではマージしない
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace LogoSelector
{
  using OctNov.Text;


  internal class Program
  {
    private static void Main(string[] args)
    {
      //テスト引数
      //args = new string[] { "あぁアァｱｲｳＡAａa", "", @"", };
      //args = new string[] { "", "", 
      //                      @"", };



      //例外を捕捉
      AppDomain.CurrentDomain.UnhandledException += OctNov.Excp.ExceptionInfo.OnUnhandledException;


      //相対パスのロゴフォルダをアプリフォルダから辿れるようにする。
      string AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string AppDir = System.IO.Path.GetDirectoryName(AppPath);
      Directory.SetCurrentDirectory(AppDir);
      var setting = new Setting_File();


      string Ch = "", Program = "", TsPath = "";
      {
        if (args.Count() == 0) return;
        if (1 <= args.Count()) Ch = args[0];
        if (2 <= args.Count()) Program = args[1];
        if (3 <= args.Count()) TsPath = args[2];
        Ch = StrConv.ToUWH(Ch);
        Program = StrConv.ToUWH(Program);
      }


      string[] Logo_Param = null;
      //引数からチャンネル名を取得済み
      if (Ch != "")
      {
        //指定キーワードから
        Logo_Param = Logo_Param ?? Searcher.GetLogo_byKeyword(Ch,
                                                              setting.SearchSet_byKeyword,
                                                              setting.LogoDir);
        //ロゴフォルダから
        Logo_Param = Logo_Param ?? Searcher.GetLogo_fromDir(Ch,
                                                            setting.LogoDir);
      }

      //見つからなかったら TsNameからチャンネル名を探す
      if (Logo_Param == null)
      {
        //ファイル名が重複したときの -(1) を除去
        string TsName;
        TsName = Path.GetFileNameWithoutExtension(TsPath);
        TsName = new Regex(@"^(.*)(-\(\d+\))$").Replace(TsName, "$1");
        TsName = StrConv.ToUWH(TsName);
        //TSName  -->  チャンネル名
        Ch = Searcher.GetCH_fromTsName(TsName,
                                       setting.LogoDir);
        if (Ch != "")
        {
          //指定キーワードから
          Logo_Param = Logo_Param ?? Searcher.GetLogo_byKeyword(Ch,
                                                                setting.SearchSet_byKeyword,
                                                                setting.LogoDir);
          //ロゴフォルダから
          Logo_Param = Logo_Param ?? Searcher.GetLogo_fromDir(Ch,
                                                              setting.LogoDir);
        }
      }
      //Not Found
      Logo_Param = Logo_Param ?? new string[] { "", "" };


      //コメント
      string comment = "";
      {
        if (Ch != "")
        {
          comment += Searcher.AddComment_byKeyword(Ch,
                                                   setting.SearchSet_AddComment);
        }
        //Not Found Comment
        if (Logo_Param[0] == "")
          comment += " " + setting.Comment_NotFoundLogo;
        if (Logo_Param[1] == "")
          comment += " " + setting.Comment_NotFoundParam;
      }


      //結果表示
      Console.WriteLine(Logo_Param[0]);
      Console.WriteLine(Logo_Param[1]);
      Console.WriteLine(comment);
      Console.Error.WriteLine();
    }




  }
}