
'--------------------------------------------------
'�`�����l���������Ƀ��S�t�@�C���̃p�X��\�����܂��B
'--------------------------------------------------
'�g����
    '                 �`�����l����  �v���O������  �t�@�C���p�X
    'LogoSelector.vbs  CBC           News7         C:\News7.ts

'����
    '�p�����[�^�t�@�C�����݂����
    '�P�s��  ���S�f�[�^�̃p�X
    '�Q�s��  ���S�p�����[�^�[�̃p�X��\�����܂��B
    '  C:\logodata\ABC.lgd
    '  C:\logodata\ABC.lgd.autoTune.param
    '
    '�p�����[�^�t�@�C�����Ȃ����-Abort_pfAdapter��\���B

'���l
    '�E�啶���������̈Ⴂ�͖�������B
    '�E�S�p���p�A�Ђ炪�ȃJ�^�J�i�̈Ⴂ�͋�ʂ���B
    '�E�p�����[�^�t�@�C���̐擪�̓��S�f�[�^���Ɠ�������B
            'ABC.lgd
            'ABC.lgd.autoTune.param
    '�E�`�����l�����Ō�����������Ȃ���ΑO�S�����Ō�������B
    '�E�t�@�C���̖����K���A���ɍ��킹�ăX�N���v�g��ύX���Ă��������B





'���S�f�[�^����������t�H���_
    LogoDirPath = "C:\LogoData\"
    



'--------------------------------------------------------------------------
    Set fso = CreateObject("Scripting.FileSystemObject")

  '�����擾
    Set objParm = Wscript.Arguments
  '�����Ȃ�
    If objParm.Count = 0 Then Quit_withoutParam()          '�I��
  '��������      
    If 1 <= objParm.Count Then channel = objParm(0)        '�`�����l����
    If 2 <= objParm.Count Then program = objParm(1)        '�v���O������
    If 3 <= objParm.Count Then tsPath  = objParm(2)        'ts�p�X
  '�e�X�g�p����
'    channel = "abc"
'    program = ""


    channel = Trim( UCase(channel))    '�啶���ɕϊ�
    program = Trim( UCase(program))
    channel_sht = Left(channel, 4)     '�Z�k��  �`�����l���̑O�S���������Ō���



'---------------------------
'�t�@�C���𒼐ڎw��       �啶���Ŕ�r
'---------------------------
    If 0 < InStr(1, channel, "CBC", 1) Then                '���pCBC
      logoPath    = fso.BuildPath(LogoDirPath, "�b�a�b.lgd")
      paramPath   = fso.BuildPath(LogoDirPath, "�b�a�b.lgd.autoTune.param")
    ElseIf 0 < InStr(1, channel, "�b�a�b", 1) Then         '�S�p�b�a�b
      logoPath    = fso.BuildPath(LogoDirPath, "�b�a�b.lgd")
      paramPath   = fso.BuildPath(LogoDirPath, "�b�a�b.lgd.autoTune.param")
    ElseIf 0 < InStr(1, channel, "NHK", 1) Then            '���pNHK
      Quit_RequestAbort()
    ElseIf 0 < InStr(1, channel, "�m�g�j", 1) Then         '�S�p�m�g�j
      Quit_RequestAbort()
'   ElseIf 0 < InStr(1, channel, "BS", 1) Then
'     comment = " -MidPrc 0"                               '�ǉ��R�����g
'   ElseIf 0 < InStr(1, channel, "�a�r", 1) Then
'     comment = " -MidPrc 0"
    End If

    '�p�����[�^�[���擾�ł�����A�I��
    If Not paramPath = "" Then Quit_withParam()




'---------------------------
'�t�H���_��������
'---------------------------
    '�t�H���_�����݂��Ȃ�
    If fso.FolderExists(LogoDirPath) = False Then Quit_withoutParam()
    If paramPath = "" Then

    '���S
    '�t�H���_���̃t�@�C����S���擾
    logoList = GetItemList( logoDirPath, channel, ".lgd", logoCnt)
    logoList_sht = GetItemList( logoDirPath, channel_sht, ".lgd", logoCnt_sht) '�Z�k���Ō���
    If logoCnt = 0 And logoCnt_sht = 0 Then Quit_withoutParam()                '���S��������Ȃ��A�I���B

    '���S����
    If 0 < logoCnt Then 
      logoPath  = logoList(0)
    Else 
      logoPath = logoList_sht(0)
    End If


        '�p�����[�^
    logoName = fso.GetFileName(logoPath)
    paramList = GetItemList( logoDirPath, logoName, ".autoTune.param", paramCnt)
    If paramCnt = 0 Then Quit_withoutParam()     '�擾�ł��Ȃ��A�I���B
        '�p�����[�^����
    paramPath = SelectParam(paramList, program)


'    '��ʂɑS���\��      �f�o�b�O
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


    '�p�����[�^�[���擾�ł�����A��ʂɕ\�����ďI��
    If Not paramPath = "" Then
      Quit_withParam()
    Else
      Quit_withoutParam()
    End If



'--------------------------------------------------------------------------
'===================================
'�I������
'===================================
'�p�����[�^�@����
    Sub Quit_withParam()
        WScript.Echo logoPath
        WScript.Echo paramPath
        WScript.Echo comment           ' -MidPrc 0
        WScript.Quit(0)
    End Sub

'�p�����[�^�@�Ȃ�
    Sub Quit_withoutParam()
      WScript.Echo "-Abort_pfAdapter"
      WScript.Quit(0)
    End Sub

'�I���v��
    Sub Quit_RequestAbort()
      WScript.Echo "-Abort_pfAdapter"
      WScript.Quit(0)
    End Sub




'===================================
'���O��key�Aext���܂ރt�@�C�����擾
'===================================
    Function GetItemList(ByVal dirPath,ByVal key, ByVal ext, ByRef itemCnt)
      '�����񂪋󂾂ƑS�ĂɃq�b�g����̂Ŏ��O�Ƀ`�F�b�N
      If key = "" Then Exit Function
      
      '�t�@�C���ꗗ�擾
      Set dirInfo = fso.GetFolder(dirPath)
      ukey = UCase(key)                '�啶���Ŕ�r
      uext = UCase(ext)
      Dim itemList()                   '���I�z���錾
      idx = 0
      
      'keyItem��z��ɂ����
      For Each file In dirInfo.Files
        ufilename = UCase(file.Name)
        If 0 < InStr(1,ufilename, ukey, 1) Then            '�t�@�C������ukey���܂܂�邩�H
          extpos = len(ufilename) - len(uext) + 1          'ext�̐���ʒu�Aext�̓t�@�C�����̖����ɂ���B
          If 0 < extpos Then
              If extpos = InStr(1, ufilename, uext, 1) Then'ext�̈ʒu�������Ă��邩�H
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
'�v���O����������p�����[�^�����߂�
'===================================
    Function SelectParam(ByVal ParamList, ByVal uprogram)

      '�p�����[�^�����當�����o
      For Each param In ParamList
      '  *.LGD  .AUTOTUNE.PARAM���͂���
        uparam = UCase(param)                              '�b�a�b.2014.LOGO.LGD.�j���[�X.COMMENT.AUTOTUNE.PARAM
        pos = InStr(uparam,  UCase(".lgd."))               '                ��                   
        midlen = Len(uparam) - (pos-1) - 5 - 15            '                     ��            ��
        If(0 < midlen) Then
          uparam = Mid( uparam, pos+5, midlen)
          '  �h�b�g���O�𒊏o                            '                     �j���[�X.COMMENT
          pos = InStr(uparam,  UCase("."))                 '                             ��      
          If(0 < pos) Then uparam = Left( uparam, pos-1)   '                     �j���[�X
          ukeyword = uparam

          '�v���O�������ɒ��o�����������܂܂�Ă��邩
          If 0 < InStr(1, uprogram, ukeyword, 1) Then
            '�q�b�g
            SelectParam = param
            Exit Function              'return
          End If
        End If
      Next

      '�q�b�g���Ȃ������A��ڂ̃p�����[�^��Ԃ��B
      SelectParam = ParamList(0)
    End Function

