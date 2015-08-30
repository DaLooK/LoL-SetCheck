Imports System.Windows.Forms

Public Class GatheringItemsDialog
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
        Dim SummonerName As String = Form1.TextBox1.Text.Trim.Replace(" ", "").ToLower
        Dim Region As RegionID = Form1.ComboBox1.SelectedIndex
        Dim SummonerData As Summoner = Await getSummonerDataAsync(SummonerName, Region)
        If SummonerData Is Nothing Then
            If LAN Then
                MessageBox.Show("Ese nombre no fue encontrado (¿seguro que es la región correcta?)", "Nombre invalido! :c", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                MessageBox.Show("We could't found this name (are you on the right region?)", "Invalid name! :c", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Me.Close()
            Exit Sub
        End If
        Dim recentGames As RecentGames = Await getSummonerRecentGamesAsync(SummonerData, Region)
        If (Not recentGames Is Nothing) Then
            Dim onSR As Integer = 0
            For Each Game In recentGames.games
                If wasOnSummonerRift(Game) Then
                    Dim NoRepeatQuery As New Parse.ParseQuery(Of ParseSeedMatch)
                    Dim Save As Boolean = True
                    NoRepeatQuery = NoRepeatQuery.WhereEqualTo("matchId", Game.gameId).WhereEqualTo("championId", Game.championId).WhereEqualTo("summonerId", SummonerData.id)
                    Try
                        Await NoRepeatQuery.FirstAsync
                        Save = False
                    Catch ex As Exception
                    End Try
                    If Save Then
                        Dim newSeedGame As New ParseSeedMatch
                        newSeedGame.matchId = Game.gameId
                        newSeedGame.championId = Game.championId
                        newSeedGame.summonerId = SummonerData.id
                        newSeedGame.tier = SummonerData.tier
                        newSeedGame.items = New List(Of Integer)
                        If (Game.stats.item0 > 0) Then
                            newSeedGame.items.Add(Game.stats.item0)
                        End If
                        If (Game.stats.item1 > 0) Then
                            newSeedGame.items.Add(Game.stats.item1)
                        End If
                        If (Game.stats.item2 > 0) Then
                            newSeedGame.items.Add(Game.stats.item2)
                        End If
                        If (Game.stats.item3 > 0) Then
                            newSeedGame.items.Add(Game.stats.item3)
                        End If
                        If (Game.stats.item4 > 0) Then
                            newSeedGame.items.Add(Game.stats.item4)
                        End If
                        If (Game.stats.item5 > 0) Then
                            newSeedGame.items.Add(Game.stats.item5)
                        End If
                        If (Game.stats.item6 > 0) Then
                            newSeedGame.items.Add(Game.stats.item6)
                        End If
                        Dim success As Boolean = False
                        Do
                            Try
                                Await newSeedGame.SaveAsync
                                success = True
                            Catch ex As Exception
                            End Try
                        Loop Until success
                    End If
                End If
            Next
        End If

        Dim mMatchHistory As List(Of SeedMatch) = Await getSummonerRecentMatchHistory(SummonerData, Region)
        If (Not mMatchHistory Is Nothing) Then
            For Each recentGame In mMatchHistory
                Dim NoRepeatQuery As New Parse.ParseQuery(Of ParseSeedMatch)
                Dim Save As Boolean = True
                NoRepeatQuery = NoRepeatQuery.WhereEqualTo("matchId", recentGame.matchId).WhereEqualTo("championId", recentGame.participants(0).championId).WhereEqualTo("summonerId", SummonerData.id)
                Try
                    Await NoRepeatQuery.FirstAsync
                    Save = False
                Catch ex As Exception
                End Try
                If Save Then
                    Dim newSeedGame As New ParseSeedMatch
                    newSeedGame.matchId = recentGame.matchId
                    newSeedGame.championId = recentGame.participants(0).championId
                    newSeedGame.summonerId = SummonerData.id
                    newSeedGame.tier = SummonerData.tier
                    newSeedGame.items = New List(Of Integer)
                    If (recentGame.participants(0).stats.item0 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item0)
                    End If
                    If (recentGame.participants(0).stats.item1 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item1)
                    End If
                    If (recentGame.participants(0).stats.item2 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item2)
                    End If
                    If (recentGame.participants(0).stats.item3 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item3)
                    End If
                    If (recentGame.participants(0).stats.item4 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item4)
                    End If
                    If (recentGame.participants(0).stats.item5 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item5)
                    End If
                    If (recentGame.participants(0).stats.item6 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item6)
                    End If
                    Dim success As Boolean = False
                    Do
                        Try
                            Await newSeedGame.SaveAsync
                            success = True
                        Catch ex As Exception
                        End Try
                    Loop Until success
                End If
            Next
        End If
        mMatchHistory = Nothing
        mMatchHistory = Await getSummonerRecentMatchHistory(SummonerData, Region, mahChamp.id)
        If (Not mMatchHistory Is Nothing) Then
            For Each recentGame In mMatchHistory
                Dim NoRepeatQuery As New Parse.ParseQuery(Of ParseSeedMatch)
                Dim Save As Boolean = True
                NoRepeatQuery = NoRepeatQuery.WhereEqualTo("matchId", recentGame.matchId).WhereEqualTo("championId", recentGame.participants(0).championId).WhereEqualTo("summonerId", SummonerData.id)
                Try
                    Await NoRepeatQuery.FirstAsync
                    Save = False
                Catch ex As Exception
                End Try
                If Save Then
                    Dim newSeedGame As New ParseSeedMatch
                    newSeedGame.matchId = recentGame.matchId
                    newSeedGame.championId = recentGame.participants(0).championId
                    newSeedGame.summonerId = SummonerData.id
                    newSeedGame.tier = SummonerData.tier
                    newSeedGame.items = New List(Of Integer)
                    If (recentGame.participants(0).stats.item0 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item0)
                    End If
                    If (recentGame.participants(0).stats.item1 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item1)
                    End If
                    If (recentGame.participants(0).stats.item2 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item2)
                    End If
                    If (recentGame.participants(0).stats.item3 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item3)
                    End If
                    If (recentGame.participants(0).stats.item4 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item4)
                    End If
                    If (recentGame.participants(0).stats.item5 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item5)
                    End If
                    If (recentGame.participants(0).stats.item6 > 0) Then
                        newSeedGame.items.Add(recentGame.participants(0).stats.item6)
                    End If
                    Dim success As Boolean = False
                    Do
                        Try
                            Await newSeedGame.SaveAsync
                            success = True
                        Catch ex As Exception
                        End Try
                    Loop Until success
                End If
            Next
        End If
        Dim SummonerQuery As New Parse.ParseQuery(Of ParseSeedMatch)
        SummonerQuery = SummonerQuery.WhereEqualTo("summonerId", SummonerData.id).WhereEqualTo("championId", mahChamp.id).Limit(1000)
        My.Computer.Clipboard.SetText("{where={""summonerId"":" & SummonerData.id & ",""championId"":" & mahChamp.id & "}")
        Dim Builds = Await SummonerQuery.FindAsync
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
            RiotAPI.currentlyShowingAttribs.showingLeague = False
            RiotAPI.currentlyShowingAttribs.summonerShowing = SummonerData
            RiotAPI.currentlyShowingAttribs.itemsSetsShowing = itemSets
            For Each itemSet In itemSets
                Dim shopRow As ShopRow = MakeShopRow(itemSet, itemSet.Name)
                shopRow.Location = New Point(0, Form1.Panel1.Controls.Count * shopRow.Size.Height + 10)
                Form1.Panel1.Controls.Add(shopRow)
            Next
            Form1.GroupBox3.Enabled = True
            Form1.GroupBox4.Controls.Clear()
            Dim currentlyshowingpanel As New CurrentlyShowingSummoner
            Dim champImagesDir As String = FileIO.FileSystem.CombinePath(RiotAPI.QuackDirectory, "champs")
            currentlyshowingpanel.PictureBox2.Image = Drawing.Image.FromFile(FileIO.FileSystem.CombinePath(champImagesDir, mahChamp.image.GetValue("full").ToString))
            currentlyshowingpanel.Label3.Text = SummonerData.name
            currentlyshowingpanel.Location = New Point(6, 19)
            Form1.GroupBox4.Controls.Add(currentlyshowingpanel)
        End If
        Me.Close()
        Form1.Focus()
    End Sub
End Class
