Imports System.IO

public interface IEntityParser

    Sub Load(input As String)

    Function GetResult() As List(Of CEntity)

End interface 