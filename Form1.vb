Imports System.IO
Imports System.Text.RegularExpressions
Imports Miz_MMD_Tool

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

    Public Enum SaveFileParam As Byte
        SaveNew = 0
        VMDTest = 1
        Save = 2

    End Enum

    Public tFont As Font


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

        If CBool(ListBone.Count) Then
            For i = 0 To ListBone.Count - 1
                Dim tbone As Bone = ListBone(i)
                If Not tbone.IsEmpty Then
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
                End If
            Next
        End If

        If CBool(ListFace.Count) Then
            For i = 0 To ListFace.Count - 1
                Dim tface As Face = ListFace(i)
                Dim ia As Short = i + ListBone.Count
                If Not tface.IsEmpty Then
                    G.DrawString(tface.Name, tFont, Brushes.DarkGreen, 5, 55 + ia * 50)
                    For j = 0 To tface.GetPointCount - 1
                        Dim tf As Integer = tface.PointList(j).Frame - ShowingFrame
                        If tf >= -19 AndAlso tf <= 20 Then
                            G.FillEllipse(Brushes.DarkGray, 190 + 20 * (tf + 19), 65 + ia * 50, 20, 20)
                            If (SelectedPoint IsNot Nothing) AndAlso ia = SelectedPointBone AndAlso tface.PointList(j).Equals(SelectedPoint) Then
                                G.FillEllipse(Brushes.DarkRed, 193 + 20 * (tf + 19), 68 + ia * 50, 14, 14)
                            End If
                        End If
                    Next
                End If
            Next
        End If

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

                Else
                    PostMsg("未知指令")
                End If
            ElseIf tst.Length = 2 Then
                Dim tcmd As String = tst(0)
                If tcmd.Contains("set") Then
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

                End If
            ElseIf tst.Length = 5 Then
                Dim tcmd As String = tst(0)
                If tcmd = "single" Then
                    PostMsg(BitConverter.ToSingle(Receive4Bytes(tst(1), tst(2), tst(3), tst(4)), 0).ToString)
                End If
            Else
                    PostMsg("未知指令")
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
                hasbone.PointList.Add(tp)

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
                hasface.PointList.Add(tp)

            Next
        End If


    End Sub

End Class

