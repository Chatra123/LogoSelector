using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Text.RegularExpressions;

#region regionTitle
#endregion

namespace LogoSelector
{
  class Program
  {
    static void Main(string[] args)
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
        Logo_Param = SearchParam_byKeyword(ch, setting.LogoDir, setting.SearchSet_byKeyword);


      //  フォルダから
      if (FoundParam(Logo_Param) == false)
      {
        //各パターンで検索
        //    比較対象であるLogoDir内のファイル名は、
        //    ・大文字全角ひらがなに変換。
        //    ・前４文字・数字記号除去はしていない。
        var logo_Param_normal = SearchParam_fromDirectory(ch, program, setting.LogoDir);            //チャンネル名
        var logo_Param__short = SearchParam_fromDirectory(shortCh, program, setting.LogoDir);       //短縮名
        var logo_Param_nonNum = SearchParam_fromDirectory(nonNumCh, program, setting.LogoDir);      //数字、記号抜き

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



      Logo_Param = Logo_Param ?? new string[] { "", "", };                       //nullなら空文字を入れる

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
    /// <param name="ch"></param>
    /// <param name="SearchSet"></param>
    /// <returns></returns>
    /// <remarks>
    ///  SearchSet_AddComment[0] = keyword
    ///                      [1] = additional comment
    /// </remarks>
    static string AddComment_byKeyword(string ch, List<List<string>> SearchSet)
    {
      var addComment = "";

      foreach (var oneset in SearchSet)
      {
        string keyword = util.ConvertUWH(oneset[0]);       //大文字全角ひらがなに変換
        string comment = oneset[1];

        //keywordがchに含まれているか？
        int found = ch.IndexOf(keyword);
        if (found != -1)                                   //hit
          addComment += comment + " ";
      }

      return addComment;
    }



    /// <summary>
    /// 指定キーワードからパラメーター検索
    /// </summary>
    /// <param name="ch"></param>
    /// <param name="logoDir"></param>
    /// <param name="SearchSet"></param>
    /// <returns></returns>
    /// <remarks>
    ///   SearchSet_byKeyword[0] = keyword
    ///                      [1] = logo name
    ///                      [2] = param name
    /// </remarks>
    static string[] SearchParam_byKeyword(string ch, string logoDir, List<List<string>> SearchSet)
    {
      foreach (var oneset in SearchSet)
      {
        string keyword = util.ConvertUWH(oneset[0]);       //大文字全角ひらがなに変換
        string logo = oneset[1];
        string param = oneset[2];

        //keywordがchに含まれているか？
        int found = ch.IndexOf(keyword);

        if (found != -1)                                   //hit
        {
          //フルパスでファイルチェック、なければlogoDir内のファイル名としてチェック
          var exist_logo = (File.Exists(logo)) ?
                            logo :
                            (File.Exists(Path.Combine(logoDir, logo))) ?
                             Path.Combine(logoDir, logo) : logo;
          var exist_param = (File.Exists(param)) ?
                            param :
                            (File.Exists(Path.Combine(logoDir, param))) ?
                             Path.Combine(logoDir, param)
                            : param;
          return new string[] { exist_logo, exist_param, };
        }

      }
      return null;
    }



    #region SearchParam_fromDirectory
    /// <summary>
    /// ロゴフォルダ内からパラメーター検索
    /// </summary>
    /// <param name="ch"></param>
    /// <param name="program"></param>
    /// <param name="logoDir"></param>
    /// <returns></returns>
    static string[] SearchParam_fromDirectory(string ch, string program, string logoDir)
    {
      if (ch == "") return null;
      if (Directory.Exists(logoDir) == false) return null;



      //Search lgd
      //ch名からロゴファイル検索
      var foundlgd = new Func<string>(() =>
      {
        var lgdfiles = Directory.GetFiles(logoDir, "*.lgd");
        foreach (var lgdpath in lgdfiles)
        {
          string lgdname = Path.GetFileName(lgdpath);
          lgdname = util.ConvertUWH(lgdname);              //大文字全角ひらがなで比較
          int found = lgdname.IndexOf(ch);
          if (found != -1) return lgdpath;                 //変形前のパスを返す。
        }
        return "";
      })();

      //not found lgd ?
      if (foundlgd == "") return null;                     //lgdが見つからない



      //
      //Search param
      //lgdファイル名からパラメーターファイル検索
      //複数見つかれば番組名の含まれているファイルを優先
      var foundparam = new Func<string, string>((lgdpath) =>
      {
        var paramlist = new List<string>();
        string lgdname = Path.GetFileName(lgdpath);
        var paramfiles = Directory.GetFiles(logoDir, lgdname + "*.autoTune.param");
        if (paramfiles.Count() == 0) return null;

        //番組名の含まれているファイルを優先
        foreach (var path in paramfiles)
        {
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
          if (found != -1) return path;                             //変形前のパスを返す。
        }
        //番組名が見つからない。ファイル郡の最初の要素を返す。
        return paramfiles[0];
      })(foundlgd);


      //not found param ?
      if (foundparam == null) return new string[] { foundlgd, "" };  //paramが見つからない




      //lgd, paramが見つかった。
      return new string[] { foundlgd, foundparam };
    }
    #endregion


    #endregion




    #region util 文字形式の変換
    class util
    {
      /// <summary>
      /// 小文字半角カタカナに変換
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static string ConvertLNK(string text)
      {
        text = Strings.StrConv(text, VbStrConv.Katakana, 0x0411);    //あ→ア
        text = Strings.StrConv(text, VbStrConv.Narrow, 0x0411);      //ア→ｱ　　”あ→ｱ”に一度で変換できない
        text = Strings.StrConv(text, VbStrConv.Lowercase, 0x0411);
        return text;
      }
      /// <summary>
      /// 大文字全角ひらがなに変換
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static string ConvertUWH(string text)
      {
        text = Strings.StrConv(text, VbStrConv.Uppercase, 0x0411);
        text = Strings.StrConv(text, VbStrConv.Wide, 0x0411);        //ｱ→ア
        text = Strings.StrConv(text, VbStrConv.Hiragana, 0x0411);    //ア→あ　　”ｱ→あ”に一度で変換できない
        return text;
      }

      /// <summary>
      /// 記号削除
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static string RemoveSymbol(string text)
      {
        var symbol_N = @" !\""#$%&'()=-~^|\\`@{[}]*:+;_?/>.<,・";
        var symbol_W = @"・" + util.ConvertUWH(symbol_N);
        text = Regex.Replace(text, @"[" + symbol_N + "]", "");
        text = Regex.Replace(text, @"[" + symbol_W + "]", "");
        return text;
      }

      /// <summary>
      /// 数字削除
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public static string RemoveNumber(string text)
      {
        return Regex.Replace(text, @"\d", "");
      }

    }
    #endregion




    #region 設定
    class setting
    {
      public string LogoDir { get; private set; }
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
        setting.LogoDir = "";
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

        if (File.Exists(txtpath) == false) { File.WriteAllText(txtpath, SettingText.Default, Encoding.UTF8); }
        var readfile = File.ReadAllLines(txtpath);

        //コメント削除
        var textAll = (from line in readfile
                       let found = line.IndexOf("//")
                       let noComm = (-1 < found) ? line.Substring(0, found) : line
                       select noComm.Trim()
                       ).ToList();



        //
        //改行ごとのブロックに分割
        var Blocks = new List<List<string>>();
        var oneBlock = new List<string>();
        foreach (var line in textAll)
        {
          //文字列がある？
          if (line != string.Empty)
          {
            //セクション名？
            var isSection = Regex.Match(line, @"^\[.*\]$", RegexOptions.IgnoreCase).Success;

            if (isSection)
            {//ブロックを追加してからセクション行追加
              if (0 < oneBlock.Count) Blocks.Add(oneBlock);
              oneBlock = new List<string>();
              Blocks.Add(new List<string>() { line });
            }
            else//ブロック追加
              oneBlock.Add(line);

          }
          else
          {
            //空白行、次のブロックへ
            if (0 < oneBlock.Count) Blocks.Add(oneBlock);
            oneBlock = new List<string>();
          }
        }
        if (0 < oneBlock.Count) Blocks.Add(oneBlock);




        //
        //セクションごとのブロック取り出し      [LogoDir]    [LogoParam]
        var TakeSection = new Func<string, int, List<List<string>>>(
          (section, blksize) =>
          {
            //セクション名までスキップ
            var section1 = Blocks.SkipWhile((block) =>
                              Regex.Match(block[0], @"^\[" + section + @"\]$", RegexOptions.IgnoreCase).Success == false).ToList();
            //セクション名削除
            if (0 < section1.Count) section1.RemoveAt(0);
            //次のセクション名までを抽出
            var section2 = section1.TakeWhile((block) =>
                              Regex.Match(block[0], @"^\[.*\]$", RegexOptions.IgnoreCase).Success == false).ToList();
            //各ブロックのサイズを制限
            var section3 = (0 < blksize) ? section2.Where((block) => block.Count == blksize)
                                          : section2;
            return section3.ToList();
          });



        //セクション取り出し
        var sectionLogoDir = TakeSection("LogoDir", -1).SelectMany(blk => blk).ToList();
        var sectionKeyword = TakeSection("SearchParam", 3);
        var sectionComment = TakeSection("AddComment", 2);
        var sectionCommNotFound = TakeSection("NotFoundParam", -1).SelectMany(blk => blk).ToList();
        var sectionOption = TakeSection("Option", -1).SelectMany(blk => blk).ToList();




        //読み取り結果
        setting.LogoDir = (0 < sectionLogoDir.Count) ? sectionLogoDir[0] : "";
        setting.SearchSet_byKeyword = sectionKeyword;
        setting.SearchSet_AddComment = sectionComment;
        setting.NotFoundParam = (0 < sectionCommNotFound.Count) ? sectionCommNotFound[0] : "";
        setting.EnableShortCH = sectionOption.Any((opt) =>
                          Regex.Match(opt, @"^AppendSearch_ShortCh$", RegexOptions.IgnoreCase).Success);
        setting.EnableNonNumCH = sectionOption.Any((opt) =>
                          Regex.Match(opt, @"^AppendSearch_NonNumCh$", RegexOptions.IgnoreCase).Success);


        #region 結果表示
        //
        //show result
        /*     
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
        //*/
        //          */
        #endregion

        return setting;
      }
    }

    #endregion



    #region 初期設定ファイル
    static class SettingText
    {
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
    * ファイルが見つかればパスを表示して終了。
4. [LogoDir]内のlgdファイルを列挙してチャンネル名が含まれているファイルを探す。
    * lgdファイルが見つかったら、lgdファイル名と一致するparamファイルを探す。
    * ファイルが見つかればパスを表示して終了。


・全角半角、大文字小文字、ひらがなカタカナの違いは無視される。

・UTF-8 bom

===============================================================================


[LogoDir]
C:\LogoData                  //指定できるフォルダは１つ


[SearchParam]
abc                          //keyword
ABCDE001.lgd                 //logo path
ABCDE.lgd.autoTune.param     //param path
                             //改行をいれる
ｱｲｳｴｵ
あいうえお123.lgd
あいうえお123.lgd.ニュース.autoTune.param


[AddComment]
//BS                           //keyword
//-midprc 0                    //comment


[NotFoundParam]
//-Abort_pfAdapter             //パラメーターが見つからないときにコメント追加


[Option]
//AppendSearch_ShortCH         //チャンネル名の前４文字でも検索する
//AppendSearch_NonNumCH        //チャンネル名から数値・記号を抜いた文字列でも検索する



";
    }
    #endregion




  }
}

