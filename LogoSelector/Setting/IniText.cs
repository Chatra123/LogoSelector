using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace LogoSelector
{
  static class IniText
  {
    public const string Default =
  @"
[LogoDir]
;;;  ロゴ、パラメーターのあるフォルダパス
Path_1 = C:\LogoData1\
Path_2 = D:\LogoData2\
Path_3 = 


[SearchKeyword]
;;;  チャンネル名に Keyword_1が含まれていると Logo_1、Param_1のファイルを検索する。
;;;  フルパスまたはファイル名を指定し、[LogoDir]内から探します。
Keyword_1 = 東海
Logo_1    = TokaiTv.2016.lgd
Param_1   = TokaiTv.2016.lgd.autoTune.param 
Keyword_2 = BSジャパン
Logo_2    = C:\LogoData1\BS-Japan.lgd
Param_2   = C:\LogoData1\BS-Japan.lgd.autoTune.param
Keyword_3 = 
Logo_3    = 
Param_3   = 


[AddComment_Keyword]
;;;  チャンネル名に Keyword_1が含まれているとコメント追加
; Keyword_1 = BS
; Comment_1 = -midprc 0 
Keyword_2 = 
Comment_2 = 
Keyword_3 = 
Comment_3 = 


[AddComment_NotFound]
;;;  ファイルが見つからないときにコメント追加
; NotFoundLogo  = -Abort
; NotFoundParam = -Abort


[Option]
;;;  検索追加　チャンネル名を前４文字に短縮して検索
;;;                   = 0  無効        = 1  有効
AppendSearch_ShortCH  = 0
;;;  検索追加　チャンネル名から数字記号を除いて検索
AppendSearch_NonNumCH = 0




";


    public const string Readme =
  @"
;;; ==================================================================
;;;    Readme
;;; ==========================
;;; # LogoSelectorについて
;;; 
;;;   ロゴ、パラメーターを検索しフルパスを表示します。
;;; 
;;; 
;;; ## 処理の流れ
;;; 
;;;   1. 引数からチャンネル名を受け取る。
;;; 
;;;   2. [SearchKeyword]のキーワードがチャンネル名に含まれているかチェック。
;;; 
;;;   3. キーワードが含まれているなら、
;;;       * [LogoDir]内のファイルとしてファイルチェック
;;;       * ファイルが見つかればフルパス、コメントを表示して終了。
;;; 
;;;   4. 含まれていない又は、見つからなかったら、
;;;       * [LogoDir]内からチャンネル名を含むファイルを探す。
;;;       * ファイルが見つかればフルパス、コメントを表示して終了。
;;; 
;;;   5. 引数のチャンネル名で見つからなければ、ＴＳファイル名から
;;;      チャンネル名を取得し 2. 3. 4. を再実行する。
;;; 
;;; 
;;;  
;;; ==========================
;;; ## lgdファイル名について
;;; 
;;;   基本は ABC.lgd のように チャンネル名.lgd にしてください。
;;;   １つの放送局でチャンネル名が複数ある場合には、Abc ABCTV.lgd の様に間に
;;;   スペースをいれます。
;;;   また、Abc ABCTV.ver2016.lgdとチャンネル名の後にピリオドをいれ
;;;   ることでコメントをいれることができます。
;;; 
;;; 
;;; ## paramファイル名について
;;; 
;;;   lgd名.autoTune.param と、lgdファイルと同じ名前にします。
;;;   こちらも、lgdファイル名の後にピリオドを入れてコメントをいれられます。
;;;   ABC.lgd.comment.param
;;; 
;;;   
;;; ## 例
;;;
;;;   ABC.lgd
;;;   ABC.lgd.autoTune.param
;;; 
;;;   Abc ABCTV.lgd
;;;   Abc ABCTV.lgd.param
;;; 
;;;   Abc ABCTV.ver2016.lgd
;;;   Abc ABCTV.ver2016.lgd.comment.param
;;; 
;;; 
;;; 
;;; ==========================
;;; ## ＴＳファイル名にチャンネル名を埋め込む
;;;
;;;     チャンネル名　番組タイトル　日付.ts
;;;     番組タイトル チャンネル名.ts
;;;     番組タイトル チャンネル名-(1).ts
;;;   のように間にスペースを入れてください。
;;; 
;;;   元になるチャンネル名の候補はlgdファイル名から収集し、前方一致で検索します。
;;;   ファイル名が重複したときの -(1) はあっても問題ありません。
;;; 
;;; 
;;; ## その他
;;;
;;;    * 検索において全角半角、大文字小文字、ひらがなカタカナの違いは無視します。
;;;   
;;;    * lgd以外にも ldp, lgd2, ldp2に一部対応 。
;;;      ldp, ldp2の場合は各ロゴ名をファイル名として扱います。
;;;   
;;;    * 連番のある項目は１～９まで設定できます 。
;;;        Path_1 =
;;;        Path_9 =
;;;
;;;    * このiniテキストの文字コード  Shift-JIS
;;; 
;;;    * ; 以降はコメント
;;; 
;;; 
;;; 


";



  }
}


