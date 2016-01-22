using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;


namespace LogoSelector
{

  class Setting_File
  {

    public List<string> LogoDir { get; private set; }
    public List<List<string>> SearchSet_byKeyword { get; private set; }
    public List<List<string>> SearchSet_AddComment { get; private set; }

    public string Comment_NotFoundLogo { get; private set; }
    public string Comment_NotFoundParam { get; private set; }

    public bool Enable_ShortCH { get; private set; }
    public bool Enable_NonNumCH { get; private set; }


    //ファイル読込み
    public static Setting_File Load()
    {
      //initialize
      var setting = new Setting_File();
      setting.LogoDir = new List<string>();
      setting.SearchSet_byKeyword = new List<List<string>>();
      setting.SearchSet_AddComment = new List<List<string>>();
      setting.Comment_NotFoundLogo = "";
      setting.Comment_NotFoundParam = "";
      setting.Enable_ShortCH = false;
      setting.Enable_NonNumCH = false;

      //パス
      var txtpath = "";
      {
        var AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var AppDir = Path.GetDirectoryName(AppPath);
        var AppName = Path.GetFileNameWithoutExtension(AppPath);
        txtpath = Path.Combine(AppDir, AppName + ".txt");
      }

      //ファイル作成
      if (File.Exists(txtpath) == false)
        File.WriteAllText(txtpath, Setting_Text_Default.Text, Encoding.UTF8);

      //読
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
      var section_LogoDir = TakeSection("LogoDir", -1).SelectMany(blk => blk).ToList();
      var section_Keyword = TakeSection("SearchKeyword", 3);
      var section_Comment = TakeSection("AddComment", 2);
      var section_CommNotFoundLogo = TakeSection("NotFoundLogo", -1).SelectMany(blk => blk).ToList();
      var section_CommNotFoundParam = TakeSection("NotFoundParam", -1).SelectMany(blk => blk).ToList();
      var section_Option = TakeSection("Option", -1).SelectMany(blk => blk).ToList();


      //
      //読み取り結果
      //
      setting.LogoDir = section_LogoDir.Where((path) => Directory.Exists(path)).ToList();
      setting.SearchSet_byKeyword = section_Keyword;
      setting.SearchSet_AddComment = section_Comment;
      setting.Comment_NotFoundLogo = (0 < section_CommNotFoundLogo.Count) ? section_CommNotFoundLogo[0] : "";
      setting.Comment_NotFoundParam = (0 < section_CommNotFoundParam.Count) ? section_CommNotFoundParam[0] : "";

      setting.Enable_ShortCH = section_Option.Any(
        (opt) =>
          Regex.Match(opt, @"^AppendSearch_ShortCh$", RegexOptions.IgnoreCase).Success
          );

      setting.Enable_NonNumCH = section_Option.Any(
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



  #region 初期設定ファイル

  static class Setting_Text_Default
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

    public const string Text =
  @"
===============================================================================
### LogoSelectorについて

  ロゴ、パラメーターを検索しフルパスを表示します。

　
### 処理の流れ

1. 引数からチャンネル名を受け取る。

2. [SearchKeyword]のキーワードがチャンネル名に含まれているかチェック。

3. キーワードが含まれているなら、
    * ロゴ、パラメーターをフルパスとしてチェック
    * [LogoDir]内のファイル名としてチェック
    * ファイルが見つかればパス、コメントを表示して終了。

4.含まれていないなら、
    * [LogoDir]内の lgdファイルを列挙して、
      チャンネル名が含まれている lgdファイルを探す。
    * lgdファイル名と一致する paramファイルを探す。
    * ファイルが見つかればパス、コメントを表示して終了。


### 他

 *  lgd以外にも ldp, lgd2, ldp2に一部対応 

 *  全角半角、大文字小文字、ひらがなカタカナの違いは無視。

 *  各行の前後の空白は無視。  //以降はコメント。

 *  このテキストの文字コード  UTF-8 bom


===============================================================================

[LogoDir]
C:\LogoData1                         //lgdファイルのあるフォルダ
D:\LogoData2


[SearchKeyword]
東海                                //keyword
東海テレビ2016.lgd                  //logo
東海テレビ2016.lgd.autoTune.param   //param
                                    //区切りとして改行

BSジャパン
C:\logo\BS-Japan.lgd                //ファイル名　又は　フルパス
C:\logo\BS-Japan.lgd.autoTune.param


[AddComment]
//                                    チャンネル名にBSを含んでいるとコメント追加
//BS                                //keyword
//-midprc 0                         //comment


[NotFoundLogo]
//-Suspend_pfAMain                   //ロゴが見つからないときにコメント追加


[NotFoundParam]
//-Suspend_pfAMain                   //パラメーターが見つからないときにコメント追加


[Option]
//AppendSearch_ShortCH              //検索追加　チャンネル名を前４文字に短縮して検索
//AppendSearch_NonNumCH             //検索追加　チャンネル名から数字記号を除いて検索

";
  }

  #endregion




}















