Imports System.IO
Imports System.Text.RegularExpressions

Module MPinyin

    Public ListPY As New List(Of cPinyin)

    Public Enum CloseMouthParam As Byte
        None = 0
        Before = 1
        SemiAfter = 2
        Both = 3
    End Enum

    Public Enum SpecialPinyin As Byte
        None = 0
        ZhiChiShiRiZiCiSi = 1
        Yu = 2

    End Enum

    Public Class cPinyin
        Public Pinyin As String = ""
        Public Vowel As New List(Of Byte)
        Public CloseMouth As CloseMouthParam = CloseMouthParam.None
        Public isPause As Boolean = False
        Public Special As SpecialPinyin = SpecialPinyin.None
        Public SingleE As Boolean = True
        Public ChangedA As Boolean = False

        Public Sub New(tpinyin As String)
            Dim tin As String = tpinyin.Trim.ToLower
            If tin.Length Then
                Pinyin = tin
                If tin = "pause" Then
                    isPause = True
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
            If Pinyin.Contains("ian") Then
                ChangedA = True
            End If
            If Pinyin = "zhi" Or Pinyin = "chi" Or Pinyin = "shi" Or Pinyin = "ri" Then
                Special = SpecialPinyin.ZhiChiShiRiZiCiSi
            ElseIf Pinyin = "zi" Or Pinyin = "ci" Or Pinyin = "si" Then
                Special = SpecialPinyin.ZhiChiShiRiZiCiSi
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

    End Class

    Public Class cWaveLable
        Public ID As Short   '0-4: a,i,u,e,o  5-7:wave1,2,3
        Public Value As New List(Of KeyValuePair(Of Single, Single))    'interval,value
    End Class

    Public Class cPinyinConfigLabel
        Public Labels As New List(Of cWaveLable)

        Public Shared Function LoadFromText(inp As String) As cPinyinConfigLabel

            Dim r As New cPinyinConfigLabel

            Dim cont() As String = Regex.Split(inp, ";")
            If cont.Count > 0 Then
                For i = 0 To cont.Count - 1
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

                Next

            End If

            Return r

        End Function
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
                            Dim lparam As String = tequ(0).Replace(" ", "")
                            Dim rparam As Single = CSng(tequ(1).Replace(" ", ""))
                            If lparam = "closemouthbefore" Then
                                cPinyinConfig.CloseMouthBef = rparam
                            ElseIf lparam = "closemouthafter" Then
                                cPinyinConfig.CloseMouthAft = rparam
                            Else
                                Throw New Exception("invalid command")
                            End If
                        Next
                    End If
                Else
                    Throw New Exception("invalid command")
                End If

            Next
        End If


    End Sub

End Module
