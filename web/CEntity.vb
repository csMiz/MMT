public class CEntity
    Public Pairs As New List(Of KeyValuePair(Of String, String))

    Public Function GetValue(inputKey As String) As String
        If Pairs.Count Then
            For Each kvp As KeyValuePair(Of String, String) In Pairs
                If kvp.Key = inputKey Then
                    Return kvp.Value
                End If
            Next
        End If
        Throw New Exception("Key Not Found!")
    End Function

End class