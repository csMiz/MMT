Imports Miz_MMD_Tool

Public Class AuxiliaryPolyline
    Implements IAuxiliaryLine

    Public K As Single = 1.0F
    Public B As Single = 0.0F

    Public Function GetValue(input As Single) As Single Implements IAuxiliaryLine.GetValue
        Return (K * input + B)
    End Function

End Class
