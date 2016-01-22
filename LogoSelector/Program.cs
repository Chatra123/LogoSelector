using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;


namespace LogoSelector
{
  using OctNov.Text;

  internal class Program
  {
    private static void Main(string[] args)
    {
      //テスト引数
      //args = new string[] { "tokai", "", "", };
      //args = new string[] { "あぁアァｱｲｳＡAａa", "", "", };


      //設定ファイル読込み
      var setting = new Setting_File();
      setting = Setting_File.Load();

      //引数
      if (args.Count() == 0) return;
      string ch = StrConverter.ToUWH(args[0]);    //チャンネル


      //コメント対象かをチェック
      string comment = Searcher.AddComment_byKeyword(ch, setting.SearchSet_AddComment);


      //
      //ロゴ、パラメーター検索
      //
      string[] Logo_Param = null;
      {
        //指定キーワードから
        Logo_Param = Searcher.byKeyword(ch, setting.SearchSet_byKeyword, setting.LogoDir);

        //フォルダから
        if (Logo_Param == null)
        {
          Logo_Param = Searcher.fromDirectory(
                                              ch,
                                              setting.Enable_ShortCH,
                                              setting.Enable_NonNumCH,
                                              setting.LogoDir
                                             );
        }

        //nullなら空文字を入れる
        Logo_Param = Logo_Param ?? new string[] { "", "", };
      }


      //NotFound コメント追加
      if (Logo_Param[0] == "")
        comment += " " + setting.Comment_NotFoundLogo;

      if (Logo_Param[1] == "")
        comment += " " + setting.Comment_NotFoundParam;

      //結果表示
      Console.WriteLine(Logo_Param[0]);
      Console.WriteLine(Logo_Param[1]);
      Console.WriteLine(comment);
      Console.WriteLine();

    }




  }
}