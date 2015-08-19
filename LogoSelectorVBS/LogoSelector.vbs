
'--------------------------------------------------
'チャンネル名を元にロゴファイルのパスを表示します。
'--------------------------------------------------
'使い方
    '                 チャンネル名  プログラム名  ファイルパス
    'LogoSelector.vbs  CBC           News7         C:\News7.ts

'動作
    'パラメータファイルがみつかれば
    '１行目  ロゴデータのパス
    '２行目  ロゴパラメーターのパスを表示します。
    '  C:\logodata\ABC.lgd
    '  C:\logodata\ABC.lgd.autoTune.param
    '
    'パラメータファイルがなければ-Abort_pfAdapterを表示。

'備考
    '・大文字小文字の違いは無視する。
    '・全角半角、ひらがなカタカナの違いは区別する。
    '・パラメータファイルの先頭はロゴデータ名と同じする。
            'ABC.lgd
            'ABC.lgd.autoTune.param
    '・チャンネル名で検索し見つからなければ前４文字で検索する。
    '・ファイルの命名規則、環境に合わせてスクリプトを変更してください。





'ロゴデータを検索するフォルダ
    LogoDirPath = "C:\LogoData\"
    



'--------------------------------------------------------------------------
    Set fso = CreateObject("Scripting.FileSystemObject")

  '引数取得
    Set objParm = Wscript.Arguments
  '引数なし
    If objParm.Count = 0 Then Quit_withoutParam()          '終了
  '引数あり      
    If 1 <= objParm.Count Then channel = objParm(0)        'チャンネル名
    If 2 <= objParm.Count Then program = objParm(1)        'プログラム名
    If 3 <= objParm.Count Then tsPath  = objParm(2)        'tsパス
  'テスト用引数
'    channel = "abc"
'    program = ""


    channel = Trim( UCase(channel))    '大文字に変換
    program = Trim( UCase(program))
    channel_sht = Left(channel, 4)     '短縮名  チャンネルの前４文字だけで検索



'---------------------------
'ファイルを直接指定       大文字で比較
'---------------------------
    If 0 < InStr(1, channel, "CBC", 1) Then                '半角CBC
      logoPath    = fso.BuildPath(LogoDirPath, "ＣＢＣ.lgd")
      paramPath   = fso.BuildPath(LogoDirPath, "ＣＢＣ.lgd.autoTune.param")
    ElseIf 0 < InStr(1, channel, "ＣＢＣ", 1) Then         '全角ＣＢＣ
      logoPath    = fso.BuildPath(LogoDirPath, "ＣＢＣ.lgd")
      paramPath   = fso.BuildPath(LogoDirPath, "ＣＢＣ.lgd.autoTune.param")
    ElseIf 0 < InStr(1, channel, "NHK", 1) Then            '半角NHK
      Quit_RequestAbort()
    ElseIf 0 < InStr(1, channel, "ＮＨＫ", 1) Then         '全角ＮＨＫ
      Quit_RequestAbort()
'   ElseIf 0 < InStr(1, channel, "BS", 1) Then
'     comment = " -MidPrc 0"                               '追加コメント
'   ElseIf 0 < InStr(1, channel, "ＢＳ", 1) Then
'     comment = " -MidPrc 0"
    End If

    'パラメーターが取得できたら、終了
    If Not paramPath = "" Then Quit_withParam()




'---------------------------
'フォルダ内を検索
'---------------------------
    'フォルダが存在しない
    If fso.FolderExists(LogoDirPath) = False Then Quit_withoutParam()
    If paramPath = "" Then

    'ロゴ
    'フォルダ内のファイルを全部取得
    logoList = GetItemList( logoDirPath, channel, ".lgd", logoCnt)
    logoList_sht = GetItemList( logoDirPath, channel_sht, ".lgd", logoCnt_sht) '短縮名で検索
    If logoCnt = 0 And logoCnt_sht = 0 Then Quit_withoutParam()                'ロゴが見つからない、終了。

    'ロゴ決定
    If 0 < logoCnt Then 
      logoPath  = logoList(0)
    Else 
      logoPath = logoList_sht(0)
    End If


        'パラメータ
    logoName = fso.GetFileName(logoPath)
    paramList = GetItemList( logoDirPath, logoName, ".autoTune.param", paramCnt)
    If paramCnt = 0 Then Quit_withoutParam()     '取得できない、終了。
        'パラメータ決定
    paramPath = SelectParam(paramList, program)


'    '画面に全部表示      デバッグ
'      WScript.Echo "---------------------"
'      WScript.Echo logoPath
'      WScript.Echo paramPath
'      WScript.Echo 
'      WScript.Echo "---------------------"
'      WScript.Echo "logoList"
'      For Each item In logoList
'        WScript.Echo item
'      Next
'      WScript.Echo "---------------------"
'      WScript.Echo "logoList_sht"
'      For Each item In logoList_sht
'        WScript.Echo item
'      Next
'      WScript.Echo "---------------------"
'      WScript.Echo "paramList"
'      For Each item In paramList
'        WScript.Echo item
'      Next
'      WScript.Sleep 10*1000
'      WScript.Quit(0)
    End If


    'パラメーターが取得できたら、画面に表示して終了
    If Not paramPath = "" Then
      Quit_withParam()
    Else
      Quit_withoutParam()
    End If



'--------------------------------------------------------------------------
'===================================
'終了処理
'===================================
'パラメータ　あり
    Sub Quit_withParam()
        WScript.Echo logoPath
        WScript.Echo paramPath
        WScript.Echo comment           ' -MidPrc 0
        WScript.Quit(0)
    End Sub

'パラメータ　なし
    Sub Quit_withoutParam()
      WScript.Echo "-Abort_pfAdapter"
      WScript.Quit(0)
    End Sub

'終了要求
    Sub Quit_RequestAbort()
      WScript.Echo "-Abort_pfAdapter"
      WScript.Quit(0)
    End Sub




'===================================
'名前にkey、extを含むファイルを取得
'===================================
    Function GetItemList(ByVal dirPath,ByVal key, ByVal ext, ByRef itemCnt)
      '文字列が空だと全てにヒットするので事前にチェック
      If key = "" Then Exit Function
      
      'ファイル一覧取得
      Set dirInfo = fso.GetFolder(dirPath)
      ukey = UCase(key)                '大文字で比較
      uext = UCase(ext)
      Dim itemList()                   '動的配列を宣言
      idx = 0
      
      'keyItemを配列にいれる
      For Each file In dirInfo.Files
        ufilename = UCase(file.Name)
        If 0 < InStr(1,ufilename, ukey, 1) Then            'ファイル名にukeyが含まれるか？
          extpos = len(ufilename) - len(uext) + 1          'extの推定位置、extはファイル名の末尾にある。
          If 0 < extpos Then
              If extpos = InStr(1, ufilename, uext, 1) Then'extの位置があっているか？
                ReDim Preserve itemList(idx)
                itemList(idx) = file.Path
                idx = idx + 1
              End If
          End If
        End If
      Next

      'return
      GetItemList = itemList
      itemCnt = idx
    End Function


'===================================
'プログラム名からパラメータを決める
'===================================
    Function SelectParam(ByVal ParamList, ByVal uprogram)

      'パラメータ名から文字抽出
      For Each param In ParamList
      '  *.LGD  .AUTOTUNE.PARAMをはずす
        uparam = UCase(param)                              'ＣＢＣ.2014.LOGO.LGD.ニュース.COMMENT.AUTOTUNE.PARAM
        pos = InStr(uparam,  UCase(".lgd."))               '                ↑                   
        midlen = Len(uparam) - (pos-1) - 5 - 15            '                     ←            →
        If(0 < midlen) Then
          uparam = Mid( uparam, pos+5, midlen)
          '  ドットより前を抽出                            '                     ニュース.COMMENT
          pos = InStr(uparam,  UCase("."))                 '                             ↑      
          If(0 < pos) Then uparam = Left( uparam, pos-1)   '                     ニュース
          ukeyword = uparam

          'プログラム名に抽出した文字が含まれているか
          If 0 < InStr(1, uprogram, ukeyword, 1) Then
            'ヒット
            SelectParam = param
            Exit Function              'return
          End If
        End If
      Next

      'ヒットしなかった、一つ目のパラメータを返す。
      SelectParam = ParamList(0)
    End Function

