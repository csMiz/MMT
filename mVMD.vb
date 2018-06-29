Imports System.IO

Module mVMD
    Public VMDStorage As List(Of String)
    Public ModelName As String = ""
    Public ListBone As List(Of Bone)
    Public ListFace As List(Of Face)


    Public Sub ReadVMD()
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

    Public Sub WriteVMD(r As BinaryWriter)

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

    End Sub

    Public Function VMDHeadBytes() As Byte()

        Dim st As String = "56 6F 63 61 6C 6F 69 64 20 4D 6F 74 69 6F 6E 20 "
        st = st & "44 61 74 61 20 30 30 30 32 00 00 00 00 00"

        Dim o() As Byte = ReceiveBytes(st)
        Dim r(29) As Byte
        For i = 0 To 29
            r(i) = o(i)
        Next
        Return r

    End Function

End Module
