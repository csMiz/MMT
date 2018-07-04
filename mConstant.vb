Module mConstant
    Public ReadOnly DefaultFont As Font = New Font("Microsoft YaHei", 18)

    Public Enum SaveFileParam As Byte
        SaveNew = 0
        VMDTest = 1
        Save = 2

    End Enum

    Public Enum eExeMode As Byte
        None = 0
        VMD = 1
        PMX = 2
        PinyinManual = 3

    End Enum
End Module
