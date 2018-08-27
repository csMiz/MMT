Imports System.Math
Imports Miz_MMD_Tool

''' <summary>
''' 简谐运动辅助线类，适用于正弦余弦相关
''' x = A * Cos(Omega * t + Phi)
''' </summary>
Public Class AuxiliarySimpleHarmonic
    Implements IAuxiliaryLine

    Private A As Single = 1.0F
    Private Omega As Single = 0.5F * PI
    Private Phi As Single = 0.0F

    ''' <summary>
    ''' 默认y = cos(Pi * x / 2)
    ''' </summary>
    Public Sub New()
    End Sub
    Public Sub New(input_a As Single, input_omega As Single, input_phi As Single)
        A = input_a
        Omega = input_omega
        Phi = input_phi
    End Sub

    ''' <summary>
    ''' 将周期T转换为Omega
    ''' </summary>
    ''' <param name="input_t">周期</param>
    Public Shared Function T2Omega(input_t As Single) As Single
        Return 2 * PI / input_t
    End Function

    Public Function GetValue(input As Single) As Single Implements IAuxiliaryLine.GetValue
        Dim result As Single = A * Cos(Omega * input + Phi)
        If result < 0 Then
            result = 0
        ElseIf result > 1 Then
            result = 1
        End If
        Return result
    End Function
End Class