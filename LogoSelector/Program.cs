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
			string ch = (1 <= args.Count()) ? args[0] : "";						//チャンネル
			string shortCh = "";
			string nonNumCh = "";
			string program = (2 <= args.Count()) ? args[1] : "";			//プログラム
			string tspath = (3 <= args.Count()) ? args[2] : "";				//tsパス


			//ファイル読込み
			setting.Load();


			//小文字半角ひらがなに変換
			ch = util.ConvertUWH(ch);
			shortCh = (3 < ch.Length) ? ch.Substring(0, 3) : ch;
			nonNumCh = ch;
			nonNumCh = util.RemoveNumber(nonNumCh);
			nonNumCh = util.RemoveSymbol(nonNumCh);
			program = util.ConvertUWH(program);
			tspath = util.ConvertUWH(tspath);


			//コメント対象かをチェック
			string comment = CommentSearch(ch, setting.CommentSearchSet);


			//ロゴパラメーター検索
			string[] logoParam = null;
			var FoundParam = new Func<string[], bool>((ary) => (ary != null && ary[1] != ""));	//パラメーターがある


			//指定キーワードから
			if (FoundParam(logoParam) == false)
				logoParam = KeywordSearch(ch, setting.LogoDir, setting.KeywordSearchSet);

			//フォルダから
			if (FoundParam(logoParam) == false)
			{
				//各パターンで検索
				var logoParam_normal = DirectorySearch(ch, program, setting.LogoDir);							//チャンネル名
				var logoParam__short = DirectorySearch(shortCh, program, setting.LogoDir);				//短縮名
				var logoParam_nonNum = DirectorySearch(nonNumCh, program, setting.LogoDir);				//数字、記号抜き

				//見つかった？
				if (FoundParam(logoParam_normal))
				{
					logoParam = logoParam_normal;
				}
				else if (setting.EnableShortCH && FoundParam(logoParam__short))
				{
					logoParam = logoParam__short;
				}
				else if (setting.EnableNonNumCH && FoundParam(logoParam_nonNum))
				{
					logoParam = logoParam_nonNum;
				}
			}


			//見つからない
			logoParam = logoParam ?? new string[] { "", "", };												//nullなら空文字を入れる
			//logoParam = (logoParam == null) ? new string[] { "", "", } : logoParam;				//nullなら空文字を入れる
			//パラメーターがない？
			if (FoundParam(logoParam) == false) comment += setting.NotFoundParam;			//NotFoundParam追加



			//結果表示
			//Console.Clear();
			Console.WriteLine(logoParam[0]);
			Console.WriteLine(logoParam[1]);
			Console.WriteLine(comment);
			//Console.ReadLine();
		}



		#region パラメーター検索
		//指定キーワードがあればコメント追加
		static string CommentSearch(string ch, List<List<string>> SearchSet)
		{
			var commliner = "";

			foreach (var block in SearchSet)
			{
				string search = util.ConvertUWH(block[0]);
				string comment = block[1];

				int found = ch.IndexOf(search);
				if (found != -1)
					commliner += comment + " ";		// hit
			}

			return commliner;
		}



		//指定キーワードからパラメーター検索
		static string[] KeywordSearch(string ch, string logoDir, List<List<string>> SearchSet)
		{
			foreach (var block in SearchSet)
			{
				string search = util.ConvertUWH(block[0]);
				string logo = block[1];
				string param = block[2];

				int found = ch.IndexOf(search);

				if (found != -1)		//hit
				{
					//パスチェック、なければlogoDir内をチェック
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


		#region ロゴフォルダ内からパラメーター検索
		//ロゴフォルダ内からパラメーター検索
		static string[] DirectorySearch(string ch, string program, string logoDir)
		{
			if (ch == "") return null;
			if (Directory.Exists(logoDir) == false) return null;

			//*.lgd
			var foundlgd = new Func<string>(() =>
			{
				var lgdfiles = Directory.GetFiles(logoDir, "*.lgd");
				foreach (var lgdpath in lgdfiles)
				{
					string lgdname = Path.GetFileName(lgdpath);
					lgdname = util.ConvertUWH(lgdname);					//大文字全角ひらがなで比較
					int found = lgdname.IndexOf(ch);
					if (found != -1) return lgdpath;						//変形前のパスを返す。
				}
				return "";
			})();

			if (foundlgd == "") return null; //lgdがない



			//*.autoTune.param
			var foundparam = new Func<string, string>((lgdpath) =>
			{
				var paramlist = new List<string>();
				string lgdname = Path.GetFileName(lgdpath);
				var paramfiles = Directory.GetFiles(logoDir, lgdname + "*.autoTune.param");
				if (paramfiles.Count() == 0) return null;

				//番組名内にkeyが含まれているか？
				foreach (var parampath in paramfiles)
				{
					string paramname = Path.GetFileName(parampath);	//ＣＢＣ.2014.lgd.ニュース.COMMENT.autoTune.param
					paramname = paramname.Replace(lgdname, "");     //               .ニュース.COMMENT.autoTune.param
					paramname = Regex.Replace(paramname,            //               .ニュース.COMMENT
											@".autoTune.param$", "", RegexOptions.IgnoreCase);
					paramname = Regex.Replace(paramname, @"\.*(.*)\.(.*)", "$1");//   ニュース
					paramname = util.ConvertUWH(paramname);					//大文字全角ひらがなで比較
					if (paramname == "") continue;

					int found = program.IndexOf(paramname);
					if (found != -1) return parampath;							//変形前のパスを返す。
				}
				//見つからない。paramfilesの最初の要素を返す。
				return paramfiles[0];
			})(foundlgd);

			if (foundparam == null) return new string[] { foundlgd, "" }; //paramがない



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
				text = Strings.StrConv(text, VbStrConv.Katakana, 0x0411);			//あ→ア
				text = Strings.StrConv(text, VbStrConv.Narrow, 0x0411);				//ア→ｱ　　”あ→ｱ”に一度で変換できない
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
				text = Strings.StrConv(text, VbStrConv.Wide, 0x0411);					//ｱ→ア
				text = Strings.StrConv(text, VbStrConv.Hiragana, 0x0411);			//ア→あ　　”ｱ→あ”に一度で変換できない
				return text;
			}

			//記号削除
			public static string RemoveSymbol(string text)
			{
				var symbol_N = @" !\""#$%&'()=-~^|\\`@{[}]*:+;_?/>.<,・";
				var symbol_W = @"・" + util.ConvertUWH(symbol_N);
				text = Regex.Replace(text, @"[" + symbol_N + "]", "");
				text = Regex.Replace(text, @"[" + symbol_W + "]", "");
				return text;
			}

			//数字削除
			public static string RemoveNumber(string text)
			{
				return Regex.Replace(text, @"\d", "");
			}


		}
		#endregion




		#region 設定ファイル
		class setting
		{
			public static string LogoDir { get; private set; }
			public static List<List<string>> KeywordSearchSet { get; private set; }
			public static List<List<string>> CommentSearchSet { get; private set; }
			public static string NotFoundParam { get; private set; }
			public static bool EnableShortCH { get; private set; }
			public static bool EnableNonNumCH { get; private set; }

			public static void Load()
			{
				//initialize
				LogoDir = "";
				KeywordSearchSet = new List<List<string>>();
				CommentSearchSet = new List<List<string>>();
				NotFoundParam = "";
				EnableShortCH = false;
				EnableNonNumCH = false;

				//パス
				var AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				var AppDir = Path.GetDirectoryName(AppPath);
				var AppName = Path.GetFileNameWithoutExtension(AppPath);
				var path = Path.Combine(AppDir, AppName + ".txt");


				//読込み
				if (File.Exists(path) == false) { File.WriteAllText(path, defaultSetting, Encoding.UTF8); }
				var readfile = File.ReadAllLines(path);

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
				//セクションごとのブロック取り出し			[LogoDir]		[LogoParam]
				var TakeSection = new Func<string, int, List<List<string>>>(
					(section, sizemax) =>
					{//セクション名までスキップ
						var section1 = Blocks.SkipWhile((block) =>
															Regex.Match(block[0], @"^\[" + section + @"\]$", RegexOptions.IgnoreCase).Success == false).ToList();
						//セクション名削除
						if (0 < section1.Count) section1.RemoveAt(0);
						//次のセクション名までを抽出
						var section2 = section1.TakeWhile((block) =>
															Regex.Match(block[0], @"^\[.*\]$", RegexOptions.IgnoreCase).Success == false).ToList();
						//各ブロックのサイズを制限
						var section3 = (0 < sizemax) ? section2.Where((block) => block.Count == sizemax)
																					: section2;
						return section3.ToList();
					});

				//セクション取り出し
				var sectionLogoDir = TakeSection("LogoDir", -1).SelectMany(blk => blk).ToList();
				var sectionKeyword = TakeSection("KeywordSerch", 3);
				var sectionComment = TakeSection("Comment", 2);
				var sectionCommNotFound = TakeSection("Not Found Param", -1).SelectMany(blk => blk).ToList();
				var sectionOption = TakeSection("Option", -1).SelectMany(blk => blk).ToList();



				//読み取り結果
				LogoDir = (0 < sectionLogoDir.Count) ? sectionLogoDir[0] : "";
				KeywordSearchSet = sectionKeyword;
				CommentSearchSet = sectionComment;
				NotFoundParam = (0 < sectionCommNotFound.Count) ? sectionCommNotFound[0] : "";
				EnableShortCH = sectionOption.Any((opt) =>
													Regex.Match(opt, @"^AppendSearch_ShortCh$", RegexOptions.IgnoreCase).Success);
				EnableNonNumCH = sectionOption.Any((opt) =>
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
				//					*/
				#endregion

				return;
			}

			#region  defaultSetting
			const string defaultSetting =
@"
===============================================================================
・LogoSelectorについて
	ロゴ、パラメーターを検索しフルパスを表示します。


・処理の流れ
１、引数からチャンネル名、プログラム名を受け取る。
２、[KeywordSerch]のキーワードがチャンネル名に含まれているかチェック。
３、キーワードが含まれているなら、
　　　→　ロゴ、パラメーターをフルパスとしてチェック
　　　→　[LogoDir]内をファイル名としてチェック
　　　→　ファイルが見つかればパスを表示して終了。
４、[LogoDir]内のlgdファイルを列挙してチャンネル名が含まれているファイルを探す。
　　　→　lgdファイルが見つかったら、lgdファイル名と一致するparamファイルを探す。
　　　→　ファイルが見つかればパスを表示して終了。

・全角半角、大文字小文字、ひらがなカタカナの違いは無視される。

・UTF-8 bom

===============================================================================



[LogoDir]
C:\LogoData				//指定できるフォルダは１つ


[KeywordSerch]
abc					//keyword
ABCDE001.lgd				//logo
ABCDE.lgd.autoTune.param		//param
					//改行をいれる
ｱｲｳｴｵ
あいうえお123.lgd
あいうえお123.lgd.ニュース.autoTune.param


[Comment]
//BS					//keyword
//-midprc 0				//comment


[Not Found Param]
//-Abort_pfAdapter			//パラメーターが見つからないときにコメント追加


[Option]
//AppendSearch_ShortCH			//チャンネル名の前三文字でも検索する
//AppendSearch_NonNumCH			//チャンネル名から数値、記号を抜いた文字列でも検索する



";
			#endregion
		}

		#endregion




	}
}

