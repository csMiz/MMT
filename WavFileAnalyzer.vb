Imports System.IO

''' <summary>
''' Wav文件解析器类
''' </summary>
Public Class WavFileAnalyzer
    Private Filename As String
    Private VocalTrackCount As Byte = 1
    Private SampleRate As Integer
    Private ByteRate As Integer
    Private BlockAlign As Byte
    Private PCMWidth As Byte
    Private AudioByteLength As Long
    Private FileContent As New List(Of Byte())      '每100000Byte分一次

    Private AudioLength As Single       '单位为秒

    Private Const CONTENT_BLOCK_SIZE As Integer = 100000

    ''' <summary>
    ''' 打开文件并读取
    ''' </summary>
    ''' <param name="file">文件流</param>
    Public Sub LoadFile(file As FileStream)
        Dim baseLength As Long = 0
        Dim reader As BinaryReader = New BinaryReader(file)

        Dim tempContent(100000) As Byte
        Dim jmax As Integer = reader.BaseStream.Length \ CONTENT_BLOCK_SIZE

        For j = 0 To jmax

            If j = jmax Then
                tempContent = reader.ReadBytes(reader.BaseStream.Length - jmax * CONTENT_BLOCK_SIZE)
            Else
                tempContent = reader.ReadBytes(CONTENT_BLOCK_SIZE)
            End If

            FileContent.Add(tempContent)
        Next
        AudioByteLength = reader.BaseStream.Length - 44
        baseLength = AudioByteLength
        reader.Close()

        'fmt chunk多了两位的情况
        Dim cursorOffset As Long = 0L
        Dim haveAdditionalInfo As Boolean = (GetByte(16) = 18)
        If haveAdditionalInfo Then cursorOffset += 2

        'optional fact chunk
        Dim factChunk As Boolean = (GetByte(36 + cursorOffset) = AscW("f")) AndAlso (GetByte(37 + cursorOffset) = AscW("a")) AndAlso (GetByte(38 + cursorOffset) = AscW("c")) AndAlso (GetByte(39 + cursorOffset) = AscW("t"))
        If factChunk Then
            cursorOffset += 12
        End If

        'get general info
        VocalTrackCount = GetByte(22)
        Dim sr() As Byte = GetByte(24, 4)
        SampleRate = sr(3) * (256 ^ 3) + sr(2) * (256 ^ 2) + sr(1) * 256 + sr(0)
        'ByteRate = SampleRate * VocalTrackCount * 2
        Dim br() As Byte = GetByte(28, 4)
        ByteRate = br(3) * (256 ^ 3) + br(2) * (256 ^ 2) + br(1) * 256 + br(0)

        Dim cl() As Byte = GetByte(40 + cursorOffset, 4)
        AudioByteLength = cl(3) * (256 ^ 3) + cl(2) * (256 ^ 2) + cl(1) * 256 + cl(0)

        If baseLength <> AudioByteLength Then
            If baseLength < AudioByteLength Then
                AudioByteLength = baseLength
            End If
        End If

        AudioLength = AudioByteLength / ByteRate

    End Sub

    Private Function GetByte(Position As Long) As Byte
        If Position >= AudioByteLength Then Return 0
        Dim X As Integer = Position Mod CONTENT_BLOCK_SIZE
        Dim Y As Integer = Position \ CONTENT_BLOCK_SIZE
        Return FileContent(Y)(X)
    End Function

    Private Function GetByte(Position As Long, Length As Integer) As Byte()
        Dim result(Length - 1) As Byte
        For i = 0 To Length - 1
            result(i) = GetByte(Position + i)
        Next
        Return result
    End Function

    <Obsolete("Use GetTip(). 请使用GetTip方法", True)>
    Public Sub GetVolume(Second As Single, ByRef Left As Single, Optional ByRef Right As Single = -2)
        Dim position As Long = CLng(Second * ByteRate) + 44
        Dim leftData() As Byte = GetByte(position, 2)
        '有符号-32768 32767
        Left = GetSignedTwoBytes(leftData)
        If Right <> -2 Then
            Dim rightData() As Byte = GetByte(position + 2, 2)
            Right = GetSignedTwoBytes(rightData)
        End If
    End Sub

    Private Function GetSignedTwoBytes(Data() As Byte) As Single
        Dim value As Integer = Data(1) * 256 + Data(0)
        If value > 32767 Then value -= 65536
        Return (value / 32768)
    End Function

    '''<summery>
    '''峰值检测
    '''</summery>
    Public Function GetTip(SampleStart As Single, SampleEnd As Single) As VectorF4
        Dim blockLength As Byte = VocalTrackCount * 2
        '按块取值，手动校正
        Dim pos1Offset As Byte = (SampleStart * ByteRate) Mod blockLength
        Dim pos2Offset As Byte = (SampleEnd * ByteRate) Mod blockLength
        Dim pos1 As Long = SampleStart * ByteRate + 44 + blockLength - pos1Offset
        Dim pos2 As Long = SampleEnd * ByteRate + 44 + blockLength - pos2Offset

        Dim maxValueLeft As Single = 0, maxValueRight As Single = 0
        Dim minValueLeft As Single = 0, minValueRight As Single = 0
        For i = pos1 To pos2 Step blockLength
            Dim valueLeft As Single, valueRight As Single
            '取值
            Dim leftData() As Byte = GetByte(i, 2)
            valueLeft = GetSignedTwoBytes(leftData)
            Dim rightData() As Byte = GetByte(i + 2, 2)
            valueRight = GetSignedTwoBytes(rightData)
            '取极值
            If valueLeft > 0 AndAlso valueLeft > maxValueLeft Then
                maxValueLeft = valueLeft
            ElseIf valueLeft < 0 AndAlso valueLeft < minValueLeft Then
                minValueLeft = valueLeft
            End If
            If valueRight > 0 AndAlso valueRight > maxValueRight Then
                maxValueRight = valueRight
            ElseIf valueRight < 0 AndAlso valueRight < minValueRight Then
                minValueRight = valueRight
            End If
        Next
        Return New VectorF4(maxValueLeft, minValueLeft, maxValueRight, minValueRight)

    End Function

    '<summery>
    '平均取值
    '</summery>
    'Private Function GetAverage(SampleStart As Single, SampleEnd As Single) As Single
    '    Dim pos1 As Long = SampleStart * ByteRate
    '    Dim pos2 As Long = SampleEnd * ByteRate
    '    Dim avgValue As Single
    '    Dim sum As Single
    '    For i = pos1 To pos2
    '        Dim value As Single = GetVolume(i)
    '        sum += value
    '    Next
    '    avgValue = sum / (pos2 - pos1 + 1)
    '    Return avgValue
    'End Function

    ''' <summary>
    ''' 获取音频时长，单位为秒
    ''' </summary>
    Public Function GetAudioLength() As Single
        Return AudioLength
    End Function

    Public Function IsMono() As Boolean
        If VocalTrackCount = 1 Then Return True
        Return False
    End Function


End Class

