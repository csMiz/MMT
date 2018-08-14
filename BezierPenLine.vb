Imports System

''' <summary>
''' ������������
''' </summary>
Public Class BezierPenLine
    Public Content As New List(Of BezierPenPoint)
    Private PartLength As New List(Of Single)
    Private Length As Single = 0.0F
    Private MathEx As MathHelper = MathHelper.Instance  '����ģʽ

    ''' <summary>
    ''' ��ȡ�����ܳ���
    ''' </summary>
    Public Function GetLength() As Single
        Return Length
    End Function

    ''' <summary>
    ''' �û���ĸֱʵ��������γ��Ⱥ��ܳ���
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
    ''' ���ݽ���ֵ��s-t���߼�����ض�λ��
    ''' </summary>
    ''' <param name="value">����ֵ��0��1��</param>
    ''' <param name="s_t_curve">s-t����</param>
    ''' <returns>��ά����</returns>
    Public Function GetPoint(value As Single, s_t_curve As IAuxiliaryLine) As PointF3
        Dim actualValue As Single = s_t_curve.GetValue(value)
        Dim partIndex As KeyValuePair(Of Integer, Single) = GetPartIndex(actualValue * Length)
        Dim position As PointF3 = GetBezier(Content(partIndex.Key), Content(partIndex.Key + 1), partIndex.Value)
        Return position
    End Function

    ''' <summary>
    ''' ͨ�������ֱʵ�ͽ���ֵ������ض�λ��
    ''' </summary>
    ''' <param name="p1">��ʼλ�øֱʵ�</param>
    ''' <param name="p2">����λ�øֱʵ�</param>
    ''' <param name="value">����ֵ��0��1��</param>
    ''' <returns>��ά����</returns>
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
    ''' ����һ�α��������ߵĳ���
    ''' </summary>
    ''' <param name="p1">��ʼλ�øֱʵ�</param>
    ''' <param name="p2">����λ�øֱʵ�</param>
    ''' <returns>���߳���</returns>
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
    ''' ���ݳ��Ȳ�����Ӧ�����߶�
    ''' </summary>
    ''' <param name="positionLength">��ѯ����</param>
    ''' <returns>��ֵ�ԣ�����ţ����ڽ���ֵ��0��1����</returns>
    Public Function GetPartIndex(positionLength As Single) As KeyValuePair(Of Integer, Single)
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
            Throw New Exception("line data error. please check line parts.")
        End If
        Return New KeyValuePair(Of Integer, Single)(0, 0.0F)
    End Function


End Class