Imports System.Windows.Forms
Imports Parse

Public Class GatheringItemsFromLeaguesDialog
    Private Const filterCore As Boolean = True
    Private Const filterBoots As Boolean = True
    Private Const filterTrinkets As Boolean = True
    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        DoTheJob()
    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        ControlBox = False
        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Async Sub DoTheJob()
        Dim Tier As Integer = Form1.ComboBox2.SelectedIndex
        Dim mahChamp As Champion
        If Not champs.TryGetValue(Form1.ListView1.SelectedItems(0).Tag, mahChamp) Then
            If RiotAPI.LAN Then
                MessageBox.Show("Algo salio terriblemente mal." + vbCrLf + "Abortando!!", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification)
            Else
                MessageBox.Show("Something went horribly wrong." + vbCrLf + "Aborting!!", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification)
            End If
            Me.Close()
            Exit Sub
        End If
        Dim ChampId As Integer = mahChamp.id
        Dim LeaguesQuery As New Parse.ParseQuery(Of ParseSeedMatch)
        LeaguesQuery = LeaguesQuery.WhereEqualTo("tier", Tier).WhereEqualTo("championId", ChampId).Limit(1000)
        Dim Builds = Await LeaguesQuery.FindAsync
        If Builds.Count = 0 Then
            If LAN Then
                MessageBox.Show("No se encontro lo que buscabas, probablemente no existe.")
            Else
                MessageBox.Show("We could't find what you're looking for, probably it doesn't exists.")
            End If
        Else
            Dim itemSets As New List(Of ItemSet)
            Dim CoreItemStatistics As New Dictionary(Of Integer, Integer)
            Dim BootsStatistics As New Dictionary(Of Integer, Integer)
            Dim TrinketStatistics As New Dictionary(Of Integer, Integer)
            For Each seedMatch As ParseSeedMatch In Builds
                For Each itemId In seedMatch.items
                    Try
                        Dim item As Item = items(itemId)
                        While Not item.inStore
                            items.TryGetValue(item.from.First, item)
                        End While
                        If item.id = 2043 Or item.id = 2044 Or item.id = 1054 Or item.id = 1055 Or item.id = 1056 Or item.tags.Contains("Consumable") Then
                        ElseIf item.group.Contains("Boots") Then
                            If item.group.Contains("Normal") Then
                                If BootsStatistics.ContainsKey(item.id) Then
                                    BootsStatistics(item.id) += 1
                                Else
                                    BootsStatistics.Add(item.id, 1)
                                End If
                            Else
                                If BootsStatistics.ContainsKey(item.from(0)) Then
                                    BootsStatistics(item.from(0)) += 1
                                Else
                                    BootsStatistics.Add(item.from(0), 1)
                                End If
                            End If
                        ElseIf item.tags.Contains("Boots") Then
                            If BootsStatistics.ContainsKey(item.id) Then
                                BootsStatistics(item.id) += 1
                            Else
                                BootsStatistics.Add(item.id, 1)
                            End If
                        ElseIf item.tags.Contains("Trinket") Then
                            If TrinketStatistics.ContainsKey(item.id) Then
                                TrinketStatistics(item.id) += 1
                            Else
                                TrinketStatistics.Add(item.id, 1)
                            End If
                        Else
                            If CoreItemStatistics.ContainsKey(item.id) Then
                                CoreItemStatistics(item.id) += 1
                            Else
                                CoreItemStatistics.Add(item.id, 1)
                            End If
                        End If
                    Catch ex As Exception
                    End Try
                Next
            Next
            If filterCore Then
                Dim newStatistics As New Dictionary(Of Integer, Integer)
                For Each ItemStatistic In CoreItemStatistics
                    Dim topItem As Item = items(ItemStatistic.Key)
                    Dim skip As Boolean = False
                    For Each intoItem In topItem.into
                        If (items(intoItem).inStore) Then
                            If (newStatistics.ContainsKey(intoItem)) Then
                                skip = True
                                Exit For
                            End If
                        End If
                    Next
                    If Not skip Then
                        Dim newCount = ItemStatistic.Value
                        Dim childs As New List(Of Integer)
                        For Each child In topItem.from
                            If (Not childs.Contains(child)) Then
                                childs.Add(child)
                                For Each child2 In items(child).from
                                    If (Not childs.Contains(child2)) Then
                                        childs.Add(child2)
                                    End If
                                Next
                            End If
                        Next
                        For Each child In childs
                            If (CoreItemStatistics.Keys.Contains(child)) Then
                                newCount += CoreItemStatistics(child)
                            End If
                        Next
                        newStatistics.Add(ItemStatistic.Key, newCount)
                    End If
                Next
                Dim tempStatistics As New Dictionary(Of Integer, Integer)
                For Each newStatistic In newStatistics
                    If items(newStatistic.Key).into.Count = 1 Then
                        Dim newitem = items(items(newStatistic.Key).into(0))
                        If newitem.inStore Then
                            If (tempStatistics.ContainsKey(newitem.id)) Then
                                tempStatistics(newitem.id) += newStatistic.Value
                            Else
                                tempStatistics.Add(newitem.id, newStatistic.Value)
                            End If
                            tempStatistics.Remove(newStatistic.Key)
                        End If
                    Else
                        If (tempStatistics.ContainsKey(newStatistic.Key)) Then
                            tempStatistics(newStatistic.Key) += newStatistic.Value
                        Else

                            tempStatistics.Add(newStatistic.Key, newStatistic.Value)
                        End If
                    End If
                Next
                CoreItemStatistics = tempStatistics
            End If
            If filterBoots Then
                If BootsStatistics.Keys.Count > 1 Then
                    Dim newStatistics As New Dictionary(Of Integer, Integer)
                    For Each pairOfBoots In BootsStatistics
                        If pairOfBoots.Key <> 1001 Then
                            newStatistics.Add(pairOfBoots.Key, pairOfBoots.Value)
                        End If
                    Next
                    BootsStatistics = newStatistics
                End If
            End If
            If filterTrinkets Then
                Dim newStatistics As New Dictionary(Of Integer, Integer)
                For Each ItemStatistic In TrinketStatistics
                    Dim Trinket As Item = items(ItemStatistic.Key)
                    If Trinket.depth = 1 Then
                        For Each BigTrinketId In Trinket.into
                            If (newStatistics.ContainsKey(BigTrinketId)) Then
                                newStatistics(BigTrinketId) += 1
                            Else
                                newStatistics.Add(BigTrinketId, 1)
                            End If
                        Next
                    Else
                        If (newStatistics.ContainsKey(ItemStatistic.Key)) Then
                            newStatistics(ItemStatistic.Key) += ItemStatistic.Value
                        Else
                            newStatistics.Add(ItemStatistic.Key, ItemStatistic.Value)
                        End If
                    End If
                Next
                TrinketStatistics = newStatistics
            End If
            Dim sorted = From pair In CoreItemStatistics
             Order By pair.Value Descending
            Dim sortedCores = sorted.ToDictionary(Function(p) p.Key, Function(p) p.Value)
            sorted = From pair In BootsStatistics
             Order By pair.Value Descending
            Dim sortedBoots = sorted.ToDictionary(Function(p) p.Key, Function(p) p.Value)
            sorted = From pair In TrinketStatistics
             Order By pair.Value Descending
            Dim sortedTrinkets = sorted.ToDictionary(Function(p) p.Key, Function(p) p.Value)
            Dim newItemSet As New ItemSet
            Dim limiter As Integer = 7
            For Each possibleItem In sortedCores
                newItemSet.Items.Add(items(possibleItem.Key))
                limiter -= 1
                If (limiter <= 0) Then
                    Exit For
                End If
            Next
            newItemSet.Name = ItemSetLabels(0)
            itemSets.Add(newItemSet)
            newItemSet = New ItemSet
            limiter = 7
            For Each possibleItem In sortedBoots
                newItemSet.Items.Add(items(possibleItem.Key))
                limiter -= 1
                If (limiter <= 0) Then
                    Exit For
                End If
            Next
            newItemSet.Name = ItemSetLabels(1)
            itemSets.Add(newItemSet)
            newItemSet = New ItemSet
            limiter = 7
            For Each possibleItem In sortedTrinkets
                newItemSet.Items.Add(items(possibleItem.Key))
                limiter -= 1
                If (limiter <= 0) Then
                    Exit For
                End If
            Next
            newItemSet.Name = ItemSetLabels(2)
            itemSets.Add(newItemSet)
            Form1.Panel1.Controls.Clear()
            RiotAPI.currentlyShowingAttribs = New ShowingAttribs
            RiotAPI.currentlyShowingAttribs.championShowing = mahChamp
            RiotAPI.currentlyShowingAttribs.showingLeague = True
            RiotAPI.currentlyShowingAttribs.itemsSetsShowing = itemSets
            RiotAPI.currentlyShowingAttribs.tierShowing = Form1.ComboBox2.Text
            Dim nameIndex = 0
            For Each itemSet In itemSets
                Dim shopRow As ShopRow = MakeShopRow(itemSet, itemSet.Name)
                shopRow.Location = New Point(0, Form1.Panel1.Controls.Count * shopRow.Size.Height + 10)
                Form1.Panel1.Controls.Add(shopRow)
            Next
            Form1.GroupBox3.Enabled = True
            Form1.GroupBox4.Controls.Clear()
            Dim currentlyshowingpanel As New CurrentlyShowingLeague
            Select Case Tier
                Case RankedTier.UNRANKED
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.provisional
                Case RankedTier.BRONZE
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.bronze
                Case RankedTier.SILVER
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.silver
                Case RankedTier.GOLD
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.gold
                Case RankedTier.PLATINUM
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.platinum
                Case RankedTier.DIAMOND
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.diamond
                Case RankedTier.MASTER
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.master
                Case RankedTier.CHALLENGER
                    currentlyshowingpanel.PictureBox1.Image = My.Resources.challenger
            End Select
            Dim champImagesDir As String = FileIO.FileSystem.CombinePath(RiotAPI.QuackDirectory, "champs")
            currentlyshowingpanel.PictureBox2.Image = Drawing.Image.FromFile(FileIO.FileSystem.CombinePath(champImagesDir, mahChamp.image.GetValue("full").ToString))
            currentlyshowingpanel.Location = New Point(6, 19)
            Form1.GroupBox4.Controls.Add(currentlyshowingpanel)
        End If
        Me.Close()
        Form1.Focus()
    End Sub

   


End Class
