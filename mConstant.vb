Module mConstant
    Public ReadOnly DEFAULT_FONT As Font = New Font("Microsoft YaHei", 18)
    Public FRAME_PER_SECOND As Short = 30
    Public ReadOnly GREEN_PEN_2PX As Pen = New Pen(Color.Green) With {.Width = 5}
    Public ReadOnly RED_PEN_2PX As Pen = New Pen(Color.Red) With {.Width = 5}
    Public ReadOnly BLUE_PEN_2PX As Pen = New Pen(Color.CornflowerBlue) With {.Width = 2}
    Public ReadOnly BLUE_BRUSH As Brush = New SolidBrush(Color.CornflowerBlue)
    Public ReadOnly BLACK_BRUSH As Brush = New SolidBrush(Color.Black)
    Public ReadOnly PINK_BRUSH As Brush = New SolidBrush(Color.Pink)

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
        BEZIER = 5

    End Enum
End Module
