''' <summary>
''' 画出来的按钮
''' </summary>
Public Class CDrawingButton
    Public Left As Single
    Public Top As Single
    Public Right As Single
    Public Bottom As Single
    Public Text As String = vbNullString
    Public TextMargin As New Vector2(60, 20)

    Protected HitPoint As New Vector2

    Public Sub New()
    End Sub
    Public Sub New(L As Single, T As Single, R As Single, B As Single, Label As String)
        Left = L
        Right = R
        Top = T
        Bottom = B
        Text = Label
    End Sub

    Public Sub DrawButton(G As Graphics)
        With G
            .FillRectangle(Brushes.LightGray, Left, Top, Right - Left, Bottom - Top)
            .DrawRectangle(Pens.Black, Left, Top, Right - Left, Bottom - Top)
            .DrawString(Text, DEFAULT_FONT, Brushes.Black, New PointF(Left + TextMargin.X, Top + TextMargin.Y))
        End With
    End Sub

    ''' <summary>
    ''' 鼠标按下事件
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns>是否点中</returns>
    Public Overridable Function MouseDown(e As MouseEventArgs) As Boolean
        Dim trueX As Single = e.X * 2
        Dim trueY As Single = e.Y * 2
        If trueX >= Left AndAlso trueX < Right AndAlso trueY >= Top AndAlso trueY < Bottom Then
            HitPoint.X = trueX
            HitPoint.Y = trueY
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' 输出横向偏移
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    Public Overridable Function MouseMove(e As MouseEventArgs) As Single
        Dim trueX As Single = e.X * 2
        Return trueX - HitPoint.X
    End Function

    ''' <summary>
    ''' 鼠标松开处理
    ''' </summary>
    ''' <param name="e"></param>
    Public Overridable Sub MouseUp(e As MouseEventArgs)
    End Sub


End Class

Public Class MoveStartButton
    Inherits CDrawingButton

    Public Sub New(L As Single, T As Single, R As Single, B As Single, Label As String)
        MyBase.New(L, T, R, B, Label)
    End Sub

    Public Overrides Sub MouseUp(e As MouseEventArgs)
        Dim trueX As Single = e.X * 2
        Dim deltaX = trueX - HitPoint.X
        deltaX = PxToFrame(deltaX)
        SelectedBlock.SetStart(deltaX)
        HitPoint.X = 0
        HitPoint.Y = 0
    End Sub
End Class

Public Class DeltaLengthButton
    Inherits CDrawingButton

    Public Sub New(L As Single, T As Single, R As Single, B As Single, Label As String)
        Left = L
        Right = R
        Top = T
        Bottom = B
        Text = Label
    End Sub

    Public Overrides Sub MouseUp(e As MouseEventArgs)
        Dim trueX As Single = e.X * 2
        Dim deltaX = trueX - HitPoint.X
        deltaX = PxToFrame(deltaX)
        SelectedBlock.DeltaLength(deltaX)
        HitPoint.X = 0
        HitPoint.Y = 0
    End Sub
End Class

Public Class ChangeLinkButton
    Inherits CDrawingButton

    Public Sub New(L As Single, T As Single, R As Single, B As Single, Label As String)
        Left = L
        Right = R
        Top = T
        Bottom = B
        Text = Label
    End Sub

    Public Overrides Sub MouseUp(e As MouseEventArgs)
        SelectedBlock.ChangeLink()
    End Sub
End Class
