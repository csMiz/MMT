Imports System.IO

Module mWav

    Public WavAnalyzer As New WavFileAnalyzer
    Public WavControlBar As New WavScreenController
    Private PYBlockList As New List(Of CPYBlock)
    'Private WavPaintLength As Integer = 90   '波形声音显示长度，默认为3秒即90帧
    'Private WavPaintStart As Integer = 0     '默认0，单位为帧.0

    Public Sub LoadWavFile()
        Dim openFile As New OpenFileDialog
        openFile.Filter = "波形声音|*.wav"
        openFile.Title = "打开"
        openFile.AddExtension = True
        openFile.AutoUpgradeEnabled = True
        If openFile.ShowDialog() = DialogResult.OK Then
            Dim fileStream As FileStream = CType(openFile.OpenFile, FileStream)
            WavAnalyzer.LoadFile(fileStream)
            fileStream.Dispose()
        Else
            Exit Sub
        End If

    End Sub

    Public Sub AddPYBlock(tBlock As CPYBlock)
        PYBlockList.Add(tBlock)
        Call SortPYBlock()
    End Sub

    ''' <summary>
    ''' 时间轴从小到大排列拼音块，并分析重叠错误
    ''' </summary>
    Public Sub SortPYBlock()
        Dim comparison As New Comparison(Of CPYBlock)(Function(a As CPYBlock, b As CPYBlock) a.GetStart - b.GetStart)
        PYBlockList.Sort(comparison)

        If PYBlockList.Count > 1 Then
            For i = 0 To PYBlockList.Count - 2
                Dim tpyb As CPYBlock = PYBlockList(i)
                With tpyb
                    Dim lengthExceed As Boolean = .GetStart + .GetLength > PYBlockList(i + 1).GetStart
                    If lengthExceed Then
                        .SetErrorCode(CPYBlock.ErrorTagCode.LengthExceed)
                    End If
                End With
            Next
        End If
    End Sub

    Public Sub PaintWavMap(G As Graphics)
        Dim audioLength As Single = WavAnalyzer.GetAudioLength      '单位为秒
        Dim startPercent As Single = WavControlBar.GetStartPoint
        Dim endPercent As Single = WavControlBar.GetEndPoint

        Dim unitInterval As Single = audioLength * (endPercent - startPercent) / 80000
        Dim startBase As Single = audioLength * startPercent / 100.0F
        Dim second As Single
        Dim result As VectorF4
        For i = 100 To 899
            second = startBase + (i - 100) * unitInterval
            If WavAnalyzer.IsMono Then
                result = WavAnalyzer.GetTip(second, second + unitInterval)
                G.DrawLine(Pens.DarkGray, i, 180 - 100 * result.W, i, 180)
                G.DrawLine(Pens.DarkGray, i, 180 - 100 * result.X, i, 180)
            Else
                result = WavAnalyzer.GetTip(second, second + unitInterval)
                G.DrawLine(Pens.DarkGray, i, 100 - 60 * result.W, i, 100)
                G.DrawLine(Pens.DarkGray, i, 100 - 60 * result.X, i, 100)
                G.DrawLine(Pens.DarkGray, i, 230 - 60 * result.Y, i, 230)
                G.DrawLine(Pens.DarkGray, i, 230 - 60 * result.Z, i, 230)
            End If

        Next

    End Sub

    'Public Sub PaintPYBlocks(G As Graphics)
    '    Dim tbytes As List(Of Byte) = waveFile.GetSampleImage(wavPaintStart, wavPaintLength, 800)
    '    For i = 100 To 899
    '        Dim tvalue As Byte = tbytes(i - 100)
    '        G.DrawLine(Pens.Gray, 100 + i, 328 - tvalue, 100 + i, 200)
    '    Next
    '    G.DrawLine(Pens.Gray, 100, 350, 900, 350)
    '    G.DrawLine(Pens.Black, 100, 328, 900, 328)
    '    G.DrawLine(Pens.Black, 100, 72, 900, 72)
    '    For i = wavPaintStart To wavPaintStart + wavPaintLength
    '        If i Mod 5 = 0 Then
    '            G.DrawString(i.ToString, DefaultFont, Brushes.DarkBlue, 5, 55 + i * 50)
    '        End If
    '        Dim tlinex As Short = 100 + (i - wavPaintStart) * 800 / wavPaintLength
    '        G.DrawLine(Pens.Gray, tlinex, 450, tlinex, 700)
    '    Next
    '    If PYBlockList.Count Then
    '        For Each tpyb As CPYBlock In PYBlockList
    '            Dim shouldShow As Boolean = tpyb.GetStart + tpyb.GetLength > WavPaintStart AndAlso tpyb.GetStart < WavPaintStart + WavPaintLength
    '            If shouldShow Then
    '                G.FillRectangle(Brushes.CornflowerBlue, tpyb.GetStart - WavPaintStart, 500, tpyb.GetStart + tpyb.GetLength - WavPaintStart, 600)
    '            End If
    '        Next
    '    End If
    'End Sub

    Public Function SelectPYBlock(X As Long) As CPYBlock
        If Not PYBlockList.Count Then Return Nothing
        Dim ub As Integer = PYBlockList.Count - 1
        Dim lb As Integer = 0
        Dim middle As Integer
        Do
            middle = (ub - lb) / 2
            If X < PYBlockList(middle).GetStart Then
                ub = middle
            ElseIf X > PYBlockList(middle).GetStart + PYBlockList(middle).GetLength Then
                lb = middle
            Else
                Return PYBlockList(middle)
            End If
        Loop

    End Function

End Module
