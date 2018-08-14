
''' <summary>
''' ±´Èû¶ûÇúÏß¸Ö±ÊµãÀà
''' </summary>
Public Class BezierPenPoint
    Public PointBefore As PointF3
    Public PointMiddle As PointF3
    Public PointAfter As PointF3

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
    End Sub

End Class