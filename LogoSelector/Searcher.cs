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
    /// TSName  -->  チャンネル名
    /// </summary>
    public static string GetCH_fromTsName(
                                           string tsName,
                                           bool enable_ShortCH,
                                           bool enable_NonNumCH,
                                           List<string> logoDir
                                         )
    {
      //lgdファイル名からチャンネル名一覧を収集
      //                  normal     short      nonNum
      List<string> chList_nm, chList_sh, chList_nN;
      {
        var filer = new LgdFiler();
        filer.Collect(logoDir);
        chList_nm = filer.Lgd_ChList;

        chList_nm = NameConv.GetUWH(chList_nm);
        chList_sh = NameConv.GetShort(chList_nm);
        chList_nN = NameConv.GetNonNum(chList_nm);
      }


      //TSファイル名をスペースで分割
      //                    normal       short        nonNum
      List<string> nameList_nm, nameList_sh, nameList_nN;
      {
        nameList_nm = tsName.Split()
                            .Where(name => name != "")
                             //番組タイトルよりも放送局名を先頭にして優先度をあげる。
                            .OrderBy((name) => name.Length)
                            .ToList();

        nameList_nm = NameConv.GetUWH(nameList_nm);
        nameList_sh = NameConv.GetShort(nameList_nm);
        nameList_nN = NameConv.GetNonNum(nameList_nm);
      }


      string Ch;
      {
        string Ch_nm = GetKey_forward(nameList_nm, chList_nm);
        string Ch_sh = GetKey_forward(nameList_sh, chList_sh);
        string Ch_nN = GetKey_forward(nameList_nN, chList_nN);
        Ch =
          Ch_nm != "" ? Ch_nm :
          Ch_sh != "" && enable_ShortCH ? Ch_sh :
          Ch_nN != "" && enable_NonNumCH ? Ch_nN :
          "";
      }

      return Ch;
    }

    /// <summary>
    /// nameList内に含まれているkeyを取得
    /// 前方一致  forward match
    /// </summary>
    private static string GetKey_forward(List<string> nameList, List<string> keyList)
    {
      foreach (var name in nameList)
        foreach (var key in keyList)
          if (name.IndexOf(key) == 0)
            return key;

      return "";
    }


    /// <summary>
    /// 指定キーワードで検索
    /// </summary>
    /// <remarks>
    ///  SearchSet[0] = keyword
    ///           [1] = logo name
    ///           [2] = param name
    /// </remarks>
    public static string[] GetLogo_byKeyword(string Ch, List<List<string>> SearchSet, List<string> LogoDir)
    {
      string backup = Directory.GetCurrentDirectory();

      foreach (var set in SearchSet)
      {
        string keyword = NameConv.GetShort(set[0]);
        string logo = set[1];
        string param = set[2];

        if (Ch.Contains(keyword))
        {
          foreach (var dir in LogoDir)
          {
            if (Directory.Exists(dir) == false)
              continue;
            try
            {
              var fi_lgd = new FileInfo(logo);
              var fi_param = new FileInfo(param);

              if (fi_lgd.Exists || fi_param.Exists)
              {
                Directory.SetCurrentDirectory(backup);
                return new string[] { fi_lgd.FullName, fi_param.FullName, };
              }
            }
            catch
            {
              continue;
            }
          }
        }

      }

      Directory.SetCurrentDirectory(backup);
      return null;
    }



    /// <summary>
    /// ロゴフォルダ内から検索
    /// </summary>
    public static string[] GetLogo_fromDir(
                                            string Ch,
                                            bool enable_ShortCH,
                                            bool enable_NonNumCH,
                                            List<string> logoDir
                                          )
    {
      string shortCh = NameConv.GetShort(Ch);
      string nonNumCh = NameConv.GetNonNum(Ch);

      var filer = new LgdFiler();
      filer.Collect(logoDir);

      //ロゴ検索
      string logo = "", logo_FullName = "";
      {
        //各パターンで検索
        //    normal  short  nonNum
        //  ・通常のチャンネル名
        //  ・前４文字
        //  ・数字、記号除去
        List<string> lgdList = filer.Lgd_NameList;
        var lgdList_nm = NameConv.GetUWH(lgdList);

        int hit_nm = GetIndex_partial(lgdList_nm, Ch);
        int hit_sh = GetIndex_partial(lgdList_nm, shortCh);
        int hit_nN = GetIndex_partial(lgdList_nm, nonNumCh);

        int hit =
          hit_nm != -1 ? hit_nm :
          hit_sh != -1 && enable_ShortCH ? hit_sh :
          hit_nN != -1 && enable_NonNumCH ? hit_nN :
           -1;
        if (hit == -1)
          return null;  //not found

        // *.lgd　→　FullNameを取得
        // *.ldp　→　lgdファイルを作成してFullNameを取得
        logo = lgdList[hit];  // UWH変換前の値
        logo_FullName = filer.GetFullName_Lgd(logo);
      }


      //パラメーター検索
      string param_FullName = "";
      {
        List<string> paramList = filer.Param_NameList;

        int hit = GetIndex_partial(paramList, logo);
        if (hit == -1)
          return new string[] { logo_FullName, "" };  //not found

        string param = paramList[hit];
        param_FullName = filer.GetFullName_Param(param);
      }

      //found
      return new string[] { logo_FullName, param_FullName };
    }

    /// <summary>
    /// keyを含んでいる要素のindexを取得
    /// 部分一致  partial match
    /// </summary>
    private static int GetIndex_partial(List<string> list, string key)
    {
      int index = 0;
      foreach (var elm in list)
      {
        if (elm.Contains(key))
          return index;
        else
          index++;
      }
      return -1;
    }



    /// <summary>
    /// 指定キーワードがあればコメント追加
    /// </summary>
    /// <remarks>
    ///  SearchSet[0] = keyword
    ///           [1] = extra comment
    /// </remarks>
    public static string AddComment_byKeyword(string Ch, List<List<string>> SearchSet)
    {
      string comment = " ";

      foreach (var set in SearchSet)
      {
        string keyword = NameConv.GetUWH(set[0]);
        string extra_comment = set[1];

        if (Ch.Contains(keyword))
          comment += extra_comment + " ";
      }

      return comment;
    }



  }

}