Public Class PYBlockAssembler

    Public Sub Convert(Source As List(Of cPinyin), ByRef Output As List(Of CPYBlock), Interval As Single)
        If Source.Count Then
            Dim lastBlock As CPYBlock = Nothing
            Dim block As CPYBlock = Nothing
            For i = 0 To Source.Count - 1
                Dim pinyin As cPinyin = Source(i)
                block = New CPYBlock(pinyin, Interval, i * Interval, True)
                If pinyin.isFullStop Then
                    If (lastBlock IsNot Nothing) AndAlso (lastBlock.IfLinkNext()) Then
                        lastBlock.BreakLinkNext()
                    End If
                    lastBlock = Nothing
                Else
                    If (lastBlock Is Nothing) OrElse (Not lastBlock.IfLinkNext()) Then
                        Output.Add(block)
                    Else
                        lastBlock.NextBlock = block
                        block.LastBlock = lastBlock
                    End If
                    lastBlock = block
                End If
            Next
            Call Me.SortPYBList(Output)
        End If
    End Sub

    Private Function ComparePYB(A As CPYBlock, B As CPYBlock) As Single
        Return (A.GetStart() - B.GetStart())
    End Function

    Public Sub SortPYBList(ByRef L As List(Of CPYBlock))
        If L.Count Then
            L.Sort(New Comparison(Of CPYBlock)(AddressOf ComparePYB))
        End If
    End Sub

End Class
