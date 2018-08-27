Imports System
Imports Miz_MMD_Tool

''' <summary>
''' 指数函数辅助线类
''' y = A ^ (S * x + H) + V
''' </summary>
Public Class AuxiliaryExponential
    Implements IAuxiliaryLine

    Private A As Single = 2.0F
    Private V As Single = -1.0F
    Private H As Single = 0.0F
    Private S As Single = 1.0F

    ''' <summary>
    ''' 默认y = 2 ^ x - 1
    ''' </summary>
    Public Sub New()
    End Sub
    ''' <summary>
    ''' y = A ^ (Scale * x + Horizontal) + Vertical
    ''' </summary>
    Public Sub New(input_a As Single, input_vertical As Single, input_horizontal As Single, input_scale As Single)
        A = input_a
        V = input_vertical
        H = input_horizontal
        S = input_scale
    End Sub

    Public Function GetValue(input As Single) As Single Implements IAuxiliaryLine.GetValue
        Dim result As Single = A ^ (S * input + H) + V
        If result < 0 Then
            result = 0
        ElseIf result > 1 Then
            result = 1
        End If
        Return result
    End Function
End Class