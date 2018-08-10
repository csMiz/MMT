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
    Private ReservedLength As Single = 0

    Public Enum ErrorTagCode As Byte
        NoError = 0
        Collided = 1
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
        ReservedLength = Interval
    End Sub

    Public Function Copy() As CPYBlock
        Dim r As New CPYBlock(Label, Interval, Startat, LinkNext)
        r.ReservedStart = r.Startat
        r.ReservedLength = r.GetLength
        Return r
    End Function

    ''' <summary>
    ''' 获取拼音块开始时值，单位为帧
    ''' </summary>
    Public Function GetStart() As Single
        If IfHaveLast() Then
            Return LastBlock.GetStart + LastBlock.GetDuration
        End If
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
        Return GetStart() + Interval
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

    Public Function GetPinyinClass() As cPinyin
        Return Label
    End Function
	
	''' <summary>
	''' 仅可用于头部块，获取此队列拼音块数量
	''' </summary>
	public function GetArrayBlockCount() as Integer
		dim count as integer = 1
		if IfHaveNext then
			count += Me.NextBlock.GetArrayBlockCount()
		end if 
		return count 
	end function 
	
	''' <summary>
	''' 仅可用于头部块，设置此队列中每一个拼音块时长
	''' </summary>
	''' <param name="value">单个块时长，单位为帧</param>
	public function SetArrayAvgLength(value as single) as boolean
		if value < 1 then return false
		Me.SetLength(value)
		if IfHaveNext then
			Me.NextBlock.SetArrayAvgLength(value)
		end if 
		return true
	end function

    Public Sub BreakLinkNext()
        If Me.IfHaveNext Then
            PYBlockList.Add(Me.NextBlock)
            Me.NextBlock.SetStartAbsolute(Me.GetStart + Me.GetDuration)
            Me.NextBlock.LastBlock = Nothing
            Me.NextBlock = Nothing
        End If
        Me.LinkNext = False
    End Sub

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

    Public Function GetHeadSecond() As Single
        Dim result As Single = 0
        If IfHaveLast() Then
            result = Me.LastBlock.GetHeadSecond()
        Else
            result = Me.GetStart() / FRAME_PER_SECOND
        End If
        Return result
    End Function

    Public Function GetTailSecond() As Single
        Dim result As Single = 0
        If IfHaveNext() Then
            result = Me.NextBlock.GetTailSecond()
        Else
            result = Me.GetEnd / FRAME_PER_SECOND
        End If
        Return result
    End Function

    Public Function GetHeadFrame() As Single
        Return Me.GetHeadSecond() * FRAME_PER_SECOND
    End Function

    Public Function GetTailFrame() As Single
        Return Me.GetTailSecond() * FRAME_PER_SECOND
    End Function

    Public Sub SetAllArrayError()
        Me.SetErrorCode(ErrorTagCode.Collided)
        If IfHaveNext() Then Me.NextBlock.SetAllArrayError()
    End Sub

    Public Sub SetAllArrayCorrect()
        Me.SetErrorCode(ErrorTagCode.NoError)
        If IfHaveNext() Then Me.NextBlock.SetAllArrayCorrect()
    End Sub

    Public Sub Expand(ByRef output As List(Of CPYBlock))
        output.Add(Me)
        If IfHaveNext() Then
            NextBlock.Expand(output)
        End If
    End Sub

    ''' <summary>
    ''' 设置绝对时长，单位为帧
    ''' </summary>
    Public Sub SetLength(value As Single)
        Interval = value
        If Interval < 1 Then Interval = 1
        ReservedLength = Interval

    End Sub

    Public Sub DeltaLength(Delta As Single)
        Interval = ReservedLength + Delta
        If Interval < 1 Then Interval = 1
        ReservedLength = Interval

    End Sub

    Public Sub DeltaTempLength(Delta As Single)
        Interval = ReservedLength + Delta
        If Interval < 1 Then Interval = 1
    End Sub

    <Obsolete("不实用", False)>
    Public Sub MoveEndLineSingle(delta As Single)
        '需要处理重叠问题
        Me.BreakLinkNext()
        Call MoveEndLineAllAfter(delta)
    End Sub

    Public Sub MoveEndLineAllAfter(Delta As Single)
        Interval += Delta   '需要及时刷新CPYBlock列表，使用RefreshFullList()
    End Sub

    Public Sub SetDeltaStart(TempDelta As Single)
        If IfLinkLast() Then
            Form1.HoldPostMsg("不可移动")
        Else
            Startat = ReservedStart + TempDelta
        End If
    End Sub

    Public Sub SetStart(Delta As Single)
        If IfLinkLast() Then
            Form1.HoldPostMsg("不可移动")
        Else
            Startat = ReservedStart + Delta
            If Startat < 0 Then Startat = 0
            ReservedStart = Startat
			call BlockAssembler.SortPYBList(PYBlockList)
        End If
    End Sub

    ''' <summary>
    ''' 直接修改，跳过前端连接判定
    ''' </summary>
    Public Sub SetStartAbsolute(Value As Single)
        Startat = Value
        ReservedStart = Startat

        Call BlockAssembler.SortPYBList(PYBlockList)
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

    ''' <summary>
    ''' 绘制Block
    ''' </summary>
    ''' <param name="G">Graphics对象</param>
    ''' <param name="StartSecond"></param>
    ''' <param name="Px_per_second"></param>
    Public Sub DrawBlock(G As Graphics, startSecond As Single, px_per_second As Single)
        Dim drawLeft As Single = 100 + (Me.GetStart / FRAME_PER_SECOND - StartSecond) * Px_per_second
        Dim drawRight As Single = drawLeft + Me.GetDuration * Px_per_second / FRAME_PER_SECOND
        If drawLeft <= 900 AndAlso drawRight >= 100 Then
            With G
                If Me.Equals(SelectedBlock) Then
					if Me.ErrTag = ErrorTagCode.Collided Then
                        .FillRectangle(Brushes.OrangeRed, drawLeft, 400, drawRight - drawLeft, 60)
                    Else
						.FillRectangle(Brushes.AntiqueWhite, drawLeft, 400, drawRight - drawLeft, 60)
					end if 
                Else
					if Me.ErrTag = ErrorTagCode.Collided then
						.FillRectangle(Brushes.Pink, drawLeft, 400, drawRight - drawLeft, 60)
					else
						.FillRectangle(Brushes.LightGray, drawLeft, 400, drawRight - drawLeft, 60)
					end if 
                End If
                If IfLinkNext() Then
                    .DrawLine(GREEN_PEN_2PX, drawRight, 400, drawRight, 460)
                Else
                    .DrawLine(RED_PEN_2PX, drawRight, 400, drawRight, 460)
                End If
                If drawRight - drawLeft >= 40 Then
                    G.DrawString(GetLabel, DEFAULT_FONT, Brushes.Black, New PointF(drawLeft + 5, 405))
                End If
            End With
        End If
    End Sub

    Public Function GetArrayDurationAfter() As Single
        If IfHaveNext() Then
            Return (Me.GetDuration() + NextBlock.GetArrayDurationAfter())
        Else
            Return Me.GetDuration()
        End If
    End Function

    Public Sub StartLinkNext(WholeList As List(Of CPYBlock))
        Me.LinkNext = True
        Me.NextBlock = GetNearestHeadAfter(WholeList)
        If Me.NextBlock IsNot Nothing Then
            WholeList.Remove(Me.NextBlock)
            Me.NextBlock.LastBlock = Me
        End If
    End Sub

    Public Sub ChangeLink()
        If IfLinkNext() Then
            BreakLinkNext()
        Else
            StartLinkNext(PYBlockList)
        End If
    End Sub

    Public Function GetNearestHeadAfter(WholeList As List(Of CPYBlock)) As CPYBlock
        If WholeList.Count Then
            Dim myEnd As Single = Me.GetEnd()
            Dim minDistance As Single = 999999
            Dim result As CPYBlock = Nothing
            For i = 0 To WholeList.Count - 1
                Dim head As CPYBlock = WholeList(i)
                If head.GetStart() >= myEnd Then
                    Dim distance As Single = head.GetStart - myEnd
                    If distance < minDistance Then
                        minDistance = distance
                        result = head
                    End If
                End If
            Next
            Return result
        End If
        Return Nothing
    End Function
	
	public function GetArrayHeadBlock() as CPYBlock
		if IfHaveLast then
			return Me.LastBlock.GetArrayHeadBlock()
		end if 
		return Me
	end function 


End Class

