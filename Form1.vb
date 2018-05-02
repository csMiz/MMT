Imports System.IO
Imports System.Text.RegularExpressions
Imports Miz_MMD_Tool
Imports System.Math

Public Class Form1

    Public MouseFlag As Boolean = False
    Public StartX As Integer = 0
    Public LastDeltaFrame As Short = 0
    Public UsingFileName As String = ""
    Public UsingFileURI As String = ""
    Public VMDStorage As List(Of String)
    Private ModelName As String = ""
    Private ListBone As List(Of Bone)
    Private ListFace As List(Of Face)
    Private SelectedPoint As MMDPoint = Nothing
    Public SelectedPointBone As Integer = -1
    Private ClipBoard As MMDPoint

    Public ShowingFrame As Integer = 0
    Public ShowingBone As Integer = 0

    Public Enum SaveFileParam As Byte
        SaveNew = 0
        VMDTest = 1
        Save = 2

    End Enum

    Public tFont As Font

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
                G.DrawString(i.ToString, tFont, Brushes.Black, 200 + 20 * (i - ShowingFrame + 19) - 10, 15)
            End If
        Next

        For i = 0 To 9
            Dim tbindex As Short = ShowingBone + i
            Dim tboneface As MMDBoneFace = GetBoneFace(tbindex)
            If tboneface IsNot Nothing Then
                If TypeOf tboneface Is Bone Then
                    Dim tbone As Bone = CType(tboneface, Bone)
                    G.DrawString(tbone.Name, tFont, Brushes.DarkBlue, 5, 55 + i * 50)
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
                    G.DrawString(tface.Name, tFont, Brushes.DarkGreen, 5, 55 + i * 50)
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
                G.DrawString("X=" & CType(SelectedPoint, BonePoint).X.ToString, tFont, Brushes.Black, 5, 715)
                G.DrawString("Y=" & CType(SelectedPoint, BonePoint).Y.ToString, tFont, Brushes.Black, 155, 705)
                G.DrawString("Z=" & CType(SelectedPoint, BonePoint).Z.ToString, tFont, Brushes.Black, 305, 715)
                '显示欧拉角
                G.DrawString("RX=" & CType(SelectedPoint, BonePoint).SX.ToString, tFont, Brushes.Black, 455, 705)
                G.DrawString("RY=" & CType(SelectedPoint, BonePoint).SY.ToString, tFont, Brushes.Black, 605, 715)
                G.DrawString("RZ=" & CType(SelectedPoint, BonePoint).SZ.ToString, tFont, Brushes.Black, 755, 705)

            Else
                G.DrawString("V=" & CType(SelectedPoint, FacePoint).V.ToString, tFont, Brushes.Black, 5, 715)

            End If
        End If



        P.Image = bm
        P.Refresh()
        G.Dispose()

    End Sub

    Public Sub PostMsg(text As String)
        TB1.Text = TB1.Text + text + vbCrLf

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

            Call ReadVMD()

            Call Paint()

            PostMsg("分析完成")

        End If

    End Sub

    Public Sub SaveNewFile(Optional param As SaveFileParam = SaveFileParam.SaveNew)
        Dim tstream As FileStream = Nothing
        If param = SaveFileParam.SaveNew Then
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
        ElseIf param = SaveFileParam.VMDTest Then
            Dim savefile As New FileStream("C:\Users\sscs\Desktop\VMDT.vmd", FileMode.OpenOrCreate)
            tstream = savefile
        ElseIf param = SaveFileParam.Save Then
            Dim savefile As New FileStream(UsingFileURI, FileMode.OpenOrCreate)
            tstream = savefile
        End If

        If tstream IsNot Nothing Then
            Dim r As BinaryWriter = New BinaryWriter(tstream)
            PostMsg("开始写入" & UsingFileName)

            r.Write(VMDHeadBytes())
            Dim omodelname As Byte() = CharToBytes(ModelName)
            Dim tmn(19) As Byte
            For i = 0 To 19
                tmn(i) = omodelname(i)
            Next
            r.Write(tmn)
            '骨骼
            Dim bpcount As Integer = 0
            If ListBone.Count Then
                For i = 0 To ListBone.Count - 1
                    bpcount += ListBone(i).GetPointCount
                Next
            End If
            r.Write(BitConverter.GetBytes(bpcount))
            If ListBone.Count Then
                For i = 0 To ListBone.Count - 1
                    If Not ListBone(i).IsEmpty Then
                        For j = 0 To ListBone(i).GetPointCount - 1
                            Dim tp As BonePoint = ListBone(i).PointList(j)

                            Dim obonename As Byte() = CharToBytes(ListBone(i).Name)
                            Dim tbn(14) As Byte
                            For k = 0 To 14
                                tbn(k) = obonename(k)
                            Next
                            r.Write(tbn)
                            r.Write(BitConverter.GetBytes(tp.Frame))
                            r.Write(BitConverter.GetBytes(tp.X))
                            r.Write(BitConverter.GetBytes(tp.Y))
                            r.Write(BitConverter.GetBytes(tp.Z))
                            r.Write(BitConverter.GetBytes(tp.GetQuaternion(BonePoint.QuaParam.QX)))
                            r.Write(BitConverter.GetBytes(tp.GetQuaternion(BonePoint.QuaParam.QY)))
                            r.Write(BitConverter.GetBytes(tp.GetQuaternion(BonePoint.QuaParam.QZ)))
                            r.Write(BitConverter.GetBytes(tp.GetQuaternion(BonePoint.QuaParam.QW)))
                            r.Write(tp.GetTweenBytes())

                        Next
                    End If
                Next
            End If
            '表情
            Dim fpcount As Integer = 0
            If ListFace.Count Then
                For i = 0 To ListFace.Count - 1
                    fpcount += ListFace(i).GetPointCount
                Next
            End If
            r.Write(BitConverter.GetBytes(fpcount))
            If ListFace.Count Then
                For i = 0 To ListFace.Count - 1
                    If Not ListFace(i).IsEmpty Then
                        For j = 0 To ListFace(i).GetPointCount - 1
                            Dim tp As FacePoint = ListFace(i).PointList(j)

                            Dim ofacename As Byte() = CharToBytes(ListFace(i).Name)
                            Dim tfn(14) As Byte
                            For k = 0 To 14
                                tfn(k) = ofacename(k)
                            Next
                            r.Write(tfn)
                            r.Write(BitConverter.GetBytes(tp.Frame))
                            r.Write(BitConverter.GetBytes(tp.V))

                        Next
                    End If
                Next
            End If

            Dim tempty(15) As Byte
            For i = 0 To 15
                tempty(i) = 0
            Next
            r.Write(tempty)

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
        tbp.Init(1, 2, 3, 4, 5, 6, 0)
        tbp.Frame = 0
        tb.AddPoint(tbp)
        Dim tbp2 As New BonePoint
        tbp2.Init(1, 2, 3, 4, 5, 6, 0)
        tbp2.Frame = 5
        tb.AddPoint(tbp2)
        ListBone.Add(tb)

        Dim tb2 As New Bone
        tb2.Name = "testbone2"
        Dim tbp3 As New BonePoint
        tbp3.Init(1, 2, 3, 4, 5, 6, 0)
        tbp3.Frame = 0
        tb2.AddPoint(tbp3)
        ListBone.Add(tb2)

        Call Paint()
        'Call SaveNewFile(SaveFileParam.VMDTest)


    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Width = 880
        VMDStorage = New List(Of String)
        ListBone = New List(Of Bone)
        ListFace = New List(Of Face)

        tFont = New Font("Microsoft YaHei", 18)
        Call InitNDTable()

    End Sub

    Private Sub TB2_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TB2.KeyPress

        If e.KeyChar = ChrW(13) Then
            Dim tin As String = TB2.Text.Trim
            PostMsg(tin)
            TB2.Text = ""
            Dim tst() As String = Regex.Split(tin, " ")
            If tst.Length = 1 Then
                Dim tcmd As String = tst(0)
                If tcmd = "open" Then
                    Call OpenFile()
                ElseIf tcmd = "test" Then
                    Call PointTest()
                ElseIf tcmd = "save" Then
                    If UsingFileURI = "" Then
                        Call SaveNewFile()
                    Else
                        Call SaveNewFile(SaveFileParam.Save)
                    End If
                ElseIf tcmd = "saveas" Then
                    Call SaveNewFile()
                ElseIf tcmd = "savetest" Then
                    Call SaveNewFile(SaveFileParam.VMDTest)
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
                ElseIf tcmd = "mouth" Then
                    Call OpenReadingText()
                    Call Paint()
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
                    Call GenerateBez()
                    Call Paint()
                ElseIf tcmd = "cardsd" Then
                    Call CardsDisperse(GetCardsPos)
                    Call Paint()
                ElseIf tcmd = "stopcheck" Then
                    Dim a = ListBone


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
                    PostMsg(CND(tv))
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
                    Call GenerateBez(0, CInt(tst(1)))
                    Call Paint()

                Else

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

                End If
            ElseIf tst.Length = 5 Then
                Dim tcmd As String = tst(0)
                If tcmd = "single" Then
                    PostMsg(BitConverter.ToSingle(Receive4Bytes(tst(1), tst(2), tst(3), tst(4)), 0).ToString)
                ElseIf tcmd = "bezier" Then
                    Dim plist As New List(Of PointF)
                    With plist
                        .Add(New PointF(CSng(tst(1)), CSng(tst(2))))
                        .Add(New PointF(CSng(tst(3)), CSng(tst(4))))
                        .Add(New PointF(0, 0))
                        .Add(New PointF(1, 1))
                    End With

                    Call GenerateBez(plist)
                    Call Paint()
                ElseIf tcmd = "cardsf" Then
                    Call CardsFix(New PointF3(CSng(tst(1)), CSng(tst(2)), CSng(tst(3))), CSng(tst(4)))
                    Call Paint()

                End If
            Else
                Dim tcmd As String = tst(0)
                If tcmd = "???" Then

                Else
                    PostMsg("未知指令")
                End If


            End If
            End If

    End Sub

    Private Sub P_MouseDown(sender As Object, e As MouseEventArgs) Handles P.MouseDown
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

    End Sub

    Private Sub P_MouseUp(sender As Object, e As MouseEventArgs) Handles P.MouseUp
        LastDeltaFrame = 0
        MouseFlag = False

    End Sub

    Private Sub P_MouseMove(sender As Object, e As MouseEventArgs) Handles P.MouseMove
        If MouseFlag Then
            Dim deltaframe As Short = CShort((StartX - e.X) \ 10)
            If deltaframe <> LastDeltaFrame Then
                ShowingFrame += (deltaframe - LastDeltaFrame)
                Call Paint()
            End If

            LastDeltaFrame = deltaframe

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

    Private Sub ReadVMD()
        '此部分仅对骨骼和表情分析，摄像机的读取方式与此不同

        '0-29是文件头，不处理
        '30-49是模型名称
        ModelName = GetBytes(VMDStorage, 30, 20)
        ModelName = ModelName.Trim(vbNullChar)
        '50-53数据块数
        Dim bonedatacount As Integer = BitConverter.ToInt32(StringTo4Bytes(GetBytes(VMDStorage, 50, 4)), 0)

        '读数据块
        Dim pointer As Integer = 54
        If bonedatacount Then
            For i = 0 To bonedatacount - 1
                Dim bonename As String = GetBytes(VMDStorage, pointer, 15)
                bonename = bonename.Trim(vbNullChar)
                pointer += 15
                Dim frame As Integer = BitConverter.ToInt32(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim px As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim py As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim pz As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim rx As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim ry As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim rz As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim rw As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim tween As String = GetBytes(VMDStorage, pointer, 64)
                pointer += 64

                Dim hasbone As Bone = Nothing
                For Each tb As Bone In ListBone
                    If tb.Name = bonename Then
                        hasbone = tb
                        Exit For
                    End If
                Next

                If hasbone Is Nothing Then
                    Dim tb As New Bone
                    tb.Name = bonename
                    ListBone.Add(tb)
                    hasbone = tb
                End If

                Dim tp As New BonePoint
                tp.Frame = frame
                tp.Init(px, py, pz, rx, ry, rz, rw)
                tp.SetTween(tween)
                hasbone.AddPoint(tp)

            Next
        End If

        Dim facedatacount As Integer = BitConverter.ToInt32(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
        pointer += 4
        If facedatacount Then
            For i = 0 To facedatacount - 1
                Dim facename As String = GetBytes(VMDStorage, pointer, 15)
                facename = facename.Trim(vbNullChar)
                pointer += 15
                Dim frame As Integer = BitConverter.ToInt32(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4
                Dim tv As Single = BitConverter.ToSingle(StringTo4Bytes(GetBytes(VMDStorage, pointer, 4)), 0)
                pointer += 4

                Dim hasface As Face = Nothing
                For Each tf As Face In ListFace
                    If tf.Name = facename Then
                        hasface = tf
                        Exit For
                    End If
                Next

                If hasface Is Nothing Then
                    Dim tf As New Face
                    tf.Name = facename
                    ListFace.Add(tf)
                    hasface = tf
                End If

                Dim tp As New FacePoint
                tp.Frame = frame
                tp.Init(tv)
                '表情无补间
                hasface.AddPoint(tp)

            Next
        End If


    End Sub

    Public Sub ShakeAsPi(Interval As Short, Len As Integer)
        Dim pointer As Integer = 0
        Dim digitcount As Integer = 0
        Dim tpi As String = getpi(1000)

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
            Dim tx As Integer = CInt((RCND(tacc) + 4) * Interval / 4)

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

        Loop While pointer < len

    End Sub

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
                    ElseIf tpy.Special <> SpecialPinyin.None Then
                        If tpy.Special = SpecialPinyin.ZhiChiShiRiZiCiSi Then
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
                        ElseIf tpy.Special = SpecialPinyin.Yu Then
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
                            If .CloseMouth = CloseMouthParam.Before Or .CloseMouth = CloseMouthParam.Both Then
                                tsta = 2
                                For j = 0 To 4
                                    SubAddVowel(pointer, 0, j)
                                    SubAddVowel(pointer + 1, 0, j)
                                Next
                            End If
                            If .CloseMouth = CloseMouthParam.SemiAfter Or .CloseMouth = CloseMouthParam.Both Then
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
                            If .CloseMouth = CloseMouthParam.SemiAfter Or .CloseMouth = CloseMouthParam.Both Then
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
        Dim bpplist As New List(Of BezierPenPoint)

        For i = 0 To ListBone(0).GetPointCount - 1 Step 2
            Dim tp1 As PointF3 = ListBone(0).PointList(i).PosToP3
            Dim tp2 As PointF3 = ListBone(0).PointList(i + 1).PosToP3
            Dim tbp As New BezierPenPoint(tp2, tp1)
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
        Dim track As New List(Of List(Of BezierPenPoint))
        Dim tracktween As New List(Of List(Of PointF))

        For i = 0 To 2
            Dim tracksublist As New List(Of BezierPenPoint)
            For j = 0 To pointcount - 1 Step 2
                Dim trackp1 As PointF3 = ListBone(i).PointList(j).PosToP3
                Dim trackp2 As PointF3 = ListBone(i).PointList(j + 1).PosToP3
                Dim trackp As New BezierPenPoint(trackp2, trackp1)
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





End Class

