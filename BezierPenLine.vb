Imports System

''' <summary>
''' 贝塞尔曲线类
''' </summary>
Public Class BezierPenLine
    Public Content As New List(Of BezierPenPoint)
    Private PartLength As New List(Of Single)
    Private Length As Single = 0.0F
    Private MathEx As MathHelper = MathHelper.Instance  '单例模式

    Private PaintingRadius As Single = 6.0F     '用于锚点
    Private PaintingRadius2 As Single = 2.0F    '用于曲线

    ''' <summary>
    ''' 从骨骼中载入钢笔锚点
    ''' </summary>
    Public Sub LoadPoints(points As List(Of PointF3))
        Dim count As Integer = points.Count
        If count AndAlso count Mod 3 = 0 Then
            For i = 0 To count - 1 Step 3
                Dim penPoint As New BezierPenPoint(points(i).Copy, points(i + 1).Copy)
                Content.Add(penPoint)
            Next
        End If
    End Sub

    ''' <summary>
    ''' 获取曲线总长度
    ''' </summary>
    Public Function GetLength() As Single
        Return Length
    End Function

    ''' <summary>
    ''' 用缓存的钢笔点计算出各段长度和总长度
    ''' </summary>
    Public Sub GenerateBezier()
        PartLength.Clear
        Length = 0.0F
        Dim total As Single = 0.0F
        If Content.Count >= 2 Then
            For i = 0 To Content.Count - 2
                Dim p1 As BezierPenPoint = Content(i)
                Dim p2 As BezierPenPoint = Content(i + 1)
                Dim part As Single = CalculatePartLength(p1, p2)
                total += part
                PartLength.Add(part)
            Next
            Length = total
        End If
    End Sub

    ''' <summary>
    ''' 根据进度值和s-t曲线计算出特定位置
    ''' </summary>
    ''' <param name="value">进度值（0到1）</param>
    ''' <param name="s_t_curve">s-t曲线</param>
    ''' <returns>三维坐标</returns>
    Public Function GetPoint(value As Single, s_t_curve As IAuxiliaryLine) As PointF3
        Dim actualValue As Single = s_t_curve.GetValue(value)
        Dim partIndex As KeyValuePair(Of Integer, Single) = GetPartIndex(actualValue * Length)
        Dim position As PointF3 = GetBezier(Content(partIndex.Key), Content(partIndex.Key + 1), partIndex.Value)
        Return position
    End Function

    ''' <summary>
    ''' 绘制三视图
    ''' </summary>
    ''' <param name="G">Graphics对象</param>
    Public Sub PaintPointsAndLine(G As Graphics, axis As PointF3, zoom As Single)
        With G
            .DrawLine(Pens.Black, 500, 0, 500, 750)
            .DrawLine(Pens.Black, 0, 375, 1000, 375)
            .DrawString("主视图", DEFAULT_FONT, BLACK_BRUSH, 10, 10)
            .DrawString("左视图", DEFAULT_FONT, BLACK_BRUSH, 510, 10)
            .DrawString("俯视图", DEFAULT_FONT, BLACK_BRUSH, 10, 385)

            If Content.Count Then

                Dim defaultLine As New AuxiliaryPolyline
                For i = 0 To 100
                    DrawPointInThreeViews(G, GetPoint(i / 100, defaultLine) - axis, BLACK_BRUSH, PaintingRadius2, zoom)
                Next

                For i = 0 To Content.Count - 1
                    DrawPointInThreeViews(G, Content(i).PointMiddle - axis, PINK_BRUSH, PaintingRadius, zoom)
                    DrawPointInThreeViews(G, Content(i).PointBefore - axis, BLUE_BRUSH, PaintingRadius, zoom)
                    DrawPointInThreeViews(G, Content(i).PointAfter - axis, BLUE_BRUSH, PaintingRadius, zoom)
                    DrawLineInThreeViews(G, Content(i).PointMiddle - axis, Content(i).PointAfter - axis, BLUE_PEN_2PX, zoom)
                    DrawLineInThreeViews(G, Content(i).PointMiddle - axis, Content(i).PointBefore - axis, BLUE_PEN_2PX, zoom)
                Next

            End If
        End With
    End Sub

    Private Sub DrawPointInThreeViews(G As Graphics, point As PointF3, colour As Brush, r As Single, zoom As Single)
        Dim diameter As Single = 2 * r
        With G
            '主视图
            Dim x1 As Single = 250 - r + point.X / zoom
            Dim y1 As Single = 187 - r - point.Y / zoom
            If x1 >= -diameter AndAlso x1 < 500 AndAlso y1 >= -diameter AndAlso y1 < 375 Then
                .FillEllipse(colour, x1, y1, diameter, diameter)
            End If
            '左视图
            Dim x4 As Single = 750 - r - point.Z / zoom
            Dim y4 As Single = y1
            If x4 >= -diameter + 500 AndAlso x4 < 1000 AndAlso y4 >= -diameter AndAlso y4 < 375 Then
                .FillEllipse(colour, x4, y4, diameter, diameter)
            End If
            '俯视图
            Dim x7 As Single = x1
            Dim y7 As Single = 562 - r - point.Z / zoom
            If x7 >= -diameter AndAlso x7 < 500 AndAlso y7 >= -diameter + 375 AndAlso y7 < 750 Then
                .FillEllipse(colour, x7, y7, diameter, diameter)
            End If
        End With

    End Sub

    Private Sub DrawLineInThreeViews(G As Graphics, pointA As PointF3, pointB As PointF3, colour As Pen, zoom As Single)
        With G
            '主视图
            Dim x1 As Single = 250 + pointA.X / zoom
            Dim y1 As Single = 187 - pointA.Y / zoom
            Dim x2 As Single = 250 + pointB.X / zoom
            Dim y2 As Single = 187 - pointB.Y / zoom
            If x1 >= 0 AndAlso x1 < 500 AndAlso y1 >= 0 AndAlso y1 < 375 AndAlso x2 >= 0 AndAlso x2 < 500 AndAlso y2 >= 0 AndAlso y2 < 375 Then
                .DrawLine(colour, x1, y1, x2, y2)
            End If
            '左视图
            Dim x4 As Single = 750 - pointA.Z / zoom
            Dim y4 As Single = y1
            Dim x5 As Single = 750 - pointB.Z / zoom
            Dim y5 As Single = y2
            If x4 >= 500 AndAlso x4 < 1000 AndAlso y4 >= 0 AndAlso y4 < 375 AndAlso x5 >= 500 AndAlso x5 < 1000 AndAlso y5 >= 0 AndAlso y5 < 375 Then
                .DrawLine(colour, x4, y4, x5, y5)
            End If
            '俯视图
            Dim x7 As Single = x1
            Dim y7 As Single = 562 - pointA.Z / zoom
            Dim x8 As Single = x2
            Dim y8 As Single = 562 - pointB.Z / zoom
            If x7 >= 0 AndAlso x7 < 500 AndAlso y7 >= 375 AndAlso y7 < 750 AndAlso x8 >= 0 AndAlso x8 < 500 AndAlso y8 >= 375 AndAlso y8 < 750 Then
                .DrawLine(colour, x7, y7, x8, y8)
            End If
        End With
    End Sub

    ''' <summary>
    ''' 通过两个钢笔点和进度值计算出特定位置
    ''' </summary>
    ''' <param name="p1">起始位置钢笔点</param>
    ''' <param name="p2">结束位置钢笔点</param>
    ''' <param name="value">进度值（0到1）</param>
    ''' <returns>三维坐标</returns>
    Private Function GetBezier(p1 As BezierPenPoint, p2 As BezierPenPoint, value As Single) As PointF3
        Dim points As New List(Of PointF3)
        With points
            .Add(p1.PointMiddle)
            .Add(p1.PointAfter)
            .Add(p2.PointBefore)
            .Add(p2.PointMiddle)
        End With
        Return MathEx.GeneralBezier(points, value)
    End Function

    ''' <summary>
    ''' 计算一段贝塞尔曲线的长度
    ''' </summary>
    ''' <param name="p1">起始位置钢笔点</param>
    ''' <param name="p2">结束位置钢笔点</param>
    ''' <returns>曲线长度</returns>
    Private Function CalculatePartLength(p1 As BezierPenPoint, p2 As BezierPenPoint) As Single
        Dim lastPoint As PointF3 = p1.PointMiddle
        Dim totalLength As Single = 0.0F
        For i = 1 To 100
            Dim value As Single = i / 100
            Dim calculatePoint As PointF3 = GetBezier(p1, p2, value)
            Dim distance As Single = PointF3.GetDistance(lastPoint, calculatePoint)
            totalLength += distance
            lastPoint = calculatePoint
        Next
        Return totalLength
    End Function

    ''' <summary>
    ''' 根据长度查找相应的曲线段
    ''' </summary>
    ''' <param name="positionLength">查询长度</param>
    ''' <returns>键值对（段序号，段内进度值（0到1））</returns>
    Private Function GetPartIndex(positionLength As Single) As KeyValuePair(Of Integer, Single)
        If positionLength > Length Then Throw New Exception("illegal position length.")
        If PartLength.Count Then
            Dim lengthLeft As Single = positionLength
            Dim index As Integer = 0
            For i = 0 To PartLength.Count - 1
                If lengthLeft > PartLength(i) Then
                    lengthLeft -= PartLength(i)
                    index += 1
                Else
                    Dim ratio As Single = lengthLeft / PartLength(i)
                    Return New KeyValuePair(Of Integer, Single)(index, ratio)
                End If
            Next
            Return New KeyValuePair(Of Integer, Single)(PartLength.Count - 1, 1.0F)
            'Throw New Exception("line data error. please check line parts.")
        End If
        Return New KeyValuePair(Of Integer, Single)(0, 0.0F)
    End Function

End Class