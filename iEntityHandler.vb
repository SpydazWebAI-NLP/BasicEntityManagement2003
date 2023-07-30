Imports System.Text.RegularExpressions
Imports EntityManagement.Recognition.Classifer.EntityClassifier
Imports EntityManagement.Recognition.Classifer.EntityClassifier.Discover
Imports EntityManagement.Recognition.DataObjects

Namespace Recognition

    Namespace DataObjects
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

        Public Structure NlpReport

            Public EntityLists As List(Of Entity)
            Public SearchPatterns As List(Of SemanticPattern)
            Public UserText As String

            Public Sub New(ByRef Usertext As String, Entitylists As List(Of Entity), ByRef SearchPatterns As List(Of SemanticPattern))
                Me.UserText = Usertext
                Me.EntityLists = Entitylists
                Me.SearchPatterns = SearchPatterns
            End Sub

        End Structure


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
    End Namespace


    Public Class ICollect




















        Public Shared FemaleNames As List(Of String)

        Public Shared MaleNames As List(Of String)

        Public Shared ObjectNames As List(Of String)

        Private Shared commonQuestionHeaders As List(Of String)

        Private Shared iPronouns As List(Of String)

        '' Example entity list to search
        'Dim entityList As New List(Of String)() From {"dolphins"}
        Private Shared questionWords As List(Of String)

        Private Shared semanticPatterns As List(Of String)

        Private conclusions As List(Of String)

        Private hypotheses As List(Of String)

        Private patterns As Dictionary(Of String, String)

        Private premises As List(Of String)

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
                Lst.AddRange(iPronouns)
                Return Lst.Distinct.ToList
            End Get
        End Property

        Public Shared Property bornInPattern As String = "\b([A-Z][a-z]+)\b relation \(born in\) \b([A-Z][a-z]+)\b"

        Public Shared Property datePattern As String = "\b\d{4}\b"

        Public Shared Property organizationPattern As String = "\b([A-Z][a-z]+)\b"

        ' Regular expression patterns for different entity types
        Public Shared Property personPattern As String = "\b([A-Z][a-z]+)\b"

        Public Shared Property programmingLanguagePattern As String = "\b[A-Z][a-z]+\.[a-z]+\b"

        Public Shared Property wroteBookPattern As String = "\b([A-Z][a-z]+)\b \(wrote a book called\) \b([A-Z][a-z]+)\b"

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
            If Classifer.EntityClassifier.Detect.DetectEntity(text, EntityList) = True Then
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

        ''' <summary>
        ''' Attempts to find Unknown Names(pronouns) identified by thier capitalization
        ''' </summary>
        ''' <param name="words"></param>
        ''' <returns></returns>
        Public Shared Function DetectNamedEntities(ByVal words() As String) As List(Of String)
            Dim namedEntities As New List(Of String)

            For i = 0 To words.Length - 1
                Dim word = words(i)
                If Char.IsUpper(word(0)) AndAlso Not Pronouns.Contains(word.ToLower()) Then
                    namedEntities.Add(word)
                End If
            Next

            Return namedEntities
        End Function

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

        ''' <summary>
        ''' Extracts context Entitys , As Well As thier context words 
        ''' </summary>
        ''' <param name="itext"></param>
        ''' <param name="contextSize"></param>
        ''' <param name="entities">Values to retrieve context for</param>
        ''' <returns></returns>
        Public Shared Function ExtractCapturedContextIntext(ByRef itext As String, ByVal contextSize As Integer, ByRef entities As List(Of String)) As List(Of CapturedContent)
            Dim wordsWithContext As New List(Of CapturedContent)

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
            If Classifer.EntityClassifier.Detect.DetectEntity(text, EntityList) = True Then
                Entitys = Classifer.EntityClassifier.Detect.DetectEntitysInText(text, EntityList)

                Str = Classifer.EntityClassifier.Discover.DiscoverShape(Str, Entitys)
                Str = Classifer.EntityClassifier.Transform.TransformText(Str, Entitys)
            End If
            Return Str
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

        '1. Objects:
        '   - Objects are typically referred to using nouns. Examples include "car," "book," "tree," "chair," "pen," etc.
        '   - Objects may have specific attributes or characteristics associated with them, such as color, size, shape, etc., which can be mentioned when referring to them.
        Public Shared Function FindAntecedent(words As String(), pronounIndex As Integer, entityList As List(Of String)) As String
            For i As Integer = pronounIndex - 1 To 0 Step -1
                Dim word As String = words(i)
                If entityList.Contains(word) Then
                    Return word
                End If
            Next

            Return ""
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

        '2. Locations:
        '   - Locations are places or areas where entities or objects exist or are situated.
        '   - Locations can be referred to using nouns that represent specific places, such as "home," "office," "school," "park," "store," "gym," "library," etc.
        '   - Locations can also be described using adjectives or prepositional phrases, such as "in the backyard," "at the beach," "on the street," "near the river," etc.
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

        '3. Antecedents:
        '   - Antecedents are the entities or objects that are referred to by pronouns or other referencing words in a sentence.
        '   - Antecedents are typically introduced in a sentence before the pronoun or referencing word. For example, "John went to the store. He bought some groceries."
        '   - Antecedents can be humans, objects, animals, or other entities. The choice of pronouns or referencing words depends on the gender and type of the antecedent. For example, "he" for a male, "she" for a female, "it" for an object, and "they" for multiple entities.
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
            Return lowerCasePronoun = "her" OrElse lowerCasePronoun = "she" OrElse lowerCasePronoun = "hers" OrElse FemaleNames.Contains(pronoun)
        End Function

        Public Shared Function IsMaleNounOrPronoun(ByVal word As String) As Boolean
            Dim imaleNouns() As String = {"him", " he", "his", ""}
            Return MaleNames.Contains(word.ToLower()) OrElse imaleNouns.Contains(word.ToLower)
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

        Public Shared Function IsNoun(word As String) As Boolean
            ' Add your own noun identification logic here
            ' You can check for patterns, word lists, or use external resources for more accurate noun detection
            ' This is a basic example that only checks for the first letter being uppercase
            Return Char.IsUpper(word(0))
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
            If StartsWithAny(sentence, questionWords) Then
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
            If StartsWithAny(sentence, commonQuestionHeaders) Then
                Return True
            End If

            ' No matching question pattern found
            Return False
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
        ' Identify antecedent indicators:
        '   - Look for pronouns or referencing words like "he," "she," "it," "they," "them," "that" in the sentence.
        '   - Check the preceding tokens to identify the most recent entity token with a matching type.
        '   - Use the identified entity as the antecedent indicator.
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

        Public Function DetectGender(name As String) As String
            ' For simplicity, let's assume any name starting with a vowel is female, and the rest are male
            If IsObjectPronoun(name) Then
                Return "Object"
            ElseIf IsMaleNounOrPronoun(name) Then
                Return "Male"
            ElseIf IsFemaleNounOrPronoun(name) Then
                Return "Female"
            Else
                Return "Unknown"
            End If
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
                If MatchesAnswerShape(sentence, answerShapes) Then
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


        Public Function ExtractPotentialAnswers(text As String, resolvedAntecedents As List(Of String), resolvedEntities As List(Of String)) As List(Of String)
            Dim answers As New List(Of String)()

            ' Split the text into sentences
            Dim sentences As String() = Split(text, ".", StringSplitOptions.RemoveEmptyEntries)

            ' Iterate through each sentence and check for potential answer sentences
            For Each sentence In sentences
                ' Check if the sentence contains any of the resolved antecedents or entities
                If ContainsResolvedAntecedentsOrEntities(sentence, resolvedAntecedents, resolvedEntities) Then

                    ' Add the sentence to the list of potential answer sentences
                    answers.Add(sentence.Trim())
                End If
            Next

            Return answers
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
                If Char.IsUpper(word(0)) AndAlso Not Pronouns.Contains(word.ToLower()) Then
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
                        If i > 0 AndAlso IsAntecedentIndicator(tokens(i - 1)) Then
                            ' Return the identified antecedent with its type
                            Return $"{token} ({entityType})"
                        End If
                    End If
                Next
            Next

            ' Return empty string if no antecedent indicator is found
            Return ""
        End Function

        Public Function IsEntity(ByRef Word As String, ByRef Entitys As List(Of String)) As Boolean
            For Each item In Entitys
                If Word = item Then
                    Return True
                End If
            Next
            Return False
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
                If IsEntityOrPronoun(currentWord, Entitys) Then
                    antecedent = currentWord
                    Exit For
                End If
            Next

            ' If no antecedent is found before the pronoun, search for antecedents after the pronoun
            If antecedent = "" Then
                For i As Integer = pronounIndex + 1 To words.Length - 1
                    Dim currentWord As String = words(i)

                    ' Check if the current word is a Entity or a pronoun
                    If IsEntityOrPronoun(currentWord, Entitys) Then
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
            If IsObjectPronoun(pronoun) Then
                ' If pronoun is an object pronoun, no need to look for antecedents
                Return pronoun
            ElseIf IsMalePronoun(pronoun) Then
                ' If pronoun is a male pronoun, search for male antecedents
                For i As Integer = pronounIndex - 1 To 0 Step -1
                    Dim currentWord As String = words(i)

                    ' Check if the current word is a noun or a pronoun
                    If IsMaleNounOrPronoun(currentWord) Then
                        antecedent = currentWord
                        Exit For
                    End If
                Next
            Else
                ' If pronoun is a female pronoun, search for female antecedents
                For i As Integer = pronounIndex - 1 To 0 Step -1
                    Dim currentWord As String = words(i)

                    ' Check if the current word is a noun or a pronoun
                    If IsFemaleNounOrPronoun(currentWord) Then
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

        Public Function ResolveCoReferences(ByRef iText As String, ByRef entities As Dictionary(Of String, String)) As Dictionary(Of String, String)
            Dim coReferences As New Dictionary(Of String, String)()

            Dim sentences() As String = Split(iText, "."c, StringSplitOptions.RemoveEmptyEntries)



            For Each sentence In sentences
                Dim words() As String = Split(sentence.Trim, " "c, StringSplitOptions.RemoveEmptyEntries)

                ' Identify pronouns and assign antecedents
                For i = 0 To words.Length - 1
                    Dim word = words(i)
                    If Pronouns.Contains(word.ToLower()) Then
                        Dim antecedent = FindNearestAntecedent(i, words, entities)
                        coReferences.Add(word, antecedent)
                    End If
                Next

                ' Identify named entities and update entities dictionary
                Dim namedEntities = DetectNamedEntities(words)
                For Each namedEntity In namedEntities
                    If Not entities.ContainsKey(namedEntity) Then
                        entities.Add(namedEntity, namedEntity)
                    End If
                Next
            Next


            Return coReferences
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
                If IsProperNoun(word) Then
                    tag = "NNP" ' Proper noun
                ElseIf IsVerb(word) Then
                    tag = "VB" ' Verb
                ElseIf IsAdjective(word) Then
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
        Public Class iSearch



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





            '```
        End Class
    End Class





End Namespace