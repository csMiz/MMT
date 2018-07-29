''' <summary>
''' 拼音块类
''' </summary>
Public Class CPYBlock
    Private Label As cPinyin = Nothing
    Private Interval As Single = 0
    Private Startat As Single = 0
    Private ErrTag As ErrorTagCode = ErrorTagCode.NoError

    'Private LinkedNextBlock As CPYBlock
    Private LinkNext As Boolean = True

    Public Enum ErrorTagCode As Byte
        NoError = 0
        LengthExceed = 1
    End Enum

    ''' <summary>
    ''' 创建一个拼音块
    ''' </summary>
    ''' <param name="tLabel">汉语拼音</param>
    ''' <param name="tInterval">拼音时长帧数</param>
    ''' <param name="tStart">拼音起始帧数</param>
    ''' <param name="ifLinkNext">是否连接下一个拼音块</param>
    Public Sub New(Optional tLabel As cPinyin = Nothing, Optional tInterval As Single = 0.0F, Optional tStart As Single = 0.0F, Optional ifLinkNext As Boolean = True)
        Label = tLabel
        Interval = tInterval
        Startat = tStart
        LinkNext = ifLinkNext
    End Sub

    Public Function Copy() As CPYBlock
        Dim r As New CPYBlock(Label, Interval, Startat, LinkNext)
        Return r
    End Function

    ''' <summary>
    ''' 获取拼音块开始时值，单位为帧
    ''' </summary>
    Public Function GetStart() As Single
        Return Startat
    End Function

    ''' <summary>
    ''' 获取拼音块长度，单位为帧
    ''' </summary>
    Public Function GetLength() As Single
        Return Interval
    End Function

    ''' <summary>
    ''' 获取拼音块结束时值，单位为帧
    ''' </summary>
    Public Function GetEnd() As Single
        Return Startat + Interval
    End Function

    Public Function GetLabel() As String
        If Label.isPause Then
            Return ","
        Else
            Return Label.Pinyin.Substring(0, 1)
        End If
    End Function

    Public Function GetPinyin() As String
        Return Label.Pinyin
    End Function

    Public Function IfLinkNext() As Boolean
        Return LinkNext
    End Function

    Public Sub SetLength(value As Single)
        Interval = value
    End Sub

    Public Sub DisconnectNext()
        LinkNext = False
    End Sub

    Public Sub MoveEndLineSingle(delta As Single)
        '需要处理重叠问题
        Me.DisconnectNext()
        Call MoveEndLineAllAfter(delta)
    End Sub

    Public Sub MoveEndLineAllAfter(delta As Single)
        Interval += delta   '需要及时刷新CPYBlock列表，使用RefreshFullList()
    End Sub

    <Obsolete("Use MoveEndLineSingle() or MoveEndLineAfter()", True)>
    Public Shared Sub AdjustLength(a As CPYBlock, b As CPYBlock, delta As Long)
        a.SetLength(a.Interval + delta)
        b.MoveBlock(delta)
        b.SetLength(b.Interval - delta)
    End Sub

    <Obsolete("Use MoveEndLineSingle() or MoveEndLineAfter()", True)>
    Public Sub MoveBlock(value As Long)
        Startat += value
    End Sub

    Public Sub SetErrorCode(err As ErrorTagCode)
        ErrTag = err
    End Sub

    ''' <summary>
    ''' 刷新CPYBlock列表
    ''' </summary>
    ''' <param name="PYBList">拼音块列表</param>
    Public Shared Sub RefreshFullList(PYBList As SortedList(Of Single, CPYBlock))

    End Sub

End Class

