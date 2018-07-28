''' <summary>
''' 拼音块类
''' </summary>
Public Class CPYBlock
    Private Label As String = vbNullString
    Private Interval As Integer = 0
    Private Startat As Integer = 0
    Private ErrTag As ErrorTagCode = ErrorTagCode.NoError

    Public Enum ErrorTagCode As Byte
        NoError = 0
        LengthExceed = 1
    End Enum

    ''' <summary>
    ''' 创建一个拼音块
    ''' </summary>
    ''' <param name="tl">汉语拼音</param>
    ''' <param name="tint">拼音时长帧数</param>
    ''' <param name="tst">拼音起始帧数</param>
    Public Sub New(Optional tl As String = vbNullString, Optional tint As Integer = 0, Optional tst As Integer = 0)
        Label = tl
        Interval = tint
        Startat = tst
    End Sub

    Public Function Copy() As CPYBlock
        Dim r As New CPYBlock(Label, Interval, Startat)
        Return r
    End Function

    Public Function GetStart() As Integer
        Return Startat
    End Function

    Public Function GetLength() As Integer
        Return Interval
    End Function

    Public Sub SetLength(value As Integer)
        Interval = value
    End Sub

    Public Shared Sub AdjustLength(a As CPYBlock, b As CPYBlock, delta As Long)
        a.SetLength(a.Interval + delta)
        b.MoveBlock(delta)
        b.SetLength(b.Interval - delta)
    End Sub

    Public Sub MoveBlock(value As Long)
        Startat += value
    End Sub

    Public Sub SetErrorCode(err As ErrorTagCode)
        ErrTag = err
    End Sub

End Class

