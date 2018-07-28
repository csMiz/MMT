Module mConstant
    Public ReadOnly DEFAULT_FONT As Font = New Font("Microsoft YaHei", 18)



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
