''' <summary>
''' 辅助线接口
''' </summary>
Public Interface IAuxiliaryLine

    ''' <summary>
    ''' 根据t获取s
    ''' </summary>
    ''' <param name="input">t值（0到1）</param>
    ''' <returns>s值（0到1）</returns>
    Function GetValue(input As Single) As Single

End Interface