Module MainApplication
    Private TEST_MODE As Boolean = False

    Public Sub main()
        If TEST_MODE Then
            Application.EnableVisualStyles()
            Application.Run(Form1)
        Else
            Dim args() As String = System.Environment.GetCommandLineArgs
            If args.Count > 1 Then
                If args(1) = "update_ok" Then
                    Application.EnableVisualStyles()
                    Application.Run(Form1)
                Else
                    Process.Start(Application.StartupPath & "\MMT_Updater.exe")
                    'Shell(Application.StartupPath & "\MMT_Updater.exe")
                End If
			Else
                Process.Start(Application.StartupPath & "\MMT_Updater.exe")
            End If
        End If


    End Sub
End Module
