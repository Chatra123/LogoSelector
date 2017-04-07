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
    /// 指定キーワードで検索
    ///   Chにkeywordが含まれていたらlogo,param pathを取得
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
        string keyword = StrConv.ToUWH(set[0]);
        string logo = set[1];
        string param = set[2];

        //指定キーワードを検索
        if (Ch.Contains(keyword))
        {
          foreach (var dir in LogoDir)
          {
            if (Directory.Exists(dir) == false)
              continue;
            try
            {
              //logo name、param nameを検索
              Directory.SetCurrentDirectory(dir);
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
    public static string[] GetLogo_fromDir(string Ch, List<string> logoDir)
    {
      var lgdFiler = new LgdFiler(logoDir);
      lgdFiler.Collect();

      //ロゴ検索
      // - Chがlgdファイル名から取得したチャンネル一覧にあるか？
      // - hitしたlgdのチャンネル名からGetLgd_FullNameを取得
      string lgd_ch = "", lgd_FullName = "";
      {
        //各パターンで検索
        //  ・normal  通常のチャンネル名
        //  ・nonNum  数字、記号除去
        string Ch_nm = Ch;
        string Ch_nN = StrConv.ToNonNum(Ch_nm);
        IEnumerable<string> lgd_Ch_nm = StrConv.ToUWH(lgdFiler.Lgd_ChList);

        int hit_nm = GetKeyIndex(Ch_nm, lgd_Ch_nm);
        int hit_nN = GetKeyIndex(Ch_nN, lgd_Ch_nm);
        int hit = hit_nm != -1 ? hit_nm :
                  hit_nN != -1 ? hit_nN :
                  -1;
        if (hit == -1)
          return null;  //not found

        //lgdファイル名から取得したチャンネル名、UWH変換前の値
        lgd_ch = lgdFiler.Lgd_ChList[hit];
        lgd_FullName = lgdFiler.GetLgd_FullName(lgd_ch);
      }

      //パラメーター検索
      string param_FullName = lgdFiler.GetParam_FullName(lgd_ch);

      //found
      return new string[] { lgd_FullName, param_FullName };
    }

    /// <summary>
    /// keyを含んでいるindexを取得
    /// 前方一致  forward match
    /// </summary>
    private static int GetKeyIndex(string Ch, IEnumerable<string> Lgd_ChKey)
    {
      int index = 0;
      foreach (var key in Lgd_ChKey)
      {
        if (Ch.IndexOf(key) == 0)
          return index;
        else
          index++;
      }
      return -1;
    }



    /// <summary>
    /// TSName  -->  チャンネル名
    /// </summary>
    public static string GetCH_fromTsName(string tsName, List<string> logoDir)
    {
      //チャンネル名候補
      //  TSファイル名をスペースで分割
      //  ・normal  通常のチャンネル名
      //  ・nonNum  数字、記号除去
      IEnumerable<string> tsName_nm, tsName_nN;
      {
        tsName_nm = tsName.Split()
                            .Where(name => name != "")
                            //番組タイトルよりも放送局名を先頭にして優先度をあげる。
                            //放送局名のほうが短い
                            .OrderBy((name) => name.Length)
                            .ToList();
        tsName_nm = StrConv.ToUWH(tsName_nm);
        tsName_nN = StrConv.ToNonNum(tsName_nm);
      }

      //lgdファイル名から全チャンネル名を収集
      IEnumerable<string> lgdCh_key;
      {
        var lgdFiler = new LgdFiler(logoDir);
        lgdFiler.Collect();
        lgdCh_key = StrConv.ToUWH(lgdFiler.Lgd_ChList);
      }

      string Ch;
      {
        string Ch_nm = GetContainsKey(tsName_nm, lgdCh_key);
        string Ch_nN = GetContainsKey(tsName_nN, lgdCh_key);
        Ch = Ch_nm != "" ? Ch_nm :
             Ch_nN != "" ? Ch_nN :
             "";
      }
      return Ch;
    }

    /// <summary>
    /// TSファイル名に含まれているlgdファイル名を取得
    /// 前方一致  forward match
    /// </summary>
    private static string GetContainsKey(IEnumerable<string> tsName, IEnumerable<string> lgdCh_key)
    {
      foreach (var name in tsName)
        foreach (var ch in lgdCh_key)
          if (name.IndexOf(ch) == 0)
            return ch;
      return "";
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
        string keyword = StrConv.ToUWH(set[0]);
        string extra_comment = set[1];
        if (Ch.Contains(keyword))
          comment += extra_comment + " ";
      }
      return comment;
    }



  }

}