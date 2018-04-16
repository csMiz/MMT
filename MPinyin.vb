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
		Public ChangedA as Boolean = False

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
			End if
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

End Module
