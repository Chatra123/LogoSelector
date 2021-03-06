﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//参照の追加　アセンブリ　Microsoft.VisualBasic
using Microsoft.VisualBasic;           //Strings.StrConv


namespace OctNov.Text
{
  /// <summary>
  /// 文字の変換、除去
  /// </summary>
  static class StrConv  //String Converter
  {
    /// <summary>
    /// 小文字半角カタカナに変換   To Lower, Narrow, Katakana 
    /// </summary>
    /// <remarks>
    /// ”あ→ｱ”に一度で変換できない。
    /// カナにした後で半角にする。（全角ひら　→　全角カナ　→　半角カナ）
    /// 漢字は全角のまま
    /// </remarks>
    public static string ToLNK(string text)
    {
      text = Strings.StrConv(text, VbStrConv.Lowercase, 0x0411);
      text = Strings.StrConv(text, VbStrConv.Katakana, 0x0411);    //あ→ア
      text = Strings.StrConv(text, VbStrConv.Narrow, 0x0411);      //ア→ｱ
      return text;
    }
    /// <summary>
    /// 小文字半角カタカナに変換
    /// </summary>
    public static IEnumerable<string> ToLNK(IEnumerable<string> list)
    {
      return list.Select(text => ToLNK(text));
    }


    /// <summary>
    /// 大文字全角ひらがなに変換   To Upper, Wide, Hiragana 
    /// </summary>
    /// <remarks>
    /// ”ｱ→あ”に一度で変換できない。
    /// 全角にした後でひらがなにする。（半角カナ　→　全角カナ　→　全角ひら）
    /// </remarks>
    public static string ToUWH(string text)
    {
      text = Strings.StrConv(text, VbStrConv.Uppercase, 0x0411);
      text = Strings.StrConv(text, VbStrConv.Wide, 0x0411);        //ｱ→ア
      text = Strings.StrConv(text, VbStrConv.Hiragana, 0x0411);    //ア→あ
      return text;
    }
    /// <summary>
    /// 大文字全角ひらがなに変換
    /// </summary>
    public static IEnumerable<string> ToUWH(IEnumerable<string> list)
    {
      return list.Select(text => ToUWH(text));
    }


    /// <summary>
    ///数字、記号除去
    /// </summary>
    public static string ToNonNum(string text)
    {
      // \W  文字、数字、アンダースコア以外
      // \d  数字
      // _   アンダースコア
      text = Regex.Replace(text, @"[\W\d_]", "");
      return text;
    }
    /// <summary>
    /// 数字、記号除去
    /// </summary>
    public static IEnumerable<string> ToNonNum(IEnumerable<string> list)
    {
      return list.Select(text => ToNonNum(text));
    }


  }
}

