Imports Parse

Namespace My

    ' The following events are available for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(sender As Object, e As ApplicationServices.StartupEventArgs) Handles Me.Startup
            ParseObject.RegisterSubclass(Of RiotAPI.ParseSeedMatch)()
            ParseClient.Initialize("Dt0FYaSbJxZLF9sBpEmLSbwOlIHl3yu92N2AnpU2", "k52nqvUmp7W8Wk7pnMQuLQUMXioc1PaVPKWMp47T")
            RiotAPI.QuackDirectory =
                  FileIO.FileSystem.CombinePath(My.Application.Info.DirectoryPath, "assets\")
        End Sub
    End Class


End Namespace

