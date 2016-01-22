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

  class Searcher
  {

    /// <summary>
    /// 指定キーワードがあればコメント追加
    /// </summary>
    /// <remarks>
    ///  SearchSet[0] = keyword
    ///           [1] = extra comment
    /// </remarks>
    public static string AddComment_byKeyword(string CH, List<List<string>> SearchSet)
    {
      string comment = "";

      foreach (var set in SearchSet)
      {
        string keyword = StrConverter.ToUWH(set[0]);       //大文字全角ひらがなに変換
        string extra_comment = set[1];

        //keywordが含まれているか？
        if (CH.Contains(keyword))
          comment += extra_comment + " ";
      }

      return comment;
    }


    /// <summary>
    /// 指定キーワードからパラメーター検索
    /// </summary>
    /// <remarks>
    ///   SearchSet[0] = keyword
    ///            [1] = logo name
    ///            [2] = param name
    /// </remarks>
    public static string[] byKeyword(string CH, List<List<string>> SearchSet, List<string> LogoDir)
    {
      foreach (var set in SearchSet)
      {
        string keyword = StrConverter.ToUWH(set[0]);       //大文字全角ひらがなに変換
        string logo = set[1];
        string param = set[2];

        //keywordが含まれているか？
        if (CH.Contains(keyword))
        {
          //ファイルが存在しているか？
          foreach (var curdir in LogoDir)
          {
            Directory.SetCurrentDirectory(curdir);
            try
            {
              var fi_lgd = new FileInfo(logo);
              var fi_param = new FileInfo(param);

              if (fi_lgd.Exists || fi_param.Exists)
                return new string[] { fi_lgd.FullName, fi_param.FullName, };
            }
            catch
            {
              // next directory
              continue;
            }
          }
        }

      }
      return null;
    }



    /// <summary>
    /// フォルダ内からパラメーター検索
    /// </summary>
    public static string[] fromDirectory(
                                          string Ch,
                                          bool appendSearch_ShortCH,
                                          bool appendSearch_NonNumCH,
                                          List<string> logoDir
                                        )
    {
      string shortCh, nonNumCh;
      {
        shortCh = Ch;
        shortCh = (4 < shortCh.Length)
                     ? shortCh.Substring(0, 4) : shortCh;  //前４文字
        nonNumCh = Ch;
        nonNumCh = StrConverter.RemoveNumber(nonNumCh);    //数字除去
        nonNumCh = StrConverter.RemoveSymbol(nonNumCh);    //記号除去
      }

      //チェック用関数
      var FoundItem = new Func<string, bool>((item) => string.IsNullOrEmpty(item) == false);

      //ファイル収集
      var filer = new FileCollector();
      filer.Collect(logoDir);


      //ロゴ検索
      string logo = "", logo_FullName = "";
      {
        //各パターンで検索
        //  ・通常のチャンネル名
        //  ・前４文字
        //  ・数字、記号除去
        var logo_normal = GetFromList(Ch, filer.Lgd_NameList);
        var logo__short = GetFromList(shortCh, filer.Lgd_NameList);
        var logo_nonNum = GetFromList(nonNumCh, filer.Lgd_NameList);

        //ロゴが見つかった？
        if (FoundItem(logo_normal))
        {
          logo = logo_normal;
        }
        else if (appendSearch_ShortCH && FoundItem(logo__short))
        {
          logo = logo__short;
        }
        else if (appendSearch_NonNumCH && FoundItem(logo_nonNum))
        {
          logo = logo_nonNum;
        }
        else
          return new string[] { "", "", "" };  //not found logo

        // *.lgd　→　fullpathを取得
        // *.ldp　→　lgdファイルを作成してfullpathを取得
        logo_FullName = filer.GetFullName_Lgd(logo);
      }

      //パラメーター検索
      string param = "", param_FullName = "";
      {
        var param_nomal = GetFromList(logo, filer.Param_NameList);

        //パラメーターが見つかった？
        if (FoundItem(param_nomal))
        {
          param = param_nomal;
        }
        else
          return new string[] { logo_FullName, "", "" };  //not found param

        param_FullName = filer.GetFullName_Param(param);
      }

      //found
      return new string[] { logo_FullName, param_FullName, "" };
    }



    /// <summary>
    /// list内にtargetが含まれているか？
    /// </summary>
    public static string GetFromList(string target, List<string> list)
    {
      if (target == "") return null;
      if (list.Count == 0) return null;

      target = StrConverter.ToUWH(target);               //大文字全角ひらがなで比較

      list = list.Where((item) =>
      {
        item = StrConverter.ToUWH(item);
        return item.Contains(target);
      }).ToList();

      return list.FirstOrDefault();
    }
  }





}