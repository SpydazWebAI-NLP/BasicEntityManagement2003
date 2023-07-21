Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization
Imports System.Windows.Forms

Namespace Recognition

    ''' <summary>
    ''' Sentence Splitter: Splits text into sentences or individual thoughts. the class returns a
    ''' list of Strings containing the sentences. the List can be considered to be a collection
    ''' of connected thoughts. or sub components of the text.
    '''
    ''' Note: Punctuation is Lost in the transformation.
    ''' </summary>
    <ComClass(SentenceSplitter.ClassId, SentenceSplitter.InterfaceId, SentenceSplitter.EventsId)>
    Public Class SentenceSplitter
        Public Const ClassId As String = "28993390-7702-401C-BAB3-38FF97BC1AC9"
        Public Const EventsId As String = "CD334307-F53E-401A-AC6D-3CFDD86FD6F1"
        Public Const InterfaceId As String = "8B3345F1-5D13-4059-829B-B531310144B5"

        ''' <summary>
        ''' punctuation markers for end of sentences(individual thoughts) Set in order of Rank
        ''' </summary>
        Public Shared EndPunctuation() As String = {".", ";", "?", "!", ":"}

        ''' <summary>
        ''' Punctuation(known)
        ''' </summary>
        Public Shared Punctuation() As String = {".", ",", ";", "?", "!", ":", "$", "%", "^", "*", "<", ">",
"/", "@", "(", ")", "'""{", "}", "[", "]", "\", "|", "+", "=", "_", "-"}

        Private mSent As List(Of String)

        ''' <summary>
        ''' Provide text for sentence definition,
        ''' </summary>
        ''' <param name="Text"></param>
        Public Sub New(ByVal Text As String)
            mSent = SplitTextToSentences(Text)
        End Sub

        ''' <summary>
        ''' Returns number of sentences found
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                For Each Sent As String In Sentences
                    Count += 1

                Next
                Return Count
            End Get
        End Property

        Public ReadOnly Property Sentences As List(Of String)
            Get
                Return mSent
            End Get
        End Property

        ''' <summary>
        ''' Removes Trailing Spaces as well as double spaces from Text Also the Text is Capitalized
        ''' </summary>
        ''' <param name="Text"></param>
        ''' <returns></returns>
        Public Shared Function FormatText(ByRef Text As String) As String
            Dim FormatTextResponse As String = ""
            'FORMAT USERINPUT
            'turn to uppercase for searching the db
            Text = LTrim(Text)
            Text = RTrim(Text)
            Text = Text.Replace("  ", " ")
            FormatTextResponse = Text
            Return FormatTextResponse
        End Function

        ''' <summary>
        ''' finds sentences in text or phrase. based on EndPunctuation markers
        ''' </summary>
        ''' <param name="InputStr"></param>
        ''' <returns>Returns a list of sentences defined in the text</returns>
        Public Shared Function GetSentences(ByRef InputStr As String) As List(Of String)
            GetSentences = New List(Of String)
            Dim s As New SentenceSplitter(InputStr)
            For Each Sent As String In s.Sentences
                GetSentences.Add(Sent)
            Next
        End Function

        ''' <summary>
        ''' Removes Punctuation from Text
        ''' </summary>
        ''' <param name="Text"></param>
        ''' <returns>Cleaned Text</returns>
        Public Shared Function RemovePunctuation(ByVal Text As String) As String
            Dim mText As String = Text
            For Each item As String In Punctuation
                mText = mText.Replace(item, " ")
            Next
            mText = mText.Replace("  ", " ")
            Return mText
        End Function

        ''' <summary>
        ''' Splits Sentences by the Punctution offered. As it may be prudent to split by "." then
        ''' after by "," for sub components of the sentence etc
        ''' </summary>
        ''' <param name="mText">          text to be examined</param>
        ''' <param name="mEndPunctuation">Punctuation to be used as end marker</param>
        ''' <returns></returns>
        Public Shared Function SplitTextToSentences(ByVal mText As String, ByVal mEndPunctuation As String) As List(Of String)

            Dim Text As String = mText

            Text = Text.Replace(mEndPunctuation, "#")

            Dim TempSentencesArray() As String = Split(Text, "#")
            Dim mSentences As New List(Of String)
            For Each SentStr As String In TempSentencesArray
                If SentStr <> "" Then
                    mSentences.Add(FormatText(SentStr))
                End If

            Next

            Return mSentences
        End Function

        ''' <summary>
        ''' Splits to sentences based on all end markers in EndPunctuation
        ''' </summary>
        ''' <param name="mText"></param>
        ''' <returns></returns>
        Private Function SplitTextToSentences(ByVal mText As String) As List(Of String)

            Dim Text As String = mText
            For Each item As String In EndPunctuation
                Text = Text.Replace(item, "#")

            Next
            Dim TempSentencesArray() As String = Split(Text, "#")
            Dim mSentences As New List(Of String)
            For Each SentStr As String In TempSentencesArray
                If SentStr <> "" Then
                    mSentences.Add(FormatText(SentStr))
                End If

            Next

            Return mSentences
        End Function

    End Class

    ''' <summary>
    ''' Created as a inheritance model to create sub recognizers , 
    ''' such as Sentiment detection and entity detection , topic detection, emotion detection
    ''' ******* this is a text based model without the use of Objects *****
    ''' Hence Inheritance to invoke the use of objects such as entitys / sentiment /topics etc
    ''' </summary>
    Public Class iEntityHandler
        ''' <summary>
        ''' Used to retrieve Learning PAtterns
        ''' Learning Pattern / Nym
        ''' </summary>
        Public Structure SemanticPattern

#Region "Fields"

            ''' <summary>
            ''' Tablename in db
            ''' </summary>
            Public Shared SemanticPatternTable As String = "SemanticPatterns"

            ''' <summary>
            ''' Used to hold the connection string
            ''' </summary>
            Public ConectionStr As String

            ''' <summary>
            ''' used to identify patterns
            ''' </summary>
            Public NymStr As String

            ''' <summary>
            ''' Search patterns A# is B#
            ''' </summary>
            Public SearchPatternStr As String

#End Region

#Region "Methods"

            ''' <summary>
            ''' filters collection of patterns by nym
            ''' </summary>
            ''' <param name="Patterns">patterns </param>
            ''' <param name="NymStr">nym to be segmented</param>
            ''' <returns></returns>
            Public Shared Function FilterSemanticPatternsbyNym(ByRef Patterns As List(Of SemanticPattern), ByRef NymStr As String) As List(Of SemanticPattern)
                Dim Lst As New List(Of SemanticPattern)
                For Each item In Patterns
                    If item.NymStr = NymStr Then
                        Lst.Add(item)
                    End If
                Next
                If Lst.Count > 0 Then
                    Return Lst
                Else
                    Return Nothing
                End If
            End Function

            ''' <summary>
            ''' Gets all Semantic Patterns From Table
            ''' </summary>
            ''' <param name="iConnectionStr"></param>
            ''' <param name="TableName"></param>
            ''' <returns></returns>
            Public Shared Function GetDBSemanticPatterns(ByRef iConnectionStr As String, ByRef TableName As String) As List(Of SemanticPattern)
                Dim DbSubjectLst As New List(Of SemanticPattern)

                Dim SQL As String = "SELECT * FROM " & TableName
                Using conn = New System.Data.OleDb.OleDbConnection(iConnectionStr)
                    Using cmd = New System.Data.OleDb.OleDbCommand(SQL, conn)
                        conn.Open()
                        Try
                            Dim dr = cmd.ExecuteReader()
                            While dr.Read()
                                Dim NewKnowledge As New SemanticPattern With {
                                .NymStr = dr("Nym").ToString(),
                                .SearchPatternStr = dr("SemanticPattern").ToString()
                            }
                                DbSubjectLst.Add(NewKnowledge)
                            End While
                        Catch e As Exception
                            ' Do some logging or something.
                            MessageBox.Show("There was an error accessing your data. GetDBSemanticPatterns: " & e.ToString())
                        End Try
                    End Using
                End Using
                Return DbSubjectLst
            End Function

            ''' <summary>
            ''' gets semantic patterns from table based on query SQL
            ''' </summary>
            ''' <param name="iConnectionStr"></param>
            ''' <param name="Query"></param>
            ''' <returns></returns>
            Public Shared Function GetDBSemanticPatternsbyQuery(ByRef iConnectionStr As String, ByRef Query As String) As List(Of SemanticPattern)
                Dim DbSubjectLst As New List(Of SemanticPattern)

                Dim SQL As String = Query
                Using conn = New System.Data.OleDb.OleDbConnection(iConnectionStr)
                    Using cmd = New System.Data.OleDb.OleDbCommand(SQL, conn)
                        conn.Open()
                        Try
                            Dim dr = cmd.ExecuteReader()
                            While dr.Read()
                                Dim NewKnowledge As New SemanticPattern With {
                                    .NymStr = dr("Nym").ToString(),
                                    .SearchPatternStr = dr("SemanticPattern").ToString()
                                }
                                DbSubjectLst.Add(NewKnowledge)
                            End While
                        Catch e As Exception
                            ' Do some logging or something.
                            MessageBox.Show("There was an error accessing your data. GetDBSemanticPatterns: " & e.ToString())
                        End Try
                    End Using
                End Using
                Return DbSubjectLst
            End Function

            ''' <summary>
            ''' gets random pattern from list
            ''' </summary>
            ''' <param name="Patterns"></param>
            ''' <returns></returns>
            Public Shared Function GetRandomPattern(ByRef Patterns As List(Of SemanticPattern)) As SemanticPattern
                Dim rnd = New Random()
                If Patterns.Count > 0 Then

                    Return Patterns(rnd.Next(0, Patterns.Count))
                Else
                    Return Nothing
                End If
            End Function

            ''' <summary>
            ''' used to generalize patterns into general search patterns
            ''' (a# is b#) to (* is a *)
            ''' </summary>
            ''' <param name="Patterns"></param>
            ''' <returns></returns>
            Public Shared Function InsertWildcardsIntoPatterns(ByRef Patterns As List(Of SemanticPattern)) As List(Of SemanticPattern)
                For Each item In Patterns
                    item.SearchPatternStr.Replace("A#", "*")
                    item.SearchPatternStr.Replace("B#", "*")
                Next
                Return Patterns
            End Function

            ''' <summary>
            ''' Adds a New Semantic pattern
            ''' </summary>
            ''' <param name="NewSemanticPattern"></param>
            Public Function AddSemanticPattern(ByRef iConnectionStr As String, ByRef Tablename As String, ByRef NewSemanticPattern As SemanticPattern) As Boolean
                AddSemanticPattern = False
                If NewSemanticPattern.NymStr IsNot Nothing And NewSemanticPattern.SearchPatternStr IsNot Nothing Then

                    Dim sql As String = "INSERT INTO " & Tablename & " (Nym, SemanticPattern) VALUES ('" & NewSemanticPattern.NymStr & "','" & NewSemanticPattern.SearchPatternStr & "')"

                    Using conn = New System.Data.OleDb.OleDbConnection(iConnectionStr)

                        Using cmd = New System.Data.OleDb.OleDbCommand(sql, conn)
                            conn.Open()
                            Try
                                cmd.ExecuteNonQuery()
                                AddSemanticPattern = True
                            Catch ex As Exception
                                MessageBox.Show("There was an error accessing your data. AddSemanticPattern: " & ex.ToString())
                            End Try
                        End Using
                    End Using
                Else
                End If
            End Function

            Public Function CheckIfSemanticPatternDetected(ByRef iConnectionStr As String, ByRef TableName As String, ByRef Userinput As String) As Boolean
                CheckIfSemanticPatternDetected = False
                For Each item In InsertWildcardsIntoPatterns(GetDBSemanticPatterns(iConnectionStr, TableName))
                    If Userinput Like item.SearchPatternStr Then
                        Return True
                    End If
                Next
            End Function

            Public Function GetDetectedSemanticPattern(ByRef iConnectionStr As String, ByRef TableName As String, ByRef Userinput As String) As SemanticPattern
                GetDetectedSemanticPattern = Nothing
                For Each item In InsertWildcardsIntoPatterns(GetDBSemanticPatterns(iConnectionStr, TableName))
                    If Userinput Like item.SearchPatternStr Then
                        Return item
                    End If
                Next
            End Function

            ''' <summary>
            ''' output in json format
            ''' </summary>
            ''' <returns></returns>
            Public Function ToJson() As String
                Dim Converter As New JavaScriptSerializer
                Return Converter.Serialize(Me)
            End Function

#End Region

        End Structure
        Public Enum EntityPositionPrediction
            None
            Before
            After
        End Enum
        ''' <summary>
        ''' Extract Entities from text using Named Entity Recognition (NER).
        ''' NER labels sequences of words in a text which are the names of things,
        ''' such as person and company names.
        ''' By creating lists On specific topics such As a list Of names / Locations , Organizations etc.
        ''' These can be used To identify these words In the text, As the entity type.
        ''' The entity's extracted can be extracted or the sentence containing the entity.
        ''' this structure is used to hold extracted Entity's and their associated sentences, and keywords
        ''' </summary>
        Public Structure DiscoveredEntity

#Region "Fields"

            ''' <summary>
            ''' Discovered Entity
            ''' </summary>
            Dim DiscoveredKeyWord As String

            ''' <summary>
            ''' Associated Sentence
            ''' </summary>
            Dim DiscoveredSentence As String

            ''' <summary>
            ''' Theme of Entitys in List / name of associated list
            ''' </summary>
            Public EntityList As String

#End Region

#Region "Methods"

            ''' <summary>
            ''' Outputs Structure to Jason(JavaScriptSerializer)
            ''' </summary>
            ''' <returns></returns>
            Public Function ToJson() As String
                Dim Converter As New JavaScriptSerializer
                Return Converter.Serialize(Me)
            End Function

#End Region

        End Structure
        Public Structure Entity
            Public Property EndIndex As Integer
            Public Property StartIndex As Integer
            Public Property Type As String
            Public Property Value As String
            Public Shared Function DetectEntitys(ByRef text As String, EntityList As List(Of Entity)) As List(Of Entity)
                Dim detectedEntitys As New List(Of Entity)()

                ' Perform entity detection logic here
                For Each item In EntityList
                    If text.Contains(item.Value) Then
                        detectedEntitys.Add(item)
                    End If
                Next

                Return detectedEntitys
            End Function
        End Structure

        Public Structure AnswerType

            Public Sub New(ByVal type As String, ByVal entities As List(Of String))
                Me.Type = type
                Me.Entities = entities
            End Sub

            Public Property Entities As List(Of String)
            Public Property Type As String
        End Structure

        Public Structure CapturedContent
            Public Sub New(ByVal word As String, ByVal precedingWords As List(Of String), ByVal followingWords As List(Of String))
                Me.Word = word
                Me.PrecedingWords = precedingWords
                Me.FollowingWords = followingWords

            End Sub

            Public Property FollowingWords As List(Of String)
            Public Property PrecedingWords As List(Of String)
            Public Property Word As String
        End Structure

        Public Structure CapturedWord
            Public Sub New(ByVal word As String, ByVal precedingWords As List(Of String), ByVal followingWords As List(Of String), ByVal person As String, ByVal location As String)
                Me.Word = word
                Me.PrecedingWords = precedingWords
                Me.FollowingWords = followingWords
                Me.Person = person
                Me.Location = location
            End Sub

            ''' <summary>
            ''' Gets or sets the context words.
            ''' </summary>
            Public Property ContextWords As List(Of String)

            Public Property Entity As String
            Public Property EntityType As String
            ''' <summary>
            ''' Gets or sets the entity types associated with the word.
            ''' </summary>
            Public Property EntityTypes As List(Of String)

            Public Property FollowingWords As List(Of String)
            ''' <summary>
            ''' Gets or sets a value indicating whether the word is recognized as an entity.
            ''' </summary>
            Public Property IsEntity As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether the word is the focus term.
            ''' </summary>
            Public Property IsFocusTerm As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether the word is a following word.
            ''' </summary>
            Public Property IsFollowing As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether the word is a preceding word.
            ''' </summary>
            Public Property IsPreceding As Boolean

            Public Property Location As String
            Public Property Person As String
            Public Property PrecedingWords As List(Of String)
            Public Property Word As String
        End Structure

        Structure NlpReport
            Public Shared DiscoveredEntitys As List(Of DiscoveredEntity) = ICollect.GetEntitysFromText(UserText, EntityLists)
            Private Shared EntityLists As List(Of Entity)
            Private Shared SearchPatterns As List(Of SemanticPattern)
            Private Shared UserText As String

            Public Sub New(ByRef Usertext As String, Entitylists As List(Of Entity), ByRef SearchPatterns As List(Of SemanticPattern))
                Me.UserText = Usertext
                Me.EntityLists = Entitylists
                Me.SearchPatterns = SearchPatterns
            End Sub

        End Structure

        ''' <summary>
        ''' Represents a word captured with its context information.
        ''' </summary>
        Public Structure WordWithContext
            ''' <summary>
            ''' Gets or sets the context words.
            ''' </summary>
            Public Property ContextWords As List(Of String)

            ''' <summary>
            ''' Gets or sets the entity types associated with the word.
            ''' </summary>
            Public Property EntityTypes As List(Of String)

            ''' <summary>
            ''' Gets or sets a value indicating whether the word is recognized as an entity.
            ''' </summary>
            Public Property IsEntity As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether the word is the focus term.
            ''' </summary>
            Public Property IsFocusTerm As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether the word is a following word.
            ''' </summary>
            Public Property IsFollowing As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether the word is a preceding word.
            ''' </summary>
            Public Property IsPreceding As Boolean

            ''' <summary>
            ''' Gets or sets the captured word.
            ''' </summary>
            Public Property Word As String
        End Structure

        Public Class iClassify
            Public Shared FemaleNames As List(Of String)
            Public Shared MaleNames As List(Of String)

            Public Shared ObjectNames As List(Of String)

            Private Shared commonQuestionHeaders As List(Of String)

            Private Shared Ipronouns As List(Of String)

            '' Example entity list to search
            'Dim entityList As New List(Of String)() From {"dolphins"}
            Private Shared questionWords As List(Of String)

            '' Example stored entity lists
            'Dim storedEntityLists As New List(Of List(Of String))()
            'storedEntityLists.Add(New List(Of String)() From {"dolphins", "whales", "sharks"})
            'storedEntityLists.Add(New List(Of String)() From {"lions", "tigers", "cheetahs"})
            'storedEntityLists.Add(New List(Of String)() From {"elephants", "rhinos", "giraffes"})
            Private Shared semanticPatterns As List(Of String)

            Private conclusions As List(Of String)

            Private hypotheses As List(Of String)

            Private premises As List(Of String)

            'The language rules For objects, locations, And antecedents can vary depending On the context And the specific language being used. However, here are some general guidelines:
            ''' <summary>
            ''' Returns all Pronouns in the model
            ''' </summary>
            ''' <returns></returns>
            Public Shared ReadOnly Property Pronouns As List(Of String)
                Get
                    Dim Lst As New List(Of String)
                    Lst.AddRange(MaleNames)
                    Lst.AddRange(FemaleNames)
                    Lst.AddRange(ObjectNames)
                    Lst.AddRange(Ipronouns)
                    Return Lst.Distinct.ToList
                End Get
            End Property

            Public Shared Function iClassifySentences(ByVal document As String) As Dictionary(Of String, List(Of String))
                Dim premises As New List(Of String)()
                Dim hypotheses As New List(Of String)()
                Dim conclusions As New List(Of String)()

                ' Split the document into sentences
                Dim sentences As String() = document.Split(New String() {". "}, StringSplitOptions.RemoveEmptyEntries)

                ' Rule-based classification
                For Each sentence In sentences
                    ' Remove leading/trailing spaces and convert to lowercase
                    sentence = sentence.Trim().ToLower()

                    ' Check if the sentence is a premise, hypothesis, or conclusion
                    If IsPremise(sentence) Then
                        premises.Add(sentence)
                    ElseIf IsHypothesis(sentence) Then
                        hypotheses.Add(sentence)
                    ElseIf IsConclusion(sentence) Then
                        conclusions.Add(sentence)
                    End If
                Next

                ' Store the classified sentences in a dictionary
                Dim classifiedSentences As New Dictionary(Of String, List(Of String))()
                classifiedSentences.Add("Premise", premises)
                classifiedSentences.Add("Hypothesis", hypotheses)
                classifiedSentences.Add("Conclusion", conclusions)

                Return classifiedSentences
            End Function

            Public Shared Function IsAdjective(word As String) As Boolean
                ' Implement your custom logic to determine if a word is an adjective
                ' Return true if the word is an adjective, false otherwise

                ' Example: Check if the word ends with "ly"
                Return word.EndsWith("ly")
            End Function

            Public Shared Function IsAntecedentIndicator(ByVal token As String) As Boolean
                ' List of antecedent indicator words
                Dim antecedentIndicators As String() = {" he", "she", "it", "they", "them", "that", "him", "we", "us", "its", "his", "thats"}

                ' Check if the token is an antecedent indicator
                Return antecedentIndicators.Contains(token.ToLower())
            End Function

            Public Shared Function IsConclusion(ByVal sentence As String) As Boolean
                ' List of indicator phrases for conclusions
                Dim conclusionIndicators As String() = {"therefore", "thus", "consequently", "hence", "in conclusion"}

                ' Check if any of the conclusion indicators are present in the sentence
                For Each indicator In conclusionIndicators
                    If sentence.Contains(indicator) Then
                        Return True
                    End If
                Next

                Return False
            End Function

            Public Shared Function IsEntityOrPronoun(word As String, ByRef Entitys As List(Of String)) As Boolean
                Dim AntecedantIdentifers() As String = {" he ", "she", "him", "her", "it", "them", "they", "that", "we"}

                ' 1.For simplicity, let's assume any word ending with "s" is a noun/pronoun
                ' 2.For simplicity, let's assume any word referring to a person is a pronoun
                Dim lowerCaseWord As String = word.ToLower()
                For Each item In Entitys
                    If item.ToLower = lowerCaseWord Then Return True

                Next
                For Each item In AntecedantIdentifers
                    If item.ToLower = lowerCaseWord Then Return True
                Next
                Return False
            End Function
            Public Shared Function IsFemaleNounOrPronoun(ByVal word As String) As Boolean
                Dim ifemaleNouns() As String = {"she", "her", "hers", "shes"}
                Return FemaleNames.Contains(word.ToLower()) OrElse FemaleNames.Contains(word.ToLower() & "s") OrElse ifemaleNouns.Contains(word.ToLower)
            End Function
            ''' <summary>
            ''' female names
            ''' </summary>
            ''' <param name="pronoun"></param>
            ''' <returns></returns>
            Public Shared Function IsFemalePronoun(pronoun As String) As Boolean
                Dim lowerCasePronoun As String = pronoun.ToLower()
                Return lowerCasePronoun = "her" OrElse lowerCasePronoun = "she" OrElse lowerCasePronoun = "hers" OrElse lowerCasePronoun = "shes" OrElse FemaleNames.Contains(pronoun)
            End Function

            Public Shared Function IsHypothesis(ByVal sentence As String) As Boolean
                ' List of indicator phrases for hypotheses
                Dim hypothesisIndicators As String() = {"if", "when", "suppose that", "let's say", "assuming that"}

                ' Check if any of the hypothesis indicators are present in the sentence
                For Each indicator In hypothesisIndicators
                    If sentence.Contains(indicator) Then
                        Return True
                    End If
                Next

                Return False
            End Function

            ''' <summary>
            ''' Replaces discovered pronoun indicators to thier detected antecedant
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <param name="entityList"></param>
            ''' <returns></returns>
            Public Shared Function IsMaleNounOrPronoun(ByVal word As String) As Boolean
                Dim imaleNouns() As String = {"him", " he", "his", ""}
                Return MaleNames.Contains(word.ToLower()) OrElse MaleNames.Contains(word.ToLower() & "s") OrElse imaleNouns.Contains(word.ToLower)
            End Function

            ''' <summary>
            ''' Malenames
            ''' </summary>
            ''' <param name="pronoun"></param>
            ''' <returns></returns>
            Public Shared Function IsMalePronoun(pronoun As String) As Boolean
                Dim lowerCasePronoun As String = pronoun.ToLower()
                Return lowerCasePronoun = " he" OrElse lowerCasePronoun = "him" OrElse lowerCasePronoun = " his" OrElse MaleNames.Contains(pronoun)
            End Function

            Public Shared Function IsObjectPronoun(ByVal word As String) As Boolean
                Dim iObjectNames() As String = {"its", "it", "that", "thats"}

                Return iObjectNames.Contains(word.ToLower()) OrElse iObjectNames.Contains(word.ToLower() & "s")
            End Function
            'Possible Output: "The person associated with John is..."
            Public Shared Function IsPersonName(word As String) As Boolean
                ' Implement your custom logic to determine if a word is a person name
                ' Return true if the word is a person name, false otherwise

                ' Example: Check if the word starts with an uppercase letter
                Return Char.IsUpper(word(0))
            End Function

            Public Shared Function IsPremise(ByVal sentence As String) As Boolean
                ' List of indicator phrases for premises
                Dim premiseIndicators As String() = {"based on", "according to", "given", "assuming", "since"}

                ' Check if any of the premise indicators are present in the sentence
                For Each indicator In premiseIndicators
                    If sentence.Contains(indicator) Then
                        Return True
                    End If
                Next

                Return False
            End Function

            Public Shared Function IsProperNoun(word As String) As Boolean
                ' Implement your custom logic to determine if a word is a proper noun
                ' Return true if the word is a proper noun, false otherwise

                ' Example: Check if the word starts with an uppercase letter
                Return Char.IsUpper(word(0))
            End Function

            Public Shared Function IsQuestion(sentence As String) As Boolean
                ' Preprocess the sentence
                sentence = sentence.ToLower().Trim()

                ' Check for question words
                If iManage.StartsWithAny(sentence, questionWords) Then
                    Return True
                End If

                ' Check for question marks
                If sentence.EndsWith("?") Then
                    Return True
                End If

                ' Check for semantic patterns
                Dim patternRegex As New Regex(String.Join("|", semanticPatterns))
                If patternRegex.IsMatch(sentence) Then
                    Return True
                End If

                ' Check for common question headers
                If iManage.StartsWithAny(sentence, commonQuestionHeaders) Then
                    Return True
                End If

                ' No matching question pattern found
                Return False
            End Function

            Public Shared Function IsVerb(word As String) As Boolean
                ' Implement your custom logic to determine if a word is a verb
                ' Return true if the word is a verb, false otherwise

                ' Example: Check if the word ends with "ing"
                Return word.EndsWith("ing")
            End Function

            Function ClassifySentence(sentence As String) As String
                ' Rule-based sentence classification

                ' Rule 1: Indicator Phrases
                Dim premiseIndicators As String() = {"According to", "Based on", "In light of", "Considering", "The evidence suggests"}
                Dim hypothesisIndicators As String() = {"If", "Suppose", "Assume", "In the case that", "Given that"}
                Dim conclusionIndicators As String() = {"Therefore", "Thus", "Consequently", "In conclusion", "As a result"}

                For Each indicator As String In premiseIndicators
                    If sentence.StartsWith(indicator) OrElse sentence.Contains(indicator) Then
                        Return "Premise"
                    End If
                Next

                For Each indicator As String In hypothesisIndicators
                    If sentence.StartsWith(indicator) OrElse sentence.Contains(indicator) Then
                        Return "Hypothesis"
                    End If
                Next

                For Each indicator As String In conclusionIndicators
                    If sentence.StartsWith(indicator) OrElse sentence.Contains(indicator) Then
                        Return "Conclusion"
                    End If
                Next

                ' Rule 2: Syntactic Patterns
                Dim premisePattern As String = "(?i)(\bThe\b.*?\b suggests\b)|(\bBased on\b)"
                Dim hypothesisPattern As String = "(?i)(\bIf\b.*?\bthen\b)|(\bSuppose\b)"
                Dim conclusionPattern As String = "(?i)(\bTherefore\b)|(\bThus\b)"

                If Regex.IsMatch(sentence, premisePattern) Then
                    Return "Premise"
                ElseIf Regex.IsMatch(sentence, hypothesisPattern) Then
                    Return "Hypothesis"
                ElseIf Regex.IsMatch(sentence, conclusionPattern) Then
                    Return "Conclusion"
                End If

                ' Rule 3: Sentence Length
                Dim words As String() = sentence.Split(New Char() {" "c, "."c, ","c, ";"c, ":"c, "?"c, "!"c}, StringSplitOptions.RemoveEmptyEntries)
                If words.Length > 10 Then
                    Return "Premise"
                ElseIf words.Length < 6 Then
                    Return "Conclusion"
                End If

                ' Rule 4: Keyword Matching
                Dim premiseKeywords As String() = {"because", "since", "given", "due to"}
                Dim hypothesisKeywords As String() = {"if", "suppose", "assuming"}
                Dim conclusionKeywords As String() = {"therefore", "thus", "consequently"}

                For Each word As String In words
                    If premiseKeywords.Contains(word.ToLower()) Then
                        Return "Premise"
                    ElseIf hypothesisKeywords.Contains(word.ToLower()) Then
                        Return "Hypothesis"
                    ElseIf conclusionKeywords.Contains(word.ToLower()) Then
                        Return "Conclusion"
                    End If
                Next

                Return "Unknown"
            End Function

            Function ClassifySentence(sentence As String, document As String) As String
                ' Rule-based sentence classification

                ' Rule 1: Indicator Phrases
                Dim premiseIndicators As String() = {"According to", "Based on", "In light of", "Considering", "The evidence suggests"}
                Dim hypothesisIndicators As String() = {"If", "Suppose", "Assume", "In the case that", "Given that"}
                Dim conclusionIndicators As String() = {"Therefore", "Thus", "Consequently", "In conclusion", "As a result"}

                For Each indicator As String In premiseIndicators
                    If sentence.StartsWith(indicator) OrElse sentence.Contains(indicator) Then
                        Return "Premise"
                    End If
                Next

                For Each indicator As String In hypothesisIndicators
                    If sentence.StartsWith(indicator) OrElse sentence.Contains(indicator) Then
                        Return "Hypothesis"
                    End If
                Next

                For Each indicator As String In conclusionIndicators
                    If sentence.StartsWith(indicator) OrElse sentence.Contains(indicator) Then
                        Return "Conclusion"
                    End If
                Next

                ' Rule 2: Syntactic Patterns
                Dim premisePattern As String = "(?i)(\bThe\b.*?\b suggests\b)|(\bBased on\b)"
                Dim hypothesisPattern As String = "(?i)(\bIf\b.*?\bthen\b)|(\bSuppose\b)"
                Dim conclusionPattern As String = "(?i)(\bTherefore\b)|(\bThus\b)"

                If Regex.IsMatch(sentence, premisePattern) Then
                    Return "Premise"
                ElseIf Regex.IsMatch(sentence, hypothesisPattern) Then
                    Return "Hypothesis"
                ElseIf Regex.IsMatch(sentence, conclusionPattern) Then
                    Return "Conclusion"
                End If

                ' Rule 3: Sentence Length
                Dim words As String() = sentence.Split(New Char() {" "c, "."c, ","c, ";"c, ":"c, "?"c, "!"c}, StringSplitOptions.RemoveEmptyEntries)
                If words.Length > 10 Then
                    Return "Premise"
                ElseIf words.Length < 6 Then
                    Return "Conclusion"
                End If

                ' Rule 4: Keyword Matching
                Dim premiseKeywords As String() = {"because", "since", "given", "due to"}
                Dim hypothesisKeywords As String() = {"if", "suppose", "assuming"}
                Dim conclusionKeywords As String() = {"therefore", "thus", "consequently"}

                For Each word As String In words
                    If premiseKeywords.Contains(word.ToLower()) Then
                        Return "Premise"
                    ElseIf hypothesisKeywords.Contains(word.ToLower()) Then
                        Return "Hypothesis"
                    ElseIf conclusionKeywords.Contains(word.ToLower()) Then
                        Return "Conclusion"
                    End If
                Next

                ' Rule 5: Contextual Analysis
                Dim premiseContext As String = "(?i)increases the risk of"
                Dim hypothesisContext As String = "(?i)it rains"
                Dim conclusionContext As String = "(?i)the company expects"

                If Regex.IsMatch(document, premiseContext) Then
                    Return "Premise"
                ElseIf Regex.IsMatch(document, hypothesisContext) Then
                    Return "Hypothesis"
                ElseIf Regex.IsMatch(document, conclusionContext) Then
                    Return "Conclusion"
                End If

                Return "Unknown"
            End Function

            Function ClassifySentences(ByVal document As String) As Dictionary(Of String, List(Of String))
                Dim premises As New List(Of String)()
                Dim hypotheses As New List(Of String)()
                Dim conclusions As New List(Of String)()

                ' Split the document into sentences
                Dim sentences As String() = document.Split({"."c}, StringSplitOptions.RemoveEmptyEntries)

                ' Rule-based classification
                For Each sentence In sentences
                    ' Remove leading/trailing spaces and convert to lowercase
                    sentence = sentence.Trim().ToLower()

                    ' Check if the sentence contains indicator phrases for premises, hypotheses, and conclusions
                    If IsPremise(sentence) Then
                        premises.Add(sentence)
                    ElseIf IsHypothesis(sentence) Then
                        hypotheses.Add(sentence)
                    ElseIf IsConclusion(sentence) Then
                        conclusions.Add(sentence)
                    End If
                Next

                ' Store the classified sentences in a dictionary
                Dim classifiedSentences As New Dictionary(Of String, List(Of String))()
                classifiedSentences.Add("Premise", premises)
                classifiedSentences.Add("Hypothesis", hypotheses)
                classifiedSentences.Add("Conclusion", conclusions)

                Return classifiedSentences
            End Function

            Public Sub DisplayClassifiedSentences()
                Console.WriteLine("Premises:")
                For Each premise In premises
                    Console.WriteLine(premise)
                Next

                Console.WriteLine("Hypotheses:")
                For Each hypothesis In hypotheses
                    Console.WriteLine(hypothesis)
                Next

                Console.WriteLine("Conclusions:")
                For Each conclusion In conclusions
                    Console.WriteLine(conclusion)
                Next

                Console.WriteLine()
            End Sub

            Public Function IsEntity(ByRef Word As String, ByRef Entitys As List(Of String)) As Boolean
                For Each item In Entitys
                    If Word = item Then
                        Return True
                    End If
                Next
                Return False
            End Function
        End Class

        '            capturedWordsWithEntityContext.Add(wordWithContext)
        '        Next
        '    End If
        '    Return capturedWordsWithEntityContext
        'End Function
        Public Class ICollect
            Public Shared Function GetEntityCountInText(ByVal text As String, ByVal entity As String) As Integer
                Dim regex As New Regex(Regex.Escape(entity))
                Return regex.Matches(text).Count
            End Function

            Public Shared Function GetEntityList(entityType As String, entities As String()) As List(Of (String, String))
                Dim entityList As New List(Of (String, String))

                For Each entity In entities
                    entityList.Add((entity, entityType))
                Next

                Return entityList
            End Function


            Public Shared Function GetEntityPatternsInText(ByRef Text As String, ByRef EntityList As List(Of String)) As List(Of String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If

                Dim Str As String = Text
                Dim Sents As List(Of String) = Str.Split(".").ToList
                Dim DiscoveredSents As New List(Of String)
                For Each item In Sents
                    Dim NewStr As String = iExtract.ExtractPattern(Str, EntityList)
                    DiscoveredSents.Add(NewStr)

                Next
                Return DiscoveredSents
            End Function

            Public Shared Function GetEntitySentencesInText(ByRef Text As String, ByRef EntityList As List(Of String)) As List(Of String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If

                Dim Str As String = Text
                Dim Sents As List(Of String) = Str.Split(".").ToList
                Dim DiscoveredSents As New List(Of String)
                For Each item In Sents
                    Dim discovered As Boolean = iDetect.Detected(Str, EntityList)
                    If discovered = True Then
                        Dim entitys As List(Of String) = iDetect.Detect(item, EntityList)
                        DiscoveredSents.Add(item)
                    End If
                Next
                Return DiscoveredSents
            End Function

            Public Shared Function GetEntitySentencesInText(ByRef Text As String, EntityList As List(Of String), ByRef EntityLabel As String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("Entitylabel")
                End If

                Dim str As String = Text
                Dim Sents As List(Of String) = str.Split(".").ToList
                Dim output As New List(Of String)
                For Each item In Sents
                    If iDetect.Detected(Text, EntityList) = True Then


                        output.Add(item)

                    Else

                    End If
                Next
                Return output
            End Function

            ''' <summary>
            ''' Attempts to find the entity's in the user text,(Paragraph level)
            ''' returning all the entity's discovered
            ''' </summary>
            ''' <param name="UserText"></param>
            ''' <param name="EntityLists">list of entity lists</param>
            ''' <returns>the discovered entity's are return in a structured data form</returns>
            Public Shared Function GetEntitysFromText(ByRef UserText As String, EntityLists As List(Of Entity)) As List(Of DiscoveredEntity)
                'Split Sentences
                Dim Sentences As List(Of String) = SentenceSplitter.GetSentences(UCase(UserText))
                Dim DiscoveredEntitys As New List(Of DiscoveredEntity)

                'Based On Entity-lists - Discovers Entities
                For Each item In Sentences

                    DiscoveredEntitys.AddRange(iExtract.ExtractEntitys(item, EntityLists))

                Next
                If DiscoveredEntitys IsNot Nothing Then

                    If DiscoveredEntitys.Count > 0 Then
                        Return DiscoveredEntitys
                    Else
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If

            End Function

            ''' <summary>
            ''' Gets the shapes of entities present in the given text.
            ''' </summary>
            ''' <param name="Text">The text to analyze.</param>
            ''' <param name="EntityList">A list of entities to search for.</param>
            ''' <returns>A list of entity shapes.</returns>
            Public Shared Function GetEntityShapesInText(ByRef Text As String, ByRef EntityList As List(Of String)) As List(Of String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                Dim Str As String = Text
                Dim Sents As List(Of String) = Str.Split(".").ToList
                Dim DiscoveredSents As New List(Of String)
                For Each item In Sents
                    Dim NewStr As String = iDiscover.DiscoverShape(Str, EntityList)
                    DiscoveredSents.Add(NewStr)

                Next
                Return DiscoveredSents
            End Function

            ' Functions for collecting entities and patterns in text
            ' ...
            Public Shared Function GetEntitysInText(ByRef Text As String, ByRef EntityList As List(Of String)) As List(Of String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                Dim Str As String = Text
                Dim entitys As List(Of String) = iDetect.Detect(Str, EntityList)
                Return entitys
            End Function

            Public Shared Function GetEntitysInText(ByRef Text As String, EntityList As List(Of String), ByRef EntityLabel As String) As List(Of String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("Entitylabel")
                End If
                Dim str As String = Text
                Dim output As New List(Of String)
                If iDetect.Detected(Text, EntityList) = True Then


                    Dim DetectedEntitys As List(Of String) = iDetect.Detect(Text, EntityList)
                    Dim Shape As String = iDiscover.DiscoverShape(Text, EntityList, EntityLabel)
                    Dim pattern As String = iExtract.ExtractPattern(Text, EntityList, EntityLabel)
                    output = DetectedEntitys
                Else

                End If
                Return output
            End Function

            ' Class for collecting entities and patterns from text
            Public Shared Function GetPatternInText(ByRef Text As String, EntityList As List(Of String), ByRef EntityLabel As String) As String
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("Entitylabel")
                End If

                Dim str As String = Text
                Dim output As String = ""
                If iDetect.Detected(Text, EntityList) = True Then


                    Dim DetectedEntitys As List(Of String) = iDetect.Detect(Text, EntityList)
                    Dim Shape As String = iDiscover.DiscoverShape(Text, EntityList, EntityLabel)
                    Dim pattern As String = iExtract.ExtractPattern(Text, EntityList, EntityLabel)
                    output = pattern
                Else

                End If
                Return output
            End Function

            Public Shared Function GetPatternsInText(ByRef Text As String, EntityList As List(Of String), ByRef EntityLabel As String) As List(Of String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("Entitylabel")
                End If
                Dim str As String = Text
                Dim output As New List(Of String)
                If iDetect.Detected(Text, EntityList) = True Then


                    Dim DetectedEntitys As List(Of String) = iDetect.Detect(Text, EntityList)
                    Dim Shape As String = iDiscover.DiscoverShape(Text, EntityList, EntityLabel)
                    Dim pattern As String = iExtract.ExtractPattern(Text, EntityList, EntityLabel)
                    output = pattern.Split(".").ToList
                Else

                End If
                Return output
            End Function

            Public Shared Function GetShapeInText(ByRef Text As String, EntityList As List(Of String), ByRef EntityLabel As String) As String
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("Entitylabel")
                End If
                Dim str As String = Text
                Dim output As String = ""
                If iDetect.Detected(Text, EntityList) = True Then


                    Dim DetectedEntitys As List(Of String) = iDetect.Detect(Text, EntityList)
                    Dim Shape As String = iDiscover.DiscoverShape(Text, EntityList, EntityLabel)
                    Dim pattern As String = iExtract.ExtractPattern(Text, EntityList, EntityLabel)
                    output = Shape
                Else

                End If
                Return output
            End Function

            Public Shared Function GetShapesInText(ByRef Text As String, EntityList As List(Of String), ByRef EntityLabel As String) As List(Of String)
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("Entitylabel")
                End If
                Dim str As String = Text
                Dim output As New List(Of String)
                If iDetect.Detected(Text, EntityList) = True Then


                    Dim DetectedEntitys As List(Of String) = iDetect.Detect(Text, EntityList)
                    Dim Shape As String = iDiscover.DiscoverShape(Text, EntityList, EntityLabel)
                    Dim pattern As String = iExtract.ExtractPattern(Text, EntityList, EntityLabel)
                    output = Shape.Split(".").ToList
                Else

                End If
                Return output
            End Function


            '' Print the associated entities
            'Console.WriteLine("Associated Entities:")
            'For Each entity As String In associatedEntities
            '    Console.WriteLine(entity)
            Function GetAssociatedEntities(entityList As List(Of String), storedEntityLists As List(Of List(Of String))) As List(Of String)
                Dim associatedEntities As New List(Of String)

                ' Iterate over each stored entity list
                For Each storedList As List(Of String) In storedEntityLists
                    ' Check if any entity from the input entity list appears in the stored list
                    Dim matchedEntities = storedList.Intersect(entityList)

                    ' Add the matched entities to the associated entities list
                    associatedEntities.AddRange(matchedEntities)
                Next

                ' Remove duplicates from the associated entities list
                associatedEntities = associatedEntities.Distinct().ToList()

                Return associatedEntities
            End Function
        End Class

        '            Dim wordWithContext As New WordWithContext() With {
        '                .Word = word,
        '                .IsEntity = entityTypes.Count > 0,
        '                .EntityTypes = entityTypes,
        '                .IsFocusTerm = (i = focusIndex),
        '                .IsPreceding = (i < focusIndex),
        '                .IsFollowing = (i > focusIndex),
        '                .ContextWords = contextWords
        '            }
        Public Class iDetect

            ''' <summary>
            ''' Detects entities in the given text.
            ''' </summary>
            ''' <param name="text">The text to be analyzed.</param>
            ''' <param name="EntityList">A list of entities to detect.</param>
            ''' <returns>A list of detected entities in the text.</returns>
            Public Shared Function Detect(ByRef text As String, ByRef EntityList As List(Of String)) As List(Of String)
                Dim Lst As New List(Of String)
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("EntityList")
                End If
                If Detected(text, EntityList) = True Then
                    For Each item In EntityList
                        If text.Contains(item) Then
                            Lst.Add(item)
                        End If
                    Next
                    Return Lst
                Else
                    Return New List(Of String)
                End If
            End Function

            ' Class for entity detection
            ''' <summary>
            ''' Checks if any entities from the EntityList are present in the text.
            ''' </summary>
            ''' <param name="text">The text to be checked.</param>
            ''' <param name="EntityList">A list of entities to search for.</param>
            ''' <returns>True if any entities are detected, False otherwise.</returns>
            Public Shared Function Detected(ByRef text As String, ByRef EntityList As List(Of String)) As Boolean
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("EntityList")
                End If

                For Each item In EntityList
                    If text.Contains(item) Then
                        Return True
                    End If
                Next
                Return False
            End Function

            ''' <summary>
            ''' Attempts to find Unknown Names(pronouns) identified by thier capitalization
            ''' </summary>
            ''' <param name="words"></param>
            ''' <returns></returns>
            Public Shared Function DetectNamedEntities(ByVal words() As String) As List(Of String)
                Dim namedEntities As New List(Of String)()

                For i = 0 To words.Length - 1
                    Dim word = words(i)
                    If Char.IsUpper(word(0)) AndAlso Not iClassify.Pronouns.Contains(word.ToLower()) Then
                        namedEntities.Add(word)
                    End If
                Next

                Return namedEntities
            End Function

            Public Function DetectGender(name As String) As String
                ' For simplicity, let's assume any name starting with a vowel is female, and the rest are male
                If iClassify.IsObjectPronoun(name) Then
                    Return "Object"
                ElseIf iClassify.IsMaleNounOrPronoun(name) Then
                    Return "Male"
                ElseIf iClassify.IsFemaleNounOrPronoun(name) Then
                    Return "Female"
                Else
                    Return "Unknown"
                End If
            End Function
        End Class

        '                If isLowConfidenceEntity Then
        '                    entityTypes.Add("Low Confidence Entity")
        '                End If
        '            End If
        Public Class iDiscover
            Public Shared Function CalculateWordOverlap(tokens1 As String(), tokens2 As String()) As Integer
                Dim overlap As Integer = 0

                ' Compare each token in sentence 1 with tokens in sentence 2
                For Each token1 As String In tokens1
                    For Each token2 As String In tokens2
                        ' If the tokens match, increment the overlap count
                        If token1.ToLower() = token2.ToLower() Then
                            overlap += 1
                            Exit For ' No need to check further tokens in sentence 2
                        End If
                    Next
                Next

                Return overlap
            End Function

            Public Shared Function CompareAndScoreSentences(sentence1 As String, sentence2 As String) As Double
                ' Implement your custom sentence comparison and scoring logic here
                ' Compare the two sentences and assign a similarity score

                ' Example sentence comparison and scoring logic
                Dim similarityScore As Double = 0.0

                ' Calculate the similarity score based on some criteria (e.g., word overlap)
                Dim words1() As String = sentence1.Split(" "c)
                Dim words2() As String = sentence2.Split(" "c)

                Dim commonWordsCount As Integer = words1.Intersect(words2).Count()
                Dim totalWordsCount As Integer = words1.Length + words2.Length

                If totalWordsCount > 0 Then
                    similarityScore = CDbl(commonWordsCount) / CDbl(totalWordsCount)
                End If

                Return similarityScore
            End Function

            Public Shared Function ContainsResolvedAntecedentsOrEntities(sentence As String, resolvedAntecedents As List(Of String), resolvedEntities As List(Of String)) As Boolean
                ' Check if the sentence contains any of the resolved antecedents or entities
                For Each antecedent In resolvedAntecedents
                    If sentence.Contains(antecedent) Then
                        Return True
                    End If
                Next

                For Each entity In resolvedEntities
                    If sentence.Contains(entity) Then
                        Return True
                    End If
                Next

                Return False
            End Function
            Public Shared Function DetermineEntailment(overlap As Integer) As Boolean
                ' Set a threshold for entailment
                Dim threshold As Integer = 2

                ' Determine entailment based on overlap
                Return overlap >= threshold
            End Function

            ''' <summary>
            ''' Attempts to find the entity's in the user text,(Paragraph level)
            ''' returning all the entity's discovered
            ''' </summary>
            ''' <param name="UserText"></param>
            ''' <param name="EntityLists">list of entity lists</param>
            ''' <returns>the discovered entity's are return in a structured data form</returns>
            Public Shared Function DiscoverEntitysInText(ByRef UserText As String, EntityLists As List(Of Entity)) As List(Of DiscoveredEntity)
                'Split Sentences
                Dim Sentences = SentenceSplitter.GetSentences(UCase(UserText))
                Dim DiscoveredEntitys As New List(Of DiscoveredEntity)

                'Based On Entity-lists - Discovers Entities
                For Each item In Sentences

                    DiscoveredEntitys.AddRange(iExtract.ExtractEntitys(item, EntityLists))

                Next
                If DiscoveredEntitys IsNot Nothing Then

                    If DiscoveredEntitys.Count > 0 Then
                        Return DiscoveredEntitys
                    Else
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If

            End Function

            ''' <summary>
            ''' Discovers shapes in the text and replaces the detected entities with entity labels.
            ''' </summary>
            ''' <param name="text">The text to discover shapes in.</param>
            ''' <param name="EntityList">A list of entities to detect and replace.</param>
            ''' <returns>The text with detected entities replaced by entity labels.</returns>
            Public Shared Function DiscoverShape(ByRef text As String, ByRef EntityList As List(Of String)) As String
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If

                Dim Entitys As New List(Of String)
                Dim Str As String = text
                If iDetect.Detected(text, EntityList) = True Then
                    Entitys = iDetect.Detect(text, EntityList)

                    Str = iTransform.TransformText(Str, Entitys)

                End If
                Return Str
            End Function

            ' Class for discovering and transforming shapes in text
            Public Shared Function DiscoverShape(ByRef Text As String, Entitylist As List(Of String), ByRef EntityLabel As String) As String
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("Entitylabel")
                End If
                If Entitylist Is Nothing Then
                    Throw New ArgumentNullException("Entitylist")
                End If
                Dim Entitys As New List(Of String)
                Dim Str As String = Text
                If iDetect.Detected(Text, Entitylist) = True Then
                    Entitys = iDetect.Detect(Text, Entitylist)

                    Str = iTransform.TransformText(Str, Entitys, EntityLabel)

                End If
                Return Str
            End Function

            Public Shared Function FindSentencesWithEntity(entity As String, storedSentences As List(Of String)) As List(Of String)
                ' Implement your custom logic to find sentences that contain the given entity
                ' Return a list of sentences that match the entity

                Dim matchingSentences As New List(Of String)()

                ' Example logic: Check if the entity appears in each stored sentence
                For Each sentence As String In storedSentences
                    If sentence.Contains(entity) Then
                        matchingSentences.Add(sentence)
                    End If
                Next

                Return matchingSentences
            End Function

            Public Shared Function MatchesAnswerShape(sentence As String, answerShapes As List(Of String)) As Boolean

                ' Check if the sentence matches any of the answer shapes using regex pattern matching
                For Each answerShape In answerShapes
                    Dim pattern As String = "\b" + Regex.Escape(answerShape) + "\b"
                    If Regex.IsMatch(sentence, pattern, RegexOptions.IgnoreCase) Then
                        Return True
                    End If
                Next

                Return False
            End Function



            Public Function FindRelationships(text As String, ByRef patterns As Dictionary(Of String, String)) As List(Of String)
                Dim relationships As New List(Of String)()
                patterns.Add("is-a", "(.*?) is-a (.*?)")
                patterns.Add("part-whole", "(.*?) (?:has|have|contains) (?:a|an|the)? (.*?)")
                patterns.Add("has-property", "(.*?) (?:has|have) (.*?)")
                patterns.Add("used-for", "(.*?) (?:is|are|used) (?:for|to) (.*?)")
                patterns.Add("located-at", "(.*?) (?:is|are|located) (?:at|in) (.*?)")
                patterns.Add("derived-from", "(.*?) (?:is|are) derived from (.*?)")

                ' Iterate through the patterns dictionary
                For Each pattern In patterns
                    Dim regexPattern As String = pattern.Value
                    Dim matches As MatchCollection = Regex.Matches(text, regexPattern, RegexOptions.IgnoreCase)

                    ' Extract the matched relationships and add them to the list
                    For Each match As Match In matches
                        Dim relationship As String = $"{match.Groups(1).Value.Trim()} - {pattern.Key} - {match.Groups(2).Value.Trim()}"
                        relationships.Add(relationship)
                    Next
                Next

                Return relationships
            End Function

            ''' <summary>
            ''' Predicts the position of an entity relative to the focus term within the context words.
            ''' </summary>
            ''' <param name="contextWords">The context words.</param>
            ''' <param name="focusTerm">The focus term.</param>
            ''' <returns>The predicted entity position.</returns>
            Public Function PredictEntityPosition(contextWords As List(Of String), focusTerm As String) As String
                Dim termIndex As Integer = contextWords.IndexOf(focusTerm)

                If termIndex >= 0 Then
                    If termIndex < contextWords.Count - 1 Then
                        Return "After"
                    ElseIf termIndex > 0 Then
                        Return "Before"
                    End If
                End If

                Return "None"
            End Function
        End Class

        '            If entityTypes.Count = 0 AndAlso prediction <> EntityPositionPrediction.None Then
        '                Dim isLowConfidenceEntity As Boolean = (prediction = EntityPositionPrediction.After AndAlso i > focusIndex) OrElse
        '                                                       (prediction = EntityPositionPrediction.Before AndAlso i < focusIndex)
        ' Class for handling entity detection, extraction, and manipulation
        Public Class iExtract




            Public Shared Property bornInPattern As String = "\b([A-Z][a-z]+)\b relation \(born in\) \b([A-Z][a-z]+)\b"

            Public Shared Property datePattern As String = "\b\d{4}\b"

            Public Shared Property organizationPattern As String = "\b([A-Z][a-z]+)\b"

            ' Regular expression patterns for different entity types
            Public Shared Property personPattern As String = "\b([A-Z][a-z]+)\b"

            Public Shared Property programmingLanguagePattern As String = "\b[A-Z][a-z]+\.[a-z]+\b"

            Public Shared Property wroteBookPattern As String = "\b([A-Z][a-z]+)\b \(wrote a book called\) \b([A-Z][a-z]+)\b"

            Private Property patterns As Dictionary(Of String, String)
                Get

                End Get
                Set(value As Dictionary(Of String, String))

                End Set
            End Property

            ''' <summary>
            ''' Extracts context Entitys , As Well As thier context words 
            ''' </summary>
            ''' <param name="itext"></param>
            ''' <param name="contextSize"></param>
            ''' <param name="entities">Values to retrieve context for</param>
            ''' <returns></returns>
            Public Shared Function ExtractCapturedContextIntext(ByRef itext As String, ByVal contextSize As Integer, ByRef entities As List(Of String)) As List(Of CapturedContent)
                Dim wordsWithContext As New List(Of CapturedContent)()

                ' Create a regular expression pattern for matching the entities
                Dim pattern As String = "(" + String.Join("|", entities.Select(Function(e) Regex.Escape(e))) + ")"

                ' Add context placeholders to the pattern
                Dim contextPattern As String = "(?:\S+\s+){" + contextSize.ToString() + "}"

                ' Combine the entity pattern and the context pattern
                pattern = contextPattern + "(" + pattern + ")" + contextPattern

                ' Find all matches in the text
                Dim matches As MatchCollection = Regex.Matches(itext, pattern)

                ' Iterate over the matches and extract the words with context
                For Each match As Match In matches
                    Dim sequence As String = match.Value.Trim()
                    Dim word As String = match.Groups(1).Value.Trim()
                    Dim precedingContext As String = match.Groups(2).Value.Trim()
                    Dim followingContext As String = match.Groups(3).Value.Trim()


                    Dim precedingWords As List(Of String) = Split(precedingContext, " "c, StringSplitOptions.RemoveEmptyEntries).ToList
                    Dim followingWords As List(Of String) = Split(followingContext, " "c, StringSplitOptions.RemoveEmptyEntries).ToList

                    Dim capturedWord As New CapturedContent(word, precedingWords, followingWords)
                    wordsWithContext.Add(capturedWord)
                Next

                Return wordsWithContext
            End Function

            ''' <summary>
            ''' Given an important Sentence Extract its surrounding context Sentences 
            ''' </summary>
            ''' <param name="Text"></param>
            ''' <param name="ImportantSentence">Important Sentence to match</param>
            ''' <param name="ConTextInt">Number of Sentences Either Side</param>
            ''' <returns></returns>
            Public Shared Function ExtractContextSentences(ByRef Text As String, ByRef ImportantSentence As String, ByRef ConTextInt As Integer) As List(Of String)
                Dim ContextSentences As New List(Of String)
                Dim CurrentSentences As New List(Of String)
                Dim Count As Integer = 0

                For Each Sent In Split(Text, ".")
                    CurrentSentences.Add(Sent)
                    Count += 1
                    If Sent = ImportantSentence Then
                        'Get Previous sentences

                        For i = 0 To ConTextInt
                            Dim Index = Count - 1
                            If Index >= 0 Or Index < CurrentSentences.Count Then

                                ContextSentences.Add(CurrentSentences(Index))

                            End If
                        Next
                        ContextSentences.Add(ImportantSentence)
                        'GetFollowing Sentences
                        For i = 0 To ConTextInt
                            If Count + i < CurrentSentences.Count Then
                                ContextSentences.Add(CurrentSentences(Count + i))
                            End If
                        Next
                    End If
                Next
                Return ContextSentences
            End Function

            ''' <summary>
            ''' Given an important Sentence Extract its surrounding context Sentences -
            ''' In some cases it may be prudent to grab only a single sentence before and multiple sentences after 
            ''' important to know which context is important in which instance
            ''' </summary>
            ''' <param name="text">Document</param>
            ''' <param name="importantSentence">Sentence to be matched</param>
            ''' <param name="numContextSentencesBefore">number of</param>
            ''' <param name="numContextSentencesAfter">number of</param>
            ''' <returns></returns>
            Public Shared Function ExtractContextSentences(ByVal text As String, ByVal importantSentence As String, ByVal numContextSentencesBefore As Integer, ByVal numContextSentencesAfter As Integer) As List(Of String)
                Dim contextSentences As New List(Of String)
                Dim allSentences As List(Of String) = text.Split("."c).ToList()
                Dim sentenceIndex As Integer = allSentences.IndexOf(importantSentence)

                ' Get sentences before the important sentence
                Dim startIndex As Integer = Math.Max(0, sentenceIndex - numContextSentencesBefore)
                For i = startIndex To sentenceIndex - 1
                    contextSentences.Add(allSentences(i))
                Next

                ' Add the important sentence
                contextSentences.Add(importantSentence)

                ' Get sentences after the important sentence
                Dim endIndex As Integer = Math.Min(sentenceIndex + numContextSentencesAfter, allSentences.Count - 1)
                For i = sentenceIndex + 1 To endIndex
                    contextSentences.Add(allSentences(i))
                Next

                Return contextSentences
            End Function

            ''' <summary>
            ''' Extracts the entity's discovered in the user text(sentence level) 
            ''' using the entity list provided
            ''' </summary>
            ''' <param name="UserSent">user text</param>
            ''' <param name="Entitylist">Single entity list</param>
            ''' <returns>a list of discovered entity's based on the list / and sentences containing them</returns>
            Public Shared Function ExtractDiscoveredEntitysInText(ByRef UserSent As String,
                                          ByRef Entitylist As List(Of Entity)) As List(Of DiscoveredEntity)

                Dim DiscoveredEntitys As New List(Of DiscoveredEntity)
                For Each ent In Entitylist
                    For Each sent In Split(UserSent, ".").ToList
                        If sent.Contains(" " & ent.Type & " ") Then
                            Dim Discovered As New DiscoveredEntity
                            Discovered.EntityList = ent.Type
                            Discovered.DiscoveredSentence = sent
                            Discovered.DiscoveredKeyWord = ent.Value
                            DiscoveredEntitys.Add(Discovered)
                        End If

                    Next
                Next
                If DiscoveredEntitys.Count > 0 Then
                    Return DiscoveredEntitys
                Else
                    Return Nothing

                End If
            End Function

            ''' <summary>
            ''' Extracts Entity With Context; 
            ''' 
            ''' </summary>
            ''' <param name="text">doc</param>
            ''' <param name="entity">Entity Value</param>
            ''' <param name="contextLength"></param>
            ''' <returns>a concat string</returns>
            Public Shared Function ExtractEntityContextFromText(ByVal text As String,
                                                ByVal entity As String,
                                                Optional contextLength As Integer = 1) As String
                Dim entityIndex As Integer = text.IndexOf(entity)
                Dim contextStartIndex As Integer = Math.Max(0, entityIndex - contextLength)
                Dim contextEndIndex As Integer = Math.Min(text.Length - 1, entityIndex + entity.Length + contextLength)

                Return text.Substring(contextStartIndex, contextEndIndex - contextStartIndex + 1)
            End Function

            ''' <summary>
            ''' Extracts patterns from the text and replaces detected entities with asterisks.
            ''' this replaces the entity's regardless of type , 
            ''' just supply the list and all items with be replaced by the pattern identifier
            ''' </summary>
            ''' <param name="text">The text to extract patterns from.</param>
            ''' <param name="EntityList">A list of entities to detect and replace.</param>
            ''' <returns>The extracted pattern with detected entities replaced by asterisks.</returns>
            Public Shared Function ExtractEntityPatternInText(ByRef text As String, ByRef EntityList As List(Of String)) As String
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("EntityList")
                End If

                Dim Entitys As New List(Of String)
                Dim Str As String = text
                If iDetect.Detected(text, EntityList) = True Then
                    Entitys = iDetect.Detect(text, EntityList)

                    Str = iDiscover.DiscoverShape(Str, Entitys)
                    Str = iTransform.TransformText(Str)
                End If
                Return Str
            End Function

            ' Class for entity extraction and text transformation
            ''' <summary>
            ''' Extracts patterns from the text, 
            ''' replaces detected entities with asterisks, 
            ''' and replaces the entity label with asterisks. 
            ''' This is to replace a specific entity in the text
            ''' </summary>
            ''' 
            ''' <param name="Text">The text to extract patterns from.</param>
            ''' <param name="Entitylist">A list of entities(values) to detect and replace.</param>
            ''' <param name="EntityLabel">The label(entityType) to replace detected entities with.</param>
            ''' <returns>The extracted pattern with detected entities and the entity label replaced by asterisks.</returns>
            Public Shared Function ExtractEntityPatternInText(ByRef Text As String, Entitylist As List(Of String), ByRef EntityLabel As String) As String
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If Entitylist Is Nothing Then
                    Throw New ArgumentNullException("EntityList")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("EntityLabel")
                End If
                Dim Entitys As New List(Of String)
                Dim Str As String = Text
                If iDetect.Detected(Text, Entitylist) = True Then
                    Entitys = iDetect.Detect(Text, Entitylist)
                    Str = iDiscover.DiscoverShape(Str, Entitys)
                    Str = iTransform.TransformText(Str)
                    Str = Str.Replace("[" & EntityLabel & "]", "*")
                End If
                Return Str
            End Function

            ''' <summary>
            ''' Extracts the entity's discovered in the user text(sentence level) 
            ''' using the entity list provided
            ''' </summary>
            ''' <param name="UserSent">user text</param>
            ''' <param name="Entitylist">Single entity list</param>
            ''' <returns>a list of extracted entity's based on the list</returns>
            Public Shared Function ExtractEntitys(ByRef UserSent As String, ByRef Entitylist As List(Of Entity)) As List(Of DiscoveredEntity)

                Dim DiscoveredEntitys As New List(Of DiscoveredEntity)
                For Each ent In Entitylist
                    If UserSent.Contains(" " & ent.Type & " ") Then
                        Dim Discovered As New DiscoveredEntity
                        Discovered.EntityList = ent.Type
                        Discovered.DiscoveredSentence = UserSent
                        Discovered.DiscoveredKeyWord = ent.Value
                        DiscoveredEntitys.Add(Discovered)
                    End If

                Next
                If DiscoveredEntitys.Count > 0 Then
                    Return DiscoveredEntitys
                Else
                    Return Nothing

                End If
            End Function

            ''' <summary>
            ''' Searches for important sentences in text , identified by the presence of an entity from this list
            ''' These lists can be specific to a particular topic or entity or a search query
            ''' </summary>
            ''' <param name="Text"></param>
            ''' <param name="EntityList">Entity list</param>
            ''' <param name="WithContext"></param>
            ''' <param name="NumberOfContextSentences"></param>
            ''' <returns></returns>
            Public Shared Function ExtractImportantSentencesInText(ByRef Text As String, EntityList As List(Of String), Optional WithContext As Boolean = False,
                                                        Optional NumberOfContextSentences As Integer = 0) As List(Of String)
                Dim Sents As New List(Of String)

                Select Case WithContext
                    Case False

                        For Each Sent In Split(Text, ".")
                            For Each Entity In EntityList
                                If Sent.Contains(Entity) Then
                                    Sents.Add(Sent)
                                End If
                            Next

                        Next
                        Return Sents.Distinct.ToList
                    Case True

                        For Each Sent In Split(Text, ".")
                            For Each Entity In EntityList
                                If Sent.ToLower.Contains(Entity.ToLower) Then
                                    Sents.AddRange(ExtractContextSentences(Text, Sent, NumberOfContextSentences))
                                End If
                            Next

                        Next
                        Return Sents.Distinct.ToList
                End Select


                Return Sents.Distinct.ToList
            End Function

            ''' <summary>
            ''' grabs important sentences from text based on the entity list provided . 
            ''' (values or terms or noun phrases or verb phrases) as this is a sentence level search 
            ''' it also grabs the context sentences surrounding it based on the inputs
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="entityList"></param>
            ''' <param name="numContextSentencesBefore"></param>
            ''' <param name="numContextSentencesAfter"></param>
            ''' <returns></returns>
            Public Shared Function ExtractImportantSentencesInText(ByVal text As String, ByVal entityList As List(Of String), ByVal numContextSentencesBefore As Integer, ByVal numContextSentencesAfter As Integer) As List(Of String)
                Dim importantSentences As New List(Of String)

                For Each sentence In text.Split("."c)
                    For Each entity In entityList
                        If sentence.ToLower.Contains(entity.ToLower) Then
                            ' Add the current sentence and the context sentences
                            importantSentences.AddRange(ExtractContextSentences(text, sentence, numContextSentencesBefore, numContextSentencesAfter))
                            Exit For ' Break out of the inner loop if the entity is found in the sentence
                        End If
                    Next
                Next

                Return importantSentences.Distinct().ToList()
            End Function

            ''' <summary>
            ''' Gets important Sentences in text with or without context
            ''' </summary>
            ''' <param name="Text"></param>
            ''' <param name="EntityList"></param>
            ''' <param name="WithContext"></param>
            ''' <param name="NumberOfContextSentencesBefore"></param>
            ''' <param name="NumberOfContextSentencesAfter"></param>
            ''' <returns></returns>
            Public Shared Function ExtractImportantSentencesInText(ByRef Text As String, EntityList As List(Of String), Optional WithContext As Boolean = False,
                                                        Optional NumberOfContextSentencesBefore As Integer = 0,
                                                        Optional NumberOfContextSentencesAfter As Integer = 0) As List(Of String)
                Dim importantSentences As New List(Of String)

                For Each sentence In Split(Text, ".")
                    For Each entity In EntityList
                        If sentence.ToLower.Contains(entity.ToLower) Then
                            importantSentences.Add(sentence)
                            Exit For ' Break out of the inner loop if the entity is found in the sentence
                        End If
                    Next
                Next

                If WithContext Then
                    Dim sentencesWithContext As New List(Of String)
                    For Each sentence In importantSentences
                        sentencesWithContext.AddRange(ExtractContextSentences(Text, sentence, NumberOfContextSentencesBefore, NumberOfContextSentencesAfter))
                    Next
                    Return sentencesWithContext
                Else
                    Return importantSentences
                End If
            End Function

            ''' <summary>
            ''' Extracts patterns from the text and replaces detected entities with asterisks.
            ''' </summary>
            ''' <param name="text">The text to extract patterns from.</param>
            ''' <param name="EntityList">A list of entities to detect and replace.</param>
            ''' <returns>The extracted pattern with detected entities replaced by asterisks.</returns>
            Public Shared Function ExtractPattern(ByRef text As String, ByRef EntityList As List(Of String)) As String
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("EntityList")
                End If

                Dim Entitys As New List(Of String)
                Dim Str As String = text
                If iDetect.Detected(text, EntityList) = True Then
                    Entitys = iDetect.Detect(text, EntityList)

                    Str = iDiscover.DiscoverShape(Str, Entitys)
                    Str = iTransform.TransformText(Str)
                End If
                Return Str
            End Function

            ' Class for entity extraction and text transformation
            ''' <summary>
            ''' Extracts patterns from the text, replaces detected entities with asterisks, and replaces the entity label with asterisks.
            ''' </summary>
            ''' <param name="Text">The text to extract patterns from.</param>
            ''' <param name="Entitylist">A list of entities to detect and replace.</param>
            ''' <param name="EntityLabel">The label to replace detected entities with.</param>
            ''' <returns>The extracted pattern with detected entities and the entity label replaced by asterisks.</returns>
            Public Shared Function ExtractPattern(ByRef Text As String, Entitylist As List(Of String), ByRef EntityLabel As String) As String
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If Entitylist Is Nothing Then
                    Throw New ArgumentNullException("EntityList")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("EntityLabel")
                End If
                Dim Entitys As New List(Of String)
                Dim Str As String = Text
                If iDetect.Detected(Text, Entitylist) = True Then
                    Entitys = iDetect.Detect(Text, Entitylist)
                    Str = iDiscover.DiscoverShape(Str, Entitys)
                    Str = iTransform.TransformText(Str)
                    Str = Str.Replace("[" & EntityLabel & "]", "*")
                End If
                Return Str
            End Function

            ''' <summary>
            ''' 1. **Dependency Parsing**:
            '''   - Input sentence: "The cat is a mammal."
            '''   - Expected output: ["cat - is-a - mammal"]
            ''' Subject - LinkingVerb - Object
            ''' Is a / ...is defined as....
            ''' x is a y (X) & (Y) are dependents... 
            ''' also determines the answer [what] is a ......(x) answer Y 
            ''' this is a single linking verb...
            ''' it will need to be used for every stored concept 
            ''' which should also have an associated question; 
            ''' this extraction is data collection and needs no question to be present to extract answer relations
            ''' 
            ''' </summary>
            ''' <param name="text"></param>
            ''' <returns></returns>
            Public Shared Function ExtractPredicateRelations(text As String, ByRef LinkingVerb As String) As List(Of String)
                ' Implement your custom dependency parsing logic here
                ' Analyze the sentence and extract the relationships

                Dim relationships As New List(Of String)()
                For Each sentence In Split(text, ".)").ToList

                    ' Example relationship extraction logic
                    If sentence.ToLower.Contains(" " & LinkingVerb.ToLower & " ") Then
                        Dim relationParts() As String = sentence.Split(" " & LinkingVerb.ToLower & " ")
                        Dim subject As String = relationParts(0).Trim()
                        Dim iobject As String = relationParts(1).Trim()

                        Dim relationship As String = $"{subject} -  {LinkingVerb } - {iobject}"
                        relationships.Add(relationship)
                    End If
                Next
                Return relationships
            End Function

            Public Shared Function ExtractUniqueWordsInText(ByVal text As String) As List(Of String)
                Dim regex As New Regex("\b\w+\b") ' Matches individual words
                Dim matches As MatchCollection = regex.Matches(text)
                Dim uniqueEntities As New List(Of String)

                For Each match As Match In matches
                    Dim entity As String = match.Value
                    If Not uniqueEntities.Contains(entity) Then
                        uniqueEntities.Add(entity)
                    End If
                Next

                Return uniqueEntities
            End Function

            ''' <summary>
            ''' Returns a List of WordsWithCOntext with the Focus word at the center surrounded by its context words,
            ''' it can be a useful pattern chunk which can be used for prediction as a context ngram (min-3)
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="focusTerm"></param>
            ''' <param name="precedingWordsCount"></param>
            ''' <param name="followingWordsCount"></param>
            ''' <returns></returns>
            Public Shared Function ExtractWordsWithContext(text As String, focusTerm As String, precedingWordsCount As Integer, followingWordsCount As Integer) As List(Of WordWithContext)
                Dim words As List(Of String) = text.Split(" "c).ToList()
                Dim focusIndex As Integer = words.IndexOf(focusTerm)

                Dim capturedWordsWithEntityContext As New List(Of WordWithContext)()

                If focusIndex <> -1 Then
                    Dim startIndex As Integer = Math.Max(0, focusIndex - precedingWordsCount)
                    Dim endIndex As Integer = Math.Min(words.Count - 1, focusIndex + followingWordsCount)

                    For i As Integer = startIndex To endIndex
                        Dim word As String = words(i)

                        Dim wordWithContext As New WordWithContext() With {
                    .Word = word,
                          .IsFocusTerm = (i = focusIndex),
                    .IsPreceding = (i < focusIndex),
                    .IsFollowing = (i > focusIndex)
                }

                        capturedWordsWithEntityContext.Add(wordWithContext)
                    Next
                End If

                Return capturedWordsWithEntityContext
            End Function

            ''' <summary>
            ''' Returns a new string with the Focus word at the center surrounded by its context words,
            ''' it can be a useful pattern chunk which can be used for prediction as a context ngram (min-3)
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="Word"></param>
            ''' <param name="contextWords"></param>
            ''' <returns></returns>
            Public Shared Function ExtractWordWithContext(text As String, Word As String, contextWords As Integer) As String
                Dim words As String() = text.Split(" "c)

                Dim capturedWord As String = ""
                For i As Integer = 0 To words.Length - 1

                    Dim Tword As String = words(i)
                    If Word = Tword Then
                        Dim startIndex As Integer = Math.Max(0, i - contextWords)
                        Dim endIndex As Integer = Math.Min(words.Length - 1, i + contextWords)
                        capturedWord = String.Join(" ", words, startIndex, endIndex - startIndex + 1)

                    End If
                Next

                Return capturedWord
            End Function

            ''' <summary>
            ''' Attempts to find the entity's in the user text,(Paragraph level)
            ''' returning all the entity's discovered
            ''' </summary>
            ''' <param name="UserText"></param>
            ''' <param name="EntityLists">list of entity lists</param>
            ''' <returns>the discovered entity's are return in a structured data form</returns>
            Public Shared Function GetEntitysFromText(ByRef UserText As String, EntityLists As List(Of Entity)) As List(Of DiscoveredEntity)
                'Split Sentences
                Dim Sentences As List(Of String) = SentenceSplitter.GetSentences(UCase(UserText))
                Dim DiscoveredEntitys As New List(Of DiscoveredEntity)

                'Based On Entity-lists - Discovers Entities
                For Each item In Sentences

                    DiscoveredEntitys.AddRange(iExtract.ExtractEntitys(item, EntityLists))

                Next
                If DiscoveredEntitys IsNot Nothing Then

                    If DiscoveredEntitys.Count > 0 Then
                        Return DiscoveredEntitys
                    Else
                        Return Nothing
                    End If
                Else
                    Return Nothing
                End If

            End Function

            '3. **Regular Expressions**:
            '   - Input text: "John wrote a book called 'The Adventures of Tom'."
            '   - Pattern: "[A-Z][a-z]+ wrote a book called '[A-Z][a-z]+'"
            '   - Expected output: ["John wrote a book called 'The Adventures of Tom'"]
            Public Function DetectRelations(text As String) As List(Of String)
                Dim relations As New List(Of String)

                Dim bornInMatches As MatchCollection = Regex.Matches(text, bornInPattern)
                For Each match As Match In bornInMatches
                    Dim person As String = match.Groups(1).Value
                    Dim location As String = match.Groups(2).Value
                    relations.Add($"Person: {person}, Relation: born in, Location: {location}")
                Next

                Dim wroteBookMatches As MatchCollection = Regex.Matches(text, wroteBookPattern)
                For Each match As Match In wroteBookMatches
                    Dim person As String = match.Groups(1).Value
                    Dim bookTitle As String = match.Groups(2).Value
                    relations.Add($"Person: {person}, Relation: wrote a book called, Book Title: {bookTitle}")
                Next

                Return relations
            End Function

            ''' <summary>
            ''' Given an Answer shape , Detect and 
            ''' Extract All Answers and context sentences
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="answerShapes"></param>
            ''' <param name="contextSentences"></param>
            ''' <returns></returns>
            Public Function ExtractAnswersWithContextFromText(text As String, answerShapes As List(Of String), contextSentences As Integer) As List(Of String)
                Dim answers As New List(Of String)()

                ' Split the text into sentences
                Dim sentences As String() = Split(text, ".", StringSplitOptions.RemoveEmptyEntries)

                ' Iterate through each sentence and check for potential answer sentences
                For i As Integer = 0 To sentences.Length - 1
                    Dim sentence As String = sentences(i).Trim()

                    ' Check if the sentence matches any of the answer shapes
                    If iDiscover.MatchesAnswerShape(sentence, answerShapes) Then
                        ' Add the current sentence and the context sentences to the list of potential answer sentences
                        Dim startIndex As Integer = Math.Max(0, i - contextSentences)
                        Dim endIndex As Integer = Math.Min(i + contextSentences, sentences.Length - 1)

                        Dim answer As String = String.Join(" ", sentences, startIndex, endIndex - startIndex + 1).Trim()
                        answers.Add(answer)
                    End If
                Next

                Return answers
            End Function

            ''' <summary>
            ''' catches words , context etc by entitys and concat context chunk
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="entities"></param>
            ''' <param name="contextSize"></param>
            ''' <returns>complex object</returns>
            Public Function ExtractCapturedContextMatchesInTextByContext(ByVal text As String, ByVal entities As List(Of String),
                                                                ByVal contextSize As Integer) As List(Of (Word As String, PrecedingWords As List(Of String), FollowingWords As List(Of String), Position As Integer))
                Dim wordsWithContext As New List(Of (Word As String, PrecedingWords As List(Of String), FollowingWords As List(Of String), Position As Integer))

                ' Create a regular expression pattern for matching the entities
                Dim pattern As String = "(" + String.Join("|", entities.Select(Function(e) Regex.Escape(e))) + ")"

                ' Add context placeholders to the pattern
                Dim contextPattern As String = "(?:\S+\s+){" + contextSize.ToString() + "}"

                ' Combine the entity pattern and the context pattern
                pattern = contextPattern + "(" + pattern + ")" + contextPattern

                ' Find all matches in the text
                Dim matches As MatchCollection = Regex.Matches(text, pattern)

                ' Iterate over the matches and extract the words with context and position
                For Each match As Match In matches
                    Dim wordWithContext As String = match.Groups(0).Value.Trim()
                    Dim word As String = match.Groups(1).Value.Trim()
                    Dim precedingContext As String = match.Groups(2).Value.Trim()
                    Dim followingContext As String = match.Groups(3).Value.Trim()
                    Dim position As Integer = match.Index

                    Dim precedingWords As List(Of String) = Split(precedingContext, " "c, StringSplitOptions.RemoveEmptyEntries).ToList()
                    Dim followingWords As List(Of String) = Split(followingContext, " "c, StringSplitOptions.RemoveEmptyEntries).ToList()

                    wordsWithContext.Add((word, precedingWords, followingWords, position))
                Next

                Return wordsWithContext
            End Function

            ''' <summary>
            ''' Creates A context item based item the inputs and searches for matches
            ''' returning the item plus its context and entitys etc
            ''' </summary>
            ''' <param name="itext"></param>
            ''' <param name="contextSize"></param>
            ''' <param name="entities"></param>
            ''' <returns></returns>
            Public Function ExtractCapturedWordsinTextByContext(ByRef itext As String, ByVal contextSize As Integer, ByRef entities As List(Of String)) As List(Of CapturedWord)
                Dim wordsWithContext As New List(Of CapturedWord)()

                ' Create a regular expression pattern for matching the entities
                Dim pattern As String = "(" + String.Join("|", entities.Select(Function(e) Regex.Escape(e))) + ")"

                ' Add context placeholders to the pattern
                Dim contextPattern As String = "(?:\S+\s+){" + contextSize.ToString() + "}"

                ' Combine the entity pattern and the context pattern
                pattern = contextPattern + "(" + pattern + ")" + contextPattern

                ' Find all matches in the text
                Dim matches As MatchCollection = Regex.Matches(itext, pattern)

                ' Iterate over the matches and extract the words with context
                For Each match As Match In matches
                    Dim sequence As String = match.Value.Trim()
                    Dim word As String = match.Groups(1).Value.Trim()
                    Dim precedingContext As String = match.Groups(2).Value.Trim()
                    Dim followingContext As String = match.Groups(3).Value.Trim()

                    Dim precedingWords As List(Of String) = Split(precedingContext, " "c, StringSplitOptions.RemoveEmptyEntries).ToList
                    Dim followingWords As List(Of String) = Split(followingContext, " "c, StringSplitOptions.RemoveEmptyEntries).ToList

                    Dim capturedWord As New CapturedWord(word, precedingWords, followingWords, "", "")
                    wordsWithContext.Add(capturedWord)
                Next

                Return wordsWithContext
            End Function
            ''' <summary>
            ''' 2. **Named Entity Recognition (NER)**:
            '''  - Input sentence: "John Smith and Sarah Johnson went to New York."
            '''   - Expected output: ["John Smith", "Sarah Johnson", "New York"]
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <param name="Entitys">list of entity values</param>
            ''' <param name="EntityLabel">Entity label</param>
            ''' <returns></returns>
            Public Function ExtractNamedEntities(sentence As String, ByRef Entitys As List(Of String), ByRef EntityLabel As String) As List(Of Entity)
                ' Implement your custom named entity recognition logic here
                ' Analyze the sentence and extract the named entities

                Dim entities As New List(Of Entity)

                ' Example named entity extraction logic
                Dim words() As String = sentence.Split(" "c)
                For i As Integer = 0 To words.Length - 1
                    For Each item In Entitys
                        If item.ToLower = words(i) Then
                            Dim ent As New Entity
                            ent.Type = EntityLabel
                            ent.Value = words(i)
                            entities.Add(ent)
                        End If
                    Next

                Next

                Return entities
            End Function

            ''' <summary>
            ''' Uses a list of resolved antecedents 
            ''' and detected entitys to discover possible answers in the text related to the antecedents and entitys, 
            ''' these sentences possibly contain rich sentences which can be used to find or formulate an answer query pool
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="resolvedAntecedents">Here the antecdents of , ie: John jumps. He was sitting on the wall = he , 
            ''' After resolving he actually he refers to john also so it should also be added </param>
            ''' <param name="resolvedEntities">entitys such as <person> or <named person> or <male name> etc</male></named></person></param>
            ''' <returns></returns>
            Public Function ExtractPotentialAnswers(text As String, resolvedAntecedents As List(Of String), resolvedEntities As List(Of String)) As List(Of String)
                Dim answers As New List(Of String)()

                ' Split the text into sentences
                Dim sentences As String() = Split(text, ".", StringSplitOptions.RemoveEmptyEntries)

                ' Iterate through each sentence and check for potential answer sentences
                For Each sentence In sentences
                    ' Check if the sentence contains any of the resolved antecedents or entities
                    If iDiscover.ContainsResolvedAntecedentsOrEntities(sentence, resolvedAntecedents, resolvedEntities) Then

                        ' Add the sentence to the list of potential answer sentences
                        answers.Add(sentence.Trim())
                    End If
                Next

                Return answers
            End Function

            '4. **Chunking**:
            '   - Input sentence: "The cat chased the mouse."
            '   - Expected output: ["cat - NN", "mouse - NN"]
            Public Function ExtractRelationsUsingChunking(sentence As String) As List(Of String)
                ' Implement your custom chunking logic here
                ' Analyze the sentence and extract the relationships

                Dim relationships As New List(Of String)()

                ' Example relationship extraction logic
                Dim chunks() As String = sentence.Split(" "c)
                For i As Integer = 0 To chunks.Length - 1
                    If chunks(i).StartsWith("B-") Then
                        Dim relationParts() As String = chunks(i).Split("-"c)
                        Dim relationType As String = relationParts(1)
                        Dim entity As String = chunks(i + 1)

                        Dim relationship As String = $"{entity} - {relationType}"
                        relationships.Add(relationship)
                    End If
                Next

                Return relationships
            End Function

            ''' <summary>
            '''  1. **Dependency Parsing* example*:
            '''  - Input sentence: "The cat is a mammal."
            '''  - Expected output: ["cat - is-a - mammal"]
            ''' </summary>
            ''' <param name="text"></param>
            ''' <returns></returns>
            Public Function ExtractRelationsUsingDependencyParsing(text As String) As List(Of String)
                ' Implement your custom dependency parsing logic here
                ' Analyze the sentence and extract the relationships

                Dim relationships As New List(Of String)()
                For Each sentence In Split(text, ".)").ToList

                    ' Example relationship extraction logic
                    If sentence.Contains("is-a") Then
                        Dim relationParts() As String = sentence.Split(" is-a ")
                        Dim subject As String = relationParts(0).Trim()
                        Dim iobject As String = relationParts(1).Trim()

                        Dim relationship As String = $"{subject} - is-a - {iobject}"
                        relationships.Add(relationship)
                    End If
                Next
                Return relationships
            End Function

            ''' <summary>
            '''5. **Semantic Role Labeling (SRL)**:
            '''  - Input sentence: "The cat is chasing the mouse."
            '''  input Sentence desired [NN] [VBZ] [NN]   or  [NP] [VP] [NP]
            '''   - Expected output: ["The cat - is chasing - the mouse"]
            '''   ' Analyze the sentence and extract the relationships(POS_TAGGED)
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <returns></returns>
            Public Function ExtractSemanticPredicates(sentence As String) As List(Of String)
                ' Implement your custom semantic role labeling logic here


                Dim relationships As New List(Of String)()

                ' Example relationship extraction logic
                If sentence.Contains("VBZ") AndAlso sentence.Contains("NN") Then
                    Dim verbIndex As Integer = sentence.IndexOf("VBZ") Or sentence.IndexOf("VP")
                    Dim nounIndex As Integer = sentence.IndexOf("NN") Or sentence.IndexOf("NP")

                    Dim subject As String = sentence.Substring(0, verbIndex).Trim()
                    Dim verb As String = sentence.Substring(verbIndex, nounIndex - verbIndex + 2).Trim()
                    Dim iobject As String = sentence.Substring(nounIndex + 2).Trim()

                    Dim relationship As String = $"{subject} - {verb} - {iobject}"
                    relationships.Add(relationship)
                End If

                Return relationships
            End Function

            '5. **Semantic Role Labeling (SRL)**:
            '   - Input sentence: "The cat is chasing the mouse."
            '   - Expected output: ["The cat - is chasing - the mouse"]
            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <returns></returns>
            Public Function ExtractSemanticRoles(sentence As String) As List(Of String)
                ' Implement your custom semantic role labeling logic here
                ' Analyze the sentence and extract the relationships

                Dim relationships As New List(Of String)()

                ' Example relationship extraction logic
                If sentence.Contains("VBZ") AndAlso sentence.Contains("NN") Then
                    Dim verbIndex As Integer = sentence.IndexOf("VBZ")
                    Dim nounIndex As Integer = sentence.IndexOf("NN")

                    Dim subject As String = sentence.Substring(0, verbIndex).Trim()
                    Dim verb As String = sentence.Substring(verbIndex, nounIndex - verbIndex + 2).Trim()
                    Dim iobject As String = sentence.Substring(nounIndex + 2).Trim()

                    Dim relationship As String = $"{subject} - {verb} - {iobject}"
                    relationships.Add(relationship)
                End If

                Return relationships
            End Function

            ''' <summary>
            ''' Requires Search Patterns: 
            ''' Extracts Entitys and the Sentences which they were detected in , 
            ''' this uses Search patterns to detect the entitys
            ''' 
            ''' </summary>
            ''' <param name="text"></param>
            ''' <returns></returns>
            Public Function ExtractSentencesAndEntities(text As String) As Dictionary(Of String, List(Of String))
                Dim sentences As New Dictionary(Of String, List(Of String))()

                ' Split the text into sentences
                Dim sentenceRegex As New Regex("(?<=\.|\?|\!)\s")
                Dim sentenceArray() As String = sentenceRegex.Split(text)

                ' Iterate through each sentence
                For Each sentence As String In sentenceArray
                    Dim entities As New List(Of String)()

                    ' Iterate through the patterns dictionary
                    For Each pattern In patterns
                        Dim regexPattern As String = pattern.Value
                        Dim match As Match = Regex.Match(sentence, regexPattern, RegexOptions.IgnoreCase)

                        ' Check if the sentence matches the pattern
                        If match.Success Then
                            ' Extract the entities from the matched pattern
                            entities.Add(match.Groups(1).Value.Trim())
                            entities.Add(match.Groups(2).Value.Trim())
                        End If
                    Next

                    ' Add the sentence and its extracted entities to the dictionary
                    sentences.Add(sentence, entities)
                Next

                Return sentences
            End Function

            ''' <summary>
            ''' captures sentences ending in ?
            ''' </summary>
            ''' <param name="input">text</param>
            ''' <returns>list of recognized question sentences</returns>
            Function ExtractSimpleQuestions(input As String) As List(Of String)
                ' Regular expression pattern to match questions
                Dim questionPattern As String = "([\w\s']+)(\?)"

                ' List to store extracted questions
                Dim questions As New List(Of String)()

                ' Match the pattern in the input text
                Dim regex As New Regex(questionPattern)
                Dim matches As MatchCollection = regex.Matches(input)

                ' Iterate over the matches and extract the questions
                For Each match As Match In matches
                    Dim question As String = match.Groups(1).Value.Trim()
                    questions.Add(question)
                Next

                Return questions
            End Function

            '3. **Regular Expressions**:
            '   - Input text: "John wrote a book called 'The Adventures of Tom'."
            '   - Pattern: "[A-Z][a-z]+ wrote a book called '[A-Z][a-z]+'"
            '   - Expected output: ["John wrote a book called 'The Adventures of Tom'"]
            Public Function ExtractUsingRegexSearchPattern(text As String, pattern As String) As List(Of String)
                Dim relationships As New List(Of String)()

                ' Use regular expression to match the desired pattern
                Dim matches As MatchCollection = Regex.Matches(text, pattern, RegexOptions.IgnoreCase)

                ' Extract the matched relationships and add them to the list
                For Each match As Match In matches
                    Dim relationship As String = match.Value
                    relationships.Add(relationship)
                Next

                Return relationships
            End Function

            Private Shared Function ContainsResolvedAntecedentsOrEntities(sentence As String, resolvedAntecedents As List(Of String), resolvedEntities As List(Of String)) As Boolean
                ' Check if the sentence contains any of the resolved antecedents or entities
                For Each antecedent In resolvedAntecedents
                    If sentence.Contains(antecedent) Then
                        Return True
                    End If
                Next

                For Each entity In resolvedEntities
                    If sentence.Contains(entity) Then
                        Return True
                    End If
                Next

                Return False
            End Function
        End Class

        '            Dim entityTypes As List(Of String) = GetEntityTypes(word)
        Public Class iGenerate

            Public Shared Function GenerateSummary(ByRef Text As String, ByRef Entitys As List(Of String)) As String
                ' Step 5: Generate the summary
                Return String.Join(vbNewLine, iExtract.ExtractImportantSentencesInText(Text, Entitys, True, 2))
            End Function

            Public Shared Function GenerateSummary(ByVal text As String, ByVal entities As List(Of String), ByVal numContextSentencesBefore As Integer, ByVal numContextSentencesAfter As Integer) As String
                ' Extract important sentences with context
                Dim importantSentences As List(Of String) = iExtract.ExtractImportantSentencesInText(text, entities, numContextSentencesBefore, numContextSentencesAfter)

                ' Generate the summary
                Dim summary As String = String.Join(". ", importantSentences)

                Return summary
            End Function

            'Dim answer As String = GenerateAnswer(answerType, entity)
            'Console.WriteLine(answer)
            Public Function GenerateAnswer(answerType As String, entity As String) As String
                ' Implement your custom answer generation logic here
                ' Generate an answer based on the answer type and entity

                Dim answer As String = ""

                ' Example answer generation logic
                Select Case answerType
                    Case "location"
                        answer = "The location of " & entity & " is [LOCATION]."
                    Case "person"
                        answer = "The person associated with " & entity & " is [PERSON]."
                    Case "organization"
                        answer = "The organization " & entity & " is [ORGANIZATION]."
                    Case "date"
                        answer = "The date related to " & entity & " is [DATE]."
                    Case "year"
                        answer = "The year associated with " & entity & " is [YEAR]."
                    Case "language"
                        answer = "The programming language " & entity & " is [LANGUAGE]."
                    Case "country"
                        answer = "The country associated with " & entity & " is [COUNTRY]."
                    Case Else
                        answer = "The information about " & entity & " is [INFORMATION]."
                End Select

                Return answer
            End Function

            'Possible Output: "Who is person(John)?"
            Public Function GenerateQuestionFromEntity(entity As String) As String
                ' Implement your custom question generation logic here
                ' Generate a question based on the given entity

                Dim question As String = ""

                ' Example question generation logic
                If entity.StartsWith("person") Then
                    question = "Who is " & entity & "?"
                ElseIf entity.StartsWith("organization") Then
                    question = "What is " & entity & "?"
                ElseIf entity.StartsWith("location") Then
                    question = "Where is " & entity & "?"
                Else
                    question = "What can you tell me about " & entity & "?"
                End If

                Return question
            End Function

            Public Function GenerateTextFromEntities(entities As List(Of String), storedSentences As List(Of String)) As String
                ' Implement your custom text generation logic here
                ' Generate text using the entities and stored sentences

                Dim generatedText As String = ""

                ' Example text generation logic
                For Each entity As String In entities
                    Dim matchingSentences As List(Of String) = iDiscover.FindSentencesWithEntity(entity, storedSentences)

                    ' Randomly select a sentence from the matching sentences
                    Dim random As New Random()
                    Dim selectedSentence As String = matchingSentences(random.Next(0, matchingSentences.Count))

                    ' Replace the entity tag with the actual entity in the selected sentence
                    Dim generatedSentence As String = selectedSentence.Replace(entity, "<<" & entity & ">>")

                    ' Append the generated sentence to the generated text
                    generatedText &= generatedSentence & " "
                Next

                Return generatedText.Trim()
            End Function
        End Class

        '        For i As Integer = startIndex To endIndex
        '            Dim word As String = words(i)
        Public Class iManage

            Public Shared Function CombineLISTS(ByVal entities1 As List(Of String), ByVal entities2 As List(Of String)) As List(Of String)
                Dim combinedEntities As New List(Of String)(entities1)
                combinedEntities.AddRange(entities2)
                Return combinedEntities
            End Function


            ''' <summary>
            ''' Get stored entity's
            ''' </summary>
            ''' <returns>List of stored entity's</returns>
            Public Shared Function GetEntityLists(ByRef ConnectionStr As String) As List(Of Entity)
                Dim DbSubjectLst As New List(Of Entity)
                Dim SQL As String = "SELECT * FROM Entity's "
                Using conn = New System.Data.OleDb.OleDbConnection(ConnectionStr)
                    Using cmd = New System.Data.OleDb.OleDbCommand(SQL, conn)
                        conn.Open()
                        Try
                            Dim dr = cmd.ExecuteReader()
                            While dr.Read()
                                Dim NewKnowledge As New Entity
                                NewKnowledge.Type = UCase(dr("EntityListName").ToString())
                                NewKnowledge.Value = UCase(dr("Entity's").ToString())

                                DbSubjectLst.Add(NewKnowledge)
                            End While
                        Catch e As Exception
                            ' Do some logging or something.
                            MessageBox.Show("There was an error accessing your data. GetDBConceptNYM: " & e.ToString())
                        End Try
                    End Using
                End Using
                Return DbSubjectLst
            End Function



            Public Shared Function StartsWithAny(str As String, values As IEnumerable(Of String)) As Boolean
                For Each value As String In values
                    If str.StartsWith(value) Then
                        Return True
                    End If
                Next

                Return False
            End Function
        End Class

        '        Dim prediction As EntityPositionPrediction = PredictEntityPosition(contextWords, focusTerm)
        Public Class Iresolve



            ' Identify antecedent indicators:
            '   - Look for pronouns or referencing words like "he," "she," "it," "they," "them," "that" in the sentence.
            '   - Check the preceding tokens to identify the most recent entity token with a matching type.
            '   - Use the identified entity as the antecedent indicator.


            '3. Antecedents:
            '   - Antecedents are the entities or objects that are referred to by pronouns or other referencing words in a sentence.
            '   - Antecedents are typically introduced in a sentence before the pronoun or referencing word. For example, "John went to the store. He bought some groceries."
            '   - Antecedents can be humans, objects, animals, or other entities. The choice of pronouns or referencing words depends on the gender and type of the antecedent. For example, "he" for a male, "she" for a female, "it" for an object, and "they" for multiple entities.


            '2. Locations:
            '   - Locations are places or areas where entities or objects exist or are situated.
            '   - Locations can be referred to using nouns that represent specific places, such as "home," "office," "school," "park," "store," "gym," "library," etc.
            '   - Locations can also be described using adjectives or prepositional phrases, such as "in the backyard," "at the beach," "on the street," "near the river," etc.


            '1. Objects:
            '   - Objects are typically referred to using nouns. Examples include "car," "book," "tree," "chair," "pen," etc.
            '   - Objects may have specific attributes or characteristics associated with them, such as color, size, shape, etc., which can be mentioned when referring to them.



            Public Shared Function CaptureWordsWithContext(text As String, entityList As List(Of String), contextWords As Integer) As List(Of String)
                Dim words As String() = text.Split(" "c)
                Dim capturedWords As New List(Of String)()

                For i As Integer = 0 To words.Length - 1
                    Dim word As String = words(i)
                    If entityList.Contains(word) Then
                        Dim startIndex As Integer = Math.Max(0, i - contextWords)
                        Dim endIndex As Integer = Math.Min(words.Length - 1, i + contextWords)
                        Dim capturedWord As String = String.Join(" ", words, startIndex, endIndex - startIndex + 1)
                        capturedWords.Add(capturedWord)
                    End If
                Next

                Return capturedWords
            End Function

            Public Shared Function FindAntecedent(words As String(), pronounIndex As Integer, entityList As List(Of String)) As String
                For i As Integer = pronounIndex - 1 To 0 Step -1
                    Dim word As String = words(i)
                    If entityList.Contains(word) Then
                        Return word
                    End If
                Next

                Return ""
            End Function

            Public Shared Function GetPronoun(word As String) As String
                ' Add mapping of pronouns to words as needed
                Select Case word
                    Case "he"
                        Return "him"
                    Case "she"
                        Return "her"
                    Case "it"
                        Return "its"
                    Case "they"
                        Return "them"
                    Case "them"
                        Return "them"
                    Case "that"
                        Return "that"
                    Case Else
                        Return ""
                End Select
            End Function

            ''' <summary>
            ''' Pronoun_mapping to normailized value
            ''' </summary>
            ''' <param name="word"></param>
            ''' <returns></returns>
            Public Shared Function GetPronounIndicator(word As String) As String
                ' Add mapping of pronouns to words as needed
                Select Case word
                    Case "shes"
                        Return "her"
                    Case "his"
                        Return "him"
                    Case "hers"
                        Return "her"
                    Case "her"
                        Return "her"
                    Case "him"
                        Return "him"
                    Case "he"
                        Return "him"
                    Case "she"
                        Return "her"
                    Case "its"
                        Return " it"
                    Case "it"
                        Return " it"
                    Case "they"
                        Return "them"
                    Case "thats"
                        Return "that"
                    Case "that"
                        Return "that"
                    Case "we"
                        Return "we"
                    Case "us"
                        Return "us"
                    Case "them"
                        Return "them"
                    Case Else
                        Return ""
                End Select
            End Function
            ''' <summary>
            ''' finds pronoun antecedants in the text a replaces them with thier names
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <param name="entityList"></param>
            ''' <returns></returns>
            Public Shared Function ResolveCoreference(sentence As String, entityList As List(Of String)) As String
                Dim words As String() = sentence.Split(" ")

                For i As Integer = 0 To words.Length - 1
                    Dim word As String = words(i)
                    If entityList.Contains(word) Then
                        Dim pronoun As String = GetPronoun(word)
                        Dim antecedent As String = FindAntecedent(words, i, entityList)
                        If Not String.IsNullOrEmpty(antecedent) Then
                            sentence = sentence.Replace(pronoun, antecedent)
                        End If
                    End If
                Next

                Return sentence
            End Function



            ''' <summary>
            ''' Resolves hypothosis and premise and concluisions in the text
            ''' </summary>
            ''' <param name="document"></param>
            ''' <returns></returns>
            Function classifiedSentencesSentences(ByVal document As String) As Dictionary(Of String, String)
                Dim premises As New Dictionary(Of Integer, String)()
                Dim hypotheses As New Dictionary(Of Integer, String)()
                Dim conclusions As New Dictionary(Of Integer, String)()

                ' Split the document into sentences
                Dim sentences As String() = document.Split(New String() {". "}, StringSplitOptions.RemoveEmptyEntries)

                ' Rule-based resolution
                Dim index As Integer = 1
                For Each sentence In sentences
                    ' Remove leading/trailing spaces and convert to lowercase
                    sentence = sentence.Trim().ToLower()

                    ' Check if the sentence is a premise, hypothesis, or conclusion
                    If iClassify.IsPremise(sentence) Then
                        premises.Add(index, sentence)
                    ElseIf iClassify.IsHypothesis(sentence) Then
                        hypotheses.Add(index, sentence)
                    ElseIf iClassify.IsConclusion(sentence) Then
                        conclusions.Add(index, sentence)
                    End If

                    index += 1
                Next

                ' Resolve the relationships based on the antecedents
                Dim resolvedSentences As New Dictionary(Of String, String)()
                For Each conclusionKvp As KeyValuePair(Of Integer, String) In conclusions
                    Dim conclusionIndex As Integer = conclusionKvp.Key
                    Dim conclusionSentence As String = conclusionKvp.Value

                    ' Find the antecedent hypothesis for the conclusion
                    Dim hypothesisIndex As Integer = conclusionIndex - 1
                    Dim hypothesisSentence As String = ""
                    If hypotheses.ContainsKey(hypothesisIndex) Then
                        hypothesisSentence = hypotheses(hypothesisIndex)
                    End If

                    ' Find the antecedent premises for the hypothesis
                    Dim premiseIndexes As New List(Of Integer)()
                    For i As Integer = hypothesisIndex - 1 To 1 Step -1
                        If premises.ContainsKey(i) Then
                            premiseIndexes.Add(i)
                        Else
                            Exit For
                        End If
                    Next

                    ' Build the resolved sentences
                    Dim resolvedSentence As String = ""
                    If Not String.IsNullOrEmpty(hypothesisSentence) Then
                        resolvedSentence += "Hypothesis: " + hypothesisSentence + " "
                    End If
                    For i As Integer = premiseIndexes.Count - 1 To 0 Step -1
                        resolvedSentence += "Premise " + (premiseIndexes.Count - i).ToString() + ": " + premises(premiseIndexes(i)) + " "
                    Next
                    resolvedSentence += "Conclusion: " + conclusionSentence

                    resolvedSentences.Add("Conclusion " + conclusionIndex.ToString(), resolvedSentence)
                Next

                Return resolvedSentences
            End Function

            Public Function FindNearestAntecedent(ByVal pronounIndex As Integer, ByVal words() As String, ByVal entities As Dictionary(Of String, String)) As String
                Dim antecedent As String = ""

                ' Search for nearest preceding noun phrase as antecedent
                For i = pronounIndex - 1 To 0 Step -1
                    If entities.ContainsKey(words(i)) Then
                        antecedent = entities(words(i))
                        Exit For
                    End If
                Next

                Return antecedent
            End Function

            Public Function GenerateRandomAntecedent(entityLists As Dictionary(Of String, List(Of String))) As String
                Dim random As New Random()
                Dim entityTypes As List(Of String) = New List(Of String)(entityLists.Keys)
                Dim entityType As String = entityTypes(random.Next(entityTypes.Count))
                Dim entities As List(Of String) = entityLists(entityType)

                Return entities(random.Next(entities.Count))
            End Function



            ''' <summary>
            ''' Returns All ProNouns detected from this model
            ''' </summary>
            ''' <param name="words"></param>
            ''' <returns></returns>
            Public Function GetPronounsInText(ByVal words() As String) As List(Of String)
                Dim namedEntities As New List(Of String)

                For i = 0 To words.Length - 1
                    Dim word = words(i)
                    If Char.IsUpper(word(0)) AndAlso Not iClassify.Pronouns.Contains(word.ToLower()) Then
                        namedEntities.Add(word)
                    End If
                Next

                Return namedEntities
            End Function

            Public Function IdentifyAntecedent(ByVal sentence As String, ByVal entityLists As Dictionary(Of String, List(Of String))) As String
                ' Tokenize the sentence
                Dim tokens As String() = sentence.Split(" "c)

                ' Iterate through the tokens
                For i As Integer = tokens.Length - 1 To 0 Step -1
                    Dim token As String = tokens(i)

                    ' Iterate through the entity lists
                    For Each entityType As String In entityLists.Keys
                        ' Check if the token matches an entity in the current entity list
                        If entityLists(entityType).Contains(token) Then
                            ' Check for antecedent indicators
                            If i > 0 AndAlso iClassify.IsAntecedentIndicator(tokens(i - 1)) Then
                                ' Return the identified antecedent with its type
                                Return $"{token} ({entityType})"
                            End If
                        End If
                    Next
                Next

                ' Return empty string if no antecedent indicator is found
                Return ""
            End Function


            ''' <summary>
            ''' Enabling to find the antecedant for given entitys or pronouns
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <param name="Entitys"></param>
            ''' <returns></returns>
            Public Function ResolveAntecedant(sentence As String, ByRef Entitys As List(Of String)) As String


                ' Tokenize the sentence into words
                Dim words = Split(sentence, " ")
                ' Find the position of the pronoun in the sentence
                Dim pronounIndex As Integer = 0
                For Each Pronoun In Entitys

                    For Each item In words
                        pronounIndex += 1
                        If item = Pronoun Then
                            Exit For
                        End If
                    Next
                    If pronounIndex = -1 Then
                        Return "Unknown."
                    End If

                Next

                ' Start from the pronoun position and search for antecedents before and after the pronoun
                Dim antecedent As String = ""

                ' Search for antecedents before the pronoun
                For i As Integer = pronounIndex - 1 To 0 Step -1
                    Dim currentWord As String = words(i)

                    ' Check if the current word is a Entity or a pronoun
                    If iClassify.IsEntityOrPronoun(currentWord, Entitys) Then
                        antecedent = currentWord
                        Exit For
                    End If
                Next

                ' If no antecedent is found before the pronoun, search for antecedents after the pronoun
                If antecedent = "" Then
                    For i As Integer = pronounIndex + 1 To words.Length - 1
                        Dim currentWord As String = words(i)

                        ' Check if the current word is a Entity or a pronoun
                        If iClassify.IsEntityOrPronoun(currentWord, Entitys) Then
                            antecedent = currentWord
                            Exit For
                        End If
                    Next
                End If

                ' If no antecedent is found, return an appropriate message
                If antecedent = "" Then
                    Return "No antecedent found for the pronoun."
                End If

                Return antecedent
            End Function

            ''' <summary>
            ''' Given a name / entity it attempts to find the antcedant
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <param name="pronoun"></param>
            ''' <returns></returns>
            Public Function ResolveAntecedant(sentence As String, pronoun As String) As String
                ' Tokenize the sentence into words
                Dim words = Split(sentence, " ")

                ' Find the position of the pronoun in the sentence
                Dim pronounIndex As Integer = 0
                For Each item In words
                    pronounIndex += 1
                    If item = pronoun Then
                        Exit For
                    End If
                Next
                If pronounIndex = -1 Then
                    Return "Unknown."
                End If

                ' Start from the pronoun position and search for antecedents before and after the pronoun
                Dim antecedent As String = ""

                ' Search for antecedents before the pronoun
                If iClassify.IsObjectPronoun(pronoun) Then
                    ' If pronoun is an object pronoun, no need to look for antecedents
                    Return pronoun
                ElseIf iClassify.IsMalePronoun(pronoun) Then
                    ' If pronoun is a male pronoun, search for male antecedents
                    For i As Integer = pronounIndex - 1 To 0 Step -1
                        Dim currentWord As String = words(i)

                        ' Check if the current word is a noun or a pronoun
                        If iClassify.IsMaleNounOrPronoun(currentWord) Then
                            antecedent = currentWord
                            Exit For
                        End If
                    Next
                Else
                    ' If pronoun is a female pronoun, search for female antecedents
                    For i As Integer = pronounIndex - 1 To 0 Step -1
                        Dim currentWord As String = words(i)

                        ' Check if the current word is a noun or a pronoun
                        If iClassify.IsFemaleNounOrPronoun(currentWord) Then
                            antecedent = currentWord
                            Exit For
                        End If
                    Next
                End If

                ' If no antecedent is found, return an appropriate message
                If antecedent = "" Then
                    Return "No antecedent found for the pronoun."
                End If

                Return antecedent
            End Function

            ''' <summary>
            ''' Resolves hypothosis and premise and concluisions in the text
            ''' </summary>
            ''' <param name="document"></param>
            ''' <returns></returns>
            Function ResolveclassifiedSentences(ByVal classifiedSentences As Dictionary(Of String, List(Of String))) As Dictionary(Of String, String)
                Dim resolvedSentences As New Dictionary(Of String, String)()

                ' Resolve relationships based on the antecedents
                Dim premiseCount As Integer = classifiedSentences("Premise").Count
                Dim hypothesisCount As Integer = classifiedSentences("Hypothesis").Count
                Dim conclusionCount As Integer = classifiedSentences("Conclusion").Count

                ' Check if the counts are consistent for resolution
                If hypothesisCount = conclusionCount AndAlso hypothesisCount = 1 AndAlso premiseCount >= 1 Then
                    Dim hypothesis As String = classifiedSentences("Hypothesis")(0)
                    Dim conclusion As String = classifiedSentences("Conclusion")(0)

                    Dim resolvedSentence As String = "Hypothesis: " + hypothesis + Environment.NewLine
                    For i As Integer = 1 To premiseCount
                        Dim premise As String = classifiedSentences("Premise")(i - 1)
                        resolvedSentence += "Premise " + i.ToString() + ": " + premise + Environment.NewLine
                    Next
                    resolvedSentence += "Conclusion: " + conclusion
                    resolvedSentences.Add("Resolved", resolvedSentence)
                Else
                    resolvedSentences.Add("Error", "Unable to resolve relationships. Counts are inconsistent.")
                End If

                Return resolvedSentences
            End Function

            Public Function ResolveCoReferences(ByRef iText As String, ByRef entities As Dictionary(Of String, String)) As Dictionary(Of String, String)
                Dim coReferences As New Dictionary(Of String, String)()

                Dim sentences() As String = Split(iText, "."c, StringSplitOptions.RemoveEmptyEntries)



                For Each sentence In sentences
                    Dim words() As String = Split(sentence.Trim, " "c, StringSplitOptions.RemoveEmptyEntries)

                    ' Identify pronouns and assign antecedents
                    For i = 0 To words.Length - 1
                        Dim word = words(i)
                        If iClassify.Pronouns.Contains(word.ToLower()) Then
                            Dim antecedent = FindNearestAntecedent(i, words, entities)
                            coReferences.Add(word, antecedent)
                        End If
                    Next

                    ' Identify named entities and update entities dictionary
                    Dim namedEntities = iDetect.DetectNamedEntities(words)
                    For Each namedEntity In namedEntities
                        If Not entities.ContainsKey(namedEntity) Then
                            entities.Add(namedEntity, namedEntity)
                        End If
                    Next
                Next


                Return coReferences
            End Function
        End Class

        Public Class iSearch

            Public Shared Function ResolveQuestion(question As String, corpus As List(Of String)) As String
                Dim maxScore As Double = Double.MinValue
                Dim bestAnswer As String = ""

                For Each document As String In corpus
                    Dim score As Double = ComputeSimilarityScore(question, document)

                    If score > maxScore Then
                        maxScore = score
                        bestAnswer = document
                    End If
                Next

                Return bestAnswer
            End Function
            Public Shared Function BuildWordVector(words As HashSet(Of String)) As Dictionary(Of String, Integer)
                Dim wordVector As New Dictionary(Of String, Integer)()

                For Each word As String In words
                    If wordVector.ContainsKey(word) Then
                        wordVector(word) += 1
                    Else
                        wordVector(word) = 1
                    End If
                Next

                Return wordVector
            End Function

            '1. Cosine Similarity Calculation:
            '```vb
            Public Shared Function ComputeCosineSimilarity(phrase1 As String, phrase2 As String) As Double
                Dim words1 As HashSet(Of String) = GetDistinctWords(phrase1)
                Dim words2 As HashSet(Of String) = GetDistinctWords(phrase2)

                Dim wordVector1 As Dictionary(Of String, Integer) = BuildWordVector(words1)
                Dim wordVector2 As Dictionary(Of String, Integer) = BuildWordVector(words2)

                Dim dotProduct As Integer = ComputeDotProduct(wordVector1, wordVector2)
                Dim magnitude1 As Double = ComputeVectorMagnitude(wordVector1)
                Dim magnitude2 As Double = ComputeVectorMagnitude(wordVector2)

                ' Compute the cosine similarity as the dot product divided by the product of magnitudes
                Dim similarityScore As Double = dotProduct / (magnitude1 * magnitude2)

                Return similarityScore
            End Function

            Public Shared Function ComputeDotProduct(vector1 As Dictionary(Of String, Integer), vector2 As Dictionary(Of String, Integer)) As Integer
                Dim dotProduct As Integer = 0

                For Each word As String In vector1.Keys
                    If vector2.ContainsKey(word) Then
                        dotProduct += vector1(word) * vector2(word)
                    End If
                Next

                Return dotProduct
            End Function

            '2. Jaccard Similarity Calculation:
            '```vb
            Public Shared Function ComputeJaccardSimilarity(phrase1 As String, phrase2 As String) As Double
                Dim words1 As HashSet(Of String) = GetDistinctWords(phrase1)
                Dim words2 As HashSet(Of String) = GetDistinctWords(phrase2)

                Dim intersectionCount As Integer = words1.Intersect(words2).Count()
                Dim unionCount As Integer = words1.Count + words2.Count - intersectionCount

                ' Compute the Jaccard Similarity as the ratio of intersection count to union count
                Dim similarityScore As Double = intersectionCount / unionCount

                Return similarityScore
            End Function

            Public Shared Function ComputeSimilarityScore(phrase As String, contextLine As String) As Double
                ' Here you can implement your own logic for computing the similarity score between the phrase and the context line.
                ' For simplicity, let's use a basic approach that counts the number of common words between them.

                Dim phraseWords As HashSet(Of String) = GetDistinctWords(phrase)
                Dim contextWords As HashSet(Of String) = GetDistinctWords(contextLine)

                Dim commonWordsCount As Integer = phraseWords.Intersect(contextWords).Count()

                Dim totalWordsCount As Integer = phraseWords.Count + contextWords.Count

                ' Compute the similarity score as the ratio of common words count to total words count
                Dim similarityScore As Double = commonWordsCount / totalWordsCount

                Return similarityScore
            End Function

            Public Shared Function ComputeVectorMagnitude(vector As Dictionary(Of String, Integer)) As Double
                Dim magnitude As Double = 0

                For Each count As Integer In vector.Values
                    magnitude += count * count
                Next

                magnitude = Math.Sqrt(magnitude)

                Return magnitude
            End Function

            ' Extract email addresses from text
            Public Shared Function ExtractEmailAddresses(text As String) As List(Of String)
                Dim emailRegex As New Regex("\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b")
                Dim emailAddresses As New List(Of String)()

                Dim matches As MatchCollection = emailRegex.Matches(text)

                For Each match As Match In matches
                    emailAddresses.Add(match.Value)
                Next

                Return emailAddresses
            End Function

            ' Information Extraction Functions
            ' Extract phone numbers from text
            Public Shared Function ExtractPhoneNumbers(text As String) As List(Of String)
                Dim phoneRegex As New Regex("\b\d{3}-\d{3}-\d{4}\b")
                Dim phoneNumbers As New List(Of String)()

                Dim matches As MatchCollection = phoneRegex.Matches(text)

                For Each match As Match In matches
                    phoneNumbers.Add(match.Value)
                Next

                Return phoneNumbers
            End Function

            Public Shared Function ExtractSimilarPhrases(text As String, searchPhrase As String, similarityThreshold As Double) As List(Of String)
                Dim result As New List(Of String)()

                Dim sentences() As String = text.Split({".", "!", "?"}, StringSplitOptions.RemoveEmptyEntries)

                For Each sentence As String In sentences
                    Dim similarityScore As Double = ComputeSimilarityScore(searchPhrase, sentence)

                    If similarityScore >= similarityThreshold Then
                        result.Add(sentence)
                    End If
                Next

                Return result
            End Function

            ''' <summary>
            ''' Extracts words between  based on the before and after words
            ''' IE: THe cat sat on the mat (before The After The) output: cat sat on
            ''' </summary>
            ''' <param name="sentence"></param>
            ''' <param name="beforeWord"></param>
            ''' <param name="afterWord"></param>
            ''' <returns></returns>
            Public Shared Function ExtractWordsBetween(sentence As String, beforeWord As String, afterWord As String) As List(Of String)
                Dim words As New List(Of String)()

                Dim sentenceWords As String() = sentence.Split(" "c)
                Dim startIndex As Integer = -1
                Dim endIndex As Integer = -1

                ' Find the starting and ending indices of the target words
                For i As Integer = 0 To sentenceWords.Length - 1
                    If sentenceWords(i).Equals(beforeWord, StringComparison.OrdinalIgnoreCase) Then
                        startIndex = i
                    End If

                    If sentenceWords(i).Equals(afterWord, StringComparison.OrdinalIgnoreCase) Then
                        endIndex = i
                    End If
                Next

                ' Extract words between the target words
                If startIndex <> -1 AndAlso endIndex <> -1 AndAlso startIndex < endIndex Then
                    For i As Integer = startIndex + 1 To endIndex - 1
                        words.Add(sentenceWords(i))
                    Next
                End If

                Return words
            End Function

            Public Shared Function GetDistinctWords(text As String) As HashSet(Of String)
                ' Split the text into words and return a HashSet of distinct words
                Dim words() As String = text.Split({" ", ".", ",", ";", ":", "!", "?"}, StringSplitOptions.RemoveEmptyEntries)
                Dim distinctWords As New HashSet(Of String)(words, StringComparer.OrdinalIgnoreCase)

                Return distinctWords
            End Function

            ''' <summary>
            ''' Returns phrase and surrounding comments and position
            ''' </summary>
            ''' <param name="corpus"></param>
            ''' <param name="phrase"></param>
            ''' <returns></returns>
            Public Shared Function SearchPhraseInCorpus(corpus As List(Of String), phrase As String) As Dictionary(Of String, List(Of String))
                Dim result As New Dictionary(Of String, List(Of String))()

                For i As Integer = 0 To corpus.Count - 1
                    Dim document As String = corpus(i)
                    Dim lines() As String = document.Split(Environment.NewLine)

                    For j As Integer = 0 To lines.Length - 1
                        Dim line As String = lines(j)
                        Dim index As Integer = line.IndexOf(phrase, StringComparison.OrdinalIgnoreCase)

                        While index >= 0
                            Dim context As New List(Of String)()

                            ' Get the surrounding context sentences
                            Dim startLine As Integer = Math.Max(0, j - 1)
                            Dim endLine As Integer = Math.Min(lines.Length - 1, j + 1)

                            For k As Integer = startLine To endLine
                                context.Add(lines(k))
                            Next

                            ' Add the result to the dictionary
                            Dim position As String = $"Document: {i + 1}, Line: {j + 1}, Character: {index + 1}"
                            result(position) = context

                            ' Continue searching for the phrase in the current line
                            index = line.IndexOf(phrase, index + 1, StringComparison.OrdinalIgnoreCase)
                        End While
                    Next
                Next

                Return result
            End Function

            ''' <summary>
            ''' Searches for phrases based on simularity ie same words
            ''' </summary>
            ''' <param name="corpus"></param>
            ''' <param name="phrase"></param>
            ''' <param name="similarityThreshold"></param>
            ''' <returns></returns>
            Public Shared Function SearchPhraseInCorpus(corpus As List(Of String), phrase As String, similarityThreshold As Double) As Dictionary(Of String, List(Of String))
                Dim result As New Dictionary(Of String, List(Of String))()

                For i As Integer = 0 To corpus.Count - 1
                    Dim document As String = corpus(i)
                    Dim lines() As String = document.Split(Environment.NewLine)

                    For j As Integer = 0 To lines.Length - 1
                        Dim line As String = lines(j)
                        Dim index As Integer = line.IndexOf(phrase, StringComparison.OrdinalIgnoreCase)

                        While index >= 0
                            Dim context As New List(Of String)()

                            ' Get the surrounding context sentences
                            Dim startLine As Integer = Math.Max(0, j - 1)
                            Dim endLine As Integer = Math.Min(lines.Length - 1, j + 1)

                            For k As Integer = startLine To endLine
                                Dim contextLine As String = lines(k)

                                ' Compute the similarity score between the context line and the phrase
                                Dim similarityScore As Double = ComputeSimilarityScore(phrase, contextLine)

                                ' Add the context line only if its similarity score exceeds the threshold
                                If similarityScore >= similarityThreshold Then
                                    context.Add(contextLine)
                                End If
                            Next

                            ' Add the result to the dictionary
                            Dim position As String = $"Document: {i + 1}, Line: {j + 1}, Character: {index + 1}"
                            result(position) = context

                            ' Continue searching for the phrase in the current line
                            index = line.IndexOf(phrase, index + 1, StringComparison.OrdinalIgnoreCase)
                        End While
                    Next
                Next

                Return result
            End Function
            '```
        End Class

        Public Class iTagger
            Public Shared Function ExtractAdjectivePhrases(taggedWords As List(Of KeyValuePair(Of String, String))) As List(Of String)
                Dim adjectivePhrases As New List(Of String)()

                Dim currentPhrase As String = ""
                Dim insideAdjectivePhrase As Boolean = False

                For Each taggedWord In taggedWords
                    Dim word As String = taggedWord.Key
                    Dim tag As String = taggedWord.Value

                    If tag.StartsWith("JJ") Then ' Adjective tag
                        If insideAdjectivePhrase Then
                            currentPhrase += " " & word
                        Else
                            currentPhrase = word
                            insideAdjectivePhrase = True
                        End If
                    Else
                        If insideAdjectivePhrase Then
                            adjectivePhrases.Add(currentPhrase)
                            insideAdjectivePhrase = False
                        End If
                    End If
                Next

                ' Add the last phrase if it is an adjective phrase
                If insideAdjectivePhrase Then
                    adjectivePhrases.Add(currentPhrase)
                End If

                Return adjectivePhrases
            End Function

            Public Shared Function ExtractNounPhrases(taggedWords As List(Of KeyValuePair(Of String, String))) As List(Of String)
                Dim nounPhrases As New List(Of String)()

                Dim currentPhrase As String = ""
                Dim insideNounPhrase As Boolean = False

                For Each taggedWord In taggedWords
                    Dim word As String = taggedWord.Key
                    Dim tag As String = taggedWord.Value

                    If tag.StartsWith("NN") Then ' Noun tag
                        If insideNounPhrase Then
                            currentPhrase += " " & word
                        Else
                            currentPhrase = word
                            insideNounPhrase = True
                        End If
                    Else
                        If insideNounPhrase Then
                            nounPhrases.Add(currentPhrase)
                            insideNounPhrase = False
                        End If
                    End If
                Next

                ' Add the last phrase if it is a noun phrase
                If insideNounPhrase Then
                    nounPhrases.Add(currentPhrase)
                End If

                Return nounPhrases
            End Function

            Public Shared Function ExtractVerbPhrases(taggedWords As List(Of KeyValuePair(Of String, String))) As List(Of String)
                Dim verbPhrases As New List(Of String)()

                Dim currentPhrase As String = ""
                Dim insideVerbPhrase As Boolean = False

                For Each taggedWord In taggedWords
                    Dim word As String = taggedWord.Key
                    Dim tag As String = taggedWord.Value

                    If tag.StartsWith("VB") Then ' Verb tag
                        If insideVerbPhrase Then
                            currentPhrase += " " & word
                        Else
                            currentPhrase = word
                            insideVerbPhrase = True
                        End If
                    Else
                        If insideVerbPhrase Then
                            verbPhrases.Add(currentPhrase)
                            insideVerbPhrase = False
                        End If
                    End If
                Next

                ' Add the last phrase if it is a verb phrase
                If insideVerbPhrase Then
                    verbPhrases.Add(currentPhrase)
                End If

                Return verbPhrases
            End Function

            Public Shared Function FindNounPhrases(sentence As String) As List(Of String)
                Dim nounPhrases As New List(Of String)()

                ' Split the sentence into individual words
                Dim words() As String = sentence.Split({" "}, StringSplitOptions.RemoveEmptyEntries)

                ' Identify noun phrases
                For i As Integer = 0 To words.Length - 1
                    If IsNoun(words(i)) Then
                        Dim nounPhrase As String = words(i)
                        Dim j As Integer = i + 1

                        ' Combine consecutive words until a non-noun word is encountered
                        While j < words.Length AndAlso IsNoun(words(j))
                            nounPhrase += " " & words(j)
                            j += 1
                        End While

                        nounPhrases.Add(nounPhrase)
                    End If
                Next

                Return nounPhrases
            End Function

            Public Shared Function FindPhrases(sentence As String, phraseType As String) As List(Of String)
                Dim phrases As New List(Of String)()

                ' Split the sentence into individual words
                Dim words() As String = sentence.Split({" "}, StringSplitOptions.RemoveEmptyEntries)

                ' Identify phrases based on the specified type
                For i As Integer = 0 To words.Length - 1
                    Dim currentWord As String = words(i)

                    If (phraseType = "verb" AndAlso IsVerb(currentWord)) OrElse
               (phraseType = "adjective" AndAlso IsAdjective(currentWord)) Then

                        Dim phrase As String = currentWord
                        Dim j As Integer = i + 1

                        ' Combine consecutive words until a non-phrase word is encountered
                        While j < words.Length AndAlso (IsVerb(words(j)) OrElse IsAdjective(words(j)))
                            phrase += " " & words(j)
                            j += 1
                        End While

                        phrases.Add(phrase)
                    End If
                Next

                Return phrases
            End Function

            Public Shared Function FindPhrases(taggedWords As List(Of KeyValuePair(Of String, String)), phraseType As String) As List(Of String)
                Dim phrases As New List(Of String)()

                ' Identify phrases based on the specified type
                For i As Integer = 0 To taggedWords.Count - 1
                    Dim currentWord As String = taggedWords(i).Key
                    Dim currentTag As String = taggedWords(i).Value

                    If (phraseType = "verb" AndAlso IsVerbTag(currentTag)) OrElse
               (phraseType = "adjective" AndAlso IsAdjectiveTag(currentTag)) Then

                        Dim phrase As String = currentWord
                        Dim j As Integer = i + 1

                        ' Combine consecutive words until a non-phrase word is encountered
                        While j < taggedWords.Count AndAlso (IsVerbTag(taggedWords(j).Value) OrElse IsAdjectiveTag(taggedWords(j).Value))
                            phrase += " " & taggedWords(j).Key
                            j += 1
                        End While

                        phrases.Add(phrase)
                    End If
                Next

                Return phrases
            End Function

            Public Shared Function IsAdjective(word As String) As Boolean
                ' Add your own adjective identification logic here
                ' This is a basic example that checks if the word ends with "ly"
                Return word.EndsWith("ly")
            End Function

            Public Shared Function IsAdjectiveTag(tag As String) As Boolean
                ' Add your own adjective tag identification logic here
                ' This is a basic example that checks if the tag starts with "JJ"
                Return tag.StartsWith("JJ")
            End Function

            Public Shared Function IsNoun(word As String) As Boolean
                ' Add your own noun identification logic here
                ' You can check for patterns, word lists, or use external resources for more accurate noun detection
                ' This is a basic example that only checks for the first letter being uppercase
                Return Char.IsUpper(word(0))
            End Function
            Public Shared Function IsVerb(word As String) As Boolean
                ' Add your own verb identification logic here
                ' This is a basic example that checks if the word ends with "ing"
                Return word.EndsWith("ing")
            End Function
            Public Shared Function IsVerbTag(tag As String) As Boolean
                ' Add your own verb tag identification logic here
                ' This is a basic example that checks if the tag starts with "V"
                Return tag.StartsWith("V")
            End Function
        End Class

        Public Class Itokenize
            Public Shared Function TokenizeTextIntoCharacterNGrams(text As String, n As Integer) As List(Of String)
                Dim tokens As New List(Of String)()

                ' Remove whitespace and convert to lowercase
                Dim cleanText As String = text.ToLower().Replace(" ", "")

                ' Generate character n-grams
                For i As Integer = 0 To cleanText.Length - n
                    Dim ngram As String = cleanText.Substring(i, n)
                    tokens.Add(ngram)
                Next

                Return tokens
            End Function

            Public Shared Function TokenizeTextIntoNGrams(ByRef text As String, n As Integer) As List(Of String)
                Dim tokens As New List(Of String)()

                ' Remove punctuation and convert to lowercase
                Dim cleanText As String = text.ToLower.Trim
                ' Split the clean text into individual words
                Dim words() As String = cleanText.Split({" ", ".", ",", ";", ":", "!", "?"}, StringSplitOptions.RemoveEmptyEntries)

                ' Generate n-grams from the words
                For i As Integer = 0 To words.Length - n
                    Dim ngram As String = String.Join(" ", words.Skip(i).Take(n))
                    tokens.Add(ngram)
                Next

                Return tokens
            End Function
            Public Shared Function TokenizeTextIntoParagraphNGrams(text As String, n As Integer) As List(Of String)
                Dim tokens As New List(Of String)()

                ' Split the text into paragraphs
                Dim paragraphs() As String = text.Split({Environment.NewLine & Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

                ' Generate paragraph n-grams
                For i As Integer = 0 To paragraphs.Length - n
                    Dim ngram As String = String.Join(Environment.NewLine & Environment.NewLine, paragraphs.Skip(i).Take(n))
                    tokens.Add(ngram)
                Next

                Return tokens
            End Function

            Public Shared Function TokenizeTextIntoSentenceNGrams(text As String, n As Integer) As List(Of String)
                Dim tokens As New List(Of String)()

                ' Split the text into sentences
                Dim sentences() As String = text.Split({".", "!", "?"}, StringSplitOptions.RemoveEmptyEntries)

                ' Generate sentence n-grams
                For i As Integer = 0 To sentences.Length - n
                    Dim ngram As String = String.Join(" ", sentences.Skip(i).Take(n))
                    tokens.Add(ngram)
                Next

                Return tokens
            End Function
        End Class
        '''' <summary>
        '''' Captures words with their context based on a focus term and the number of preceding and following words to include.
        '''' </summary>
        '''' <param name="text">The input text.</param>
        '''' <param name="focusTerm">The focus term to capture.</param>
        '''' <param name="precedingWordsCount">The number of preceding words to capture.</param>
        '''' <param name="followingWordsCount">The number of following words to capture.</param>
        '''' <returns>A list of WordWithContext objects containing captured words and their context information.</returns>
        'Public Function CaptureWordsWithEntityContext(text As String, focusTerm As String, precedingWordsCount As Integer, followingWordsCount As Integer) As List(Of WordWithContext)
        '    Dim words As List(Of String) = text.Split(" "c).ToList()
        '    Dim focusIndex As Integer = words.IndexOf(focusTerm)

        '    Dim capturedWordsWithEntityContext As New List(Of WordWithContext)()

        '    If focusIndex <> -1 Then
        '        Dim startIndex As Integer = Math.Max(0, focusIndex - precedingWordsCount)
        '        Dim endIndex As Integer = Math.Min(words.Count - 1, focusIndex + followingWordsCount)

        '        Dim contextWords As List(Of String) = words.GetRange(startIndex, endIndex - startIndex + 1)
        Public Class iTransform
            Public Shared Function RemoveEntitiesFromText(ByVal text As String, ByVal entities As List(Of String)) As String
                For Each entity As String In entities
                    text = text.Replace(entity, String.Empty)
                Next

                Return text
            End Function

            ''' <summary>
            ''' Replaces the encapsulated [Entity Label] with an asterisk (*)
            ''' ie the [Entity] walked 
            ''' </summary>
            ''' <param name="text">The text to be modified.</param>
            ''' <param name="Entitylabel">The label to replace the entities with.</param>
            ''' <returns>The text with entities replaced by the label.</returns>
            Public Shared Function ReplaceEntityLabel(ByRef text As String, ByRef EntityLabel As String) As String
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("EntityLabel")
                End If
                Dim str As String = text
                str = str.Replace("[" & EntityLabel & "]", "*")
                Return str
            End Function

            Public Shared Function TransformText(ByRef Text As String, ByRef Entitys As List(Of String), Optional iLabel As String = "Entity") As String
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If Entitys Is Nothing Then
                    Throw New ArgumentNullException("Entitys")
                End If
                Dim Str As String = Text
                For Each item In Entitys
                    Str = Str.Replace(item, "[" & iLabel & "]")
                Next
                Return Str
            End Function
            ''' <summary>
            ''' Replaces occurrences of entities or entity labels in the text with asterisks.
            ''' </summary>
            ''' <param name="text">The text to transform.</param>
            ''' <returns>The transformed text with entities or entity labels replaced by asterisks.</returns>
            Public Shared Function TransformText(ByRef text As String) As String
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If
                Dim str As String = text
                str = str.Replace("[Entity]", "*")
                Return str
            End Function

            ''' <summary>
            ''' Replaces the detected entities with the specified label in the text.
            ''' </summary>
            ''' <param name="text">The text to be modified.</param>
            ''' <param name="Entitylabel">The label to replace the entities with.</param>
            ''' <returns>The text with entities replaced by the label.</returns>
            Public Shared Function TransformText(ByRef text As String, ByRef EntityLabel As String) As String
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If
                If EntityLabel Is Nothing Then
                    Throw New ArgumentNullException("EntityLabel")
                End If
                Dim str As String = text
                str = str.Replace("[" & EntityLabel & "]", "*")
                Return str
            End Function

            Public Function ReplaceEntities(sentence As String, ByRef ENTITYS As List(Of String)) As String
                ' Replace discovered entities in the sentence with their entity type
                For Each entity As String In ENTITYS
                    Dim entityType As String = entity.Substring(0, entity.IndexOf("("))
                    Dim entityValue As String = entity.Substring(entity.IndexOf("(") + 1, entity.Length - entity.IndexOf("(") - 2)
                    sentence = sentence.Replace(entityValue, entityType)
                Next

                Return sentence
            End Function

            Public Function ReplaceTagsInSentence(sentence As String, taggedEntities As Dictionary(Of String, String)) As String
                ' Implement your custom rule-based tag replacement logic here
                ' Analyze the sentence and replace tagged entities with their corresponding tags

                ' Example tag replacement logic
                For Each taggedEntity As KeyValuePair(Of String, String) In taggedEntities
                    Dim entity As String = taggedEntity.Key
                    Dim tag As String = taggedEntity.Value

                    ' Replace the tagged entity with its tag in the sentence
                    sentence = sentence.Replace(entity, tag)
                Next

                Return sentence
            End Function

            Public Function TagSentence(sentence As String) As List(Of String)
                ' Implement your custom rule-based sentence tagging logic here
                ' Analyze the sentence and assign tags to words or phrases

                Dim taggedSentence As New List(Of String)()

                ' Example sentence tagging logic
                Dim words() As String = sentence.Split(" "c)
                For Each word As String In words
                    Dim tag As String = ""

                    ' Assign tags based on some criteria (e.g., part-of-speech, named entity)
                    If iClassify.IsProperNoun(word) Then
                        tag = "NNP" ' Proper noun
                    ElseIf iClassify.IsVerb(word) Then
                        tag = "VB" ' Verb
                    ElseIf iClassify.IsAdjective(word) Then
                        tag = "JJ" ' Adjective
                    Else
                        tag = "NN" ' Noun
                    End If

                    ' Create a tagged word (word/tag) and add it to the tagged sentence
                    Dim taggedWord As String = $"{word}/{tag}"
                    taggedSentence.Add(taggedWord)
                Next

                Return taggedSentence
            End Function
        End Class
    End Class


End Namespace