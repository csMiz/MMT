''' <summary>
''' 拼音块类
''' </summary>
Public Class CPYBlock
    Private Label As cPinyin = Nothing
    Private Interval As Single = 0
    Private Startat As Single = 0
    Private ErrTag As ErrorTagCode = ErrorTagCode.NoError

    Public NextBlock As CPYBlock
    Public LastBlock As CPYBlock
    Private LinkNext As Boolean = True

    Private ReservedStart As Single = 0

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

        ReservedStart = Startat
    End Sub

    Public Function Copy() As CPYBlock
        Dim r As New CPYBlock(Label, Interval, Startat, LinkNext)
        r.ReservedStart = r.Startat
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
    ''' 获取拼音块长度，单位为帧，相当于GetLength()
    ''' </summary>
    Public Function GetDuration() As Single
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

    Public Function IfHaveNext() As Boolean
        Return (Me.IfLinkNext() AndAlso (NextBlock IsNot Nothing))
    End Function

    Public Function IfLinkNext() As Boolean
        Return LinkNext
    End Function

    Public Function IfHaveLast() As Boolean
        Return (LastBlock IsNot Nothing)
    End Function

    ''' <summary>
    ''' 等同于IfHaveLast()
    ''' </summary>
    ''' <returns></returns>
    Public Function IfLinkLast() As Boolean
        Return IfHaveLast()
    End Function

    Public Sub Expand(ByRef output As List(Of CPYBlock))
        output.Add(Me)
        If IfHaveNext() Then
            NextBlock.Expand(output)
        End If
    End Sub

    Public Sub SetLength(value As Single)
        Interval = value
    End Sub

    Public Sub DisconnectNext()
        LinkNext = False
    End Sub

    <Obsolete("不实用", False)>
    Public Sub MoveEndLineSingle(delta As Single)
        '需要处理重叠问题
        Me.DisconnectNext()
        Call MoveEndLineAllAfter(delta)
    End Sub

    Public Sub MoveEndLineAllAfter(Delta As Single)
        Interval += Delta   '需要及时刷新CPYBlock列表，使用RefreshFullList()
    End Sub

    Public Sub SetDeltaStart(TempDelta As Single)
        If IfLinkLast() Then
            Form1.PostMsg("Cannot move this block")
        Else
            Startat = ReservedStart + TempDelta
        End If
    End Sub

    Public Sub SetStart(Delta As Single)
        If IfLinkLast() Then
            Form1.PostMsg("Cannot move this block")
        Else
            Startat = ReservedStart + Delta
            ReservedStart = Startat
        End If
    End Sub

    Public Sub SetStartAbsolute(Value As Single)
        If IfLinkLast() Then
            Form1.PostMsg("Cannot move this block")
        Else
            Startat = Value
            ReservedStart = Startat
        End If
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

    Public Sub RefreshArray()
        If IfLinkLast() Then
            Startat = LastBlock.GetStart + LastBlock.GetDuration
        End If
        If IfHaveNext() Then
            NextBlock.RefreshArray()
        End If
    End Sub

    Public Sub MoveRightInArray()
        If IfHaveNext() Then
            Dim nextNextBlock As CPYBlock = NextBlock.NextBlock
            If nextNextBlock IsNot Nothing Then
                nextNextBlock.LastBlock = Me
            End If
            NextBlock.NextBlock = Me
            NextBlock.LastBlock = Me.LastBlock
            Me.LastBlock = Me.NextBlock
            Me.NextBlock = nextNextBlock
            Me.LastBlock.RefreshArray()
        Else
            Form1.PostMsg("No right element found")
        End If
    End Sub

    Public Sub MoveLeftInArray()
        If IfLinkLast() Then
            Dim lastLastBlock As CPYBlock = LastBlock.LastBlock
            If lastLastBlock IsNot Nothing Then
                lastLastBlock.NextBlock = Me
            End If
            LastBlock.LastBlock = Me
            LastBlock.NextBlock = Me.NextBlock
            Me.NextBlock = Me.LastBlock
            Me.LastBlock = lastLastBlock
            Me.RefreshArray()
        Else
            Form1.PostMsg("No left element found")
        End If
    End Sub

    Public Sub DeleteInArray()
        If IfLinkLast() Then
            LastBlock.NextBlock = Me.NextBlock
        End If
        If IfHaveNext() Then
            NextBlock.LastBlock = Me.LastBlock
        End If
        If (Not IfHaveLast()) AndAlso IfHaveNext() Then
            PYBlockList.Remove(Me)
            PYBlockList.Add(Me.NextBlock)
        ElseIf (Not IfHaveLast()) AndAlso (Not IfHaveNext()) Then
            PYBlockList.Remove(Me)
        End If
    End Sub

    Public Sub InsertAfterInArray(NewBlock As CPYBlock)
        If IfLinkNext() Then
            If IfHaveNext() Then
                NextBlock.LastBlock = NewBlock
                NewBlock.NextBlock = NextBlock
            End If
            NewBlock.LastBlock = Me
            Me.NextBlock = NewBlock
        Else    '独立项
            PYBlockList.Add(NewBlock)
        End If

    End Sub

    Public Function SearchInArray(InputFrame As Single) As CPYBlock
        If Me.GetEnd >= InputFrame Then
            Return Me
        Else
            If IfHaveNext() Then
                Return NextBlock.SearchInArray(InputFrame)
            End If
            Return Nothing
        End If
    End Function

    Public Sub DrawBlock(G As Graphics)
        '绘制Block
    End Sub

    Public Function GetArrayDurationAfter() As Single
        If IfHaveNext() Then
            Return (Me.GetDuration() + NextBlock.GetArrayDurationAfter())
        Else
            Return Me.GetDuration()
        End If
    End Function
End Class

