Imports System.Math

''' <summary>
''' MMT�ֱʹ�����
''' </summary>
Public Class PenTool
    ''' <summary>
    ''' �����е�Ψһ����·��
    ''' </summary>
    Private Penline As BezierPenLine = Nothing
    ''' <summary>
    ''' �������ű��ʣ�zoom = source_distance / draw_pixel
    ''' </summary>
    Private Zoom As Single = 1.0F
    ''' <summary>
    ''' ����������ƫ�ƣ���source_distance
    ''' </summary>
    Private AxisOrigin As PointF3 = Nothing
    Private TempAxisOrigin As New PointF3
    ''' <summary>
    ''' �϶�ʱ��ʱ�����ĸֱ�ê��
    ''' </summary>
    Private DraggingPoint As KeyValuePair(Of BezierPenPoint, Byte) = Nothing
    ''' <summary>
    ''' ��ǰ������ģʽ
    ''' </summary>
    Private DraggingMode As MouseDraggingMode = MouseDraggingMode.NONE

    Private OriginPostition As New Vector2(0, 0)

    Private District As Byte = 0

    Public Property CTRLPressed As Boolean = False

    Public Property ALTPressed As Boolean = False
    ''' <summary>
    ''' ������ģʽö��
    ''' </summary>
    Private Enum MouseDraggingMode As Byte
        NONE = 0
        ''' <summary>
        ''' ê���ƶ�
        ''' </summary>
        MOUSE_LEFT = 1
        ''' <summary>
        ''' �����϶�
        ''' </summary>
        MOUSE_RIGHT = 2
        MOUSE_MIDDLE = 3
        ''' <summary>
        ''' ��������
        ''' </summary>
        MOUSE_SCROLL = 4
    End Enum

    ''' <summary>
    ''' ���ز���·��
    ''' </summary>
    ''' <param name="line">�ֱ�·��</param>
    ''' <param name="canvas_zoom">��������</param>
    ''' <param name="canvas_origin">����������ƫ��</param>
    Public Sub LoadData(ByRef line As BezierPenLine, ByRef canvas_zoom As Single, ByRef canvas_origin As PointF3)
        Penline = line
        Zoom = canvas_zoom
        AxisOrigin = canvas_origin
    End Sub

    ''' <summary>
    ''' ����ڻ����ϰ���
    ''' </summary>
    Public Sub Mouse_Down(e As MouseEventArgs)
        OriginPostition.X = e.X * 2
        OriginPostition.Y = e.Y * 2
        If e.Button = MouseButtons.Left Then
            DraggingPoint = PointSelector(e)
            DraggingMode = MouseDraggingMode.MOUSE_LEFT
        ElseIf e.Button = MouseButtons.Right Then
            With TempAxisOrigin
                .X = AxisOrigin.X
                .Y = AxisOrigin.Y
                .Z = AxisOrigin.Z
            End With
            Call GetMouseDistrict(e)
            DraggingMode = MouseDraggingMode.MOUSE_RIGHT
        End If
    End Sub

    ''' <summary>
    ''' ����ڻ������ƶ�/�϶�
    ''' </summary>
    Public Sub Mouse_Move(e As MouseEventArgs)
        If DraggingMode = MouseDraggingMode.MOUSE_LEFT Then
            If DraggingPoint.Key IsNot Nothing Then
                If District = 1 Then
                    DraggingPoint.Key.TempMove((e.X * 2 - OriginPostition.X) * Zoom, (-e.Y * 2 + OriginPostition.Y) * Zoom, 0, DraggingPoint.Value, CTRLPressed)
                ElseIf District = 2 Then
                    DraggingPoint.Key.TempMove(0, (-e.Y * 2 + OriginPostition.Y) * Zoom, (-e.X * 2 + OriginPostition.X) * Zoom, DraggingPoint.Value, CTRLPressed)
                ElseIf District = 3 Then
                    DraggingPoint.Key.TempMove((e.X * 2 - OriginPostition.X) * Zoom, 0, (-e.Y * 2 + OriginPostition.Y) * Zoom, DraggingPoint.Value, CTRLPressed)
                End If
            End If
        ElseIf DraggingMode = MouseDraggingMode.MOUSE_RIGHT Then
            If District = 1 Then
                AxisOrigin.X = TempAxisOrigin.X - (e.X * 2 - OriginPostition.X) * Zoom
                AxisOrigin.Y = TempAxisOrigin.Y + (e.Y * 2 - OriginPostition.Y) * Zoom
            ElseIf District = 2 Then
                AxisOrigin.Z = TempAxisOrigin.Z + (e.X * 2 - OriginPostition.X) * Zoom
                AxisOrigin.Y = TempAxisOrigin.Y + (e.Y * 2 - OriginPostition.Y) * Zoom
            ElseIf District = 3 Then
                AxisOrigin.X = TempAxisOrigin.X - (e.X * 2 - OriginPostition.X) * Zoom
                AxisOrigin.Z = TempAxisOrigin.Z + (e.Y * 2 - OriginPostition.Y) * Zoom
            End If

        End If
    End Sub

    ''' <summary>
    ''' ����ڻ������ɿ�
    ''' </summary>
    Public Sub Mouse_Up(e As MouseEventArgs)
        DraggingMode = MouseDraggingMode.NONE
        If DraggingPoint.Key IsNot Nothing AndAlso ALTPressed Then
            If PointF3.GetDistance(DraggingPoint.Key.PointMiddle, DraggingPoint.Key.PointAfter) < 0.1 Then
                DraggingPoint.Key.ReverseAltClick()
            Else
                DraggingPoint.Key.ApplyAltClick()
            End If
        End If
        Penline.GenerateBezier()

    End Sub

    ''' <summary>
    ''' ��������ͼ�ϵ�������ж�ѡ�еĸֱʵ㣬��ʶ��ѡ�е����м�㣬��㻹��ǰ��
    ''' </summary>
    Private Function PointSelector(e As MouseEventArgs) As KeyValuePair(Of BezierPenPoint, Byte)
        Dim trueX As Single = e.X * 2
        Dim trueY As Single = e.Y * 2
        Dim candidates As New List(Of KeyValuePair(Of BezierPenPoint, Byte))
        Dim view As Byte = 0
        If trueX <= 500 AndAlso trueY <= 375 Then   '����ͼ
            view = 1
            Dim sX As Single = (trueX - 250) * Zoom + AxisOrigin.X
            Dim sY As Single = -(trueY - 187) * Zoom + AxisOrigin.Y
            For Each bp As BezierPenPoint In Penline.Content
                Dim children As PointF3() = bp.GetPoints
                For i = 0 To 2
                    Dim p As PointF3 = children(i)
                    If Sqrt((p.X - sX) ^ 2 + (p.Y - sY) ^ 2) <= 12 * Zoom Then
                        candidates.Add(New KeyValuePair(Of BezierPenPoint, Byte)(bp, i))
                    End If
                Next
            Next
        ElseIf trueX > 500 AndAlso trueY <= 375 Then    '����ͼ
            view = 2
            Dim sZ As Single = (-trueX + 750) * Zoom + AxisOrigin.Z
            Dim sY As Single = -(trueY - 187) * Zoom + AxisOrigin.Y
            For Each bp As BezierPenPoint In Penline.Content
                Dim children As PointF3() = bp.GetPoints
                For i = 0 To 2
                    Dim p As PointF3 = children(i)
                    If Sqrt((p.Z - sZ) ^ 2 + (p.Y - sY) ^ 2) <= 12 * Zoom Then
                        candidates.Add(New KeyValuePair(Of BezierPenPoint, Byte)(bp, i))
                    End If
                Next
            Next
        ElseIf trueX <= 500 AndAlso trueY > 375 Then    '����ͼ
            view = 3
            Dim sX As Single = (trueX - 250) * Zoom + AxisOrigin.X
            Dim sZ As Single = -(trueY - 562) * Zoom + AxisOrigin.Z
            For Each bp As BezierPenPoint In Penline.Content
                Dim children As PointF3() = bp.GetPoints
                For i = 0 To 2
                    Dim p As PointF3 = children(i)
                    If Sqrt((p.X - sX) ^ 2 + (p.Z - sZ) ^ 2) <= 12 * Zoom Then
                        candidates.Add(New KeyValuePair(Of BezierPenPoint, Byte)(bp, i))
                    End If
                Next
            Next
        Else    '͸��ͼ����ʱ������
        End If
        District = view
        If candidates.Count Then
            If view = 1 Then
                candidates.Sort(New Comparison(Of KeyValuePair(Of BezierPenPoint, Byte))(Function(a As KeyValuePair(Of BezierPenPoint, Byte), b As KeyValuePair(Of BezierPenPoint, Byte))
                                                                                             Return (b.Key.GetPoints(b.Value).Z - a.Key.GetPoints(a.Value).Z)
                                                                                         End Function))
            ElseIf view = 2 Then
                candidates.Sort(New Comparison(Of KeyValuePair(Of BezierPenPoint, Byte))(Function(a As KeyValuePair(Of BezierPenPoint, Byte), b As KeyValuePair(Of BezierPenPoint, Byte))
                                                                                             Return (b.Key.GetPoints(b.Value).X - a.Key.GetPoints(a.Value).X)
                                                                                         End Function))
            ElseIf view = 3 Then
                candidates.Sort(New Comparison(Of KeyValuePair(Of BezierPenPoint, Byte))(Function(a As KeyValuePair(Of BezierPenPoint, Byte), b As KeyValuePair(Of BezierPenPoint, Byte))
                                                                                             Return (b.Key.GetPoints(b.Value).Y - a.Key.GetPoints(a.Value).Y)
                                                                                         End Function))
            End If
            candidates(0).Key.RefreshTemp()
            Return candidates(0)
        End If
        Return Nothing
    End Function

    Private Sub GetMouseDistrict(e As MouseEventArgs)
        Dim trueX As Single = e.X * 2
        Dim trueY As Single = e.Y * 2
        If trueX <= 500 AndAlso trueY <= 375 Then   '����ͼ
            District = 1
        ElseIf trueX > 500 AndAlso trueY <= 375 Then    '����ͼ
            District = 2
        ElseIf trueX <= 500 AndAlso trueY > 375 Then    '����ͼ
            District = 3
        End If
    End Sub

    ''' <summary>
    ''' ��������
    ''' </summary>
    Public Sub DrawBezierLine(G As Graphics)
        Call Penline.PaintPointsAndLine(G, AxisOrigin, Zoom)
    End Sub

    Public Sub SetZoom(value As Single)
        Zoom = value
    End Sub

    Public Function GetZoom() As Single
        Return Zoom
    End Function

End Class