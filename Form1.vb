﻿Imports System.IO
Imports System.Text.RegularExpressions
Imports Miz_MMD_Tool
Imports System.Math
Imports JSONEntityParser

Public Class Form1

    Public MouseFlag As Boolean = False
    Public StartX As Integer = 0
    Public LastDeltaFrame As Short = 0
    Public UsingFileName As String = ""
    Public UsingFileURI As String = ""

    Private SelectedPoint As MMDPoint = Nothing
    Public SelectedPointBone As Integer = -1
    Private ClipBoard As MMDPoint

    Public ShowingFrame As Integer = 0
    Public ShowingBone As Integer = 0

    Public AllowUpdate As Boolean = True

    Private ExeMode As eExeMode = eExeMode.NONE

    Private HoldMsgList As New List(Of String)
    Private JSONParser As New CommonEntityParser
    Private MathEx As MathHelper = MathHelper.Instance  '单例
    Public WavPinList As New List(Of Single)

    Public PenLine As New BezierPenLine
    Public CanvasPenTool As New PenTool

    Public ME_VERSION As String = VersionControl.VersionControl.GetVersion
    Public ME_RELEASE_TIME As String = VersionControl.VersionControl.GetReleaseTime

    Private Function GetBoneFace(Index As Short) As MMDBoneFace

        If Index < ListBone.Count Then
            Return ListBone(Index)
        ElseIf Index < ListBone.Count + ListFace.Count Then
            Return ListFace(Index - ListBone.Count)
        Else
            Return Nothing
        End If

    End Function

    Public Shadows Sub Paint()
        '1000*750
        P.Image = Nothing
        Dim bm As New Bitmap(1000, 750)
        Dim G As Graphics = Graphics.FromImage(bm)
        G.Clear(Color.White)

        If ExeMode = eExeMode.WAV Then

            WavControlBar.DrawBar(G)
            PaintWavMap(G)
            DrawPYBlocklist(G)
            PaintWavUIGrid(G)

        ElseIf ExeMode = eExeMode.BEZIER Then

            CanvasPenTool.DrawBezierLine(G)

        ElseIf ExeMode = eExeMode.VMD OrElse ExeMode = eExeMode.NONE Then

            '左边200显示名称，上面50显示帧
            For i = 0 To 39
                G.DrawLine(Pens.Gray, 200 + 20 * i, 50, 200 + 20 * i, 750)
                G.DrawLine(Pens.Gray, 0, 50, 1000, 50)
            Next

            For i = ShowingFrame - 19 To ShowingFrame + 20
                If i >= 0 AndAlso i Mod 5 = 0 Then
                    Dim tp As New Pen(Color.Gray)
                    tp.Width = 3

                    G.DrawLine(tp, 200 + 20 * (i - ShowingFrame + 19), 50, 200 + 20 * (i - ShowingFrame + 19), 750)
                    G.DrawString(i.ToString, DEFAULT_FONT, Brushes.Black, 200 + 20 * (i - ShowingFrame + 19) - 10, 15)
                End If
            Next

            For i = 0 To 9
                Dim tbindex As Short = ShowingBone + i
                Dim tboneface As MMDBoneFace = GetBoneFace(tbindex)
                If tboneface IsNot Nothing Then
                    If TypeOf tboneface Is Bone Then
                        Dim tbone As Bone = CType(tboneface, Bone)
                        G.DrawString(tbone.Name, DEFAULT_FONT, Brushes.DarkBlue, 5, 55 + i * 50)
                        For j = 0 To tbone.GetPointCount - 1
                            Dim tf As Integer = tbone.PointList(j).Frame - ShowingFrame
                            If tf >= -19 AndAlso tf <= 20 Then
                                G.FillEllipse(Brushes.DarkGray, 190 + 20 * (tf + 19), 65 + i * 50, 20, 20)
                                If (SelectedPoint IsNot Nothing) AndAlso i = SelectedPointBone AndAlso tbone.PointList(j).Equals(SelectedPoint) Then
                                    G.FillEllipse(Brushes.DarkRed, 193 + 20 * (tf + 19), 68 + i * 50, 14, 14)
                                End If
                            End If
                        Next
                    Else
                        Dim tface As Face = CType(tboneface, Face)
                        G.DrawString(tface.Name, DEFAULT_FONT, Brushes.DarkGreen, 5, 55 + i * 50)
                        For j = 0 To tface.GetPointCount - 1
                            Dim tf As Integer = tface.PointList(j).Frame - ShowingFrame
                            If tf >= -19 AndAlso tf <= 20 Then
                                G.FillEllipse(Brushes.DarkGray, 190 + 20 * (tf + 19), 65 + i * 50, 20, 20)
                                If (SelectedPoint IsNot Nothing) AndAlso i = SelectedPointBone AndAlso tface.PointList(j).Equals(SelectedPoint) Then
                                    G.FillEllipse(Brushes.DarkRed, 193 + 20 * (tf + 19), 68 + i * 50, 14, 14)
                                End If
                            End If
                        Next
                    End If
                End If

            Next

            '700以下显示数值
            If SelectedPoint IsNot Nothing Then
                G.FillRectangle(Brushes.LightGray, 0, 700, 1000, 50)
                If TypeOf SelectedPoint Is BonePoint Then
                    G.DrawString("X=" & CType(SelectedPoint, BonePoint).X.ToString, DEFAULT_FONT, Brushes.Black, 5, 715)
                    G.DrawString("Y=" & CType(SelectedPoint, BonePoint).Y.ToString, DEFAULT_FONT, Brushes.Black, 155, 705)
                    G.DrawString("Z=" & CType(SelectedPoint, BonePoint).Z.ToString, DEFAULT_FONT, Brushes.Black, 305, 715)
                    '显示欧拉角
                    G.DrawString("RX=" & CType(SelectedPoint, BonePoint).SX.ToString, DEFAULT_FONT, Brushes.Black, 455, 705)
                    G.DrawString("RY=" & CType(SelectedPoint, BonePoint).SY.ToString, DEFAULT_FONT, Brushes.Black, 605, 715)
                    G.DrawString("RZ=" & CType(SelectedPoint, BonePoint).SZ.ToString, DEFAULT_FONT, Brushes.Black, 755, 705)

                Else
                    G.DrawString("V=" & CType(SelectedPoint, FacePoint).V.ToString, DEFAULT_FONT, Brushes.Black, 5, 715)

                End If
            End If


        End If


        P.Image = bm
        P.Refresh()
        G.Dispose()

    End Sub

    ''' <summary>
    ''' 在控制栏输出一行信息
    ''' </summary>
    Public Sub PostMsg(text As String)
        TB1.Text = TB1.Text + text + vbCrLf
    End Sub

    ''' <summary>
    ''' 在鼠标松开前缓存信息
    ''' </summary>
    Public Sub HoldPostMsg(text As String)
        If HoldMsgList.Count Then
            For Each s As String In HoldMsgList
                If s = text Then
                    Return
                Else
                    HoldMsgList.Add(text)
                End If
            Next
        Else
            HoldMsgList.Add(text)
        End If
    End Sub

    ''' <summary>
    ''' 输出缓存中的信息
    ''' </summary>
    Public Sub ReleaseHoldMsg()
        If HoldMsgList.Count Then
            For Each s As String In HoldMsgList
                PostMsg(s)
            Next

            HoldMsgList.Clear()
        End If
    End Sub

    Public Sub OpenFile()
        Dim openFile As New OpenFileDialog
        openFile.Filter = "vmd动作文件|*.vmd"
        openFile.Title = "打开"
        openFile.AddExtension = True
        openFile.AutoUpgradeEnabled = True
        If openFile.ShowDialog() = DialogResult.OK Then
            UsingFileName = openFile.SafeFileName
            UsingFileName = UsingFileName.Remove(UsingFileName.Length - 4)
            UsingFileURI = openFile.FileName
            Dim tstr As FileStream = CType(openFile.OpenFile, FileStream)
            Dim r As BinaryReader = New BinaryReader(tstr)
            PostMsg("正在读取" & UsingFileName)
            '正在读取

            Dim tempcontent As Byte = 0
            Dim jmax As Integer = CInt(r.BaseStream.Length) \ 2000

            For j = 0 To jmax
                Dim tcline As String = ""
                For i = 0 To 1999
                    If j = jmax Then
                        If jmax * 2000 + i >= r.BaseStream.Length Then Exit For
                    End If
                    tempcontent = r.ReadByte()
                    tcline = tcline & ChrW(tempcontent)
                Next
                VMDStorage.Add(tcline)
            Next
            r.Close()
            tstr.Dispose()

            PostMsg("正在分析")

            ListBone.Clear()
            ListFace.Clear()

            ExeMode = eExeMode.VMD
            Call ReadVMD()

            Call Paint()

            PostMsg("分析完成")

        End If

    End Sub

    Private Sub SaveNewFile(Optional param As SaveFileParam = SaveFileParam.SAVENEW)
        If ExeMode = eExeMode.WAV Then
            Call cPinyinConfig.ApplySinglePinyinOut(PYBlockList)
        End If
        Dim tstream As FileStream = Nothing
        If param = SaveFileParam.SAVENEW Then
            Dim saveFile As New SaveFileDialog
            saveFile.Filter = "vmd动作文件|*.vmd"
            saveFile.Title = "另存为"
            saveFile.AddExtension = True
            saveFile.AutoUpgradeEnabled = True
            saveFile.FileName = UsingFileName
            If saveFile.ShowDialog() <> DialogResult.OK Then
                GoTo lblFail
            End If
            tstream = CType(saveFile.OpenFile, FileStream)
            UsingFileName = saveFile.FileName
            UsingFileName = UsingFileName.Remove(UsingFileName.Length - 4)
        ElseIf param = SaveFileParam.VMDTEST Then
            Dim savefile As New FileStream("C:\Users\sscs\Desktop\VMDT.vmd", FileMode.OpenOrCreate)
            tstream = savefile
        ElseIf param = SaveFileParam.SAVE Then
            Dim savefile As New FileStream(UsingFileURI, FileMode.OpenOrCreate)
            tstream = savefile
        End If

        If tstream IsNot Nothing Then
            Dim r As BinaryWriter = New BinaryWriter(tstream)
            PostMsg("开始写入" & UsingFileName)

            Call WriteVMD(r)

            r.Close()
            tstream.Close()
            PostMsg("写入完成")

        End If

lblFail:


    End Sub

    Public Sub PointTest()
        PostMsg("自动测试...")

        ModelName = "testmodel"

        ListBone.Clear()
        ListFace.Clear()

        Dim tb As New Bone
        tb.Name = "testbone1"
        Dim tbp As New BonePoint
        'tbp.Init(1, 2, 3, 4, 5, 6, 0)
        tbp.Init(0, 0, 0, 0, 0, 0, 0)
        tbp.Frame = 0
        tb.AddPoint(tbp)
        Dim tbp2 As New BonePoint
        tbp2.Init(0, 0, 20, 0, 0, 0, 0)
        tbp2.Frame = 1
        tb.AddPoint(tbp2)
        Dim tbp3 As New BonePoint
        tbp3.Init(50, 0, 0, 0, 0, 0, 0)
        tbp3.Frame = 2
        tb.AddPoint(tbp3)
        Dim tbp4 As New BonePoint
        tbp4.Init(60, 0, -20, 0, 0, 0, 0)
        tbp4.Frame = 3
        tb.AddPoint(tbp4)
        Dim tbp5 As New BonePoint
        tbp5.Init(100, 0, 0, 0, 0, 0, 0)
        tbp5.Frame = 4
        tb.AddPoint(tbp5)
        Dim tbp6 As New BonePoint
        tbp6.Init(120, 0, 30, 0, 0, 0, 0)
        tbp6.Frame = 5
        tb.AddPoint(tbp6)

        ListBone.Add(tb)


        'Dim tb2 As New Bone
        'tb2.Name = "testbone2"
        'Dim tbp3 As New BonePoint
        'tbp3.Init(1, 2, 3, 4, 5, 6, 0)
        'tbp3.Frame = 0
        'tb2.AddPoint(tbp3)
        'ListBone.Add(tb2)

        Call Paint()
        'Call SaveNewFile(SaveFileParam.VMDTest)


    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Width = 880
        VMDStorage = New List(Of String)
        ListBone = New List(Of Bone)
        ListFace = New List(Of Face)


    End Sub

    ''' <summary>
    ''' 所有功能
    ''' </summary>
    Private Sub TB2_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TB2.KeyPress

        If e.KeyChar = ChrW(13) Then
            Dim tin As String = TB2.Text.Trim
            PostMsg(tin)
            TB2.Text = ""
            Dim tst() As String = Regex.Split(tin, " ")
            If tst.Length = 1 Then
                Dim tcmd As String = tst(0).ToLower
                If tcmd = "open" Then
                    Call OpenFile()
                ElseIf tcmd = "test" Then
                    Call PointTest()
                ElseIf tcmd = "save" Then
                    If UsingFileURI = "" Then
                        Call SaveNewFile()
                    Else
                        Call SaveNewFile(SaveFileParam.SAVE)
                    End If
                ElseIf tcmd = "saveas" Then
                    Call SaveNewFile()
                ElseIf tcmd = "savetest" Then
                    Call SaveNewFile(SaveFileParam.VMDTEST)
                ElseIf tcmd = "paint" Then
                    Call Paint()
                ElseIf tcmd = "copy" Then
                    ClipBoard = SelectedPoint.Copy
                ElseIf tcmd = "paste" Then
                    If SelectedPoint IsNot Nothing Then
                        SelectedPoint.Apply(ClipBoard)
                        Call Paint()
                    Else
                        PostMsg("未选中任何点")
                    End If
                ElseIf tcmd = "cut" Then
                    ClipBoard = SelectedPoint.Copy
                    ListBone(SelectedPointBone).PointList.Remove(CType(SelectedPoint, BonePoint))
                    SelectedPoint = Nothing
                ElseIf tcmd = "cls" Then
                    TB1.Text = ""
                ElseIf tcmd = "py" Then
                    PostMsg("指令已废弃，请使用'pyb'")
                    'Call LoadPinyinConfig()
                    'Call ApplyPinyin(7.5)
                    'Call Paint()
                ElseIf tcmd = "pysingle" Then
                    PostMsg("指令已废弃，请使用'pyb'")
                    'ExeMode = eExeMode.PINYIN_MANUAL
                    'Call LoadPinyinConfig()
                    'Call ApplyPinyin()
                    'PostMsg("下一个：" & ListPY(0).Pinyin)
                ElseIf tcmd = "pyb" Then
                    If ExeMode <> eExeMode.WAV Then
                        ExeMode = eExeMode.WAV
                        WavAnalyzer.AddedBlankSecond = 10
                        PostMsg("没有载入wav声音参考，自动创建10秒长度")
                    End If
                    Call LoadPinyinConfig()
                    Call ApplyPinyinWithBlocks()
                    Call Paint()
                ElseIf tcmd = "mouth" Then
                    PostMsg("指令已废弃，请使用'pyb'")
                    'Call OpenReadingText()
                    'Call Paint()
                ElseIf tcmd = "cards" Then
                    Call CardsTogether()
                    Call Paint()
                ElseIf tcmd = "cardse" Then
                    Call CardsExplode()
                    Call Paint()
                ElseIf tcmd = "getqua" Then
                    PostMsg("rx=" & CType(SelectedPoint, BonePoint).GetQuaternion(BonePoint.QuaParam.QX))
                    PostMsg("ry=" & CType(SelectedPoint, BonePoint).GetQuaternion(BonePoint.QuaParam.QY))
                    PostMsg("rz=" & CType(SelectedPoint, BonePoint).GetQuaternion(BonePoint.QuaParam.QZ))
                    PostMsg("rw=" & CType(SelectedPoint, BonePoint).GetQuaternion(BonePoint.QuaParam.QW))
                ElseIf tcmd = "bezier" Then
                    ExeMode = eExeMode.BEZIER
                    SortPoint()
                    Dim points As New List(Of PointF3)
                    For i = 0 To ListBone(0).GetPointCount - 1 Step 2
                        points.Add(ListBone(0).PointList(i).PosToP3)
                        points.Add(ListBone(0).PointList(i + 1).PosToP3)
                        points.Add(New PointF3)
                    Next
                    PenLine.LoadPoints(points)
                    PenLine.GenerateBezier()
                    Dim tempzoom As Single = 1.0F
                    CanvasPenTool.LoadData(PenLine, tempzoom, New PointF3)
                    'Call GenerateBez()
                    Call Paint()
                ElseIf tcmd = "cardsd" Then
                    Call CardsDisperse(GetCardsPos)
                    Call Paint()
                ElseIf tcmd = "sinewave" Then
                    Call SinWaveMove()
                    Call Paint()
                ElseIf tcmd = "wav" Then
                    Call LoadWavFile()
                    ExeMode = eExeMode.WAV
                    Call Paint()
                ElseIf tcmd = "checkupdate" Then
                    Call CheckUpdate()
                    'ElseIf tcmd = "shutupdate" Then    不允许更改吧
                    '    AllowUpdate = False
                    'ElseIf tcmd = "openupdate" Then
                    '    AllowUpdate = True
                ElseIf tcmd = "feedback" Then
                    Call OpenFeedbackPage()
                ElseIf tcmd = "about" OrElse tcmd = "info" Then
                    Call SoftwareInfo()
                ElseIf tcmd = "applypy" Then
                    Call cPinyinConfig.ApplySinglePinyinOut(PYBlockList)
                    Call Paint()
                ElseIf tcmd = "vmdmode" Then
                    ExeMode = eExeMode.VMD
                    Call Paint()
                ElseIf tcmd = "wavmode" Then
                    ExeMode = eExeMode.WAV
                    Call Paint()
                ElseIf tcmd = "bezmode" Then
                    ExeMode = eExeMode.BEZIER
                    Call Paint()
                ElseIf tcmd = "clearface" Then
                    For Each face As Face In ListFace
                        face.PointList.Clear()
                    Next
                    Call Paint()
                ElseIf tcmd = "addbez" Then
                    PenLine.Content.Add(New BezierPenPoint(New PointF3(0, 0, 0), New PointF3(5, 5, 5)))
                    PenLine.GenerateBezier()
                    Call Paint()

                Else
                    PostMsg("未知指令")
                End If
            ElseIf tst.Length = 2 Then
                Dim tcmd As String = tst(0)
                If tcmd.Contains("set") Then
                    If tcmd.Contains("all") Then
                        If tcmd = "setally" Then
                            For i = 0 To ListBone(0).GetPointCount - 1
                                ListBone(0).PointList(i).SY = CSng(tst(1))
                            Next
                        End If
                        Call Paint()
                    Else
                        If SelectedPoint IsNot Nothing Then
                            If tcmd = "setx" Then
                                CType(SelectedPoint, BonePoint).X = CSng(tst(1))
                            ElseIf tcmd = "sety" Then
                                CType(SelectedPoint, BonePoint).Y = CSng(tst(1))
                            ElseIf tcmd = "setz" Then
                                CType(SelectedPoint, BonePoint).Z = CSng(tst(1))
                            ElseIf tcmd = "setrx" Then
                                CType(SelectedPoint, BonePoint).SX = CSng(tst(1))
                            ElseIf tcmd = "setry" Then
                                CType(SelectedPoint, BonePoint).SY = CSng(tst(1))
                            ElseIf tcmd = "setrz" Then
                                CType(SelectedPoint, BonePoint).SZ = CSng(tst(1))
                            ElseIf tcmd = "setv" Then
                                CType(SelectedPoint, FacePoint).V = CSng(tst(1))
                            End If
                            Call Paint()
                        Else
                            PostMsg("未选中任何点")
                        End If
                    End If
                ElseIf tcmd = "normaldist" Then
                    Dim tv As Double = CDbl(tst(1))
                    PostMsg(MathEx.ND.CND(tv))
                ElseIf tcmd = "bonelist" Then
                    ShowingBone = tst(1)
                    Call Paint()
                ElseIf tcmd = "cardsb" Then
                    Call CardsBounce(GetCardsPos, CSng(tst(1)))
                    Call Paint()
                ElseIf tcmd = "cardsc" Then
                    Call CardsCollect(CInt(tst(1)))
                    Call Paint()
                ElseIf tcmd = "bezier" Then
                    PostMsg("指令已废弃")
                    'Call GenerateBez(0, CInt(tst(1)))
                    'Call Paint()
                ElseIf tcmd = "py" Then
                    PostMsg("指令已废弃，请使用'pyb'")
                    'If ExeMode = eExeMode.PINYIN_MANUAL Then
                    '    Call cPinyinConfig.ApplySinglePinyin(ListFace, CInt(tst(1)))
                    '    Call Paint()
                    '    If cPinyinConfig.usingIndex <> -1 Then
                    '        PostMsg("下一个：" & ListPY(cPinyinConfig.usingIndex).Pinyin)
                    '    Else
                    '        PostMsg("结束")
                    '    End If
                    '    Call Paint()
                    'Else
                    '    Call LoadPinyinConfig()
                    '    Call ApplyPinyin(CSng(tst(1)))
                    '    Call Paint()
                    'End If
                ElseIf tcmd = "addwavlength" Then
                    WavAnalyzer.AddedBlankSecond += CSng(tst(1))
                    If WavAnalyzer.AddedBlankSecond < 0 Then WavAnalyzer.AddedBlankSecond = 0
                    Call Paint()
                ElseIf tcmd = "frame_per_second" Then
                    FRAME_PER_SECOND = CInt(tst(1))
                    Call Paint()
                ElseIf tcmd = "blocklength" Then
                    If SelectedBlock IsNot Nothing Then
                        Dim head As CPYBlock = SelectedBlock.GetArrayHeadBlock
                        head.SetArrayAvgLength(CSng(tst(1)))
                    Else
                        postmsg("没有选中拼音块")
                    End If
                    Call Paint()
                ElseIf tcmd = "bezapply" Then
                    Dim frame As Integer = CInt(tst(1))
                    PenLine.GenerateBezier()
                    For i = 0 To frame
                        Dim p As New BonePoint
                        Dim k As New AuxiliaryPolyline
                        p.SetPos(PenLine.GetPoint(i / frame, k))
                        p.Frame = i
                        ListBone(0).AddPoint(p)
                    Next
                    ExeMode = eExeMode.VMD
                    Call Paint()
                ElseIf tcmd = "bezzoom" Then
                    Dim zoom As Single = CSng(tst(1))
                    If zoom <= 0 Then
                        PostMsg("invalid value")
                    Else
                        CanvasPenTool.SetZoom(zoom)
                        Call Paint()
                    End If
                ElseIf tcmd = "addallx" Then
                    Dim value = CSng(tst(1))
                    For Each p As BonePoint In ListBone(0).PointList
                        p.X += value
                    Next
                    Call Paint()

                Else
                    PostMsg("未知指令")
                End If

            ElseIf tst.Length = 3 Then
                Dim tcmd As String = tst(0)
                If tcmd = "addp" Then
                    Dim tp As New BonePoint
                    tp.Frame = CInt(tst(2))
                    ListBone(CInt(tst(1))).AddPoint(tp)
                    Call SortPoint()
                    Call Paint()
                ElseIf tcmd = "paste" Then
                    Dim tp As New BonePoint
                    tp.Frame = CInt(tst(2))
                    tp.Apply(ClipBoard)
                    ListBone(CInt(tst(1))).AddPoint(tp)
                    Call SortPoint()
                    Call Paint()
                ElseIf tcmd = "blink" Then
                    Dim tInterval As Short = CShort(tst(1)) '单位为帧
                    Dim tLength As Integer = CInt(tst(2))   '单位为帧
                    Call SetBlink(tInterval, tLength)
                    Call Paint()
                ElseIf tcmd = "shakeaspi" Then
                    Dim tInterval As Short = CShort(tst(1)) '单位为帧
                    Dim tLength As Integer = CInt(tst(2))   '单位为帧
                    Call ShakeAsPi(tInterval, tLength)
                    Call Paint()
				elseif tcmd = "sentence" then
					dim startFrame as single = csng(tst(1))
					dim endFrame as single = csng(tst(2))
					if exemode = eExeMode.WAV andalso SelectedBlock isnot nothing then
						dim headBlock as CPYBlock = SelectedBlock.GetArrayHeadBlock()
						dim success as Boolean = SentenceOperation.SetSentence(headBlock, startFrame, endFrame)
						call CheckPYBListError(PYBlockList)
						call paint()
						if not success then
							postmsg("参数存在错误")
						end if 
					else
						postmsg("没有选中对象")
					end if 
					
				Else
                    PostMsg("未知指令")
                End If
            ElseIf tst.Length = 5 Then
                Dim tcmd As String = tst(0)
                If tcmd = "single" Then
                    PostMsg(BitConverter.ToSingle(Receive4Bytes(tst(1), tst(2), tst(3), tst(4)), 0).ToString)
                ElseIf tcmd = "bezier" Then
                    PostMsg("指令已废弃")
                    'Dim plist As New List(Of PointF)
                    'With plist
                    '    .Add(New PointF(CSng(tst(1)), CSng(tst(2))))
                    '    .Add(New PointF(CSng(tst(3)), CSng(tst(4))))
                    '    .Add(New PointF(0, 0))
                    '    .Add(New PointF(1, 1))
                    'End With

                    'Call GenerateBez(plist)
                    'Call Paint()
                ElseIf tcmd = "cardsf" Then
                    Call CardsFix(New PointF3(CSng(tst(1)), CSng(tst(2)), CSng(tst(3))), CSng(tst(4)))
                    Call Paint()

				Else
                    PostMsg("未知指令")
                End If
            Else
                Dim tcmd As String = tst(0)
                If tcmd = "???" Then
                    PostMsg("hello[1.1.2]")

                Else
                    PostMsg("未知指令")
                End If


            End If
        End If

    End Sub

    Private Sub P_MouseDown(sender As Object, e As MouseEventArgs) Handles P.MouseDown
        If ExeMode = eExeMode.WAV Then
            If e.Y >= 675 / 2 AndAlso SelectedBlock IsNot Nothing Then
                SelectedButton = -1
                If Btn1.MouseDown(e) Then
                    SelectedButton = 1
                ElseIf Btn2.MouseDown(e) Then
                    SelectedButton = 2
                ElseIf Btn3.MouseDown(e) Then
                    SelectedButton = 3
                End If
                If SelectedButton <> -1 Then MouseFlag = True
            ElseIf e.Y >= 400 / 2 AndAlso e.Y <= 460 / 2 Then
                SelectedBlock = FindBlock(e)
                Call Paint()
            ElseIf e.Y < 300 / 2 Then
                WavControlBar.SetWavClickTag(e)
            Else
                If WavControlBar.JudgeHit(e) Then
                    MouseFlag = True
                    StartX = e.X
                End If
            End If
        ElseIf ExeMode = eExeMode.BEZIER Then
            CanvasPenTool.Mouse_Down(e)
            Call Paint()
        Else
            If e.Button = MouseButtons.Left Then
                If SelectedPoint IsNot Nothing AndAlso e.Y >= 350 Then
                    If TypeOf SelectedPoint Is BonePoint Then
                        Dim skey As Short = CShort((e.X - 3) \ 50)
                        Select Case skey
                            Case 0
                                TB2.Text = "setx " & CType(SelectedPoint, BonePoint).X
                            Case 1
                                TB2.Text = "sety " & CType(SelectedPoint, BonePoint).Y
                            Case 2
                                TB2.Text = "setz " & CType(SelectedPoint, BonePoint).Z
                            Case 3
                                TB2.Text = "setrx " & CType(SelectedPoint, BonePoint).SX
                            Case 4
                                TB2.Text = "setry " & CType(SelectedPoint, BonePoint).SY
                            Case 5
                                TB2.Text = "setrz " & CType(SelectedPoint, BonePoint).SZ
                        End Select
                    Else
                        TB2.Text = "setv " & CType(SelectedPoint, FacePoint).V
                    End If
                Else
                    StartX = e.X
                    MouseFlag = True
                End If
            ElseIf e.Button = MouseButtons.Right Then
                Dim sb As Integer = (e.Y - 25) \ 25
                Dim sf As Integer = ShowingFrame + CInt(Math.Ceiling((e.X - 285) / 10) - 1)
                SelectedPoint = FindPoint(sb, sf)
                SelectedPointBone = sb
                Call Paint()
            End If
        End If


    End Sub

    Private Sub P_MouseUp(sender As Object, e As MouseEventArgs) Handles P.MouseUp
        If ExeMode = eExeMode.WAV Then
            WavControlBar.MouseHitState = WavScreenController.EMouseHitState.NONE
            If SelectedButton = 1 Then
                Btn1.MouseUp(e)
            ElseIf SelectedButton = 2 Then
                Btn2.MouseUp(e)
            ElseIf SelectedButton = 3 Then
                Btn3.MouseUp(e)
            End If
            SelectedButton = -1
        ElseIf ExeMode = eExeMode.BEZIER Then
            CanvasPenTool.Mouse_Up(e)
            Call Paint()
        End If
        LastDeltaFrame = 0
        MouseFlag = False
        Call CheckPYBListError(PYBlockList)
        Call Paint()
        Call ReleaseHoldMsg()
    End Sub

    Private Sub P_MouseMove(sender As Object, e As MouseEventArgs) Handles P.MouseMove

        If MouseFlag Then
            If ExeMode = eExeMode.WAV Then
                If SelectedButton <> -1 Then    '按钮拖动
                    If SelectedButton = 1 Then  '移动
                        Dim deltaX As Single = Btn1.MouseMove(e)
                        deltaX = PxToFrame(deltaX)
                        If (Not SelectedBlock.IfHaveLast()) Then
                            SelectedBlock.SetDeltaStart(deltaX)
                        Else
                            HoldPostMsg("不可移动")
                        End If
                    ElseIf SelectedButton = 2 Then  '时长
                        Dim deltaX As Single = Btn2.MouseMove(e)
                        deltaX = PxToFrame(deltaX)
                        SelectedBlock.DeltaTempLength(deltaX)
                    End If

                Else    '音频控制栏移动
                    Dim delta As Single = -StartX + e.X
                    WavControlBar.MouseMove(delta)
                End If
                Call Paint()
            Else
                Dim deltaframe As Short = CShort((StartX - e.X) \ 10)
                If deltaframe <> LastDeltaFrame Then
                    ShowingFrame += (deltaframe - LastDeltaFrame)
                    Call Paint()
                End If
                LastDeltaFrame = deltaframe
            End If
        End If

        If ExeMode = eExeMode.BEZIER Then
            CanvasPenTool.Mouse_Move(e)
            Call Paint()
        End If

    End Sub

    Private Sub P_MouseWheel(sender As Object, e As MouseEventArgs) Handles P.MouseWheel, TB1.MouseWheel, TB2.MouseWheel
        If ExeMode = eExeMode.BEZIER Then
            If e.Delta > 0 Then
                CanvasPenTool.SetZoom(CanvasPenTool.GetZoom * 0.9)
            ElseIf e.Delta < 0 Then
                CanvasPenTool.SetZoom(CanvasPenTool.GetZoom / 0.9)
            End If
            Call Paint()
        End If
    End Sub

    Private Function FindPoint(boneindex As Integer, frame As Integer) As MMDPoint
        If boneindex < ListBone.Count Then
            If Not ListBone(boneindex).IsEmpty Then
                For Each tp As BonePoint In ListBone(boneindex).PointList
                    If tp.Frame = frame Then
                        Return tp
                    End If
                Next
            End If
        ElseIf boneindex >= ListBone.Count And boneindex < ListBone.Count + ListFace.Count Then
            Dim tbi As Short = boneindex - ListBone.Count
            If Not ListFace(tbi).IsEmpty Then
                For Each tp As FacePoint In ListFace(tbi).PointList
                    If tp.Frame = frame Then
                        Return tp
                    End If
                Next
            End If
        End If
        Return Nothing
    End Function

    Public Sub SortPoint()
        If CBool(ListBone.Count) Then
            For i = 0 To ListBone.Count - 1
                Dim a As New Comparison(Of MMDPoint)(AddressOf cp)
                ListBone(i).PointList.Sort(a)
            Next
        End If
    End Sub

    Private Function cp(a As MMDPoint, b As MMDPoint) As Integer
        Return (a.Frame - b.Frame)
    End Function

    Public Sub ShakeAsPi(Interval As Short, Len As Integer)
        Dim pointer As Integer = 0
        Dim digitcount As Integer = 0
        Dim tpi As String = MathEx.GetPI(1000)

        Do
            Dim tg As Byte = CInt(tpi.Substring(digitcount, 1))
            digitcount += 1
            Dim p1 As New BonePoint
            With p1
                .Frame = pointer
                .SZ = -18 + 4 * tg
                .DefaultTween()
            End With
            ListBone(0).AddPoint(p1)
            pointer += 10
            Dim p2 As BonePoint = p1.Copy
            p2.DefaultTween()
            p2.Frame = pointer
            ListBone(0).AddPoint(p2)

            pointer += Interval
        Loop While pointer < Len


    End Sub

    Public Sub SetBlink(Interval As Short, Len As Integer)
        Dim pointer As Integer = 0
        Dim ran As New Random()

        Do

            Dim tacc As Single = CSng(ran.NextDouble())
            Dim tx As Integer = CInt((MathEx.ND.RCND(tacc) + 4) * Interval / 4)

            pointer += tx
            ListFace(0).AddPoint(New FacePoint(pointer, 0))
            pointer += 1
            ListFace(0).AddPoint(New FacePoint(pointer, 0.5))
            pointer += 1
            ListFace(0).AddPoint(New FacePoint(pointer, 1))
            pointer += 1
            ListFace(0).AddPoint(New FacePoint(pointer, 1))
            pointer += 1
            ListFace(0).AddPoint(New FacePoint(pointer, 0.6))
            pointer += 1
            ListFace(0).AddPoint(New FacePoint(pointer, 0))
            pointer += 1

        Loop While pointer < Len

    End Sub

    Public Sub ApplyPinyinWithBlocks()
        Dim openFile As New OpenFileDialog
        openFile.Filter = "拼音文档|*.txt"
        openFile.Title = "打开"
        openFile.AddExtension = True
        openFile.AutoUpgradeEnabled = True
        If openFile.ShowDialog() = DialogResult.OK Then
            Dim tstr As FileStream = CType(openFile.OpenFile, FileStream)
            Dim r As StreamReader = New StreamReader(tstr)
            PostMsg("正在读取拼音")

            Dim pytext As String = ""
            pytext = r.ReadToEnd
            r.Close()
            tstr.Dispose()

            PostMsg("正在分析拼音")

            Call LoadText(pytext)

            PostMsg("共读取" & ListPY.Count & "个")
            Application.DoEvents()

            If ListPY.Count Then
                For i = ListPY.Count - 1 To 0 Step -1
                    Dim py As cPinyin = ListPY(i)
                    If py.Pinyin.Length = 0 Then
                        ListPY.Remove(py)
                    End If
                Next
            End If

            BlockAssembler.Convert(ListPY, PYBlockList, WavAnalyzer.GetAudioLength * FRAME_PER_SECOND / ListPY.Count)

            PostMsg("完成")
        End If

    End Sub

    <Obsolete("已经改为使用PYBlocks了"， False)>
    Public Overloads Sub ApplyPinyin(Interval As Single)

        Dim openFile As New OpenFileDialog
        openFile.Filter = "拼音文档|*.txt"
        openFile.Title = "打开"
        openFile.AddExtension = True
        openFile.AutoUpgradeEnabled = True
        If openFile.ShowDialog() = DialogResult.OK Then
            Dim tstr As FileStream = CType(openFile.OpenFile, FileStream)
            Dim r As StreamReader = New StreamReader(tstr)
            PostMsg("正在读取拼音")

            Dim pytext As String = ""
            pytext = r.ReadToEnd
            r.Close()
            tstr.Dispose()

            PostMsg("正在分析拼音")

            Call LoadText(pytext)

            PostMsg("共读取" & ListPY.Count & "个")
            PostMsg("正在转换为口型")
            Application.DoEvents()

            If ListPY.Count > 0 Then
                cPinyinConfig.Interval = Interval
                For i = 0 To ListPY.Count - 1
                    Dim tpy As cPinyin = ListPY(i)
                    If tpy.isPause Then
                        cPinyinConfig.ApplyPause(ListFace)
                    ElseIf tpy.GetSpecial = SpecialPinyin.ZhiChiShiRi Then
                        cPinyinConfig.ApplyZhi(ListFace)
                    ElseIf tpy.GetSpecial = SpecialPinyin.ZiCiSi Then
                        cPinyinConfig.ApplyZi(ListFace)
                    ElseIf tpy.GetSpecial = SpecialPinyin.Yu Then
                        cPinyinConfig.ApplyYu(ListFace)
                    ElseIf tpy.GetSpecial = SpecialPinyin.None Then
                        cPinyinConfig.ApplyNormal(ListFace, tpy)
                    End If

                    If cPinyinConfig.Pointer Mod 200 = 0 Then
                        PostMsg((cPinyinConfig.Pointer \ Interval) & " / " & ListPY.Count)
                        Application.DoEvents()
                    End If
                Next
            End If
            If ListPY.Count Then
                For i = ListPY.Count - 1 To 0 Step -1
                    Dim py As cPinyin = ListPY(i)
                    If py.Pinyin.Length = 0 Then
                        ListPY.Remove(py)
                    End If
                Next
            End If

            BlockAssembler.Convert(ListPY, PYBlockList, WavAnalyzer.GetAudioLength * FRAME_PER_SECOND / ListPY.Count)

            PostMsg("完成")
        End If



    End Sub

    <Obsolete("已经改为使用PYBlocks了"， False)>
    Public Overloads Sub ApplyPinyin()
        Dim openFile As New OpenFileDialog
        openFile.Filter = "拼音文档|*.txt"
        openFile.Title = "打开"
        openFile.AddExtension = True
        openFile.AutoUpgradeEnabled = True
        If openFile.ShowDialog() = DialogResult.OK Then
            Dim tstr As FileStream = CType(openFile.OpenFile, FileStream)
            Dim r As StreamReader = New StreamReader(tstr)
            PostMsg("正在读取拼音")

            Dim pytext As String = ""
            pytext = r.ReadToEnd
            r.Close()
            tstr.Dispose()

            PostMsg("正在分析拼音")

            Call LoadText(pytext)

            PostMsg("共读取" & ListPY.Count & "个")
        End If

    End Sub

    <Obsolete("", True)>
    Public Sub OpenReadingText()
        Dim openFile As New OpenFileDialog
        openFile.Filter = "拼音文档|*.txt"
        openFile.Title = "打开"
        openFile.AddExtension = True
        openFile.AutoUpgradeEnabled = True
        If openFile.ShowDialog() = DialogResult.OK Then
            Dim tstr As FileStream = CType(openFile.OpenFile, FileStream)
            Dim r As StreamReader = New StreamReader(tstr)
            PostMsg("正在读取拼音")

            Dim pytext As String = ""

            pytext = r.ReadToEnd

            r.Close()
            tstr.Dispose()

            PostMsg("正在分析拼音")

            Call LoadText(pytext)

            PostMsg("共读取" & ListPY.Count & "个")
            PostMsg("正在转换为口型")
            Application.DoEvents()

            If ListPY.Count Then
                Dim pointer As Integer = 0
                Dim seveneight As Short = 7
                Dim progresscount As Integer = 0
                For i = 0 To ListPY.Count - 1
                    Dim tpy As cPinyin = ListPY(i)
                    If tpy.isPause Then
                        For j = 0 To 4
                            Dim p1 As New FacePoint(pointer, 0)
                            ListFace(j).AddPoint(p1)
                            Dim p2 As New FacePoint(pointer + 14, 0)
                            ListFace(j).AddPoint(p2)
                        Next
                    ElseIf tpy.GetSpecial <> SpecialPinyin.None Then
                        'If tpy.Special = SpecialPinyin.ZhiChiShiRiZiCiSi Then
                        If tpy.GetSpecial = SpecialPinyin.ZhiChiShiRi Then
                            Dim tk() As Short = {0, 1, 2, -1}
                            Dim tv() As Single = {0.15, 0.3, 0.5, 0.5}
                            For j = 0 To tk.Count - 1
                                If j = tk.Count - 1 Then
                                    Dim p1 As New FacePoint(pointer + seveneight - 1, tv(j))
                                    ListFace(1).AddPoint(p1)    'i
                                    Dim p2 As New FacePoint(pointer + seveneight - 1, tv(j))
                                    ListFace(3).AddPoint(p2)    'e
                                Else
                                    Dim p1 As New FacePoint(pointer + tk(j), tv(j))
                                    ListFace(1).AddPoint(p1)    'i
                                    Dim p2 As New FacePoint(pointer + tk(j), tv(j))
                                    ListFace(3).AddPoint(p2)    'e
                                End If

                            Next
                        ElseIf tpy.GetSpecial = SpecialPinyin.Yu Then
                            Dim p1 As New FacePoint(pointer, 0.6)
                            ListFace(2).AddPoint(p1)    'u
                            Dim p2 As New FacePoint(pointer + seveneight - 1, 0.6)
                            ListFace(2).AddPoint(p2)    'u
                        End If
                    Else
                        With tpy
                            Dim sep() As Short
                            Dim vc As Short = .Vowel.Count
                            If vc = 0 Then
                                PostMsg("错误：没有元音 拼音：" & .Pinyin)
                                Exit Sub
                            ElseIf vc = 1 Then
                                sep = {0, -1}
                            ElseIf vc = 2 Then
                                sep = {0, 4, -1}
                            ElseIf vc = 3 Then
                                sep = {0, 2, 4, -1}
                            Else
                                PostMsg("错误：太多元音 拼音：" & .Pinyin)
                                Exit Sub
                            End If

                            'If vc = 3 AndAlso (.CloseMouth = CloseMouthParam.Before Or .CloseMouth = CloseMouthParam.Both) Then
                            '    PostMsg("错误：三元音闭口冲突 拼音：" & .Pinyin)
                            '    Exit Sub
                            'End If

                            Dim tsta As Byte = 0
                            Dim tend As Byte = seveneight
                            If .GetCloseMouth = CloseMouthParam.Before Or .GetCloseMouth = CloseMouthParam.Both Then
                                tsta = 2
                                For j = 0 To 4
                                    SubAddVowel(pointer, 0, j)
                                    SubAddVowel(pointer + 1, 0, j)
                                Next
                            End If
                            If .GetCloseMouth = CloseMouthParam.SemiAfter Or .GetCloseMouth = CloseMouthParam.Both Then
                                tend -= 2
                            End If
                            If vc = 1 Then
                                Dim tvo As Byte = .Vowel(0)
                                If .SingleE AndAlso tvo = 3 Then
                                    tvo = 5     '【饿】的发音
                                End If

                                SubAddVowel(pointer + tsta, 0.1, tvo)
                                Dim midf As Short = CShort((tsta + seveneight) / 2)
                                SubAddVowel(pointer + midf - 1, 0.5, tvo)
                                SubAddVowel(pointer + tend, 0.5, tvo)

                            ElseIf vc = 2 Then
                                Dim tvo1 As Byte = .Vowel(0)
                                Dim tvo2 As Byte = .Vowel(1)
                                If .SingleE Then
                                    If tvo1 = 3 Then
                                        tvo1 = 5
                                    End If
                                    If tvo2 = 3 Then
                                        tvo2 = 5
                                    End If
                                End If
                                If .ChangedA AndAlso tvo2 = 0 Then
                                    tvo2 = 3    'ian
                                End If

                                SubAddVowel(pointer + tsta, 0.1, tvo1)
                                Dim midf As Short = CShort((tsta + seveneight) / 2)
                                SubAddVowel(pointer + midf - 2, 0.55, tvo1)
                                SubAddVowel(pointer + midf, 0, tvo1)

                                SubAddVowel(pointer + midf - 2, 0, tvo2)
                                SubAddVowel(pointer + midf, 0.55, tvo2)
                                SubAddVowel(pointer + tend, 0.1, tvo2)

                            ElseIf vc = 3 Then
                                Dim tvo1 As Byte = .Vowel(0)
                                Dim tvo2 As Byte = .Vowel(1)
                                Dim tvo3 As Byte = .Vowel(2)
                                If .SingleE Then
                                    If tvo1 = 3 Then
                                        tvo1 = 5
                                    End If
                                    If tvo2 = 3 Then
                                        tvo2 = 5
                                    End If
                                    If tvo3 = 3 Then
                                        tvo3 = 5
                                    End If
                                End If
                                If .ChangedA AndAlso tvo3 = 0 Then
                                    tvo3 = 3
                                End If

                                If tsta = 0 Then
                                    SubAddVowel(pointer, 0.1, tvo1)
                                    SubAddVowel(pointer, 0, tvo2)

                                    SubAddVowel(pointer + 1, 0.3, tvo1)

                                    SubAddVowel(pointer + 2, 0, tvo1)
                                    SubAddVowel(pointer + 2, 0.4, tvo2)
                                Else
                                    SubAddVowel(pointer + 2, 0.15, tvo1)
                                    SubAddVowel(pointer + 2, 0.1, tvo2)

                                    SubAddVowel(pointer + 3, 0, tvo1)
                                End If

                                SubAddVowel(pointer + 3, 0.3, tvo2)
                                SubAddVowel(pointer + 3, 0, tvo3)

                                SubAddVowel(pointer + 4, 0.4, tvo3)

                                SubAddVowel(pointer + 5, 0, tvo2)
                                SubAddVowel(pointer + 5, 0.5, tvo3)

                                SubAddVowel(pointer + tend, 0.1, tvo3)


                            End If
                            If .GetCloseMouth = CloseMouthParam.SemiAfter Or .GetCloseMouth = CloseMouthParam.Both Then
                                Dim fv As Byte = .Vowel(.Vowel.Count - 1)
                                If .SingleE AndAlso fv = 3 Then
                                    fv = 5
                                End If
                                If .ChangedA AndAlso fv = 0 Then
                                    fv = 3    'ian
                                End If
                                SubAddVowel(pointer + tend + 1, 0.05, fv)
                            End If


                        End With
                    End If


                    progresscount += 1
                    If progresscount Mod 100 = 0 Then
                        PostMsg(progresscount & " / " & ListPY.Count)
                        Application.DoEvents()
                    End If
                    If tpy.isPause Then
                        pointer += 15
                    Else
                        pointer += seveneight
                        seveneight = 15 - seveneight
                    End If
                Next
            End If

            PostMsg("完成")
        End If

    End Sub

    Private Sub SubAddVowel(tf As Integer, tv As Single, tvo As Byte)

        If tvo = 5 Then     'e-ex
            Dim p1 As New FacePoint(tf, tv * 0.75)
            ListFace(2).AddPoint(p1)    'u
            Dim p2 As New FacePoint(tf, tv * 0.75)
            ListFace(3).AddPoint(p2)    'e
        Else
            Dim p1 As New FacePoint(tf, tv)
            ListFace(tvo).AddPoint(p1)
        End If

    End Sub

    Public Sub CardsTogether()

        Dim poslist(53) As Short    '54张牌
        Dim ordlist As New List(Of Short)
        For i = 0 To 53
            ordlist.Add(i)
        Next
        Dim ran As New Random
        For i = 0 To 53
            Dim ti As Short = ran.Next(0, 54 - i)
            poslist(i) = ordlist(ti)
            ordlist.RemoveAt(ti)
        Next

        For i = 0 To 4
            For j = 0 To 12
                If i * 13 + j >= 54 Then
                    Exit For
                End If
                Dim tb As Bone = ListBone(i * 13 + j)
                Dim p1 As New BonePoint()
                p1.Frame = 0
                p1.Init(1.225 * j, 0.015 * poslist(i * 13 + j), -1.9 * i, 0, 0, 0, 0)
                tb.AddPoint(p1)
            Next
        Next


    End Sub

    Public Function GetCardsPos() As Short()
        Dim r(53) As Short
        For i = 0 To 53
            Dim tb As Bone = ListBone(i)
            Dim tp As BonePoint = tb.PointList(0)
            r(i) = tp.Y / 0.015
        Next
        Return r
    End Function

    Public Sub CardsBounce(poslist() As Short, v0mid As Single)

        Dim g As Single = 10
        Dim ran As New Random


        For i = 0 To 4
            For j = 0 To 12
                If i * 13 + j >= 54 Then
                    Exit For
                End If
                Dim tb As Bone = ListBone(i * 13 + j)

                Dim tx As Single = 0.001 * ran.Next(-54 + poslist(i * 13 + j), 54 - poslist(i * 13 + j))
                Dim tz As Single = 0.001 * ran.Next(-54 + poslist(i * 13 + j), 54 - poslist(i * 13 + j))
                Dim tyv As Single = -(v0mid + 0.0015 * (27 - poslist(i * 13 + j)))
                Dim slow As Single = 0.05
                For k = 1 To 89
                    Dim tv As Vector3
                    If k <= 8 Then
                        tyv += g / 30
                        tv = New Vector3(tx, tyv, tz)
                    Else    '时间变慢
                        tyv += g * slow / 30
                        tv = New Vector3(slow * tx, slow * tyv, slow * tz)
                    End If

                    Dim tp As BonePoint = tb.PointList(k - 1).Copy
                    tp.Frame = k
                    tp.DeltaPos(tv)
                    tb.AddPoint(tp)
                Next

            Next
        Next


    End Sub

    Public Sub CardsExplode()

        Dim ran As New Random

        For i = 0 To 4
            For j = 0 To 12
                If i * 13 + j >= 54 Then
                    Exit For
                End If
                Dim tb As Bone = ListBone(i * 13 + j)

                Dim rx As Single = ran.NextDouble() * 2 * PI - PI
                Dim ry As Single = ran.NextDouble() * 2 * PI - PI
                Dim rz As Single = ran.NextDouble() * 2 * PI - PI
                Dim v0 As Single = 6

                Dim vec0 As VectorF4 = tb.PointList(0).QuaToV4

                Dim rvmax As Single = 30
                Dim rxv As Single = ran.NextDouble() * 2 * rvmax - rvmax
                Dim ryv As Single = ran.NextDouble() * 2 * rvmax - rvmax
                Dim rzv As Single = ran.NextDouble() * 2 * rvmax - rvmax
                Dim deltasample As New BonePoint
                With deltasample
                    .SX = rxv
                    .SY = ryv
                    .SZ = rzv
                End With

                Dim vec1 As VectorF4 = deltasample.QuaToV4

                Dim slow As Single = 0.006
                Dim v2 As Single = 1
                For k = 1 To 30
                    Dim tv As Vector3
                    Dim v1 As Single = 0

                    If k <= 4 Then
                        v1 = v0
                        v2 += 1
                    Else    '时间变慢
                        v1 = v0 * slow
                        v2 += slow
                    End If
                    Dim dx As Single = v1 * Cos(ry) * Sin(rz)
                    Dim dy As Single = v1 * Cos(rx) * Cos(rz)
                    Dim dz As Single = v1 * Sin(rx) * Cos(ry)

                    tv = New Vector3(dx, -dy, dz)
                    Dim tp As BonePoint = tb.PointList(k - 1).Copy
                    tp.Frame = k
                    tp.DeltaPos(tv)

                    Dim vec2 As VectorF4 = ReverseSlerp(vec0, vec1, v2)
                    tp.SetQuaternion(vec2)
                    tb.AddPoint(tp)
                Next

            Next
        Next


    End Sub

    Public Overloads Sub GenerateBez()
        Dim pcount As Short = ListBone(0).GetPointCount
        Dim framecount As Integer = ListBone(0).PointList(pcount - 1).Frame
        Dim plist As New List(Of PointF3)
        For Each tp As BonePoint In ListBone(0).PointList
            Dim tvec As PointF3 = tp.PosToP3
            plist.Add(tvec)
        Next
        For i = 1 To framecount - 1
            Dim posvalue As PointF3 = Bezier(plist, i / framecount)
            Dim tp As New BonePoint
            tp.SetPos(posvalue)
            tp.Frame = i
            ListBone(0).AddPoint(tp)
        Next
    End Sub

    Public Overloads Sub GenerateBez(tween As List(Of PointF))
        Dim pcount As Short = ListBone(0).GetPointCount
        Dim framecount As Integer = ListBone(0).PointList(pcount - 1).Frame
        Dim plist As New List(Of PointF3)
        For Each tp As BonePoint In ListBone(0).PointList
            Dim tvec As PointF3 = tp.PosToP3
            plist.Add(tvec)
        Next
        For i = 1 To framecount - 1
            Dim posvalue As PointF3 = Bezier(plist, Bezier(tween, i / framecount).Y)
            Dim tp As New BonePoint
            tp.SetPos(posvalue)
            tp.Frame = i
            ListBone(0).AddPoint(tp)
        Next

    End Sub

    Public Overloads Sub GenerateBez(bezpentweentype As Short, lastframe As Integer)
        '匀速运动
        SortPoint()

        Dim distsum As Single = 0
        Dim bpplist As New List(Of BezierPenPointObsolete)

        For i = 0 To ListBone(0).GetPointCount - 1 Step 2
            Dim tp1 As PointF3 = ListBone(0).PointList(i).PosToP3
            Dim tp2 As PointF3 = ListBone(0).PointList(i + 1).PosToP3
            Dim tbp As New BezierPenPointObsolete(tp2, tp1)
            bpplist.Add(tbp)
        Next

        Dim poskvp As New List(Of KeyValuePair(Of Integer, Single))
        Dim lastpoint As PointF3 = Nothing
        Dim accudist As Single = 0
        For i = 0 To bpplist.Count - 2
            For j = 0 To 99
                Dim posvalue As PointF3 = Bezier(bpplist, CInt(i * 100 + j))
                If lastpoint IsNot Nothing Then
                    Dim deltadist As Single = CalcDist(posvalue, lastpoint)
                    accudist += deltadist
                End If
                lastpoint = posvalue.Copy
                poskvp.Add(New KeyValuePair(Of Integer, Single)(i * 100 + j, accudist))
            Next
        Next
        distsum = accudist

        For i = 0 To lastframe
            Dim tdist As Single = distsum * i / lastframe
            'then, search for the kvp position

            Dim tre2 As Integer = -1
            'binary search
            Dim tre As Integer = -1
            Dim ub As Integer = poskvp.Count - 1
            Dim lb As Integer = 0
            Dim mid As Integer = 0

            While (lb <= ub)
                If (ub = lb + 1) Then
                    tre = lb
                    Exit While
                End If
                mid = (lb + ub) / 2
                Dim tv As Single = poskvp(mid).Value
                If (tv = tdist) Then
                    tre = mid
                    Exit While
                ElseIf (tdist > tv) Then
                    lb = mid
                Else
                    ub = mid
                End If
            End While

            If tre = poskvp.Count - 1 Then
                tre2 = tre
            Else
                If poskvp(tre).Value - tdist <= tdist - poskvp(tre + 1).Value Then
                    tre2 = tre + 1
                Else
                    tre2 = tre
                End If
            End If

            Dim slidevalue As Integer = poskvp(tre2).Key

            Dim posvalue2 As PointF3 = Bezier(bpplist, slidevalue)
            Dim tp As New BonePoint
            tp.SetPos(posvalue2)
            tp.Frame = i
            ListBone(0).AddPoint(tp)
        Next

    End Sub

    Public Sub CardsDisperse(poslist As Short())
        Dim po As PointF3 = ListBone(0).PointList(0).PosToP3
        For i = 0 To 29
            For j = 0 To 53
                If poslist(j) < i Then
                    Dim tp As New BonePoint
                    tp.Frame = i
                    tp.SetPos(po)
                    tp.X += 2 * poslist(j)
                    ListBone(j).AddPoint(tp)
                Else
                    Dim tp As New BonePoint
                    tp.Frame = i
                    tp.SetPos(po)
                    tp.X += 2 * i
                    ListBone(j).AddPoint(tp)
                End If
            Next
        Next

    End Sub

    Public Sub CardsFlip(pat As Short)  '0-r 1-head 2-tail
        If pat = 0 Then
            'NA
        ElseIf pat = 1 Then
            For i = 0 To 53
                Dim tp As BonePoint = ListBone(i).PointList(0).Copy
                tp.Frame = 1
                tp.SZ = 180
                ListBone(i).AddPoint(tp)
            Next
        Else
            For i = 0 To 53
                Dim tp As BonePoint = ListBone(i).PointList(0).Copy
                tp.Frame = 1
                tp.SZ = 0
                ListBone(i).AddPoint(tp)
            Next
        End If
    End Sub

    Private Sub CardsCollect(pointcount As Short)
        Dim track As New List(Of List(Of BezierPenPointObsolete))
        Dim tracktween As New List(Of List(Of PointF))

        For i = 0 To 2
            Dim tracksublist As New List(Of BezierPenPointObsolete)
            For j = 0 To pointcount - 1 Step 2
                Dim trackp1 As PointF3 = ListBone(i).PointList(j).PosToP3
                Dim trackp2 As PointF3 = ListBone(i).PointList(j + 1).PosToP3
                Dim trackp As New BezierPenPointObsolete(trackp2, trackp1)
                tracksublist.Add(trackp)

            Next
            track.Add(tracksublist)
            Dim tracksubtween As New List(Of PointF)
            tracksubtween.Add(New PointF(0, 0))
            tracksubtween.Add(New PointF(0.5, 0.05))
            tracksubtween.Add(New PointF(0.5, 0.95))
            tracksubtween.Add(New PointF(1, 1))
            tracktween.Add(tracksubtween)
        Next


        Dim trackcount As Short = track.Count
        Dim weightlist As New List(Of Double)

        For k = 0 To 53
            For i = 1 To 90
                Dim distlist As New List(Of Single)
                Dim nearestpoint As New PointF3
                For j = 0 To trackcount - 1
                    Dim tp As PointF3 = Bezier(track(j), Bezier(tracktween(j), i / 90).Y)
                    Dim tdis As Single = CalcDist(ListBone(k).PointList(i - 1).PosToP3, tp)
                    If distlist.Count Then
                        If tdis < distlist.Min Then
                            nearestpoint = tp
                        End If
                    Else
                        nearestpoint = tp
                    End If
                    Dim ki = k \ 13
                    Dim kj = k Mod 13
                    With nearestpoint
                        .X += 1.225 * kj
                        .Y += 0
                        .Z += -1.9 * ki
                    End With

                    distlist.Add(tdis)
                Next
                Dim sumdist As Single = distlist.Sum
                Dim trackdelta As New List(Of Vector3)
                For j = 0 To trackcount - 1
                    Dim tdv As New Vector3
                    With tdv
                        .X = Bezier(track(j), Bezier(tracktween(j), i / 90).Y).X - Bezier(track(j), Bezier(tracktween(j), (i - 1) / 90).Y).X
                        .Y = Bezier(track(j), Bezier(tracktween(j), i / 90).Y).Y - Bezier(track(j), Bezier(tracktween(j), (i - 1) / 90).Y).Y
                        .Z = Bezier(track(j), Bezier(tracktween(j), i / 90).Y).Z - Bezier(track(j), Bezier(tracktween(j), (i - 1) / 90).Y).Z
                    End With
                    trackdelta.Add(tdv)
                Next
                Dim rp As New Vector3
                For j = 0 To trackcount - 1
                    rp.X += trackdelta(j).X * distlist(j) / sumdist
                    rp.Y += trackdelta(j).Y * distlist(j) / sumdist
                    rp.Z += trackdelta(j).Z * distlist(j) / sumdist
                Next
                Dim tpo As PointF3 = ListBone(k).PointList(i - 1).PosToP3
                tpo.Move(rp)

                Dim rbp As BonePoint = ListBone(k).PointList(i - 1).Copy
                With rbp
                    .Frame = i
                    .SetPos(tpo)
                    Dim tpdesti As PointF3 = .PosToP3
                    Dim tpdr As New PointF3
                    tpdr.X = tpdesti.X * (90 - i) / 90 + nearestpoint.X * (i / 90)
                    tpdr.Y = tpdesti.Y * (90 - i) / 90 + nearestpoint.Y * (i / 90)
                    tpdr.Z = tpdesti.Z * (90 - i) / 90 + nearestpoint.Z * (i / 90)
                    .SetPos(tpdr)

                End With
                ListBone(k).AddPoint(rbp)

            Next
        Next

    End Sub

    Private Sub CardsFix(pcentre As PointF3, pr As Single)
        Dim ran As New Random
        For i = 0 To 53
            Dim rx As Single = ran.NextDouble() * 2 * PI - PI
            Dim ry As Single = ran.NextDouble() * 2 * PI - PI
            Dim rz As Single = ran.NextDouble() * 2 * PI - PI

            Dim dx As Single = pr * Cos(ry) * Sin(rz)
            Dim dy As Single = pr * Cos(rx) * Cos(rz)
            Dim dz As Single = pr * Sin(rx) * Cos(ry)
            For j = 0 To 90

                Dim po As PointF3 = ListBone(i).PointList(j).PosToP3
                Dim tp As New PointF3
                With tp
                    .X = po.X * (90 - j) / 90 + (dx + pcentre.X) * j / 90
                    .Y = po.Y * (90 - j) / 90 + (dy + pcentre.Y) * j / 90
                    .Z = po.Z * (90 - j) / 90 + (dz + pcentre.Z) * j / 90
                End With

                Dim ki = i \ 13
                Dim kj = i Mod 13
                With tp
                    .X += 1.225 * kj
                    .Y += 0
                    .Z += -1.9 * ki
                End With

                ListBone(i).PointList(j).SetPos(tp)
            Next
        Next

    End Sub

    Private Sub SinWaveMove(Optional amp As Single = 6, Optional omega As Single = 0.1)

        Call SortPoint()
        Dim maxframe As Integer = ListBone(0).GetPointCount - 1

        Dim c As Single = ListBone(0).PointList(0).Y
        'Dim amp As Single = Abs(ListBone(0).PointList(1).Y - c)
        'Dim Tquater As Single = Abs(CalcDist(ListBone(0).PointList(0).PosToP3.GetXZ, ListBone(0).PointList(1).PosToP3.GetXZ))
        'Dim omega As Single = PI / (2 * Tquater)

        Dim ax As Single = 0
        For i = 0 To maxframe
            Dim tp As BonePoint = ListBone(0).PointList(i)
            If i >= 1 Then
                Dim dx As Single = CalcDist(tp.PosToP3.GetXZ, ListBone(0).PointList(i - 1).PosToP3.GetXZ)
                ax += dx
            End If
            Dim ry As Single = amp * Sin(omega * ax) + c
            tp.Y = ry

        Next

    End Sub

    ''' <summary>
    ''' 启动MMT Updater.exe
    ''' </summary>
    Public Sub CheckUpdate()
        Shell(Application.StartupPath & "\MMT_Updater.exe")
        End
    End Sub

    ''' <summary>
    ''' 打开反馈窗口
    ''' </summary>
    Public Sub OpenFeedbackPage()
        FormFeedback.Show()
    End Sub

    ''' <summary>
    ''' 从服务器获取最新一条通知
    ''' </summary>
    Public Async Sub PullMessage()
        Dim hc1 As New Net.Http.HttpClient
        Dim uri As String = "http://207.148.14.219:8083/mmt/getNews"
        Dim response As String = vbNullString
        Try
            response = Await hc1.GetStringAsync(uri)
        Catch ex As Exception
            Return
        End Try
        JSONParser.Load(response)
        Dim entityList As List(Of JSONEntity) = JSONParser.GetResult
        If entityList.Count Then
            Dim msgEntity As JSONEntity = entityList(0)
            PostMsg(msgEntity.GetValue("date").Substring(0, 10))
            PostMsg(msgEntity.GetValue("description"))
        End If
    End Sub

    ''' <summary>
    ''' 从服务器获取所有远程命令
    ''' </summary>
    <Obsolete("暂不可用", False)>
    Public Async Sub PullRemoteCommand()
        '暂时不实现此功能
    End Sub

    ''' <summary>
    ''' 显示软件信息
    ''' </summary>
    Public Sub SoftwareInfo()
        Dim info As String = "Miz_MMD_Tool ver." & ME_VERSION & vbcrlf
        info = info & ME_RELEASE_TIME & vbCrlf
        info = info & "开发者：sscs" & vbCrLf
        info = info & "项目主页 https://github.com/csMiz/MMT/tree/master/"

        MsgBox(info, MsgBoxStyle.Information, "关于此软件")

    End Sub

    Private Sub TB2_KeyDown(sender As Object, e As KeyEventArgs) Handles TB2.KeyDown, TB1.KeyDown
        If ExeMode = eExeMode.BEZIER Then
            CanvasPenTool.CTRLPressed = e.Control
            CanvasPenTool.ALTPressed = e.Alt
        End If

    End Sub

    Private Sub TB2_KeyUp(sender As Object, e As KeyEventArgs) Handles TB2.KeyUp, TB1.KeyUp
        If ExeMode = eExeMode.BEZIER Then
            CanvasPenTool.CTRLPressed = e.Control
            CanvasPenTool.ALTPressed = e.Alt
        End If
    End Sub
End Class

