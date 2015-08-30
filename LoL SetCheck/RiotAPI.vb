Imports Parse
Imports Newtonsoft.Json
Module RiotAPI
    Public LAN As Boolean = System.Threading.Thread.CurrentThread.CurrentUICulture.Equals(System.Globalization.CultureInfo.GetCultureInfo("es-419"))
    Public champs As IDictionary(Of String, Champion)
    Public items As IDictionary(Of Integer, Item)
    Public QuackDirectory As String
    Public dataDragon As Linq.JObject

    Public ItemSetLabels As String() =
       IIf(LAN,
        New String() {"Items Principales", "Botas", "Baratija"},
       New String() {"Main Items", "Boots", "Trinket"})
    
    Public currentlyShowingAttribs As New ShowingAttribs

    Class ShowingAttribs
        Public itemsSetsShowing As New List(Of ItemSet)
        Public championShowing As New Champion
        Public summonerShowing As New Summoner
        Public tierShowing As String = "Unranked"
        Public showingLeague As Boolean = False
    End Class

    Enum RegionID
        BR
        EUNE
        EUW
        KR
        LAN
        LAS
        NA
        OCE
        TR
        RU
        PBE
    End Enum

    Enum RankedTier
        UNRANKED
        BRONZE
        SILVER
        GOLD
        PLATINUM
        DIAMOND
        MASTER
        CHALLENGER
    End Enum



    Async Sub getSummonerData(SummonerName As String, Region As RegionID)
        If Not String.IsNullOrEmpty(My.Settings.Key) Then
            Dim regionString As String = Region.ToString.ToLower
            Dim res As String = Await GetPageString("https://lan.api.pvp.net/api/lol/" & regionString & "/v1.4/summoner/by-name/" & SummonerName & "?api_key=" & My.Settings.Key)
            Dim data As Dictionary(Of String, Summoner) = Await Task.Factory.StartNew(Function() As Dictionary(Of String, Summoner)
                                                                                          Return JsonConvert.DeserializeObject(Of Dictionary(Of String, Summoner))(res)
                                                                                      End Function)
            Dim mSummoner As Summoner = data.First.Value
        End If
    End Sub

    Interface getSummonerDataListener
        Sub getSummonerDataFinished(Res As Summoner)
    End Interface

    Async Sub ConseguirKey(Optional listener As ConseguirKeyListener = Nothing)
        Try
            Dim kk As Parse.ParseQuery(Of ParseObject) = New ParseQuery(Of ParseObject)("MahKey")
            Dim r As ParseObject = Await kk.FirstAsync
            If Not r Is Nothing Then
                My.Settings.Key = r.Get(Of String)("Key")
                My.Settings.Save()
            End If
            listener.ConseguirKeyFinished(True)
        Catch ex As Exception
            listener.ConseguirKeyFinished(False)
        End Try
    End Sub

    Interface ConseguirKeyListener
        Sub ConseguirKeyFinished(Success As Boolean)
    End Interface

    Async Function getSummonerDataAsync(SummonerName As String, Region As RegionID) As Task(Of Summoner)
        If Not String.IsNullOrEmpty(My.Settings.Key) Then
            Dim regionString As String = Region.ToString.ToLower
            Dim res As String = Await GetPageString("https://" & regionString & ".api.pvp.net/api/lol/" & regionString & "/v1.4/summoner/by-name/" & SummonerName & "?api_key=" & My.Settings.Key)
            If res Is Nothing Then
                Return Nothing
            End If
            Dim data As Dictionary(Of String, Summoner) = Await Task.Factory.StartNew(Function() As Dictionary(Of String, Summoner)
                                                                                          Return JsonConvert.DeserializeObject(Of Dictionary(Of String, Summoner))(res)
                                                                                      End Function)
            Dim mSummoner = data.First.Value
            res = Await GetPageString("https://" & regionString & ".api.pvp.net/api/lol/" & regionString & "/v2.5/league/by-summoner/" & mSummoner.id & "/entry?api_key=" & My.Settings.Key)
            If res Is Nothing Then
                mSummoner.tier = [Enum].Parse(GetType(RankedTier), "UNRANKED", True)
                Return mSummoner
            End If
            Dim Leagues = Await Task.Factory.StartNew(Function() As List(Of League)
                                                          Return JsonConvert.DeserializeObject(Of List(Of League))(Linq.JObject.Parse(res).GetValue(mSummoner.id).ToString)
                                                      End Function)
            For Each League In Leagues
                If League.queue.Contains("SOLO") Then
                    mSummoner.tier = [Enum].Parse(GetType(RankedTier), League.tier, True)
                    Exit For
                End If
            Next
            If String.IsNullOrEmpty(mSummoner.tier) Then
                mSummoner.tier = [Enum].Parse(GetType(RankedTier), "UNRANKED", True)
            End If
            Return mSummoner
        End If
        Return Nothing
    End Function

    Async Function getSummonerRecentGamesAsync(Summoner As Summoner, Region As RegionID) As Task(Of RecentGames)
        If Not String.IsNullOrEmpty(My.Settings.Key) Then
            Dim regionString As String = Region.ToString.ToLower
            
            Dim res As String = Await GetPageString("https://" & regionString & ".api.pvp.net/api/lol/" & regionString & "/v1.3/game/by-summoner/" & Summoner.id & "/recent?api_key=" & My.Settings.Key)
            If res Is Nothing Then
                Return Nothing
            End If
            Dim mRecentGames As RecentGames = Await Task.Factory.StartNew(Function() As RecentGames
                                                                              Return JsonConvert.DeserializeObject(Of RecentGames)(res)
                                                                          End Function)
            Return mRecentGames
        End If
        Return Nothing
    End Function

    Async Function getSummonerRecentMatchHistory(Summoner As Summoner, Region As RegionID, Optional ChampionId As String = "") As Task(Of List(Of SeedMatch))
        If Not String.IsNullOrEmpty(My.Settings.Key) Then
            Dim regionString As String = Region.ToString.ToLower
            Dim champQuery As String = ""
            If Not String.IsNullOrWhiteSpace(ChampionId) Then
                champQuery = "championIds=" & ChampionId & "&"
            End If
            Dim res As String = Await GetPageString("https://" & regionString & ".api.pvp.net/api/lol/" & regionString & "/v2.2/matchhistory/" & Summoner.id & "?" & champQuery & "rankedQueues=RANKED_SOLO_5x5,RANKED_TEAM_5x5")
            If res Is Nothing Then
                Return Nothing
            End If
            Dim matchHistoryWrapper As Linq.JObject = Linq.JObject.Parse(res)
            Dim matchesValue As Linq.JToken

            If Not matchHistoryWrapper.TryGetValue("matches", matchesValue) Then
                Return Nothing
            End If
           

            Dim mRecentGames As List(Of SeedMatch) =
             Await Task.Factory.StartNew(Function() As List(Of SeedMatch)
                                             Return JsonConvert.DeserializeObject(Of List(Of SeedMatch))(matchesValue.ToString)
                                         End Function)
            Return mRecentGames
        End If
        Return Nothing
    End Function

    Function wasOnSummonerRift(Game As Game) As Boolean
        If Game.gameType.Contains("MATCHED") Then
            If Game.subType.ToLower.Equals("normal") Or Game.subType.ToLower.Equals("ranked_team_5x5") Or Game.subType.ToLower.Equals("ranked_solo_5x5") Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function CompareItemSets(itemSet1 As ItemSet, itemSet2 As ItemSet, ByRef newItemSet As ItemSet) As Integer
        newItemSet = New ItemSet
        For Each item1 In itemSet1.Items
            If (itemSet2.Items.Contains(item1)) Then
                newItemSet.Items.Add(item1)
            Else
                For Each item2 In itemSet2.Items
                    If (itemSet1.Items.Contains(item2)) Then
                        newItemSet.Items.Add(item2)
                    Else
                    End If
                Next
            End If
        Next
    End Function

    Public Function MakeShopRow(itemSet As ItemSet, Optional name As String = ":)") As ShopRow
        Dim shopRow As New ShopRow()
        Dim itemsDir = FileIO.FileSystem.CombinePath(QuackDirectory, "items")
        Dim itemsPut As Integer = 0
        For Each item In itemSet.Items
            CType(shopRow.Controls.Item(7 - itemsPut), PictureBox).Image = System.Drawing.Image.FromFile(FileIO.FileSystem.CombinePath(itemsDir, item.image.GetValue("full").ToString))
            itemsPut += 1
        Next
        shopRow.Label1.Text = name
        Return shopRow
    End Function

    Class Summoner
        Public id As Integer
        Public name As String
        Public profileIconId As Integer
        Public tier As RankedTier
    End Class

    Class Champion
        Implements IComparable

        Public id As Integer
        Public name As String
        Public image As Linq.JObject
        Public key As String

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            End If
            Dim otro As Champion = CType(obj, Champion)
            If otro.name < Me.name Then
                Return 1
            ElseIf otro.name > Me.name Then
                Return -1
            Else
                Return 0
            End If
        End Function
    End Class

    Class Item
        Public id As Integer
        Public name As String
        Public image As Linq.JObject
        Public into As New List(Of String)
        Public from As New List(Of String)
        Public depth As Integer = 1
        Public tags As New List(Of String)
        Public inStore As Boolean = True
        Public group As String = ""
    End Class

    Class ItemSet
        Public Items As New List(Of Item)
        Public Name As String = ":)"
    End Class

    Class League
        Public tier As String
        Public queue As String
    End Class

    Class Game
        Public gameId As Long
        Public championId As Integer
        Public mapId As Integer
        Public gameType As String
        Public subType As String
        Public stats As SeedParticipantStats
    End Class

    Class RecentGames
        Public summonerId As Long
        Public games As New List(Of Game)
    End Class

    Async Sub GetAllTheAssets(listener As GetAssetsListener)
        If Not String.IsNullOrEmpty(My.Settings.Key) Then
            Dim WC As Net.WebClient = New Net.WebClient()
            WC.Headers.Add("X-Riot-Token", My.Settings.Key)
            WC.Encoding = System.Text.Encoding.UTF8
            Dim res As String = Nothing
            Do
                res = Await GetPageString("https://global.api.pvp.net/api/lol/static-data/" & IIf(LAN, "lan", "na") & "/v1.2/champion?champData=image")
            Loop Until Not res Is Nothing
            Dim wrapper As Linq.JObject = Linq.JObject.Parse(res)
            champs = Await Task.Factory.StartNew(Function() As Dictionary(Of String, Champion)
                                                     Return JsonConvert.DeserializeObject(Of Dictionary(Of String, Champion))(wrapper.GetValue("data").ToString)
                                                 End Function)
            res = Nothing
            Do
                Try
                    res = Await WC.DownloadStringTaskAsync("https://global.api.pvp.net/api/lol/static-data/" & IIf(LAN, "lan", "na") & "/v1.2/item?itemListData=all")

                Catch ex As Exception

                End Try
                If res Is Nothing Then
                    Await Task.Delay(1000)
                End If
            Loop Until Not res Is Nothing
            wrapper = Linq.JObject.Parse(res)
            items = Await Task.Factory.StartNew(Function() As Dictionary(Of Integer, Item)
                                                    Return JsonConvert.DeserializeObject(Of Dictionary(Of Integer, Item))(wrapper.GetValue("data").ToString)
                                                End Function)
            Do

                Try

                    res = Await WC.DownloadStringTaskAsync("https://global.api.pvp.net/api/lol/static-data/" & IIf(LAN, "lan", "na") & "/v1.2/realm?api_key=" & My.Settings.Key)
                Catch ex As Exception

                End Try
                If res Is Nothing Then
                    Await Task.Delay(1000)
                End If
            Loop Until Not res Is Nothing
            dataDragon = Linq.JObject.Parse(res)
            If (My.Settings.LastVersionFetched <> wrapper.GetValue("version")) Then
                If dataDragon.GetValue("dd").ToString = wrapper.GetValue("version") Or String.IsNullOrEmpty(My.Settings.LastVersionFetched) Then
                    listener.ShowDownloadingDialog()

                End If
            Else
                Dim assetsDirectory As String = FileIO.FileSystem.CombinePath(QuackDirectory, "champs")
                If FileIO.FileSystem.DirectoryExists(assetsDirectory) Then
                    If FileIO.FileSystem.GetFiles(assetsDirectory).Count <> champs.Values.Count Then
                        listener.ShowDownloadingDialog()
                    Else
                        assetsDirectory = FileIO.FileSystem.CombinePath(QuackDirectory, "items")
                        If FileIO.FileSystem.DirectoryExists(assetsDirectory) Then
                            If FileIO.FileSystem.GetFiles(assetsDirectory).Count <> items.Values.Count Then
                                listener.ShowDownloadingDialog()
                            End If
                        Else
                            listener.ShowDownloadingDialog()
                        End If
                    End If
                Else
                    listener.ShowDownloadingDialog()
                End If
            End If
        End If
        listener.GetAssetsFinished()
    End Sub

    Interface GetAssetsListener
        Sub ShowDownloadingDialog()
        Sub GetAssetsFinished()
    End Interface



    'I'll left this just in case..
    Async Sub UpdateSeedingsFromAWS()
        For index = 1 To 10
            Dim res As String = Nothing
            Do
                res = Await GetPageString("https://s3-us-west-1.amazonaws.com/riot-api/seed_data/matches" & index & ".json")
            Loop While res Is Nothing
            Dim seedWrapper As SeedWrapper = Await Task.Factory.StartNew(Function() As SeedWrapper
                                                                             Return JsonConvert.DeserializeObject(Of SeedWrapper)(res)
                                                                         End Function)
            For Each seedGame In seedWrapper.matches
                For Each SeedParticipant In seedGame.participants
                    Dim save As Boolean = True
                    Dim query As New ParseQuery(Of ParseObject)("Matches")
                    query = query.WhereEqualTo("matchId", seedGame.matchId).WhereEqualTo("championId", SeedParticipant.championId)
                    Try
                        Await query.FirstAsync
                        save = False
                    Catch ex As Exception
                    End Try
                    If save Then
                        Dim tempSeedGame As ParseSeedMatch = New ParseSeedMatch()
                        tempSeedGame.items = New List(Of Integer)
                        If String.IsNullOrEmpty(SeedParticipant.highestAchievedSeasonTier) Then
                            tempSeedGame.tier = [Enum].Parse(GetType(RankedTier), "UNRANKED", True)
                        Else
                            tempSeedGame.tier = [Enum].Parse(GetType(RankedTier), SeedParticipant.highestAchievedSeasonTier, True)
                        End If
                        tempSeedGame.matchId = seedGame.matchId
                        tempSeedGame.championId = SeedParticipant.championId
                        For Each id In seedGame.participantIdentities
                            If id.participantId = SeedParticipant.participantId Then
                                tempSeedGame.summonerId = id.player.summonerId
                                Exit For
                            End If
                        Next
                        If (SeedParticipant.stats.item0 > 0) Then
                            tempSeedGame.items.Add(SeedParticipant.stats.item0)
                        End If
                        If (SeedParticipant.stats.item1 > 0) Then
                            tempSeedGame.items.Add(SeedParticipant.stats.item1)
                        End If
                        If (SeedParticipant.stats.item2 > 0) Then
                            tempSeedGame.items.Add(SeedParticipant.stats.item2)
                        End If
                        If (SeedParticipant.stats.item3 > 0) Then
                            tempSeedGame.items.Add(SeedParticipant.stats.item3)
                        End If
                        If (SeedParticipant.stats.item4 > 0) Then
                            tempSeedGame.items.Add(SeedParticipant.stats.item4)
                        End If
                        If (SeedParticipant.stats.item5 > 0) Then
                            tempSeedGame.items.Add(SeedParticipant.stats.item5)
                        End If
                        If (SeedParticipant.stats.item6 > 0) Then
                            tempSeedGame.items.Add(SeedParticipant.stats.item6)
                        End If
                        Dim success As Boolean = False
                        Do
                            Try

                                Await tempSeedGame.SaveAsync
                                success = True
                            Catch ex As Exception

                            End Try
                        Loop Until success
                    End If
                Next
            Next
        Next
        MessageBox.Show("Seeding just finished")
    End Sub

    Class SeedWrapper
        Public matches As IList(Of SeedMatch)
    End Class

    Class SeedMatch
        Public matchId As Long
        Public participants As IList(Of SeedParticipant)
        Public participantIdentities As IList(Of SeedParticipantId)
    End Class

    Class SeedParticipant
        Public championId As Integer
        Public highestAchievedSeasonTier As String
        Public stats As SeedParticipantStats
        Public participantId As Integer
    End Class

    Class SeedParticipantId
        Public participantId As Integer
        Public player As SeedPlayer
    End Class

    Class SeedPlayer
        Public summonerId As Long
        Public summonerName As String
    End Class

    Class SeedParticipantStats
        Public item0 As Integer
        Public item1 As Integer
        Public item2 As Integer
        Public item3 As Integer
        Public item4 As Integer
        Public item5 As Integer
        Public item6 As Integer
    End Class

    <ParseClassName("Matches")>
    Class ParseSeedMatch
        Inherits ParseObject

        <ParseFieldName("matchId")>
        Property matchId() As Integer
            Get
                Return GetProperty(Of Integer)()
            End Get
            Set(value As Integer)
                SetProperty(Of Integer)(value)
            End Set
        End Property

        <ParseFieldName("summonerId")>
        Property summonerId() As Integer
            Get
                Return GetProperty(Of Integer)()
            End Get
            Set(value As Integer)
                SetProperty(Of Integer)(value)
            End Set
        End Property

        <ParseFieldName("championId")>
        Property championId() As Integer
            Get
                Return GetProperty(Of Integer)()
            End Get
            Set(value As Integer)
                SetProperty(Of Integer)(value)
            End Set
        End Property

        <ParseFieldName("tier")>
        Property tier() As RankedTier
            Get
                Return GetProperty(Of Integer)()
            End Get
            Set(value As RankedTier)
                SetProperty(Of Integer)(value)
            End Set
        End Property

        <ParseFieldName("items")>
        Property items() As IList(Of Integer)
            Get
                Return GetProperty(Of IList(Of Integer))()
            End Get
            Set(value As IList(Of Integer))
                SetProperty(Of IList(Of Integer))(value)
            End Set
        End Property

    End Class

    Class FileItemSet
        Public title As String = "CustomSet"
        Public type As String = "custom"
        Public map As String = "any"
        Public mode As String = "any"
        Public priority As Boolean = False
        Public sortrank As Integer
        Public blocks As New List(Of FileItemBlock)

    End Class

    Class FileItemBlock
        Public type As String = "Build"
        'Public recMath As Boolean = False
        'Public minSummonerLevel As Integer = -1
        'Public maxSummonerLevel As Integer = -1
        'Public showIfSummonerSpell As String = ""
        'Public hideIfSummonerSpell As String = ""
        Public items As New List(Of FileItem)
    End Class

    Class FileItem
        Public id As String = "1001"
        Public count As Integer = 1
    End Class

    Async Function GetPageString(URL As String) As Task(Of String)
        Dim WC As Net.WebClient = New Net.WebClient()
        WC.Headers.Add("X-Riot-Token", My.Settings.Key)
        WC.Encoding = System.Text.Encoding.UTF8
        Dim GotIt As Boolean = True
        Dim res As String = Nothing
        Do
            Try
                res = Await WC.DownloadStringTaskAsync(URL)
                GotIt = True
            Catch ex As Net.WebException
                If Not ex.Response Is Nothing Then
                    Dim webResponse As Net.HttpWebResponse = ex.Response
                    If webResponse.StatusCode = Net.HttpStatusCode.NotFound Then
                        Return Nothing
                    ElseIf webResponse.StatusCode = 429 Then

                    End If
                End If
                GotIt = False
            End Try
            If Not GotIt Then
                Await Task.Delay(1000)
            End If
        Loop Until GotIt
        Return res
    End Function

End Module


