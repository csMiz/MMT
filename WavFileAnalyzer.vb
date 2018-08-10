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
	Private FullFileByteLength as Long
    Private AudioByteLength As Long
	private HeadCursorOffset as integer = 0
    Private FileContent As New List(Of Byte())      '每100000Byte分一次
	private FileContent64 as new list(Of VectorF4)	'每64个样本压缩为1个样本
	private FileContent1024 as new list(Of VectorF4)	'每1024个样本压缩为1个样本

    Private AudioLength As Single       '单位为秒

    Public AddedBlankSecond As Single = 0

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
		HeadCursorOffset = cursorOffset

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
        AddedBlankSecond -= AudioLength
        If AddedBlankSecond < 0 Then AddedBlankSecond = 0
		
		call Compress64()
		call Compress1024()

    End Sub
	
	''' <summary>
	''' 压缩样本64->1
	''' </summary>
	private sub Compress64()
		dim maxCursor as Long = AudioByteLength + 44 + HeadCursorOffset
		Dim blockLength As Byte = VocalTrackCount * 2
		dim cursor as Long = 44 + HeadCursorOffset
		dim compressedCount as integer = 0
		dim compressedBuffer(63) as Vector2
		for i = 0 to 63
			compressedBuffer(i) = new Vector2(0.0F, 0.0F)
		next
		do
			if compressedCount = 64 then
				compressedCount = 0
				dim result as new VectorF4(0.0F, 0.0F, 0.0F, 0.0F)
				for i = 0 to 63
					dim item as Vector2 = compressedBuffer(i)
					if item.X > 0 andalso item.X > result.W then
						result.W = item.X
					elseif item.X < 0 andalso item.X < result.X then 
						result.X = item.X 
					end if
					if item.Y > 0 andalso item.Y > result.Y then
						result.Y = item.Y
					elseif item.Y < 0 andalso item.Y < result.Z then
						result.Z = item.Y
					end if 
				next
				FileContent64.Add(result)
			end if 
			dim leftValue as single = GetSignedTwoBytes(GetByte(cursor, 2))		'只能处理双声道的音频
			dim rightValue as single = GetSignedTwoBytes(GetByte(cursor + 2, 2))
			with compressedBuffer(compressedCount)
				.X = leftValue
				.Y = rightValue
			end with 
			compressedCount += 1
			cursor += blockLength
		loop until cursor >= maxCursor
	end sub 
	
	private sub Compress1024()
		'1024->64->1
		dim compressedCount as integer = 0
		dim compressedBuffer(15) as VectorF4
		if FileContent64.count then
			for each vector as VectorF4 in FileContent64
				if compressedCount = 16 then
					compressedCount = 0
					dim result as new VectorF4(0.0F, 0.0F, 0.0F, 0.0F)
					for i = 0 to 15
					dim item as VectorF4 = compressedBuffer(i)
					if item.W > result.W then result.W = item.W
					if item.X < result.X then result.X = item.X
					if item.Y > result.Y then result.Y = item.Y
					if item.Z < result.Z then result.Z = item.Z	
				next
				FileContent1024.Add(result)
				end if 
				compressedBuffer(compressedCount) = vector
				compressedCount += 1
			next 
		end if 
	end sub 
	
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

    ''' <summary>
    ''' 峰值检测
    ''' </summary>
    Public Function GetTip(SampleStart As Single, SampleEnd As Single) As VectorF4
        Dim blockLength As Byte = VocalTrackCount * 2
        '按块取值，手动校正
        Dim pos1Offset As Byte = (SampleStart * ByteRate) Mod blockLength
        Dim pos2Offset As Byte = (SampleEnd * ByteRate) Mod blockLength
        Dim pos1 As Long = SampleStart * ByteRate + (44 + HeadCursorOffset) + blockLength - pos1Offset
        Dim pos2 As Long = SampleEnd * ByteRate + (44 + HeadCursorOffset) + blockLength - pos2Offset

        Dim maxValueLeft As Single = 0, maxValueRight As Single = 0		'只能处理双声道的音频
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
	
	''' <summary>
	''' 从压缩样本中获取数据
	''' </summary>
	public Function GetCompressedTip(sampleStart as single , sampleEnd as single, compressRate as Integer) as VectorF4
        Dim blockLength As Byte = VocalTrackCount * 2
        Dim compressedStart As Single = sampleStart * ByteRate / (compressRate * blockLength)
        Dim compressedEnd As Single = sampleEnd * ByteRate / (compressRate * blockLength)
        Dim blockStartIndex as long = Math.Floor(compressedStart)
		dim blockEndIndex as long = Math.Floor(compressedEnd)
		dim result as New VectorF4
        If blockEndIndex - 1 >= blockStartIndex Then
            For i = blockStartIndex To blockEndIndex - 1
                Dim item As VectorF4
                If compressRate = 64 Then
                    If i >= FileContent64.Count Then
                        item = New VectorF4(0, 0, 0, 0)
                    Else
                        item = FileContent64(i)
                    End If
                ElseIf compressRate = 1024 Then
                    If i >= FileContent1024.Count Then
                        item = New VectorF4(0, 0, 0, 0)
                    Else
                        item = FileContent1024(i)
                    End If
                Else    '压缩率不可用，返回零值
                    Return result
                End If
                If item.W > result.W Then result.W = item.W
                If item.X < result.X Then result.X = item.X
                If item.Y > result.Y Then result.Y = item.Y
                If item.Z < result.Z Then result.Z = item.Z
            Next
        End If
        Return result
	end function

    ' <summary>
    ' 平均取值
    ' </summary>
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
        Return AudioLength + AddedBlankSecond
    End Function

    Public Function IsMono() As Boolean
        If VocalTrackCount = 1 Then Return True
        Return False
    End Function


End Class

Public Class VectorB4
    Public W As Byte
    Public X As Byte
    Public Y As Byte
    Public Z As Byte

    Public Sub New()
        W = 0
        X = 0
        Y = 0
        Z = 0
    End Sub

    Public Sub New(inputW As Byte, inputX As Byte, inputY As Byte, inputZ As Byte)
        W = inputW
        X = inputX
        Y = inputY
        Z = inputZ
    End Sub

End Class

