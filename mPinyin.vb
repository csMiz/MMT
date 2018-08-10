Imports System.IO
Imports System.Text.RegularExpressions

Module mPinyin

    Public ListPY As New List(Of cPinyin)
    'Public OutListPY As New List(Of cPinyin)

    Public Enum CloseMouthParam As Byte
        None = 0
        Before = 1
        SemiAfter = 2
        Both = 3
    End Enum

    Public Enum SpecialPinyin As Byte
        None = 0
        ZhiChiShiRi = 1
        ZiCiSi = 2
        Yu = 3

    End Enum

    Public Class cWaveLable
        Public ID As Short   '0-4: a,i,u,e,o  5-7:wave1,2,3
        Public Value As New List(Of KeyValuePair(Of Single, Single))    'interval,value

        Public Function Copy() As cWaveLable
            Dim r As New cWaveLable
            r.ID = ID
            For Each c As KeyValuePair(Of Single, Single) In Value
                r.Value.Add(New KeyValuePair(Of Single, Single)(c.Key, c.Value))
            Next
            Return r
        End Function

        Public Function Calc(X As Single) As Single
            For i = 0 To Value.Count - 2
                If X >= Value(i).Key AndAlso X < Value(i + 1).Key Then
                    Dim k As Single = (Value(i + 1).Value - Value(i).Value) / (Value(i + 1).Key - (Value(i).Key))
                    Dim r As Single = Value(i).Value + k * (X - Value(i).Key)
                    Return r
                End If
            Next
            Return 0
        End Function
    End Class

    Public Class cPinyinConfigLabel
        Public Labels As New List(Of cWaveLable)

        Public Function Copy() As cPinyinConfigLabel
            Dim r As New cPinyinConfigLabel
            For Each c As cWaveLable In Labels
                r.Labels.Add(c.Copy)
            Next
            Return r
        End Function

        Public Shared Function LoadFromText(inp As String) As cPinyinConfigLabel

            Dim r As New cPinyinConfigLabel

            Dim cont() As String = Regex.Split(inp, ";")
            If cont.Count > 0 Then
                For i = 0 To cont.Count - 1
                    If cont(i).Replace(" ", "") = "" Then
                        Continue For
                    End If
                    Dim tco As String = cont(i)
                    Dim tcmd As String = tco.Substring(0, 1)
                    Dim tl As New cWaveLable
                    With tl
                        If tcmd = "a" Then
                            .ID = 0
                        ElseIf tcmd = "i" Then
                            .ID = 1
                        ElseIf tcmd = "u" Then
                            .ID = 2
                        ElseIf tcmd = "e" Then
                            .ID = 3
                        ElseIf tcmd = "o" Then
                            .ID = 4
                        ElseIf tcmd = "v" Then
                            If tco.Contains("value1") Then
                                .ID = 5
                            ElseIf tco.Contains("value2") Then
                                .ID = 6
                            ElseIf tco.Contains("value3") Then
                                .ID = 7
                            Else
                                Throw New Exception("invalid command")
                                Return Nothing
                            End If
                        Else
                            Throw New Exception("invalid command")
                            Return Nothing
                        End If
                        Dim mc1 As MatchCollection = Regex.Matches(tco, "\((.+?)\)")
                        If mc1.Count > 0 Then
                            For j = 0 To mc1.Count - 1
                                Dim tva As String = mc1(j).Groups(1).Value
                                Dim ttwovalue() As String = Regex.Split(tva, ",")
                                Dim tv1 As Single = CSng(ttwovalue(0))
                                Dim tv2 As Single = CSng(ttwovalue(1))
                                .Value.Add(New KeyValuePair(Of Single, Single)(tv1, tv2))
                            Next
                        End If

                    End With

                    r.Labels.Add(tl)

                Next

            End If

            Return r

        End Function

        ''' <summary>
        ''' 写入拼音关键帧，返回写入后的时间指针位置
        ''' </summary>
        ''' <param name="faces">输出表情列表</param>
        ''' <param name="Interval">时长</param>
        ''' <param name="Startat">起始</param>
        ''' <param name="wav"></param>
        ''' <returns>写入后的时值</returns>
        Public Function Write(faces As List(Of Face), interval As Single, ByVal startat As Single, Optional wav As List(Of Byte) = Nothing) As Single

			'这个补零是错误的
            'Dim used() As Boolean = {False, False, False, False, False}

            If Labels.Count Then
                For i = 0 To Labels.Count - 1
                    Dim tl As cWaveLable = Labels(i)
					Dim tvowel As Short = -1
                    If wav Is Nothing Then
                        tvowel = tl.ID
                    Else
                        tvowel = wav(tl.ID - 5)
                    End If
                    For j = 0 To tl.Value.Count - 1
                        Dim tkvp As KeyValuePair(Of Single, Single) = tl.Value(j)
                        Dim tx As Single = tkvp.Key * interval

                        'used(tvowel) = True
                        If Math.Abs(Startat + tx - CInt(Startat + tx)) < 0.05 Then
                            Dim tfp As New FacePoint(CInt(Startat + tx), tkvp.Value)
                            faces(tvowel).AddPoint(tfp)
                        Else
                            Dim tfp As New FacePoint(Math.Truncate(Startat + tx), tl.Calc(Math.Truncate(tx) / Interval))
                            Dim tfp2 As New FacePoint(Math.Truncate(Startat + tx) + 1, tl.Calc(Math.Truncate(tx + 1) / Interval))
                            faces(tvowel).AddPoint(tfp)
                            faces(tvowel).AddPoint(tfp2)
                        End If

                    Next
					'加入边界补零
					'不能使用覆盖，应该使用最大值
					dim leftValue as single = startat+tl.Value(0).Key*interval
					dim rightValue as single = startat+tl.Value(tl.Value.Count-1).Key*interval
					dim leftEdge as integer = Math.Truncate(leftValue)
					dim rightEdge as integer = Math.Ceiling(rightValue)
                    'If leftEdge = leftValue Then leftEdge -= 1
                    If faces(tvowel).GetAt(leftEdge) IsNot Nothing Then
                        leftEdge -= 1
                    End If
                    If leftEdge < 0 then leftEdge = 0
                    'if rightEdge = rightValue then rightEdge += 1
                    If faces(tvowel).GetAt(rightEdge) IsNot Nothing Then
                        rightEdge += 1
                    End If

                    Dim leftEdgePoint as new FacePoint(leftEdge, 0)
					dim rightEdgePoint as new FacePoint(rightEdge, 0)
					faces(tvowel).AddPointApplyMax(leftEdgePoint)
					faces(tvowel).AddPointApplyMax(rightEdgePoint)
					
                Next
            End If

            'For i = 0 To 4
            '    If used(i) Then
            '        Dim tfp As New FacePoint(Math.Truncate(Startat) + 1, 0)
            '        faces(i).AddPoint(tfp)
            '    End If
            'Next

            Return Startat + Interval
        End Function

        Public Sub CloneLabel(sourceL As Short, copycount As Short)
            For i = 1 To copycount
                Labels.Insert(sourceL, Labels(sourceL).Copy)
            Next

        End Sub

    End Class

    Public Class cPinyinConfig
        Public Shared Pause As cPinyinConfigLabel
        Public Shared Zhi As cPinyinConfigLabel
        Public Shared Zi As cPinyinConfigLabel
        Public Shared Yu As cPinyinConfigLabel

        Public Shared SingleE As cPinyinConfigLabel
        Public Shared Ian As cPinyinConfigLabel

        Public Shared Wav1 As cPinyinConfigLabel
        Public Shared Wav2 As cPinyinConfigLabel
        Public Shared Wav3 As cPinyinConfigLabel

        Public Shared CloseMouthBef As Single
        Public Shared CloseMouthAft As Single

        Public Shared Pointer As Single = 0
        Public Shared Interval As Single = 7.5

        Public Shared usingIndex As Long = 0

        Public Shared Sub ApplyPause(faces As List(Of Face), Optional start As Single = -1)
            If start < 0 Then
                Pointer = Pause.Write(faces, Interval, Pointer)
            Else
                Pause.Write(faces, Interval, start)
            End If
        End Sub
        Public Shared Sub ApplyZhi(faces As List(Of Face), Optional start As Single = -1)
            If start < 0 Then
                Pointer = Zhi.Write(faces, Interval, Pointer)
            Else
                Zhi.Write(faces, Interval, start)
            End If
        End Sub
        Public Shared Sub ApplyZi(faces As List(Of Face), Optional start As Single = -1)
            If start < 0 Then
                Pointer = Zi.Write(faces, Interval, Pointer)
            Else
                Zi.Write(faces, Interval, start)
            End If
        End Sub
        Public Shared Sub ApplyYu(faces As List(Of Face), Optional start As Single = -1)
            If start < 0 Then
                Pointer = Yu.Write(faces, Interval, Pointer)
            Else
                Yu.Write(faces, Interval, start)
            End If
        End Sub
        Public Shared Sub ApplyNormal(faces As List(Of Face), tpy As cPinyin, Optional start As Single = -1)
            Dim tpycl As cPinyinConfigLabel = Nothing
            Dim tvc As Short = tpy.Vowel.Count
            If tvc = 1 Then
                tpycl = Wav1.Copy
            ElseIf tvc = 2 Then
                tpycl = Wav2.Copy
            ElseIf tvc = 3 Then
                tpycl = Wav3.Copy
            Else
                Exit Sub
            End If
            For i = 0 To tvc - 1
                tpycl.Labels(i).ID = tpy.Vowel(i)
            Next
            If tpy.SingleE Then
                If tpycl.Labels(0).ID = 3 Then
                    tpycl.CloneLabel(0, SingleE.Labels.Count - 1)
                    For i = 0 To tpycl.Labels.Count - 1
                        tpycl.Labels(i).ID = SingleE.Labels(i).ID
                    Next
                End If
            ElseIf tpy.ChangedA Then
                tpycl.Labels(1).ID = 3
            End If
            If tpy.GetCloseMouth = CloseMouthParam.Before Or tpy.GetCloseMouth = CloseMouthParam.Both Then
                For i = 0 To tpycl.Labels.Count - 1
                    For j = 0 To tpycl.Labels(i).Value.Count - 1
                        Dim tokv As KeyValuePair(Of Single, Single) = tpycl.Labels(i).Value(j)
                        tpycl.Labels(i).Value(j) = New KeyValuePair(Of Single, Single)(CloseMouthBef + tokv.Key * (1 - CloseMouthBef), tokv.Value)
                        tokv = Nothing
                    Next
                    tpycl.Labels(i).Value.Insert(0, New KeyValuePair(Of Single, Single)(0.0F, 0.0F))
                Next
            End If
            If tpy.GetCloseMouth = CloseMouthParam.SemiAfter Or tpy.GetCloseMouth = CloseMouthParam.Both Then
                For i = 0 To tpycl.Labels.Count - 1
                    For j = 0 To tpycl.Labels(i).Value.Count - 1
                        Dim tokv As KeyValuePair(Of Single, Single) = tpycl.Labels(i).Value(j)
                        If tokv.Key > CloseMouthAft Then
                            tpycl.Labels(i).Value(j) = New KeyValuePair(Of Single, Single)(tokv.Key, tokv.Value * 0.5)
                        End If
                        tokv = Nothing
                    Next
                Next
            End If

            'tpycl.Write(faces, Interval, Pointer)
            If start < 0 Then
                Pointer = tpycl.Write(faces, Interval, Pointer)
            Else
                tpycl.Write(faces, Interval, start)
            End If

        End Sub

        <Obsolete("改用PYBlock了", False)>
        Public Shared Sub ApplySinglePinyin(faces As List(Of Face), interval As Short)

            Dim tpy As cPinyin = ListPY(usingIndex)
            cPinyinConfig.Interval = interval
            If tpy.isPause Then
                cPinyinConfig.ApplyPause(ListFace)
            ElseIf tpy.getSpecial = SpecialPinyin.ZhiChiShiRi Then
                cPinyinConfig.ApplyZhi(ListFace)
            ElseIf tpy.getSpecial = SpecialPinyin.ZiCiSi Then
                cPinyinConfig.ApplyZi(ListFace)
            ElseIf tpy.getSpecial = SpecialPinyin.Yu Then
                cPinyinConfig.ApplyYu(ListFace)
            ElseIf tpy.getSpecial = SpecialPinyin.None Then
                cPinyinConfig.ApplyNormal(ListFace, tpy)
            End If
            usingIndex += 1
            If usingIndex >= ListPY.Count Then usingIndex = -1

        End Sub

        ''' <summary>
        ''' 将拼音块信息写成VMD关键帧
        ''' </summary>
        Public Shared Sub ApplySinglePinyinOut(pybs As List(Of CPYBlock))
            For i = 0 To 4
                ListFace(i).PointList.Clear()
            Next
            If pybs.Count Then
                If CheckPYBListError(PYBlockList) Then
                    For Each pybHead As CPYBlock In pybs
                        WriteBlock(pybHead)
                    Next
                Else
                    Form1.PostMsg("拼音块布局存在错误")
                End If
            End If
        End Sub

        Private Shared Sub WriteBlock(pyb As CPYBlock)

            Dim tpy As cPinyin = pyb.GetPinyinClass
            cPinyinConfig.Interval = pyb.GetDuration
            Dim start As Single = pyb.GetStart
            If tpy.isPause Then
                cPinyinConfig.ApplyPause(ListFace, start)
            ElseIf tpy.GetSpecial = SpecialPinyin.ZhiChiShiRi Then
                cPinyinConfig.ApplyZhi(ListFace, start)
            ElseIf tpy.GetSpecial = SpecialPinyin.ZiCiSi Then
                cPinyinConfig.ApplyZi(ListFace, start)
            ElseIf tpy.GetSpecial = SpecialPinyin.Yu Then
                cPinyinConfig.ApplyYu(ListFace, start)
            ElseIf tpy.GetSpecial = SpecialPinyin.None Then
                cPinyinConfig.ApplyNormal(ListFace, tpy, start)
            End If
            If pyb.IfHaveNext Then
                Call WriteBlock(pyb.NextBlock)
            End If

        End Sub



    End Class

    Public Sub LoadText(text As String)

        ListPY.Clear()
        Dim tst() As String = Regex.Split(text, " ")
        If tst.Length Then
            For i = 0 To tst.Length - 1
                Dim tpy As New cPinyin(tst(i))
                ListPY.Add(tpy)
            Next
        End If

    End Sub

    Public Sub LoadPinyinConfig()

        Dim savefile As New FileStream("pinyin.mmt", FileMode.OpenOrCreate)
        Dim tstr As Stream = savefile
        Dim r As StreamReader = New StreamReader(tstr)
        Dim configtext As String = ""
        configtext = r.ReadToEnd
        r.Close()
        tstr.Dispose()

        'analyse
        '//(interval,value)

        Dim mc0 As MatchCollection = Regex.Matches(configtext, "<!--(.[\s\S]+?)-->")
        If mc0.Count > 0 Then
            For i = 0 To mc0.Count - 1
                configtext = configtext.Replace("<!--" & mc0(i).Groups(1).Value & "-->", "")
            Next
        End If
        mc0 = Regex.Matches(configtext, "<info(.[\s\S]+?)</info>")
        If mc0.Count Then
            For i = 0 To mc0.Count - 1
                configtext = configtext.Replace("<info" & mc0(i).Groups(1).Value & "</info>", "")
            Next
        End If
        Dim mc1 As MatchCollection = Regex.Matches(configtext, "<(.[\s\S]+?)>(.[\s\S]+?)</")
        If mc1.Count > 0 Then
            For i = 0 To mc1.Count - 1
                Dim tcmdo As String = mc1(i).Groups(1).Value
                Dim tcono As String = mc1(i).Groups(2).Value
                tcono = tcono.Replace(vbCrLf, "")

                Dim cmd() As String = Regex.Split(tcmdo, " ")
                If cmd(0) = "defsp" Then
                    If cmd(1) = "pause" Then
                        cPinyinConfig.Pause = cPinyinConfigLabel.LoadFromText(tcono)
                    ElseIf cmd(1) = "Zhi" Then
                        cPinyinConfig.Zhi = cPinyinConfigLabel.LoadFromText(tcono)
                    ElseIf cmd(1) = "Zi" Then
                        cPinyinConfig.Zi = cPinyinConfigLabel.LoadFromText(tcono)
                    ElseIf cmd(1) = "Yu" Then
                        cPinyinConfig.Yu = cPinyinConfigLabel.LoadFromText(tcono)
                    Else
                        Throw New Exception("invalid command")
                    End If
                ElseIf cmd(0) = "def" Then
                    If cmd(1) = "SingleE" Then
                        cPinyinConfig.SingleE = cPinyinConfigLabel.LoadFromText(tcono)
                    ElseIf cmd(1) = "Ian" Then
                        cPinyinConfig.Ian = cPinyinConfigLabel.LoadFromText(tcono)
                    End If
                ElseIf cmd(0) = "wav1" Then
                    cPinyinConfig.Wav1 = cPinyinConfigLabel.LoadFromText(tcono)
                ElseIf cmd(0) = "wav2" Then
                    cPinyinConfig.Wav2 = cPinyinConfigLabel.LoadFromText(tcono)
                ElseIf cmd(0) = "wav3" Then
                    cPinyinConfig.Wav3 = cPinyinConfigLabel.LoadFromText(tcono)
                ElseIf cmd(0) = "set" Then
                    Dim tset() As String = Regex.Split(tcono, ";")
                    If tset.Length > 0 Then
                        For j = 0 To tset.Length - 1
                            Dim tequ() As String = Regex.Split(tset(j), "=")
                            If tequ.Length >= 2 Then
                                Dim lparam As String = tequ(0).Replace(" ", "")
                                Dim rparam As Single = CSng(tequ(1).Replace(" ", ""))
                                If lparam = "closemouthbefore" Then
                                    cPinyinConfig.CloseMouthBef = rparam
                                ElseIf lparam = "closemouthafter" Then
                                    cPinyinConfig.CloseMouthAft = rparam
                                Else
                                    Throw New Exception("invalid command")
                                End If
                            End If
                        Next
                    End If
                Else
                    Throw New Exception("invalid command")
                End If

            Next
        End If


    End Sub

    ''' <summary>
    ''' 将拼音列表转换为拼音块列表
    ''' </summary>
    <Obsolete("Use PYBlockAssembler Class", True)>
    Public Sub ConvertToPYBlock(Source As List(Of cPinyin), ByRef Output As List(Of CPYBlock), Interval As Single)
        If Source.Count Then
            Dim lastPyb As CPYBlock = Nothing
            For i = 0 To Source.Count - 1
                Dim py As cPinyin = Source(i)
                Dim pyb As New CPYBlock(py, Interval, i * Interval, True)
                If py.isFullStop Then
                    If lastPyb IsNot Nothing Then
                        If lastPyb.IfLinkNext Then
                            lastPyb.BreakLinkNext()
                        End If
                    End If
                Else
                    lastPyb.NextBlock = pyb
                    pyb.LastBlock = lastPyb
                    Output.Add(pyb)
                End If
                lastPyb = pyb
            Next
        End If
    End Sub

End Module

''' <summary>
''' 汉语拼音信息类
''' </summary>
Public Class cPinyin
    Public Pinyin As String = ""
    Public Vowel As New List(Of Byte)
    Private CloseMouth As CloseMouthParam = CloseMouthParam.None
    Public isPause As Boolean = False
    Public isFullStop As Boolean = False
    Private Special As SpecialPinyin = SpecialPinyin.None
    Public SingleE As Boolean = True
    Public ChangedA As Boolean = False

    Public Sub New(tpinyin As String)
        Dim tin As String = tpinyin.Trim.ToLower
        If tin.Length Then
            Pinyin = tin
            If tin = "pause" Then
                isPause = True
            ElseIf tin = "break" Then
                isFullStop = true
            Else
                Call Ana()
            End If
        End If
    End Sub

    Private Sub Ana()
        Vowel.Clear()
        CloseMouth = 0
        If Pinyin.Contains("ie") Or Pinyin.Contains("ei") Or Pinyin.Contains("ue") Or Pinyin.Contains("ve") Or Pinyin.Contains("ye") Then
            SingleE = False
        End If
        If Pinyin.Contains("ian") AndAlso Not Pinyin.Contains("iang") Then
            ChangedA = True
        End If
        If Pinyin = "zhi" Or Pinyin = "chi" Or Pinyin = "shi" Or Pinyin = "ri" Then
            Special = SpecialPinyin.ZhiChiShiRi
        ElseIf Pinyin = "zi" Or Pinyin = "ci" Or Pinyin = "si" Then
            Special = SpecialPinyin.ZiCiSi
        ElseIf Pinyin = "yu" Then
            Special = SpecialPinyin.Yu
        Else
            For i = 0 To Pinyin.Length - 1
                Dim tchar As String = Pinyin.Substring(i, 1)
                If tchar = "a" Then
                    Vowel.Add(0)
                ElseIf tchar = "i" Or tchar = "y" Then
                    Vowel.Add(1)
                ElseIf tchar = "u" Or tchar = "w" Or tchar = "v" Then
                    Vowel.Add(2)
                ElseIf tchar = "e" Then
                    Vowel.Add(3)
                ElseIf tchar = "o" Then
                    Vowel.Add(4)
                End If
                If i = 0 AndAlso (tchar = "b" Or tchar = "p" Or tchar = "m" Or tchar = "f") Then
                    CloseMouth += CloseMouthParam.Before
                End If
                If i = Pinyin.Length - 1 AndAlso (tchar = "n") Then
                    CloseMouth += CloseMouthParam.SemiAfter
                End If
            Next
        End If

    End Sub

    Public Function GetCloseMouth() As Byte
        Return CloseMouth
    End Function

    Public Function GetSpecial() As Byte
        Return Special
    End Function

End Class

