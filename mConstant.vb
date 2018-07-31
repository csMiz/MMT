Module mConstant
    Public ReadOnly DEFAULT_FONT As Font = New Font("Microsoft YaHei", 18)
    Public FRAME_PER_SECOND As Short = 30
    Public ReadOnly GREEN_PEN_2PX As Pen = New Pen(Color.Green) With {.Width = 5}

    ''' <summary>
    ''' 当前版本号
    ''' </summary>
    Public Const ME_VERSION As String = "1.0.11"


    Public Enum SaveFileParam As Byte
        SAVENEW = 0
        VMDTEST = 1
        SAVE = 2

    End Enum

    Public Enum eExeMode As Byte
        NONE = 0
        VMD = 1
        PMX = 2
        PINYIN_MANUAL = 3
        WAV = 4

    End Enum
End Module
