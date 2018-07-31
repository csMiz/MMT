Imports System.IO
Imports System.Text.RegularExpressions
Imports Miz_MMD_Tool

Public Class CommonEntityParser
    Implements IEntityParser
    Private Source As String = vbNullString
    Private Result As New List(Of CEntity)

    Public Sub Load(input As String) Implements IEntityParser.Load
        Dim mc1 As MatchCollection = Regex.Matches(input, "{(.+?)}")
        If mc1.Count Then
            For i = 0 To mc1.Count - 1
                Dim entity As New CEntity
                Dim content As String = mc1(i).Groups(1).Value
                Dim params() As String = Regex.Split(content, ",")
                If params.Length Then
                    For j = 0 To params.Length - 1
                        Dim param As String = params(j)
                        Dim kv() As String = Regex.Split(param, ":")
                        If kv.Length >= 2 Then
                            '不知道怎么处理一大堆问号
                            Dim key As String = kv(0).Replace("""", vbNullString)
                            Dim value As String = param.Substring(key.Length + 3)
                            entity.Pairs.Add(New KeyValuePair(Of String, String)(key, value))
                        End If
                    Next
                End If
                Result.Add(entity)
            Next
        End If
    End Sub

    Public Function GetResult() As List(Of CEntity) Implements IEntityParser.GetResult
        If Result.Count Then
            Return Result
        End If
        Return Nothing
    End Function
End Class