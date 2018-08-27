Imports System
Imports Miz_MMD_Tool

''' <summary>
''' 多项式辅助线类，适用于匀加速运动模型等
''' </summary>
Public Class AuxiliaryPolynomial
    Implements IAuxiliaryLine

    ''' <summary>
    ''' 多项式系数
    ''' </summary>
    Private Coefficients As New List(Of Single)

    ''' <summary>
    ''' 默认y = x ^ 2
    ''' </summary>
    Public Sub New()
        With Coefficients
            .Add(1.0F)
            .Add(0.0F)
            .Add(0.0F)
        End With
    End Sub
    Public Sub New(input_coefficients As List(Of Single))
        Coefficients.AddRange(input_coefficients)
    End Sub

    Public Function GetValue(input As Single) As Single Implements IAuxiliaryLine.GetValue
        Dim count As Integer = Coefficients.Count
        Dim result As Single = 0
        If count Then
            For i = 0 To count - 1
                result += Coefficients(i) * (input ^ (count - 1 - i))
            Next
        End If
        If result < 0 Then
            result = 0
        ElseIf result > 1 Then
        result = 1
        End If
        Return result
    End Function
End Class