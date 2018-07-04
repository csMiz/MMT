Imports System.IO

Module mWav

    Private WaveFile As New CWaveFile

    Private PYBlockList As New List(Of CPYBlock)
    Private WavPaintLength As Integer = 90   '波形声音显示长度，默认为3秒即90帧
    Private WavPaintStart As Integer = 0     '默认0，单位为帧.0

    Private Class CWaveFile
        Private Content As New List(Of String)
        Private Length As Long = 0
        Private Pointer As Long = 44

        Public Sub LoadWavFile()

            Dim openFile As New OpenFileDialog
            openFile.Filter = "波形声音|*.wav"
            openFile.Title = "打开"
            openFile.AddExtension = True
            openFile.AutoUpgradeEnabled = True
            If openFile.ShowDialog() = DialogResult.OK Then
                Dim tstr As FileStream = CType(openFile.OpenFile, FileStream)
                Dim r As BinaryReader = New BinaryReader(tstr)

                Dim tempcontent As Byte = 0
                Dim jmax As Integer = r.BaseStream.Length \ 10240
                length = r.BaseStream.Length - 44

                For j = 0 To jmax
                    Dim tcline As String = ""
                    For i = 0 To 10239
                        If j = jmax Then
                            If jmax * 10240 + i >= r.BaseStream.Length Then Exit For
                        End If
                        tempcontent = r.ReadByte()
                        tcline = tcline & ChrW(tempcontent)
                    Next
                    content.Add(tcline)
                Next

                r.Close()
                tstr.Dispose()
            Else
                Exit Sub
            End If

        End Sub

        Public Function GetValue(tx As Long) As Byte

            Dim tv1 As Integer = tx \ 10240
            Dim tv2 As Integer = tx Mod 10240
            Return AscW(content(tv1).Substring(tv2, 1))

        End Function

        ''' <summary>
        ''' 返回整个wav的样本
        ''' </summary>
        ''' <param name="x">样本数</param>
        ''' <returns></returns>
        Public Function GetSampleImage(x As Integer) As List(Of Byte)
            Dim r As New List(Of Byte)

            For i = 0 To x - 1
                Dim tx As Long = CLng(44 + i * length / x)
                Dim ty As Byte = GetValue(tx)
                r.Add(ty)
            Next

            Return r

        End Function

        ''' <summary>
        ''' 返回wav特定段的样本
        ''' </summary>
        ''' <param name="start">起始帧</param>
        ''' <param name="length">抽样长度</param>
        ''' <param name="x">样本数</param>
        ''' <returns></returns>
        Public Function GetSampleImage(start As Long, length As Long, x As Integer)
            Throw New Exception("implement me")
        End Function


    End Class

    Public Class CPYBlock
        Private Label As String = vbNullString
        Private Interval As Integer = 0
        Private Startat As Integer = 0
        Private ErrTag As ErrorTagCode = ErrorTagCode.NoError

        Public Enum ErrorTagCode As Byte
            NoError = 0
            LengthExceed = 1
        End Enum

        ''' <summary>
        ''' 创建一个拼音块
        ''' </summary>
        ''' <param name="tl">汉语拼音</param>
        ''' <param name="tint">拼音时长帧数</param>
        ''' <param name="tst">拼音起始帧数</param>
        Public Sub New(Optional tl As String = vbNullString, Optional tint As Integer = 0, Optional tst As Integer = 0)
            Label = tl
            Interval = tint
            Startat = tst
        End Sub

        Public Function Copy() As CPYBlock
            Dim r As New CPYBlock(Label, Interval, Startat)
            Return r
        End Function

        Public Function GetStart() As Integer
            Return Startat
        End Function

        Public Function GetLength() As Integer
            Return Interval
        End Function

        Public Sub SetLength(value As Integer)
            Interval = value
        End Sub

        Public Shared Sub AdjustLength(a As CPYBlock, b As CPYBlock, delta As Long)
            a.SetLength(a.Interval + delta)
            b.MoveBlock(delta)
            b.SetLength(b.Interval - delta)
        End Sub

        Public Sub MoveBlock(value As Long)
            Startat += value
        End Sub

        Public Sub SetErrorCode(err As ErrorTagCode)
            ErrTag = err
        End Sub

    End Class

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

    Public Sub PaintPYBlocks(G As Graphics)
        Dim tbytes As List(Of Byte) = waveFile.GetSampleImage(wavPaintStart, wavPaintLength, 800)
        For i = 100 To 899
            Dim tvalue As Byte = tbytes(i - 100)
            G.DrawLine(Pens.Gray, 100 + i, 328 - tvalue, 100 + i, 200)
        Next
        G.DrawLine(Pens.Gray, 100, 350, 900, 350)
        G.DrawLine(Pens.Black, 100, 328, 900, 328)
        G.DrawLine(Pens.Black, 100, 72, 900, 72)
        For i = wavPaintStart To wavPaintStart + wavPaintLength
            If i Mod 5 = 0 Then
                G.DrawString(i.ToString, DefaultFont, Brushes.DarkBlue, 5, 55 + i * 50)
            End If
            Dim tlinex As Short = 100 + (i - wavPaintStart) * 800 / wavPaintLength
            G.DrawLine(Pens.Gray, tlinex, 450, tlinex, 700)
        Next
        If pyBlockList.Count Then
            For Each tpyb As CPYBlock In PYBlockList
                Dim shouldShow As Boolean = tpyb.GetStart + tpyb.GetLength > WavPaintStart AndAlso tpyb.GetStart < WavPaintStart + WavPaintLength
                If shouldShow Then
                    G.FillRectangle(Brushes.CornflowerBlue, tpyb.GetStart - WavPaintStart, 500, tpyb.GetStart + tpyb.GetLength - WavPaintStart, 600)
                End If
            Next
        End If
    End Sub

    Public Function SelectPYBlock(X As Long) As CPYBlock
        If Not pyBlockList.Count Then Return Nothing
        Dim ub As Integer = pyBlockList.Count - 1
        Dim lb As Integer = 0
        Dim middle As Integer
        Do
            middle = (ub - lb) / 2
            If X < PYBlockList(middle).GetStart Then
                ub = middle
            ElseIf X > PYBlockList(middle).GetStart + PYBlockList(middle).GetLength Then
                lb = middle
            Else
                Return pyBlockList(middle)
            End If
        Loop

    End Function

End Module
