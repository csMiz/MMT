Imports System.IO

Module mWav

    Public WavAnalyzer As New WavFileAnalyzer
    Public WavControlBar As New WavScreenController
    Public PYBlockList As New List(Of CPYBlock)

    Public Btn1 As New CDrawingButton(100, 675, 300, 750, "移 动")
    Public Btn2 As New CDrawingButton(300, 675, 500, 750, "时 长")
    Public Btn3 As New CDrawingButton(500, 675, 700, 750, "连 接")
    Public SelectedButton As Integer = -1

    Public SelectedBlock As Integer = -1
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
        'Call SortPYBlock()
    End Sub

    ''' <summary>
    ''' 时间轴从小到大排列拼音块，并分析重叠错误
    ''' </summary>
    <Obsolete("改用sorted list以后不再需要此方法", True)>
    Public Sub SortPYBlock()
        Dim comparison As New Comparison(Of CPYBlock)(Function(a As CPYBlock, b As CPYBlock) a.GetStart - b.GetStart)
        'PYBlockList.Sort(comparison)

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

    Public Sub DrawPYBlocklist(G As Graphics)
        If PYBlockList.Count Then
            For i = 0 To PYBlockList.Count - 1
                Dim headBlock As CPYBlock = PYBlockList(i)
                Dim blockArray As New List(Of CPYBlock)
                headBlock.Expand(blockArray)    'ByRef方法，将列表展开
                For j = blockArray.Count - 1 To 0 Step -1
                    blockArray(j).DrawBlock(G)
                Next
            Next
        End If
    End Sub

    Public Sub RefreshAllBlocks()
        If PYBlockList.Count Then
            For i = 0 To PYBlockList.Count - 1
                PYBlockList(i).RefreshArray()
            Next
        End If
    End Sub

    Public Function FindBlock(e As MouseEventArgs) As CPYBlock
        Dim trueX As Single = e.X * 2
        Dim FrameX As Single 'trueX.. 进行转换

        Throw New NotImplementedException()

        For i = PYBlockList.Count - 1 To 0 Step -1
            If trueX >= PYBlockList(i).GetStart Then
                Return PYBlockList(i).SearchInArray(FrameX)
            End If
        Next
        Return Nothing
    End Function

    <Obsolete("Use DrawPYBlockList()", True)>
    Public Sub PaintPYBlocks(G As Graphics)
        If PYBlockList.Count Then
            '区间单位为帧
            Dim showingStart As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength * FRAME_PER_SECOND / 100
            Dim showingEnd As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength * FRAME_PER_SECOND / 100

            Dim displayIndexStart As Integer = 0
            Dim displayIndexEnd As Integer = PYBlockList.Count - 1
            '计算绘制区间
            For i = 0 To PYBlockList.Count - 1
                Dim block As CPYBlock = PYBlockList(i)
                If block.GetStart + block.GetLength > showingStart Then
                    displayIndexStart = i
                    Exit For
                End If
            Next
            For i = displayIndexStart To PYBlockList.Count - 1
                Dim block As CPYBlock = PYBlockList(i)
                If block.GetStart > showingEnd Then
                    displayIndexEnd = i - 1
                    Exit For
                End If
            Next
            If displayIndexEnd < displayIndexStart Then displayIndexEnd = displayIndexStart
            '绘制
            For i = displayIndexEnd To displayIndexStart Step -1
                Dim block As CPYBlock = PYBlockList(i)
                Dim startSecond As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength / 100
                Dim endSecond As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength / 100
                Dim px_per_second As Single = 800 / (endSecond - startSecond)
                Dim drawLeft As Single = 100 + (block.GetStart / FRAME_PER_SECOND - startSecond) * px_per_second
                Dim drawWidth As Single = (block.GetLength / FRAME_PER_SECOND) * px_per_second
                If drawWidth < 0.0F Then drawWidth = 0.0F
                If SelectedBlock = i Then
                    G.FillRectangle(Brushes.AntiqueWhite, drawLeft, 400, drawWidth, 60)
                Else
                    G.FillRectangle(Brushes.LightGray, drawLeft, 400, drawWidth, 60)
                End If
                G.DrawRectangle(Pens.Black, drawLeft, 400, drawWidth, 60)
                If block.IfLinkNext Then
                    G.DrawLine(GREEN_PEN_2PX, drawLeft + drawWidth, 400, drawLeft + drawWidth, 460)
                End If
                If drawWidth >= 40 Then
                    G.DrawString(block.GetLabel, DEFAULT_FONT, Brushes.Black, New PointF(drawLeft + 5, 405))
                End If

            Next

            If SelectedBlock <> -1 Then
                Dim block As CPYBlock = PYBlockList(SelectedBlock)
                Dim lblPinyin As String = block.GetPinyin
                Dim lblStartFrame As Single = block.GetStart
                Dim lblStartSecond As Single = block.GetStart / FRAME_PER_SECOND
                Dim lblEndFrame As Single = block.GetEnd
                Dim lblEndSecond As Single = block.GetEnd / FRAME_PER_SECOND
                Dim lblLinkNext As Boolean = block.IfLinkNext

                G.DrawString("拼音：" + lblPinyin, DEFAULT_FONT, Brushes.Black, New PointF(110, 470))
                G.DrawString("起始时间：" + lblStartSecond.ToString("F2") + "秒[" + lblStartFrame.ToString("F2") + "帧]", DEFAULT_FONT, Brushes.Black, New PointF(110, 500))
                G.DrawString("结束时间：" + lblEndSecond.ToString("F2") + "秒[" + lblEndFrame.ToString("F2") + "帧]", DEFAULT_FONT, Brushes.Black, New PointF(110, 530))
                G.DrawString("自动连接下一个拼音：", DEFAULT_FONT, Brushes.Black, New PointF(110, 560))
                If lblLinkNext Then
                    G.DrawString("是", DEFAULT_FONT, Brushes.Black, New PointF(400, 560))
                Else
                    G.DrawString("否", DEFAULT_FONT, Brushes.Black, New PointF(400, 560))
                End If

                Btn1.DrawButton(G)
                Btn2.DrawButton(G)
                Btn3.DrawButton(G)
            End If

        End If
    End Sub

    Public Sub PaintWavUIGrid(G As Graphics)
        G.FillRectangle(Brushes.White, 0, 0, 99, 750)
        G.FillRectangle(Brushes.White, 900, 0, 100, 750)

        G.DrawLine(Pens.Black, 100.0F, 0.0F, 100.0F, 750.0F)
        G.DrawLine(Pens.Black, 900.0F, 0.0F, 900.0F, 750.0F)
        G.DrawLine(Pens.Black, 100.0F, 360.0F, 900.0F, 360.0F)

        If WavAnalyzer.IsMono Then
            G.DrawString("单声道", DEFAULT_FONT, Brushes.Black, New PointF(10, 160))
        Else
            G.DrawString("左声道", DEFAULT_FONT, Brushes.Black, New PointF(10, 80))
            G.DrawString("右声道", DEFAULT_FONT, Brushes.Black, New PointF(10, 210))
        End If

        G.DrawString("缩放", DEFAULT_FONT, Brushes.Black, New PointF(10, 300))
        G.DrawString("拼音", DEFAULT_FONT, Brushes.Black, New PointF(10, 380))

        Dim startSecond As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength / 100
        Dim endSecond As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength / 100

        G.DrawString(startSecond.ToString("F2") + "秒", DEFAULT_FONT, Brushes.Black, New PointF(5, 500))
        G.DrawString(endSecond.ToString("F2") + "秒", DEFAULT_FONT, Brushes.Black, New PointF(905, 500))

        G.DrawString((startSecond * FRAME_PER_SECOND).ToString("F1") + "帧", DEFAULT_FONT, Brushes.Black, New PointF(5, 550))
        G.DrawString((endSecond * FRAME_PER_SECOND).ToString("F1") + "帧", DEFAULT_FONT, Brushes.Black, New PointF(905, 550))

    End Sub

    ''' <summary>
    ''' 判定选中
    ''' </summary>
    <Obsolete("", True)>
    Public Function SelectPYBlock(e As MouseEventArgs) As Integer
        Dim trueX As Single = e.X * 2
        Dim trueY As Single = e.Y * 2
        Dim startSecond As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength / 100
        Dim endSecond As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength / 100
        Dim px_per_second As Single = 800 / (endSecond - startSecond)

        If PYBlockList.Count = 0 Then Return -1
        Dim ub As Integer = PYBlockList.Count - 1
        Dim lb As Integer = 0
        Dim middle As Integer
        If trueY >= 400 AndAlso trueY <= 460 AndAlso trueX > 100 AndAlso trueX < 900 Then
            Dim recCount As Integer = 0
            Dim stableResult As Boolean = False
            Do
                middle = (ub + lb) / 2
                Dim obj As CPYBlock = PYBlockList(middle)
                If trueX < (obj.GetStart / FRAME_PER_SECOND - startSecond) * px_per_second + 100 Then
                    ub = middle
                ElseIf trueX > ((obj.GetStart + obj.GetLength) / FRAME_PER_SECOND - startSecond) * px_per_second + 100 Then
                    lb = middle
                Else
                    Return middle
                End If
                If (lb = ub - 1) Then
                    If stableResult Then
                        Return ub
                    End If
                    stableResult = True
                End If
                recCount += 1
                If recCount > 10000 Then
                    Return -1
                End If
            Loop
        Else
            Return -1
        End If

    End Function

End Module
