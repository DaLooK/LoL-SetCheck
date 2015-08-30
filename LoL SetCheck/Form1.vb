Imports System.Globalization
Imports System.Threading
Imports Parse
Imports Newtonsoft.Json

Public Class Form1
    Implements RiotAPI.ConseguirKeyListener, RiotAPI.GetAssetsListener
    Dim fullChampList As Array


    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ConseguirKey(Me)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.SelectedIndex = 4
        ComboBox2.SelectedIndex = 0
    End Sub


    Public Sub ConseguirKeyFinished(Success As Boolean) Implements ConseguirKeyListener.ConseguirKeyFinished
        If Success Then
            GetAllTheAssets(Me)
        Else
            If LAN Then
                MessageBox.Show("No se pudo conseguir la clave para las conexiones." + vbCrLf + "Se cerrara la aplicación ya que no servira de nada asi." + vbCrLf + "Verifíca tu conexión a internet e intentalo de nuevo.", "Lo siento", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                MessageBox.Show("We couldn't get the key for the requests." + vbCrLf + "The application will close because it can't be used this way." + vbCrLf + "Check your internet connection and try again later.", "I'm sorry", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub

    Public Sub GetAssetsFinished() Implements GetAssetsListener.GetAssetsFinished
        Dim orderedChamps As Array = champs.Values.ToArray
        Array.Sort(orderedChamps)
        ListView1.SmallImageList = New ImageList()
        For Each champImage In FileIO.FileSystem.GetFiles(FileIO.FileSystem.CombinePath(QuackDirectory, "champs"))
            ListView1.SmallImageList.Images.Add(FileIO.FileSystem.GetFileInfo(champImage).Name, System.Drawing.Image.FromFile(champImage))
        Next
        fullChampList = orderedChamps
        FilterChamps("")
        enableControls()
    End Sub


    Public Sub FilterChamps(FilterString As String)
        ListView1.Items.Clear()
        For Each champ As Champion In fullChampList
            If (champ.name.ToLower.Contains(FilterString.ToLower)) Then
                Dim newChampListItem As ListViewItem = New ListViewItem(champ.name, champ.image.GetValue("full").ToString)
                newChampListItem.Tag = champ.key
                ListView1.Items.Add(newChampListItem)
            End If
        Next
    End Sub

    Public Sub ShowDownloadingDialog() Implements GetAssetsListener.ShowDownloadingDialog
        DownloadingDialog.ShowDialog()
    End Sub

    Sub enableControls()
        GroupBox1.Enabled = True
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        FilterChamps(TextBox2.Text.Trim)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TabControl1.SelectedIndex = 0 Then
            If Not String.IsNullOrWhiteSpace(TextBox1.Text) And ListView1.SelectedIndices.Count = 1 Then
                GatheringItemsDialog.ShowDialog()
            Else
                If LAN Then
                    MessageBox.Show("Escribe un nombre y selecciona un cameón.", "HEY!", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                Else
                    MessageBox.Show("Type a name and pick a champion", "HEY!", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                End If
            End If
        Else
            If ListView1.SelectedIndices.Count = 1 Then
                GatheringItemsFromLeaguesDialog.ShowDialog()
            Else
                If LAN Then
                    MessageBox.Show("Selecciona un cameón.", "HEY!", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                Else
                    MessageBox.Show("Pick a champion", "HEY!", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                End If
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If currentlyShowingAttribs.itemsSetsShowing.Count > 0 Then
            If FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
                Dim newFile As New FileItemSet
                If currentlyShowingAttribs.showingLeague Then
                    newFile.title = currentlyShowingAttribs.tierShowing
                Else
                    newFile.title = currentlyShowingAttribs.summonerShowing.name
                End If
                For Each block In currentlyShowingAttribs.itemsSetsShowing
                    Dim newBlock As New FileItemBlock
                    newBlock.type = block.Name
                    For Each blockItem In block.Items
                        Dim newItem As New FileItem
                        newItem.id = blockItem.id
                        newBlock.items.Add(newItem)
                    Next
                    newFile.blocks.Add(newBlock)
                Next
                Dim ChampDir = FileIO.FileSystem.CombinePath(FolderBrowserDialog1.SelectedPath, "Champions")
                ChampDir = FileIO.FileSystem.CombinePath(ChampDir, currentlyShowingAttribs.championShowing.key)
                ChampDir = FileIO.FileSystem.CombinePath(ChampDir, "Recommended")
                If Not FileIO.FileSystem.DirectoryExists(ChampDir) Then
                    Dim QuestionResult As Windows.Forms.DialogResult
                    If LAN Then
                        QuestionResult = MessageBox.Show("No se encontro la carpeta del campeón seleccionado" + vbCrLf + "¿Deseas guardar de todos modos?", "Ocurrio un problema", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                    Else
                        QuestionResult = MessageBox.Show("We couldn't found the expected champion folder" + vbCrLf + "Would you like to save anyway?", "There was a problem", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    End If
                    If QuestionResult = Windows.Forms.DialogResult.No Then
                        Exit Sub
                    End If
                    FileIO.FileSystem.CreateDirectory(ChampDir)
                End If
                Dim ItemSetFilePath = FileIO.FileSystem.CombinePath(ChampDir, currentlyShowingAttribs.championShowing.key & "_" & IIf(currentlyShowingAttribs.showingLeague, currentlyShowingAttribs.tierShowing.ToLower, currentlyShowingAttribs.summonerShowing.id) & ".json")
                FileIO.FileSystem.WriteAllText(ItemSetFilePath, JsonConvert.SerializeObject(newFile), False, System.Text.Encoding.Default)
                If LAN Then
                    MessageBox.Show("EXITO!!!", "Felicidades!!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification)
                Else
                    MessageBox.Show("SUCESS!!!", "Congratulations!!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification)
                End If
                Me.Focus()
            End If
            End If
    End Sub

End Class
