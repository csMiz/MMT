
Imports System.Math

''' <summary>
''' MMT数学助手类，单例
''' </summary>
Public NotInheritable Class MathHelper

    Shared Me_instance As MathHelper = Nothing

    ''' <summary>
    ''' 正态分布
    ''' </summary>
    Public ND As NormalDistribution

    Private Sub New()
        ND = New NormalDistribution
    End Sub

    Public Shared ReadOnly Property Instance() As MathHelper
        Get
            If Me_instance Is Nothing Then
                Me_instance = New MathHelper()
            End If
            Return Me_instance
        End Get
    End Property

    ''' <summary>
    ''' 获取PI
    ''' </summary>
    Public Function GetPI(nums As Integer) As String
        nums = nums / 5
        Dim max As Long, result() As String
        Dim i As Long, j As Long, t, d As Long, g, k As Long, f()
        max = 18 * nums
        ReDim f(0 To max)
        ReDim result(nums)
        For i = 0 To max
            f(i) = 20000
        Next
        g = 20000
        For j = max To 1 Step -18
            t = 0
            For i = j To 1 Step -1
                t = t + f(i) * 100000
                d = 2 * i + 1
                f(i) = t - Int(t / d) * d
                t = Int(t / d) * i
            Next
            k = k + 1
            result(k) = Format(Int(g + t / 100000) Mod 100000, "00000")

            g = t Mod 100000
        Next
        Return Join(result, "")
    End Function

    Public Function GeneralBezier(points As List(Of PointF3), value As Double) As PointF3
        If points.Count = 0 Then Return Nothing
        Dim r As New PointF3
        Dim n As Short = points.Count - 1
        For i = 0 To n
            r.X += PascalT(n, i) * points(i).X * (1 - value) ^ (n - i) * value ^ i
            r.Y += PascalT(n, i) * points(i).Y * (1 - value) ^ (n - i) * value ^ i
            r.Z += PascalT(n, i) * points(i).Z * (1 - value) ^ (n - i) * value ^ i
        Next
        Return r
    End Function

    Public Function PascalT(a As Short, t As Short) As Integer
        Dim r1 As Double = 1
        Dim r2 As Double = 1
        If t = 0 Then Return 1
        For i = 0 To t - 1
            r1 *= (a - i)
            r2 *= (i + 1)
        Next
        Return CInt(r1 / r2)
    End Function

End Class

''' <summary>
''' 正态分布类
''' </summary>
Public Class NormalDistribution
    Private NDTable As New List(Of PointF)

    Public Sub New()
        Call InitNDTable()

    End Sub

    Public Sub InitNDTable()
        For i = -400 To 399
            Dim tx As Double = i / 100
            Dim tv As Double = CND(tx)
            NDTable.Add(New PointF(CSng(tx), CSng(tv)))
        Next
    End Sub

    Public Function RCND(Acc As Single) As Single
        For i = 0 To NDTable.Count - 2
            If Acc < NDTable(i + 1).Y AndAlso Acc >= NDTable(i).Y Then
                Return NDTable(i).X
            End If
        Next
        Return 3.99
    End Function

    Public Function CND(X As Double) As Double

        Dim L As Double = 0.0
        Dim K As Double = 0.0
        Dim dCND As Double = 0.0
        Const a1 As Double = 0.31938153
        Const a2 As Double = -0.356563782
        Const a3 As Double = 1.781477937
        Const a4 As Double = -1.821255978
        Const a5 As Double = 1.330274429
        L = Abs(X)
        K = 1.0 / (1.0 + 0.2316419 * L)
        dCND = 1.0 - 1.0 / Sqrt(2 * Convert.ToDouble(PI.ToString())) * Exp(-L * L / 2.0) * (a1 * K + a2 * K * K + a3 * Pow(K, 3.0) + a4 * Pow(K, 4.0) + a5 * Pow(K, 5.0))

        If (X < 0) Then
            Return 1.0 - dCND
        Else
        End If
        Return dCND

    End Function
End Class

''' <summary>
''' 三维向量
''' </summary>
Public Class Vector3
    Public X As Single = 0
    Public Y As Single = 0
    Public Z As Single = 0

    Public Sub New(Optional tx As Single = 0, Optional ty As Single = 0, Optional tz As Single = 0)
        X = tx
        Y = ty
        Z = tz
    End Sub
End Class

''' <summary>
''' 可复制点接口
''' </summary>
Public Interface iPointEx
    Function Copy() As iPointEx
End Interface

''' <summary>
''' 三位坐标点
''' </summary>
Public Class PointF3
    Implements iPointEx
    Public X As Single = 0, Y As Single = 0, Z As Single = 0

    Public Sub New(Optional tx As Single = 0, Optional ty As Single = 0, Optional tz As Single = 0)
        X = tx
        Y = ty
        Z = tz
    End Sub

    Public Sub Move(v As Vector3)
        X += v.X
        Y += v.Y
        Z += v.Z
    End Sub

    Public Function Copy() As iPointEx Implements iPointEx.Copy
        Dim r As New PointF3(X, Y, Z)
        Return r
    End Function

    Public Function GetXZ() As PointF
        Dim r As New PointF(X, Z)
        Return r
    End Function

    Public Shared Function GetDistance(p1 As PointF3, p2 As PointF3) As Single
        Return Sqrt((p1.X - p2.X) ^ 2 + (p1.Y - p2.Y) ^ 2 + (p1.Z - p2.Z) ^ 2)
    End Function

    Public Shared Operator -(a As PointF3, b As PointF3) As PointF3
        Dim r As New PointF3(a.X - b.X, a.Y - b.Y, a.Z - b.Z)
        Return r
    End Operator


End Class