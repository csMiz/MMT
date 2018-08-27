
''' <summary>
''' 贝塞尔曲线钢笔点类
''' </summary>
Public Class BezierPenPoint
    Public PointBefore As PointF3
    Public PointMiddle As PointF3
    Public PointAfter As PointF3

    Public TempPointBefore As PointF3
    Public TempPointMiddle As PointF3
    Public TempPointAfter As PointF3

    Public Sub New(middle As PointF3, after As PointF3, Optional before As PointF3 = Nothing)
        PointAfter = after
        PointMiddle = middle
        If before Is Nothing Then
            Dim mirrorPoint As New PointF3
            With mirrorPoint
                .X = 2 * middle.X - after.X
                .Y = 2 * middle.Y - after.Y
                .Z = 2 * middle.Z - after.Z
            End With
            PointBefore = mirrorPoint
        Else
            PointBefore = before
        End If

        TempPointBefore = PointBefore.Copy
        TempPointMiddle = PointMiddle.Copy
        TempPointAfter = PointAfter.Copy
    End Sub

    Public Function GetPoints() As PointF3()
        Dim result(2) As PointF3
        result(0) = PointMiddle
        result(1) = PointAfter
        result(2) = PointBefore
        Return result
    End Function

    ''' <summary>
    ''' 临时移动锚点
    ''' </summary>
    ''' <param name="dx">deltaX</param>
    ''' <param name="dy">deltaY</param>
    ''' <param name="dz">deltaZ</param>
    ''' <param name="index">pointIndex</param>
    Public Sub TempMove(dx As Single, dy As Single, dz As Single, index As Byte, CTRL As Boolean)
        If index = 0 Then
            With PointMiddle
                .X = TempPointMiddle.X + dx
                .Y = TempPointMiddle.Y + dy
                .Z = TempPointMiddle.Z + dz
            End With
            With PointAfter
                .X = TempPointAfter.X + dx
                .Y = TempPointAfter.Y + dy
                .Z = TempPointAfter.Z + dz
            End With
            With PointBefore
                .X = TempPointBefore.X + dx
                .Y = TempPointBefore.Y + dy
                .Z = TempPointBefore.Z + dz
            End With
        ElseIf index = 1 Then
            With PointAfter
                .X = TempPointAfter.X + dx
                .Y = TempPointAfter.Y + dy
                .Z = TempPointAfter.Z + dz
            End With
            If Not CTRL Then
                With PointBefore
                    .X = TempPointBefore.X - dx
                    .Y = TempPointBefore.Y - dy
                    .Z = TempPointBefore.Z - dz
                End With
            End If
        ElseIf index = 2 Then
            With PointBefore
                .X = TempPointBefore.X + dx
                .Y = TempPointBefore.Y + dy
                .Z = TempPointBefore.Z + dz
            End With
            If Not CTRL Then
                With PointAfter
                    .X = TempPointAfter.X - dx
                    .Y = TempPointAfter.Y - dy
                    .Z = TempPointAfter.Z - dz
                End With
            End If
        End If
    End Sub

    Public Sub RefreshTemp()
        TempPointBefore = PointBefore.Copy
        TempPointMiddle = PointMiddle.Copy
        TempPointAfter = PointAfter.Copy
    End Sub

    Public Sub ApplyAltClick()
        With PointAfter
            .X = PointMiddle.X
            .Y = PointMiddle.Y
            .Z = PointMiddle.Z
        End With
        Call RefreshTemp()
    End Sub

    Public Sub ReverseAltClick()
        With PointAfter
            .X = 2 * PointMiddle.X - PointBefore.X
            .Y = 2 * PointMiddle.Y - PointBefore.Y
            .Z = 2 * PointMiddle.Z - PointBefore.Z
        End With
        Call RefreshTemp()
    End Sub

End Class