Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.RegularExpressions

Public Class FormFeedback

    'https://blog.csdn.net/magictong/article/details/40753519
    'https://blog.csdn.net/lyp951018/article/details/21813199
    Declare Auto Function GetNTVersion Lib "ntdll.dll" Alias "RtlGetNtVersionNumbers" (ByRef dwMajorVer As UInt32, ByRef dwMinorVer As UInt32, ByRef dwBuildNumber As UInt32) As Boolean

    Private Username As String = vbNullString
    Private Content As String = vbNullString
    Private DeviceInfo As String = vbNullString
    Private ContactEmail As String = vbNullString

    Public Const STRING_DOT As String = "."
    Public Const STRING_NA As String = "N/A"

    Public Sub Form_Load(sender As Object, e As Object) Handles Me.Load
        Me.Width = 420
        Me.Height = 620
        tbContent.Text = vbNullString
        tbUsername.Text = vbNullString
        tbEmail.Text = vbNullString
        btn1.Enabled = True
        btn1.Text = "提交"
    End Sub

    '''<summary>
    '''提交反馈
    '''</summary>
    Public Async Sub btn1_Click(sender As Object, e As Object) Handles btn1.Click
        'https://www.cnblogs.com/amosli/p/3918538.html
        btn1.Enabled = False
        btn1.Text = "处理中..."
        Username = tbUsername.Text.Trim
        Content = tbContent.Text.Trim
        ContactEmail = tbEmail.Text.Trim
        Dim formatCorrect As Boolean = CheckSubmitForm()
        If formatCorrect Then
            Dim hc1 As New HttpClient
            Dim response As String = vbNullString
            Dim uri As String = "http://207.148.14.219:8083/mmt/writeReview"
            DeviceInfo = GetDeviceInfo()
            Dim paramList As List(Of KeyValuePair(Of String, String)) = New List(Of KeyValuePair(Of String, String))
            paramList.Add(New KeyValuePair(Of String, String)("sender", Username))
            paramList.Add(New KeyValuePair(Of String, String)("content", Content))
            paramList.Add(New KeyValuePair(Of String, String)("email", ContactEmail))
            paramList.Add(New KeyValuePair(Of String, String)("info", DeviceInfo))
            '不需要转义
            Try
                '使用Post请求
                Dim co As HttpContent = New FormUrlEncodedContent(paramList)
                hc1.DefaultRequestHeaders.Add("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8")

                Dim responseMsg = hc1.PostAsync(New Uri(uri), co).Result
                response = responseMsg.Content.ReadAsStringAsync().Result
            Catch ex As Exception
                MsgBox("Connection Failed")
                btn1.Enabled = True
                btn1.Text = "提交"
                Return
            End Try

            If response = "success" Then
                MsgBox("Submit successfully")
                Me.Hide()
            Else
                MsgBox("Error")
            End If

        End If
        btn1.Enabled = True
        btn1.Text = "提交"
    End Sub

    '''<summary>
    '''转义字符处理
    '''</summary>
    Private Function ConvertChars(Input As String) As String
        Dim result = vbNullString
        If Input.Length Then
            For i = 0 To Input.Length - 1
                Dim chara As String = Input.Substring(i, 1)
                Dim ascCode As Integer = AscW(chara)
                Dim ascChar As String = ConvertAsc(ascCode)
                result = result & ascChar
            Next
        End If
        Return result
    End Function

    Private Function ConvertAsc(Input As Integer) As String
        'Hex函数将十进制Byte转换为十六进制String
        If Input < 256 Then
            Dim val As String = Hex(Input)
            Return "%" & val
        Else
            Dim val1 As String = Hex(Input \ 256)
            Dim val2 As String = Hex(Input Mod 256)
            Return "%" & val1 & "%" & val2
        End If
    End Function

    '''<summary>
    '''获取系统及设备信息
    '''</summary>
    Private Function GetDeviceInfo() As String
        'https://docs.microsoft.com/en-us/windows/desktop/api/Winbase/nf-winbase-getcomputernamea
        Dim computerName As String = SystemInformation.ComputerName
        Dim windowsVersion As String = GetUsingWindowsVersion()

        Return computerName & " " & Form1.ME_VERSION & " " & windowsVersion
    End Function

    '''<summary>
    '''获取当前运行Windows系统的NT版本信息
    '''</summary>
    Public Function GetUsingWindowsVersion() As String
        Dim v1 As UInt32 = 0, v2 As UInt32 = 0, v3 As UInt32 = 0
        Dim success As Boolean = GetNTVersion(v1, v2, v3)
        If success Then
			if v3 > 4026531840 then v3 -= 4026531840
            Return v1.ToString & STRING_DOT & v2.ToString & STRING_DOT & v3.ToString
        End If
        Return STRING_NA
    End Function

    '''<summary>
    '''检查表单格式
    '''</summary>
    Private Function CheckSubmitForm() As Boolean
        Dim checkContent As Boolean = True
        Dim checkUsername As Boolean = True
        Dim checkEmail As Boolean = True
        Dim msg As String = vbNullString

        If Content.Length() = 0 Then        '检查内容
            msg = msg & "Content cannot be EMPTY!" & vbCrLf
            checkContent = False
        ElseIf Content.Length() > 255 Then
            msg = msg & "Content too long!(Max Length is 255)" & vbCrLf
            checkContent = False
        End If

        If Username.Length() = 0 Then   '检查昵称
            msg = msg & "Nickname cannot be EMPTY!" & vbCrLf
            checkUsername = False
        ElseIf Username.Length() > 31 Then
            msg = msg & "Nickname too long!(Max Length is 31)" & vbCrLf
            checkUsername = False
        End If

        If ContactEmail.Length() = 0 Then      '检查邮箱地址
            msg = msg & "Email cannot be EMPTY!" & vbCrLf
            checkEmail = False
        ElseIf ContactEmail.Length() > 63 Then
            msg = msg & "Email too long!(Max Length is 63)" & vbCrLf
            checkEmail = False
        Else
            Dim address As String = ContactEmail
            If address.Contains("@") Then
                Dim args() As String = Regex.Split(address, "@")
                If args.Length = 2 Then
                    Dim emailUser As String = args(0)
                    Dim emailDomain As String = args(1)
                    If emailUser.Length = 0 Then
                        checkEmail = False
                    Else
                        If emailDomain.Contains(".") Then
                            Dim domains() As String = Regex.Split(emailDomain, "\.")
                            For Each domain As String In domains
                                If domain.Length = 0 Then
                                    checkEmail = False
                                    Exit For
                                End If
                            Next
                        Else
                            checkEmail = False
                        End If
                    End If

                Else
                    checkEmail = False
                End If
            Else
                checkEmail = False
            End If
            If (Not checkEmail) Then
                msg = msg & "Incorrect Email format!"
            End If
        End If

        If checkContent AndAlso checkUsername AndAlso checkEmail Then
            Return True
        End If
        MsgBox(msg)
        Return False
    End Function

End Class