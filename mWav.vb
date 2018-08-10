Imports System.IO

Module mWav

    Public WavAnalyzer As New WavFileAnalyzer
    Public WavControlBar As New WavScreenController
    Public PYBlockList As New List(Of CPYBlock)
    Public BlockAssembler As New PYBlockAssembler

    Public Btn1 As New MoveStartButton(100, 675, 300, 750, "移 动")
    Public Btn2 As New DeltaLengthButton(300, 675, 500, 750, "时 长")
    Public Btn3 As New ChangeLinkButton(500, 675, 700, 750, "连 接")
    Public SelectedButton As Integer = -1

    Public SelectedBlock As CPYBlock = Nothing
    'Private WavPaintLength As Integer = 90   '波形声音显示长度，默认为3秒即90帧
    'Private WavPaintStart As Integer = 0     '默认0，单位为帧.0

    Public Sub LoadWavFile()
        Dim openFile As New OpenFileDialog
        openFile.Filter = "波形声音|*.wav"
        openFile.Title = "打开"
        openFile.AddExtension = True
        openFile.AutoUpgradeEnabled = True
        If openFile.ShowDialog() = DialogResult.OK Then
            Form1.PostMsg("正在解析wav音频...")
            Application.DoEvents()
            Dim fileStream As FileStream = CType(openFile.OpenFile, FileStream)
            WavAnalyzer.LoadFile(fileStream)
            fileStream.Dispose()
            Form1.PostMsg("解析完成")
        Else
            Exit Sub
        End If

    End Sub

    Public Sub AddPYBlock(tBlock As CPYBlock)
        PYBlockList.Add(tBlock)
        'Call SortPYBlock()
    End Sub

    ''' <summary>
    ''' 将像素转换为当前缩放状态下的秒
    ''' </summary>
    Public Function PxToSecond(Source As Single) As Single
        Dim startSecond As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength / 100
        Dim endSecond As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength / 100
        Dim px_per_second As Single = 800 / (endSecond - startSecond)

        Return (Source / px_per_second)
    End Function

    ''' <summary>
    ''' 将像素转换为当前缩放状态下的帧
    ''' </summary>
    Public Function PxToFrame(Source As Single) As Single
        Return (PxToSecond(Source) * FRAME_PER_SECOND)
    End Function

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
                        .SetErrorCode(CPYBlock.ErrorTagCode.Collided)
                    End If
                End With
            Next
        End If
    End Sub

    Public Sub PaintWavMap(G As Graphics)
        With G
            Dim audioLength As Single = WavAnalyzer.GetAudioLength      '单位为秒
            Dim startPercent As Single = WavControlBar.GetStartPoint
            Dim endPercent As Single = WavControlBar.GetEndPoint

            Dim duration As Single = audioLength * (endPercent - startPercent) / 100.0F
            Dim unitInterval As Single = duration / 800.0F
            Dim startBase As Single = audioLength * startPercent / 100.0F
            Dim second As Single
            Dim result As VectorF4
            If WavAnalyzer.IsMono Then
                If duration < 30 Then
                    For i = 100 To 899
                        second = startBase + (i - 100) * unitInterval
                        result = WavAnalyzer.GetTip(second, second + unitInterval)
                        .DrawLine(Pens.DarkGray, i, 180 - 100 * result.W, i, 180 - 100 * result.X)
                    Next
                Else
                    For i = 100 To 899
                        second = startBase + (i - 100) * unitInterval
                        result = WavAnalyzer.GetCompressedTip(second, second + unitInterval, 1024)
                        .DrawLine(Pens.DarkGray, i, 180 - 100 * result.W, i, 180 - 100 * result.X)
                    Next
                End If
            Else
                If duration < 30 Then
                    For i = 100 To 899
                        second = startBase + (i - 100) * unitInterval
                        result = WavAnalyzer.GetCompressedTip(second, second + unitInterval, 64)
                        .DrawLine(Pens.DarkGray, i, 100 - 60 * result.W, i, 100 - 60 * result.X)
                        .DrawLine(Pens.DarkGray, i, 230 - 60 * result.Y, i, 230 - 60 * result.Z)
                    Next
                Else
                    For i = 100 To 899
                        second = startBase + (i - 100) * unitInterval
                        result = WavAnalyzer.GetCompressedTip(second, second + unitInterval, 1024)
                        .DrawLine(Pens.DarkGray, i, 100 - 60 * result.W, i, 100 - 60 * result.X)
                        .DrawLine(Pens.DarkGray, i, 230 - 60 * result.Y, i, 230 - 60 * result.Z)
                    Next
                End If
            End If

        End With

    End Sub

    Public Sub DrawPYBlocklist(G As Graphics)
        If PYBlockList.Count Then
            Dim startSecond As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength / 100
            Dim endSecond As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength / 100
            Dim px_per_second As Single = 800 / (endSecond - startSecond)
            For i = 0 To PYBlockList.Count - 1
                Dim headBlock As CPYBlock = PYBlockList(i)
                Dim blockArray As New List(Of CPYBlock)
                headBlock.Expand(blockArray)    'ByRef方法，将列表展开
                For j = blockArray.Count - 1 To 0 Step -1
                    blockArray(j).DrawBlock(G, startSecond, px_per_second)
                Next
            Next

            If SelectedBlock IsNot Nothing Then
                Dim block As CPYBlock = SelectedBlock
                Dim lblPinyin As String = block.GetPinyin
                Dim lblStartFrame As Single = block.GetStart
                Dim lblStartSecond As Single = block.GetStart / FRAME_PER_SECOND
                Dim lblEndFrame As Single = block.GetEnd
                Dim lblEndSecond As Single = block.GetEnd / FRAME_PER_SECOND
				Dim lblLastFrame As Single = lblEndFrame - lblStartFrame
                Dim lblLastSecond As Single = lblEndSecond - lblStartSecond
                Dim lblLinkNext As Boolean = block.IfLinkNext

                With G
                    .DrawString("拼音：" + lblPinyin, DEFAULT_FONT, Brushes.Black, New PointF(110, 470))
                    .DrawString("起始时间：" + lblStartSecond.ToString("F2") + "秒[" + lblStartFrame.ToString("F2") + "帧]", DEFAULT_FONT, Brushes.Black, New PointF(110, 500))
                    .DrawString("结束时间：" + lblEndSecond.ToString("F2") + "秒[" + lblEndFrame.ToString("F2") + "帧]", DEFAULT_FONT, Brushes.Black, New PointF(110, 530))
					.DrawString("持续时长：" + lblLastSecond.ToString("F2") + "秒[" + lblLastFrame.ToString("F2") + "帧]", DEFAULT_FONT, Brushes.Black, New PointF(110, 560))
                    .DrawString("自动连接下一个拼音：", DEFAULT_FONT, Brushes.Black, New PointF(110, 590))
                    If lblLinkNext Then
                        .DrawString("是", DEFAULT_FONT, Brushes.Black, New PointF(400, 590))
                    Else
                        .DrawString("否", DEFAULT_FONT, Brushes.Black, New PointF(400, 590))
                    End If
                End With

                Btn1.DrawButton(G)
                Btn2.DrawButton(G)
                Btn3.DrawButton(G)
            End If
        End If
    End Sub

    Public Sub RefreshAllBlocks()
        If PYBlockList.Count Then
            For i = 0 To PYBlockList.Count - 1
                PYBlockList(i).RefreshArray()
            Next
        End If
    End Sub

    '''<summary>
    '''检查所有PYB是否有重叠，如果全部正确则返回True
    '''</summary>
    Public Function CheckPYBListError(target As List(Of CPYBlock)) As Boolean
        Dim allPass As Boolean = True
        If target.Count Then
            For i = 0 To target.Count - 1
                Dim headBlock As CPYBlock = target(i)
                headBlock.SetAllArrayCorrect()
            Next
            For i = 0 To target.Count - 1
                Dim headBlock As CPYBlock = target(i)
                Dim headHead As Single = headBlock.GetHeadSecond()
                If headHead < 0 Then
                    allPass = False
                    headBlock.SetAllArrayError()
                End If
                If i > 0 Then
                    For j = 0 To i - 1
                        Dim compareBlock As CPYBlock = target(j)
                        Dim compareHead As Single = compareBlock.GetHeadSecond()
                        Dim headTail As Single = headBlock.GetTailSecond()
                        Dim compareTail As Single = compareBlock.GetTailSecond()
                        Dim actualLength As Single = 0
                        If headHead > compareHead Then
                            actualLength = headTail - compareHead
                        Else
                            actualLength = compareTail - headHead
                        End If
                        Dim acceptLength As Single = (headTail - headHead) + (compareTail - compareHead)
                        If actualLength < acceptLength Then
                            allPass = False
                            headBlock.SetAllArrayError()
                            compareBlock.SetAllArrayError()
                        End If
                    Next
                End If
            Next
        End If
        Return allPass
    End Function

    Public Function FindBlock(e As MouseEventArgs) As CPYBlock
        Dim trueX As Single = e.X * 2
        If trueX >= 100 AndAlso trueX < 900 Then
            Dim startSecond As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength / 100
            Dim endSecond As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength / 100
            Dim px_per_second As Single = 800 / (endSecond - startSecond)
            Dim FrameX As Single = (startSecond + (trueX - 100) / px_per_second) * FRAME_PER_SECOND

            For i = PYBlockList.Count - 1 To 0 Step -1
                If FrameX >= PYBlockList(i).GetHeadFrame AndAlso FrameX < PYBlockList(i).GetTailFrame Then
                    Return PYBlockList(i).SearchInArray(FrameX)
                End If
            Next
        End If
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
                If SelectedBlock.Equals(PYBlockList(i)) Then
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

            If SelectedBlock Is Nothing Then
                Dim block As CPYBlock = SelectedBlock
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
        With G
            .FillRectangle(Brushes.White, 0, 0, 99, 750)
            .FillRectangle(Brushes.White, 900, 0, 100, 750)

            .DrawLine(Pens.Black, 100.0F, 0.0F, 100.0F, 750.0F)
            .DrawLine(Pens.Black, 900.0F, 0.0F, 900.0F, 750.0F)
            .DrawLine(Pens.Black, 100.0F, 360.0F, 900.0F, 360.0F)

            If WavAnalyzer.IsMono Then
                .DrawString("单声道", DEFAULT_FONT, Brushes.Black, New PointF(10, 160))
                .DrawLine(Pens.DarkGray, 101, 180, 899, 180)
            Else
                .DrawString("左声道", DEFAULT_FONT, Brushes.Black, New PointF(10, 80))
                .DrawString("右声道", DEFAULT_FONT, Brushes.Black, New PointF(10, 210))
                .DrawLine(Pens.DarkGray, 101, 100, 899, 100)
                .DrawLine(Pens.DarkGray, 101, 230, 899, 230)
            End If

            .DrawString("缩放", DEFAULT_FONT, Brushes.Black, New PointF(10, 300))
            .DrawString("拼音", DEFAULT_FONT, Brushes.Black, New PointF(10, 380))

            Dim startSecond As Single = WavControlBar.GetStartPoint * WavAnalyzer.GetAudioLength / 100
            Dim endSecond As Single = WavControlBar.GetEndPoint * WavAnalyzer.GetAudioLength / 100

            .DrawString(startSecond.ToString("F2") + "秒", DEFAULT_FONT, Brushes.Black, New PointF(5, 500))
            .DrawString(endSecond.ToString("F2") + "秒", DEFAULT_FONT, Brushes.Black, New PointF(905, 500))

            .DrawString((startSecond * FRAME_PER_SECOND).ToString("F1") + "帧", DEFAULT_FONT, Brushes.Black, New PointF(5, 550))
            .DrawString((endSecond * FRAME_PER_SECOND).ToString("F1") + "帧", DEFAULT_FONT, Brushes.Black, New PointF(905, 550))
        End With

    End Sub

    ''' <summary>
    ''' 判定选中
    ''' </summary>
    <Obsolete("Use FindBlock()", True)>
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
