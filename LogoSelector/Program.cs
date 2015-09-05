using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogoSelector
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      //テスト引数
      //args = new string[] { "CBC0123", "", "", };
      //args = new string[] { "あぁアァｱｲｳＡAａa", "", "", };

      //引数
      string ch = (1 <= args.Count()) ? args[0] : "";      //チャンネル
      string shortCh = "";
      string nonNumCh = "";
      string program = (2 <= args.Count()) ? args[1] : ""; //プログラム
      string tspath = (3 <= args.Count()) ? args[2] : "";  //tsパス

      //設定ファイル読込み
      var setting = new setting();
      setting = setting.Load();

      //大文字全角ひらがなに変換
      ch = util.ConvertUWH(ch);
      shortCh = (4 < ch.Length) ? ch.Substring(0, 4) : ch; //前４文字
      nonNumCh = ch;
      nonNumCh = util.RemoveNumber(nonNumCh);              //数字除去
      nonNumCh = util.RemoveSymbol(nonNumCh);              //記号除去
      program = util.ConvertUWH(program);
      tspath = util.ConvertUWH(tspath);

      //コメント対象かをチェック
      string comment = AddComment_byKeyword(ch, setting.SearchSet_AddComment);

      //ロゴパラメーター検索
      string[] Logo_Param = null;
      var FoundParam = new Func<string[], bool>((ary) => (ary != null && ary[1] != ""));           //チェック用 Func<bool>

      //  指定キーワードから
      if (FoundParam(Logo_Param) == false)
        Logo_Param = SearchParam_byKeyword(ch, setting.SearchSet_byKeyword, setting.LogoDir);

      //  フォルダから
      if (FoundParam(Logo_Param) == false)
      {
        //各パターンで検索
        //    比較対象であるLogoDir内の各ファイル名に対して、
        //    ・前４文字
        //    ・数字、記号除去
        //　　はしていない。
        var logo_Param_normal = SearchParam_fromDirectory(ch, program, setting.LogoDir);           //チャンネル名
        var logo_Param__short = SearchParam_fromDirectory(shortCh, program, setting.LogoDir);      //短縮名
        var logo_Param_nonNum = SearchParam_fromDirectory(nonNumCh, program, setting.LogoDir);     //数字、記号抜き

        //パラメーターが見つかった？
        if (FoundParam(logo_Param_normal))
        {
          Logo_Param = logo_Param_normal;
        }
        else if (setting.EnableShortCH && FoundParam(logo_Param__short))
        {
          Logo_Param = logo_Param__short;
        }
        else if (setting.EnableNonNumCH && FoundParam(logo_Param_nonNum))
        {
          Logo_Param = logo_Param_nonNum;
        }
      }

      Logo_Param = Logo_Param ?? new string[] { "", "", };                      //nullなら空文字を入れる

      if (FoundParam(Logo_Param) == false) comment += setting.NotFoundParam;    //NotFoundParamのコメント追加

      //結果表示
      Console.WriteLine(Logo_Param[0]);
      Console.WriteLine(Logo_Param[1]);
      Console.WriteLine(comment);
    }

    #region 検索

    /// <summary>
    /// 指定キーワードがあればコメント追加
    /// </summary>
    /// <remarks>
    ///  SearchSet_AddComment[0] = keyword
    ///                      [1] = additional comment
    /// </remarks>
    private static string AddComment_byKeyword(string ch, List<List<string>> SearchSet)
    {
      var addComment = "";

      foreach (var oneset in SearchSet)
      {
        string keyword = util.ConvertUWH(oneset[0]);       //大文字全角ひらがなに変換
        string comment = oneset[1];

        //keywordがchに含まれているか？
        int found = ch.IndexOf(keyword);
        if (0 <= found)
          addComment += comment + " ";
      }

      return addComment;
    }

    /// <summary>
    /// 指定キーワードからパラメーター検索
    /// </summary>
    /// <remarks>
    ///   SearchSet_byKeyword[0] = keyword
    ///                      [1] = logo name
    ///                      [2] = param name
    /// </remarks>
    private static string[] SearchParam_byKeyword(string ch, List<List<string>> SearchSet, List<string> LogoDir)
    {
      foreach (var curdir in LogoDir)
      {
        Directory.SetCurrentDirectory(curdir);

        foreach (var oneset in SearchSet)
        {
          string keyword = util.ConvertUWH(oneset[0]);       //大文字全角ひらがなに変換
          string logo = oneset[1];
          string param = oneset[2];

          //keywordがchに含まれているか？
          int found = ch.IndexOf(keyword);

          if (0 <= found)
          {
            //ファイルチェック
            var exist_logo = File.Exists(logo) ? logo : "";
            var exist_param = File.Exists(param) ? param : "";

            return new string[] { exist_logo, exist_param, };
          }
        }
      }
      return null;
    }

    #region ロゴフォルダ内を検索

    /// <summary>
    /// ロゴフォルダ内からパラメーター検索
    /// </summary>
    private static string[] SearchParam_fromDirectory(string ch, string program, List<string> LogoDir)
    {
      if (ch == "") return null;
      if (LogoDir.Count == 0) return null;

      //Search lgd
      //  ch名からロゴファイル検索
      var lgdPath = new Func<string>(() =>
      {
        foreach (var logodir in LogoDir)
        {
          var lgdfiles = Directory.GetFiles(logodir, "*.lgd");

          foreach (var lgdpath in lgdfiles)
          {
            string lgdname = Path.GetFileName(lgdpath);
            lgdname = util.ConvertUWH(lgdname);            //大文字全角ひらがなで比較
            int found = lgdname.IndexOf(ch);

            if (0 <= found) return lgdpath;                //変形前のパスを返す。
          }
        }

        return "";
      })();

      //not found lgd ?
      if (lgdPath == "") return null;                      //lgdが見つからない。

      //
      //Search param
      //  lgdファイル名からパラメーター検索
      //  複数見つかれば番組名の含まれているファイルを優先
      var paramPath = new Func<string, string>((lgdpath) =>
      {
        var paramlist = new List<string>();
        string lgdname = Path.GetFileName(lgdpath);

        foreach (var logodir in LogoDir)
        {
          //ファイル検索
          var paramfiles = Directory.GetFiles(logodir, lgdname + "*.autoTune.param");
          if (paramfiles.Count() == 0) continue;

          //番組名が含まれているか？
          foreach (var path in paramfiles)
          {
            //パス　→　ファイル名
            string name = Path.GetFileName(path);                 //           ＣＢＣ.2014.lgd.ニュース.COMMENT.autoTune.param
            //lgdname除去
            name = name.Replace(lgdname, "");                     //                          .ニュース.COMMENT.autoTune.param
            //.autoTune.param除去
            name = Regex.Replace(name,                            //                          .ニュース.COMMENT
                        @".autoTune.param$", "", RegexOptions.IgnoreCase);
            //先頭のピリオド除去
            name = Regex.Replace(name, @"\.+(.*)", "$1");         //                           ニュース.COMMENT
            //先頭からピリオド以外取得、ピリオドがあればそこまで
            name = Regex.Replace(name, @"([^.]*)?\.?(.*)", "$1"); //                           ニュース

            name = util.ConvertUWH(name);

            if (name == "") continue;

            int found = program.IndexOf(name);
            if (0 <= found) return path;                          //番組名が含まれているparam
          }

          //番組名は見つからない。paramの最初の要素を返す。
          return paramfiles[0];
        }

        //paramが見つからない。
        return "";
      })(lgdPath);

      //not found param ?
      if (paramPath == "") return new string[] { lgdPath, "" };  //paramだけ見つからない。

      //lgd, param両方見つかった。
      return new string[] { lgdPath, paramPath };
    }

    #endregion ロゴフォルダ内を検索

    #endregion 検索

    #region util 文字形式の変換

    private class util
    {
      /// <summary>
      /// 小文字半角カタカナに変換
      /// </summary>
      /// <remarks>
      /// ”半角ひら”は存在しないのでカタカナにした後で半角にする。
      /// ”あ→ｱ”に一度で変換できない　（全角ひら　→　半角カナ）
      /// </remarks>
      public static string ConvertLNK(string text)
      {
        text = Strings.StrConv(text, VbStrConv.Lowercase, 0x0411);
        text = Strings.StrConv(text, VbStrConv.Katakana, 0x0411);    //あ→ア
        text = Strings.StrConv(text, VbStrConv.Narrow, 0x0411);      //ア→ｱ
        return text;
      }

      /// <summary>
      /// 大文字全角ひらがなに変換
      /// </summary>
      /// <remarks>
      /// ”半角ひら”は存在しないので全角にした後でひらがなにする。
      /// ”ｱ→あ”に一度で変換できない　（半角カナ　→　全角ひら）
      /// </remarks>
      public static string ConvertUWH(string text)
      {
        text = Strings.StrConv(text, VbStrConv.Uppercase, 0x0411);
        text = Strings.StrConv(text, VbStrConv.Wide, 0x0411);        //ｱ→ア
        text = Strings.StrConv(text, VbStrConv.Hiragana, 0x0411);    //ア→あ
        return text;
      }

      /// <summary>
      /// 記号削除
      /// </summary>
      public static string RemoveSymbol(string text)
      {
        //半角
        var symbol_N = @" !\""#$%&'()=-~^|\\`@{[}]*:+;_?/>.<,・";
        //全角
        var symbol_W = @"・☆〇×￣【】" + Strings.StrConv(symbol_N, VbStrConv.Wide, 0x0411);

        text = Regex.Replace(text, @"[" + symbol_N + "]", "");
        text = Regex.Replace(text, @"[" + symbol_W + "]", "");
        return text;
      }

      /// <summary>
      /// 数字削除
      /// </summary>
      public static string RemoveNumber(string text)
      {
        return Regex.Replace(text, @"\d", "");
      }
    }

    #endregion util 文字形式の変換

    #region 設定

    private class setting
    {
      public List<string> LogoDir { get; private set; }
      public List<List<string>> SearchSet_byKeyword { get; private set; }
      public List<List<string>> SearchSet_AddComment { get; private set; }
      public string NotFoundParam { get; private set; }
      public bool EnableShortCH { get; private set; }
      public bool EnableNonNumCH { get; private set; }

      //ファイル読込み
      public static setting Load()
      {
        //initialize
        var setting = new setting();
        setting.LogoDir = new List<string>();
        setting.SearchSet_byKeyword = new List<List<string>>();
        setting.SearchSet_AddComment = new List<List<string>>();
        setting.NotFoundParam = "";
        setting.EnableShortCH = false;
        setting.EnableNonNumCH = false;

        //ファイル読込み
        var txtpath = new Func<string>(() =>
        {
          var AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
          var AppDir = Path.GetDirectoryName(AppPath);
          var AppName = Path.GetFileNameWithoutExtension(AppPath);
          var path = Path.Combine(AppDir, AppName + ".txt");
          return path;
        })();

        if (File.Exists(txtpath) == false)
        {
          File.WriteAllText(txtpath, SettingText.Default, Encoding.UTF8);
        }

        var readfile = File.ReadAllLines(txtpath);

        //コメント削除
        readfile = (from line in readfile
                    let found = line.IndexOf("//")
                    let trimComm = (0 <= found) ? line.Substring(0, found) : line
                    select trimComm.Trim()
                    ).ToArray();

        //
        //改行 or [セクション名]で分割
        //
        var Blocks = new List<List<string>>();
        var oneBlock = new List<string>();       //改行ごとの塊をoneBlockとする

        foreach (var line in readfile)
        {
          //文字列がある？
          if (line != string.Empty)
          {
            //セクション名？
            var isSection = Regex.Match(line, @"^\[.*\]$", RegexOptions.IgnoreCase).Success;

            if (isSection)
            {
              //前のブロックを追加してからセクション名追加
              if (0 < oneBlock.Count) Blocks.Add(oneBlock);
              oneBlock = new List<string>();
              Blocks.Add(new List<string>() { line });
            }
            else
              //ブロック追加
              oneBlock.Add(line);
          }
          else
          {
            //空白行、次のブロック検索へ
            if (0 < oneBlock.Count) Blocks.Add(oneBlock);
            oneBlock = new List<string>();
          }
        }
        if (0 < oneBlock.Count) Blocks.Add(oneBlock);

        //
        //セクションごとのブロック取り出し
        //
        var TakeSection = new Func<string, int, List<List<string>>>(
          (section, blksize) =>
          {
            //セクション名までスキップ      [LogoDir]    [LogoParam]
            var section1 = Blocks.SkipWhile((block) =>
                              Regex.Match(block[0], @"^\[" + section + @"\]$"
                                  , RegexOptions.IgnoreCase).Success == false).ToList();

            //セクション名削除
            if (0 < section1.Count) section1.RemoveAt(0);

            //次のセクション名までを抽出
            var section2 = section1.TakeWhile((block) =>
                              Regex.Match(block[0], @"^\[.*\]$"
                                  , RegexOptions.IgnoreCase).Success == false).ToList();

            //ブロックの行数を制限
            //指定のサイズでないブロックは排除
            //blksize==-1で制限しない
            var section3 = (0 < blksize)
                                  ? section2.Where((block) => block.Count == blksize)
                                  : section2;

            return section3.ToList();
          });

        //
        //セクション取り出し
        //
        var sectionLogoDir = TakeSection("LogoDir", -1).SelectMany(blk => blk).ToList();
        var sectionKeyword = TakeSection("SearchParam", 3);
        var sectionComment = TakeSection("AddComment", 2);
        var sectionCommNotFound = TakeSection("NotFoundParam", -1).SelectMany(blk => blk).ToList();
        var sectionOption = TakeSection("Option", -1).SelectMany(blk => blk).ToList();

        //
        //読み取り結果
        //
        setting.LogoDir = sectionLogoDir.Where((path) => Directory.Exists(path)).ToList();
        setting.SearchSet_byKeyword = sectionKeyword;
        setting.SearchSet_AddComment = sectionComment;
        setting.NotFoundParam = (0 < sectionCommNotFound.Count) ? sectionCommNotFound[0] : "";

        setting.EnableShortCH = sectionOption.Any(
          (opt) =>
            Regex.Match(opt, @"^AppendSearch_ShortCh$", RegexOptions.IgnoreCase).Success
            );

        setting.EnableNonNumCH = sectionOption.Any(
          (opt) =>
            Regex.Match(opt, @"^AppendSearch_NonNumCh$", RegexOptions.IgnoreCase).Success
            );

        #region 結果表示

        /*
        //
        //show result
        //
        Console.Error.WriteLine("[LogoDir]");
        Console.Error.WriteLine(LogoDir);
        Console.Error.WriteLine();
        Console.Error.WriteLine("[LogoParam]");
        foreach (var block in KeywordSearchSet)
        {
          foreach (var one in block)
            Console.Error.WriteLine(one);
          Console.Error.WriteLine();
        }
        Console.Error.WriteLine("[Comment]");
        foreach (var block in CommentSearchSet)
        {
          foreach (var one in block)
            Console.Error.WriteLine(one);
          Console.Error.WriteLine();
        }
        Console.Error.WriteLine("[Option]");
        Console.Error.WriteLine("EnableShortCH = " + EnableShortCH);
        */

        #endregion 結果表示

        return setting;
      }
    }

    #endregion 設定

    #region 初期設定ファイル

    private static class SettingText
    {
      /*
       * 設定ファイルの形式
       *
       * [セクション名]
       * ブロック１
       * ブロック１
       * ブロック１
       *
       * ブロック２
       * ブロック２
       * ブロック２
       *
       * [セクション名]
       * ブロック３
       * ブロック３
       *
      */

      public const string Default =
@"
===============================================================================
### LogoSelectorについて

  ロゴ、パラメーターを検索しフルパスを表示します。

　
### 処理の流れ

1. 引数からチャンネル名、プログラム名を受け取る。

2. [SearchParam]のキーワードがチャンネル名に含まれているかチェック。

3. キーワードが含まれているなら、
    * ロゴ、パラメーターをフルパスとしてチェック
    * [LogoDir]内をファイル名としてチェック
    * ファイルが見つかればパス、コメントを表示して終了。

4. [LogoDir]内のlgdファイルを列挙して、
    * チャンネル名が含まれているlgdファイルを探す。
    * lgdファイル名と一致するparamファイルを探す。
    * ファイルが見つかればパス、コメントを表示して終了。


### 他

・全角半角、大文字小文字、ひらがなカタカナの違いは無視される。

・//以降の文字と前後の空白は無視します。

・UTF-8 bom


===============================================================================

[LogoDir]
C:\LogoData1                         //lgdファイルのあるフォルダ
D:\LogoData2


[SearchParam]
TokaiTV                             //keyword
東海.lgd                            //logo
東海.lgd.autoTune.param             //param
                                    //区切りとして改行

BSJapan
c:\logo\BS-Japan_2015.lgd           //ファイル名又はフルパス
c:\logo\BS-Japan.lgd.ニュース.autoTune.param


[AddComment]
//BS                                //keyword  チャンネル名にBSを含んでいるとコメント追加
//-midprc 0                         //comment


[NotFoundParam]
//-Abort_pfAdapter                  //パラメーターが見つからないときにコメント追加


[Option]
//AppendSearch_ShortCH              //チャンネル名の前４文字の文字列でも検索する
//AppendSearch_NonNumCH             //チャンネル名から数字・記号を抜いた文字列でも検索する

";
    }

    #endregion 初期設定ファイル
  }
}