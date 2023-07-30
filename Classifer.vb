Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization
Imports System.Windows.Forms

Namespace Recognition

    <ComClass(SentenceSplitter.ClassId, SentenceSplitter.InterfaceId, SentenceSplitter.EventsId)>
    Public Class SentenceSplitter
        Public Const ClassId As String = "28993390-7702-401C-BAB3-38FF97BC1AC9"
        Public Const EventsId As String = "CD334307-F53E-401A-AC6D-3CFDD86FD6F1"
        Public Const InterfaceId As String = "8B3345B1-5D13-4059-829B-B531310144B5"

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

    Public Class iCompare

        Public Shared Function GetDistinctWords(text As String) As HashSet(Of String)
            ' Split the text into words and return a HashSet of distinct words
            Dim words() As String = text.Split({" ", ".", ",", ";", ":", "!", "?"}, StringSplitOptions.RemoveEmptyEntries)
            Dim distinctWords As New HashSet(Of String)(words, StringComparer.OrdinalIgnoreCase)

            Return distinctWords
        End Function

        Public Shared Function BuildWordVector(words As HashSet(Of String)) As Dictionary(Of String, Integer)
            Dim wordVector As New Dictionary(Of String, Integer)

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

    End Class

    Namespace Classifer
        Public Module ProgramResolver
            Public Sub Main()
                Dim sentence As String = "John gave Mary a book, and she thanked him for it."

                Dim iresolve As New Resolver
                Dim Pronouns As List(Of String) = iresolve.FemalePersonalNouns.ToList
                Pronouns.AddRange(iresolve.MalePersonalNouns)
                Pronouns.AddRange(iresolve.MalePronouns)
                Pronouns.AddRange(iresolve.FemalePronouns)
                Dim resolvedAntecedent As String = ""
                For Each item In Pronouns
                    If sentence.ToLower.Contains(item.ToLower) Then


                        resolvedAntecedent = iresolve.ResolvePronoun(sentence.ToLower, item.ToLower)
                        Console.WriteLine($"Resolved antecedent for '{item}': {resolvedAntecedent}")
                        resolvedAntecedent = iresolve.ResolveGender(item)
                        Console.WriteLine($"Resolved Gender for '{item}': {resolvedAntecedent}")
                    End If
                Next
                Console.WriteLine("finished")
                Console.ReadLine()


            End Sub
        End Module
        Public Class Resolver
            Public Function ResolvePronoun(sentence As String, pronoun As String) As String
                ' Tokenize the sentence into words
                Dim words As String() = Split(sentence, " ",)

                ' Find the position of the pronoun in the sentence
                Dim pronounIndex As Integer = Array.IndexOf(words, pronoun)

                ' Search for antecedents before the pronoun
                For i As Integer = pronounIndex - 1 To 0 Step -1
                    Dim currentWord As String = words(i)
                    ' Check if the current word is a noun or a pronoun
                    If IsNounOrPronoun(currentWord.ToLower) Then
                        Return currentWord
                    End If
                Next

                ' Search for antecedents after the pronoun
                For i As Integer = pronounIndex + 1 To words.Length - 1
                    Dim currentWord As String = words(i)
                    ' Check if the current word is a noun or a pronoun
                    If IsNounOrPronoun(currentWord.ToLower) Then
                        Return currentWord
                    End If
                Next

                ' If no antecedent is found, return an appropriate message
                Return "No antecedent found for the pronoun."
            End Function
            Public Function ResolveGender(ByRef Pronoun As String) As String
                ' If IsProNoun(Pronoun) = True Then

                If IsFemale(Pronoun) = True Then Return "Female"

                If IsMale(Pronoun) = True Then Return "Male"

                'End If
                Return "Non-Binary"
            End Function


            Private Nounlist As New List(Of String)
            Private MaleNames As New List(Of String)
            Private FemaleNames As New List(Of String)
            Public MalePronouns As New List(Of String)
            Public FemalePronouns As New List(Of String)
            Private PronounList As New List(Of String)
            Public MalePersonalNouns() As String = {"him", " he", "his"}
            Public FemalePersonalNouns() As String = {"she", " her", "hers"}
            Public Sub New()
                LoadLists()

            End Sub

            Private Sub LoadLists()
                Dim corpusRoot As String = Application.StartupPath & "\data\"
                Dim wordlistPath As String = Path.Combine(corpusRoot, "NounList.txt")
                Dim wordlistReader As New WordListReader(wordlistPath)
                Nounlist = wordlistReader.GetWords()
                wordlistPath = Path.Combine(corpusRoot, "ProNounList.txt")
                wordlistReader = New WordListReader(wordlistPath)
                PronounList = wordlistReader.GetWords()
                wordlistPath = Path.Combine(corpusRoot, "MaleNames.txt")
                wordlistReader = New WordListReader(wordlistPath)
                MaleNames = wordlistReader.GetWords()
                wordlistPath = Path.Combine(corpusRoot, "FemaleNames.txt")
                wordlistReader = New WordListReader(wordlistPath)
                FemaleNames = wordlistReader.GetWords()
                wordlistPath = Path.Combine(corpusRoot, "MalePronouns.txt")
                wordlistReader = New WordListReader(wordlistPath)
                MalePronouns = wordlistReader.GetWords()
                wordlistPath = Path.Combine(corpusRoot, "FemalePronouns.txt")
                wordlistReader = New WordListReader(wordlistPath)
                FemalePronouns = wordlistReader.GetWords()
            End Sub
            Private Function IsNounOrPronoun(ByRef Word As String) As Boolean
                If IsNoun(Word) = True Then Return True
                If IsProNoun(Word) = True Then Return True

                Return False
            End Function
            Public Function IsProNoun(ByRef Word As String) As Boolean
                For Each item In PronounList
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In FemalePronouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In MalePronouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In FemalePersonalNouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In MalePersonalNouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                Return False
            End Function
            Public Function IsMale(ByRef Word As String) As Boolean
                For Each item In MaleNames
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In MalePronouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In MalePersonalNouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                Return False
            End Function
            Public Function IsFemale(ByRef Word As String)
                For Each item In FemaleNames
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In FemalePronouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                For Each item In FemalePersonalNouns
                    If Word.ToLower = item.ToLower Then Return True
                Next
                Return False
            End Function
            Public Function IsNoun(ByRef Word As String) As Boolean
                For Each item In Nounlist
                    If Word.ToLower = item.ToLower Then Return True
                Next

                Return False
            End Function
        End Class
        Public Class WordListReader
            Private wordList As List(Of String)

            Public Sub New(filePath As String)
                wordList = New List(Of String)()
                ReadWordList(filePath)
            End Sub

            Private Sub ReadWordList(filePath As String)
                Using reader As New StreamReader(filePath)
                    While Not reader.EndOfStream
                        Dim line As String = reader.ReadLine()
                        If Not String.IsNullOrEmpty(line) Then
                            wordList.Add(line.Trim.ToLower)
                        End If
                    End While
                End Using
            End Sub

            Public Function GetWords() As List(Of String)
                Return wordList
            End Function
            ' Usage Example:
            Public Shared Sub Main()
                ' Assuming you have a wordlist file named 'words.txt' in the same directory
                Dim corpusRoot As String = "."
                Dim wordlistPath As String = Path.Combine(corpusRoot, "wordlist.txt")

                Dim wordlistReader As New WordListReader(wordlistPath)
                Dim words As List(Of String) = wordlistReader.GetWords()

                For Each word As String In words
                    Console.WriteLine(word)
                Next
                Console.ReadLine()
                ' Rest of your code...
            End Sub


        End Class
        Public Class Co_Occurrence_Matrix
            Public Shared Function PrintOccurrenceMatrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer)), entityList As List(Of String)) As String
                ' Prepare the header row
                Dim headerRow As String = "|               |"

                For Each entity As String In entityList
                    If coOccurrenceMatrix.ContainsKey(entity) Then
                        headerRow &= $" [{entity}] ({coOccurrenceMatrix(entity).Count}) |"
                    End If
                Next

                Dim str As String = ""
                ' Print the header row
                Console.WriteLine(headerRow)

                str &= headerRow & vbNewLine
                ' Print the co-occurrence matrix
                For Each entity As String In coOccurrenceMatrix.Keys
                    Dim rowString As String = $"| [{entity}] ({coOccurrenceMatrix(entity).Count})        |"

                    For Each coOccurringEntity As String In entityList
                        Dim count As Integer = 0
                        If coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                            count = coOccurrenceMatrix(entity)(coOccurringEntity)
                        End If
                        rowString &= $"{count.ToString().PadLeft(7)} "
                    Next

                    Console.WriteLine(rowString)
                    str &= rowString & vbNewLine
                Next
                Return str
            End Function

            ''' <summary>
            ''' The co-occurrence matrix shows the frequency of co-occurrences between different entities in the given text. Each row represents an entity, and each column represents another entity. The values in the matrix indicate how many times each entity appeared within the specified window size of the other entities. A value of 0 means that the two entities did not co-occur within the given window size.
            ''' </summary>
            ''' <param name="text"></param>
            ''' <param name="entityList"></param>
            ''' <param name="windowSize"></param>
            ''' <returns></returns>
            Public Shared Function iCoOccurrenceMatrix(text As String, entityList As List(Of String), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Integer))
                Dim coOccurrenceMatrix As New Dictionary(Of String, Dictionary(Of String, Integer))

                Dim words() As String = text.ToLower().Split(" "c) ' Convert the text to lowercase here
                For i As Integer = 0 To words.Length - 1
                    If entityList.Contains(words(i)) Then
                        Dim entity As String = words(i)
                        If Not coOccurrenceMatrix.ContainsKey(entity) Then
                            coOccurrenceMatrix(entity) = New Dictionary(Of String, Integer)()
                        End If

                        For j As Integer = i - windowSize To i + windowSize
                            If j >= 0 AndAlso j < words.Length AndAlso i <> j AndAlso entityList.Contains(words(j)) Then
                                Dim coOccurringEntity As String = words(j)
                                If Not coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                                    coOccurrenceMatrix(entity)(coOccurringEntity) = 0
                                End If

                                coOccurrenceMatrix(entity)(coOccurringEntity) += 1
                            End If
                        Next
                    End If
                Next

                Return coOccurrenceMatrix
            End Function

            ''' <summary>
            ''' The PMI matrix measures the statistical association or co-occurrence patterns between different entities in the text. It is calculated based on the co-occurrence matrix. PMI values are used to assess how much more likely two entities are to co-occur together than they would be if their occurrences were independent of each other.
            '''
            '''  positive PMI value indicates that the two entities are likely To co-occur more often than expected by chance, suggesting a positive association between them.
            '''  PMI value Of 0 means that the two entities co-occur As often As expected by chance, suggesting no significant association.
            '''  negative PMI value indicates that the two entities are less likely To co-occur than expected by chance, suggesting a negative association Or avoidance.
            ''' </summary>
            ''' <param name="coOccurrenceMatrix"></param>
            ''' <returns></returns>
            Public Shared Function CalculatePMI(coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer))) As Dictionary(Of String, Dictionary(Of String, Double))
                Dim pmiMatrix As New Dictionary(Of String, Dictionary(Of String, Double))

                For Each entity As String In coOccurrenceMatrix.Keys
                    Dim entityOccurrences As Integer = coOccurrenceMatrix(entity).Sum(Function(kv) kv.Value)

                    If Not pmiMatrix.ContainsKey(entity) Then
                        pmiMatrix(entity) = New Dictionary(Of String, Double)()
                    End If

                    For Each coOccurringEntity As String In coOccurrenceMatrix(entity).Keys
                        Dim coOccurringEntityOccurrences As Integer = coOccurrenceMatrix(entity)(coOccurringEntity)

                        Dim pmi As Double = Math.Log((coOccurringEntityOccurrences * coOccurrenceMatrix.Count) / (entityOccurrences * coOccurrenceMatrix(coOccurringEntity).Sum(Function(kv) kv.Value)), 2)
                        pmiMatrix(entity)(coOccurringEntity) = pmi
                    Next
                Next

                Return pmiMatrix
            End Function
            Public Shared Function PrintOccurrenceMatrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Double)), entityList As List(Of String)) As String
                ' Prepare the header row
                Dim headerRow As String = "|               |"

                For Each entity As String In entityList
                    If coOccurrenceMatrix.ContainsKey(entity) Then
                        headerRow &= $" [{entity}] ({coOccurrenceMatrix(entity).Count}) |"
                    End If
                Next

                Dim str As String = ""
                ' Print the header row
                Console.WriteLine(headerRow)

                str &= headerRow & vbNewLine
                ' Print the co-occurrence matrix
                For Each entity As String In coOccurrenceMatrix.Keys
                    Dim rowString As String = $"| [{entity}] ({coOccurrenceMatrix(entity).Count})        |"

                    For Each coOccurringEntity As String In entityList
                        Dim count As Integer = 0
                        If coOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                            count = coOccurrenceMatrix(entity)(coOccurringEntity)
                        End If
                        rowString &= $"{count.ToString().PadLeft(7)} "
                    Next

                    Console.WriteLine(rowString)
                    str &= rowString & vbNewLine
                Next
                Return str
            End Function
            ''' <summary>
            ''' The PMI matrix measures the statistical association or co-occurrence patterns between different entities in the text. It is calculated based on the co-occurrence matrix. PMI values are used to assess how much more likely two entities are to co-occur together than they would be if their occurrences were independent of each other.
            '''
            '''  positive PMI value indicates that the two entities are likely To co-occur more often than expected by chance, suggesting a positive association between them.
            '''  PMI value Of 0 means that the two entities co-occur As often As expected by chance, suggesting no significant association.
            '''  negative PMI value indicates that the two entities are less likely To co-occur than expected by chance, suggesting a negative association Or avoidance.
            ''' </summary>
            ''' <param name="coOccurrenceMatrix"></param>
            ''' <returns></returns>
            Public Shared Function GetPM_Matrix(ByRef coOccurrenceMatrix As Dictionary(Of String, Dictionary(Of String, Integer))) As Dictionary(Of String, Dictionary(Of String, Double))

                Dim pmiMatrix As Dictionary(Of String, Dictionary(Of String, Double)) = CalculatePMI(coOccurrenceMatrix)
                Return pmiMatrix

            End Function
            Public Shared Sub Main()
                Dim text As String = "The cat sat on the mat. The dog played on the grass. the horse was running on the grass"
                Dim entityList As New List(Of String) From {"cat", "dog", "mat", "played", "sat", "grass", "the", "horse"}

                ' Use a larger window size for more co-occurrences
                Dim windowSize As Integer = 9
                Dim contextWindow As Integer = 1
                Dim matrixBuilder As New Word2WordMatrix()

                For Each document As String In text.Split(".")
                    matrixBuilder.AddDocument(document, contextWindow)
                Next

                Dim wordWordMatrix As Dictionary(Of String, Dictionary(Of String, Integer)) = matrixBuilder.GetWordWordMatrix()
                ' Create the DataGridView control
                Dim wordWordMatrixdataGridView As DataGridView = Word2WordMatrix.CreateDataGridView(wordWordMatrix)


                ' Create a form and add the DataGridView to it
                Dim kform As New Form()
                kform.Text = "Word-Word Matrix"
                kform.Size = New Size(800, 600)
                kform.Controls.Add(wordWordMatrixdataGridView)

                ' Display the form
                Application.Run(kform)


                Dim coOccurrenceMatrix = iCoOccurrenceMatrix(text, entityList, windowSize)

                ' Print the co-occurrence matrix to the console
                For Each entity As String In coOccurrenceMatrix.Keys
                    Console.Write(entity & ": ")
                    For Each coOccurringEntity As String In coOccurrenceMatrix(entity).Keys
                        Console.Write(coOccurrenceMatrix(entity)(coOccurringEntity) & " ")
                    Next
                    Console.WriteLine()
                Next

                Console.WriteLine("----- Co-Occurrence Matrix -----")
                PrintOccurrenceMatrix(coOccurrenceMatrix, entityList)
                ' Create the DataGridView control
                Dim coMatrixdataGridView As DataGridView = Word2WordMatrix.CreateDataGridView(coOccurrenceMatrix)

                ' Create a form and add the DataGridView to it
                Dim iform As New Form()
                iform.Text = " Co-Occurrence Matrix"
                iform.Size = New Size(800, 600)
                iform.Controls.Add(coMatrixdataGridView)

                ' Display the form
                Application.Run(iform)
                'Calc PMI
                Dim PMIMatrix = GetPM_Matrix(coOccurrenceMatrix)

                Console.WriteLine("----- PMI Matrix -----")
                ' Print the PMIco-occurrence matrix to the console
                For Each entity As String In PMIMatrix.Keys
                    Console.Write(entity & ": ")
                    For Each coOccurringEntity As String In PMIMatrix(entity).Keys
                        Console.Write(PMIMatrix(entity)(coOccurringEntity) & " ")
                    Next
                    Console.WriteLine()
                Next

                Console.WriteLine("----- PMI Co-Occurrence Matrix -----")
                PrintOccurrenceMatrix(PMIMatrix, entityList)
                ' Create the DataGridView control
                Dim PMIMatrixdataGridView As DataGridView = Word2WordMatrix.CreateDataGridView(PMIMatrix)

                ' Create a form and add the DataGridView to it
                Dim form As New Form()
                form.Text = " PMI Co-Occurrence Matrix"
                form.Size = New Size(800, 600)
                form.Controls.Add(PMIMatrixdataGridView)

                ' Display the form
                Application.Run(form)
                Console.ReadLine()
            End Sub


        End Class

        Public Class EntityClassifier

            Public Shared Sub Main()
                ' Example usage of EntityClassifier

                ' Sample input text
                Dim inputText As String = "John Smith and Sarah Johnson went to New York. They both work at XYZ Company. Jane works in the office, She is a NLP Specialist"

                ' Sample list of named entities
                Dim namedEntities As New List(Of String) From {"John Smith", "Sarah Johnson", "Jane", "New York", "XYZ Company", "the Office", "Specialist", "NLP"}

                ' Sample list of linking verbs for relationship extraction
                Dim linkingVerbs As New List(Of String) From {"went to", "work at", "works in"}
                Dim iClassifier As New EntityClassifier(namedEntities, "Business", linkingVerbs)

                Dim Result As List(Of EntityClassifier.DiscoveredEntity) = iClassifier.Forwards(inputText)
                ' Display the detected entities

                For Each item In Result.Distinct.ToList
                    Console.WriteLine()
                    Console.WriteLine("RESULT :")
                    Console.WriteLine("-------------------------------------------")
                    Console.WriteLine("Sentence :")
                    Console.WriteLine(item.DiscoveredSentence)
                    Console.WriteLine()
                    Console.WriteLine("Detected Entities:")
                    For Each Ent In item.DiscoveredEntitys
                        Console.WriteLine(Ent)
                    Next
                    Console.WriteLine()
                    Console.WriteLine("Entitys With Context:")
                    For Each Ent In item.EntitysWithContext
                        Console.WriteLine(Ent)
                    Next
                    Console.WriteLine()
                    Console.WriteLine("Sentence Shape:")
                    Console.WriteLine(item.SentenceShape)
                    Console.WriteLine()
                    Console.WriteLine("Entity Sentence :")
                    Console.WriteLine(item.EntitySentence)
                    Console.WriteLine()
                    Console.WriteLine("Relationships:")
                    For Each Ent In item.Relationships
                        Console.WriteLine("Source :" & Ent.SourceEntity)
                        Console.WriteLine("RelationType :" & Ent.RelationshipType)
                        Console.WriteLine("Target :" & Ent.TargetEntity)
                        Console.WriteLine("Relations: " & Ent.Sentence)
                        Console.WriteLine()
                    Next
                    Console.WriteLine("......................................")

                Next
                Console.WriteLine(".......Relation Extraction........")
                Console.WriteLine()
                For Each Ent In iClassifier.DiscoverEntityRelationships(inputText)
                    Console.WriteLine("Related Entities:")
                    For Each item In Ent.DiscoveredEntitys
                        Console.WriteLine(item)

                    Next
                    Console.WriteLine()
                    Console.WriteLine("Relationships:")
                    Console.WriteLine()
                    For Each item In Ent.Relationships

                        Console.WriteLine("Source :" & item.SourceEntity)
                        Console.WriteLine("RelationType :" & item.RelationshipType)
                        Console.WriteLine("Target :" & item.TargetEntity)
                        Console.WriteLine("Relations: " & item.Sentence)
                        Console.WriteLine()

                    Next
                    Console.WriteLine()
                    Console.WriteLine("__________")
                    Console.WriteLine()
                Next
                Console.ReadLine()

            End Sub

            ''' <summary>
            ''' basic relationships between predicates
            ''' </summary>
            Private Shared ReadOnly BasicRelations() As String = {"capeable of", "related to", "relates to", "does", "did", "defined as",
                "can be defined as", "is described as", "is a", "is desired", "is an effect of", "is effected by", "was affected", "is a",
                "made of", "is part of", "a part of", "has the properties", "a property of", "used for", "used as", "is located in",
                "situated in", "is on", "is above", "is below", "begins with", "starts with",
                "was born in", "wrote book", "works at", "works in", "married to"}

            ''' <summary>
            ''' (can be used for topics or entity-lists or sentiment lists)
            ''' </summary>
            Private EntityList As New List(Of String)

            ''' <summary>
            ''' type of List (Describer) ie = "Negative" or "Cars" or "Organizations"
            ''' </summary>
            Private EntityType As String

            ''' <summary>
            ''' Is a,Part Of, Married to, Works at ...
            ''' </summary>
            Private Relations As New List(Of String)

            Public Sub New(entityList As List(Of String), entityType As String, relations As List(Of String))
                If entityList Is Nothing Then
                    Throw New ArgumentNullException(NameOf(entityList))
                End If

                If entityType Is Nothing Then
                    Throw New ArgumentNullException(NameOf(entityType))
                End If

                If relations Is Nothing Then
                    Throw New ArgumentNullException(NameOf(relations))
                End If
                relations.AddRange(BasicRelations.ToList)
                Me.EntityList = entityList
                Me.EntityType = entityType
                Me.Relations.AddRange(relations)
                relations = relations.Distinct.ToList
            End Sub

            ''' <summary>
            ''' Enables for extracting Lists as Single Item
            ''' </summary>
            ''' <returns></returns>
            Public ReadOnly Property CurrentEntityList As List(Of (String, String))
                Get
                    Return GetEntityList()
                End Get
            End Property

            ' Method to extract entity relationships
            Public Function DiscoverEntityRelationships(ByVal document As String) As List(Of DiscoveredEntity)
                Dim EntitySentences As New List(Of DiscoveredEntity)
                Dim endOfSentenceMarkers As String() = {".", "!", "?"}

                ' Split the document into sentences
                Dim sentences As String() = document.Split(endOfSentenceMarkers, StringSplitOptions.RemoveEmptyEntries)

                For Each Sentence In sentences
                    Sentence = Sentence.Trim().ToLower()

                    ' Discover entities in the sentence
                    Dim detectedEntities As List(Of String) = DetectEntitysInText(Sentence)

                    ' Find relationships between entities based on patterns/rules
                    Dim relationships As List(Of DiscoveredEntity.EntityRelationship) = FindEntityRelationships(detectedEntities, Sentence, Relations)

                    ' Create the DiscoveredEntity object with relationships
                    Dim discoveredEntity As New DiscoveredEntity With {
                                .DiscoveredEntitys = detectedEntities,
                                .DiscoveredSentence = Sentence,
                                .EntitysWithContext = DetectEntitysWithContextInText(Sentence, 2),
                                .SentenceShape = DiscoverShape(Sentence),
                                .Relationships = relationships.Distinct.ToList
                            }

                    EntitySentences.Add(discoveredEntity)
                Next

                Return EntitySentences.Distinct.ToList
            End Function

            ' Add this method to the EntityClassifier class
            Public Function Forwards(ByVal documents As List(Of String)) As List(Of List(Of DiscoveredEntity))
                Dim batchResults As New List(Of List(Of DiscoveredEntity))

                For Each document In documents
                    Dim documentEntities As List(Of DiscoveredEntity) = Forwards(document)
                    batchResults.Add(documentEntities)
                Next

                Return batchResults.Distinct.ToList
            End Function

            ''' <summary>
            ''' Classify Entity Sentences
            ''' </summary>
            ''' <param name="document"></param>
            ''' <returns>Entity Sentences by Type</returns>
            Public Function Forwards(ByVal document As String) As List(Of DiscoveredEntity)
                Dim EntitySentences As New List(Of DiscoveredEntity)
                ' Store the classified sentences in a dictionary
                Dim classifiedSentences As New Dictionary(Of String, List(Of String))
                ' Define a list of possible end-of-sentence punctuation markers
                Dim endOfSentenceMarkers As String() = {".", "!", "?"}

                ' Split the document into sentences
                Dim sentences As String() = document.Split(endOfSentenceMarkers, StringSplitOptions.RemoveEmptyEntries)

                ' Rule-based classification
                For Each sentence In sentences
                    ' Remove leading/trailing spaces and convert to lowercase
                    sentence = sentence.ToLower()

                    'Discover
                    For Each EntityItem In EntityList
                        If sentence.ToLower.Contains(EntityItem.ToLower) Then
                            Dim Sent As New DiscoveredEntity

                            Sent.DiscoveredEntitys = DetectEntitysInText(sentence).Distinct.ToList
                            Sent.DiscoveredSentence = sentence
                            Sent.EntitysWithContext = DetectEntitysWithContextInText(sentence, 5).Distinct.ToList

                            Sent.Relationships = FindEntityRelationships(Sent.DiscoveredEntitys, sentence, Relations).Distinct.ToList
                            Sent.EntitySentence = TransformText(sentence)
                            Sent.SentenceShape = DiscoverShape(sentence)
                            EntitySentences.Add(Sent)

                        End If
                    Next

                Next
                Return EntitySentences.Distinct.ToList
            End Function

            ''' <summary>
            ''' Extracts patterns from the text,
            ''' replaces detected entities with asterisks,
            ''' and replaces the entity label with asterisks.
            ''' This is to replace a specific entity in the text
            ''' </summary>
            '''
            ''' <param name="Text">The text to extract patterns from.</param>
            ''' <returns>The extracted pattern with detected entities and the entity label replaced by asterisks.</returns>
            Public Function TransformText(ByRef Text As String) As String
                If Text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                Dim Entitys As New List(Of String)
                Dim Str As String = Text
                If DetectEntity(Text) = True Then
                    Entitys = DetectEntitysInText(Text)
                    ' Str = DiscoverShape(Str)
                    Str = Transform.TransformText(Str.ToLower, Entitys, EntityType)
                End If
                Return Str
            End Function

            ''' <summary>
            ''' Checks if any entities from the EntityList are present in the text.
            ''' </summary>
            ''' <param name="text">The text to be checked.</param>
            ''' <returns>True if any entities are detected, False otherwise.</returns>
            Private Function DetectEntity(ByRef text As String) As Boolean
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                For Each item In EntityList
                    If text.ToLower.Contains(item.ToLower) Then
                        Return True
                    End If
                Next
                Return False
            End Function

            ''' <summary>
            ''' Detects entities in the given text.
            ''' </summary>
            ''' <returns>A list of detected entities in the text.</returns>
            Private Function DetectEntitysInText(ByRef text As String) As List(Of String)
                Dim Lst As New List(Of String)
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If DetectEntity(text) = True Then
                    For Each item In EntityList
                        If text.ToLower.Contains(item.ToLower) Then
                            Lst.Add(item)
                        End If
                    Next
                    Return Lst.Distinct.ToList
                Else

                End If
                Return New List(Of String)
            End Function

            Private Function DetectEntitysWithContextInText(ByRef text As String, Optional contextLength As Integer = 1) As List(Of String)
                Dim Lst As New List(Of String)
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If

                If EntityList Is Nothing Then
                    Throw New ArgumentNullException("EntityList")
                End If
                If DetectEntity(text) = True Then
                    For Each item In EntityList
                        If text.ToLower.Contains(item.ToLower) Then
                            'Add Context
                            Lst.Add(ExtractEntityContextFromText(text, item, contextLength))
                        End If
                    Next
                    Return Lst.Distinct.ToList
                Else
                    Return New List(Of String)
                End If
            End Function

            ''' <summary>
            ''' Discovers shapes in the text and replaces the detected entities with entity labels.
            ''' </summary>
            ''' <param name="text">The text to discover shapes in.</param>
            ''' <returns>The text with detected entities replaced by entity labels.</returns>
            Private Function DiscoverShape(ByRef text As String) As String
                If text Is Nothing Then
                    Throw New ArgumentNullException("text")
                End If
                Dim Entitys As New List(Of String)
                Dim Str As String = text
                If DetectEntity(text) = True Then
                    Entitys = DetectEntitysInText(text)

                    Str = Transform.TransformText(Str, Entitys, "*")

                End If
                Return Str
            End Function

            ''' <summary>
            ''' Extracts Entity With Context;
            '''
            ''' </summary>
            ''' <param name="text">doc</param>
            ''' <param name="entity">Entity Value</param>
            ''' <param name="contextLength"></param>
            ''' <returns>a concat string</returns>
            Private Function ExtractEntityContextFromText(ByVal text As String,
                                                ByVal entity As String,
                                                Optional contextLength As Integer = 4) As String
                Dim entityIndex As Integer = text.ToLower.IndexOf(entity.ToLower)
                Dim contextStartIndex As Integer = Math.Max(0, entityIndex - contextLength)
                Dim contextEndIndex As Integer = Math.Min(text.Length - 1, entityIndex + entity.Length + contextLength)
                Dim NewEntity As New Entity
                NewEntity.StartIndex = contextStartIndex
                NewEntity.EndIndex = contextEndIndex
                NewEntity.Value = text.Substring(contextStartIndex, contextEndIndex - contextStartIndex + 1)

                Return text.Substring(contextStartIndex, contextEndIndex - contextStartIndex + 1)
            End Function

            ' Method to find entity relationships based on patterns/rules
            Public Function FindEntityRelationships(ByVal entities As List(Of String), ByVal sentence As String, ByRef ConceptRelations As List(Of String)) As List(Of DiscoveredEntity.EntityRelationship)
                ' Define relationship patterns/rules based on the specific use case
                ' For example, "works at," "is the CEO of," etc.

                Dim relationships As New List(Of DiscoveredEntity.EntityRelationship)

                ' Sample rule for demonstration (Assuming "works at" relationship)
                For i = 0 To entities.Count - 1
                    For j = 0 To entities.Count - 1
                        If i <> j Then
                            For Each Relation In ConceptRelations
                                If sentence.ToLower.Contains(entities(i).ToLower & " " & Relation.ToLower & " " & entities(j).ToLower) Then
                                    relationships.Add(New DiscoveredEntity.EntityRelationship With {
                                                .SourceEntity = entities(i),
                                                .TargetEntity = entities(j),
                                                .RelationshipType = Relation,
                                                .Sentence = ExtractPredicateRelation(sentence.ToLower, Relation.ToLower)
                                            })
                                End If
                            Next
                        End If
                    Next
                Next

                ' Add more rules and patterns as needed for your specific use case
                ' Example: "is the CEO of," "married to," etc.

                Return relationships.Distinct.ToList
            End Function

            Public Shared Function ExtractPredicateRelation(Sentence As String, ByRef LinkingVerb As String) As String
                ' Implement your custom dependency parsing logic here
                ' Analyze the sentence and extract the relationships
                Dim relationship As String = ""

                ' Example relationship extraction logic
                If Sentence.ToLower.Contains(" " & LinkingVerb.ToLower & " ") Then
                    Dim subject As String = ""
                    Dim iobject As String = ""
                    Discover.SplitPhrase(Sentence.ToLower, LinkingVerb.ToLower, subject, iobject)

                    relationship = $"{subject} -  {LinkingVerb } - {iobject}"

                End If

                Return relationship
            End Function

            Private Function GetEntityList() As List(Of (String, String))
                Dim iEntityList As New List(Of (String, String))
                For Each entity In EntityList
                    iEntityList.Add((entity, EntityType))
                Next

                Return iEntityList.Distinct.ToList
            End Function

            Public Structure DiscoveredEntity

                ''' <summary>
                ''' Discovered Entity
                ''' </summary>
                Public DiscoveredEntitys As List(Of String)

                ''' <summary>
                ''' Associated Sentence
                ''' </summary>
                Public DiscoveredSentence As String

                ''' <summary>
                ''' Entity Sentence
                ''' </summary>
                Public EntitySentence As String

                ''' <summary>
                ''' Entity with surrounding Context
                ''' </summary>
                Public EntitysWithContext As List(Of String)

                Public Relationships As List(Of EntityRelationship)

                ''' <summary>
                ''' Associated Sentence Sentence Shape
                ''' </summary>
                Public SentenceShape As String

                ''' <summary>
                ''' Outputs Structure to Jason(JavaScriptSerializer)
                ''' </summary>
                ''' <returns></returns>
                Public Function ToJson() As String
                    Dim Converter As New JavaScriptSerializer
                    Return Converter.Serialize(Me)
                End Function

                ' New property for relationships

                ' New structure to represent entity relationships
                Public Structure EntityRelationship
                    Public Property RelationshipType As String
                    Public Property Sentence As String
                    Public Property SourceEntity As String
                    Public Property TargetEntity As String
                End Structure

            End Structure

            Public Structure Entity
                Public Property EndIndex As Integer
                Public Property StartIndex As Integer
                Public Property Type As String
                Public Property Value As String

            End Structure

            Public Class Detect

                ''' <summary>
                ''' Checks if any entities from the EntityList are present in the text.
                ''' </summary>
                ''' <param name="text">The text to be checked.</param>
                ''' <returns>True if any entities are detected, False otherwise.</returns>
                Public Shared Function DetectEntity(ByRef text As String, ByRef EntityList As List(Of String)) As Boolean
                    If text Is Nothing Then
                        Throw New ArgumentNullException("text")
                    End If

                    For Each item In EntityList
                        If text.Contains(item) Then
                            Return True
                        End If
                    Next
                    Return False
                End Function

                ' Modify DetectEntity and DetectEntitysInText methods to handle multiple entity types
                Public Shared Function DetectEntity(ByRef text As String, ByRef entityTypes As Dictionary(Of String, List(Of String))) As Boolean
                    If text Is Nothing Then
                        Throw New ArgumentNullException("text")
                    End If

                    For Each iEntityType In entityTypes
                        For Each item In iEntityType.Value
                            If text.Contains(item) Then
                                Return True
                            End If
                        Next
                    Next

                    Return False
                End Function

                Public Shared Function DetectEntitysInText(ByRef text As String, ByRef Entitylist As List(Of String)) As List(Of String)
                    Dim Lst As New List(Of String)
                    If text Is Nothing Then
                        Throw New ArgumentNullException("text")
                    End If

                    If Entitylist Is Nothing Then
                        Throw New ArgumentNullException("EntityList")
                    End If
                    If DetectEntity(text, Entitylist) = True Then
                        For Each item In Entitylist
                            If text.Contains(item) Then
                                Lst.Add(item)
                            End If
                        Next
                        Return Lst.Distinct.ToList
                    Else
                        Return New List(Of String)
                    End If
                End Function

                Public Shared Function DetectEntitysInText(ByRef Text As String, EntityList As List(Of String), ByRef EntityLabel As String) As List(Of String)
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
                    If EntityClassifier.Detect.DetectEntity(Text, EntityList) = True Then

                        Dim DetectedEntitys As List(Of String) = EntityClassifier.Detect.DetectEntitysInText(Text, EntityList)
                        Dim Shape As String = Discover.DiscoverShape(Text, EntityList, EntityLabel)
                        Dim pattern As String = Discover.ExtractPattern(Text, EntityList, EntityLabel)
                        output = DetectedEntitys
                    Else

                    End If
                    Return output.Distinct.ToList
                End Function

                Public Shared Function DetectEntitysInText(ByRef text As String, ByRef entityTypes As Dictionary(Of String, List(Of String))) As List(Of Entity)
                    Dim detectedEntities As New List(Of Entity)

                    If text Is Nothing Then
                        Throw New ArgumentNullException("text")
                    End If

                    For Each iEntityType In entityTypes
                        For Each item In iEntityType.Value
                            If text.Contains(item) Then
                                Dim startIndex As Integer = text.IndexOf(item)
                                Dim endIndex As Integer = startIndex + item.Length - 1
                                detectedEntities.Add(New Entity With {
                                         .StartIndex = startIndex,
                                         .EndIndex = endIndex,
                                         .Type = iEntityType.Key,
                                         .Value = item
                                     })
                            End If
                        Next
                    Next

                    Return detectedEntities.Distinct.ToList
                End Function

                ''' <summary>
                ''' Predicts the position of an entity relative to the focus term within the context words.
                ''' </summary>
                ''' <param name="contextWords">The context words.</param>
                ''' <param name="focusTerm">The focus term.</param>
                ''' <returns>The predicted entity position.</returns>
                Public Shared Function PredictEntityPosition(contextWords As List(Of String), focusTerm As String) As String
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

                Public Shared Function WordIsEntity(ByRef Word As String, ByRef Entitys As List(Of String)) As Boolean
                    For Each item In Entitys
                        If Word = item Then
                            Return True
                        End If
                    Next
                    Return False
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
                Public Shared Function DetectNamedEntities(sentence As String, ByRef Entitys As List(Of String), ByRef EntityLabel As String) As List(Of Entity)
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

                    Return entities.Distinct.ToList
                End Function

            End Class

            Public Class Discover

                ''' <summary>
                ''' Used to retrieve Learning Patterns
                ''' Learning Pattern / Nym
                ''' </summary>
                Public Structure SemanticPattern

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

                End Structure

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

                ''' <summary>
                ''' Attempts to find Entitys identified by thier capitalization
                ''' </summary>
                ''' <param name="words"></param>
                ''' <returns></returns>
                Public Shared Function DetectNamedEntities(ByVal words() As String) As List(Of String)
                    Dim namedEntities As New List(Of String)

                    For i = 0 To words.Length - 1
                        Dim word = words(i)
                        If Char.IsUpper(word(0)) Then
                            namedEntities.Add(word)
                        End If
                    Next

                    Return namedEntities.Distinct.ToList
                End Function

                Public Shared Function DetermineEntailment(overlap As Integer) As Boolean
                    ' Set a threshold for entailment
                    Dim threshold As Integer = 2

                    ' Determine entailment based on overlap
                    Return overlap >= threshold
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
                    If Classifer.EntityClassifier.Detect.DetectEntity(text, EntityList) = True Then
                        Entitys = EntityClassifier.Detect.DetectEntitysInText(text, EntityList)

                        Str = Transform.TransformText(Str, Entitys)

                    End If
                    Return Str
                End Function

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
                    If Classifer.EntityClassifier.Detect.DetectEntity(Text, Entitylist) = True Then
                        Entitys = Classifer.EntityClassifier.Detect.DetectEntitysInText(Text, Entitylist)

                        Str = Transform.TransformText(Str, Entitys, EntityLabel)

                    End If
                    Return Str
                End Function

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
                    If Classifer.EntityClassifier.Detect.DetectEntity(Text, Entitylist) = True Then
                        Entitys = Classifer.EntityClassifier.Detect.DetectEntitysInText(Text, Entitylist)
                        Str = Discover.DiscoverShape(Str, Entitys)
                        Str = Transform.TransformText(Str, Entitys)
                        Str = Str.Replace("[" & EntityLabel & "]", "*")
                    End If
                    Return Str
                End Function

                Public Shared Function ExtractPredicateRelations(text As String, ByRef LinkingVerb As String) As List(Of String)
                    ' Implement your custom dependency parsing logic here
                    ' Analyze the sentence and extract the relationships

                    Dim relationships As New List(Of String)
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
                    Return relationships.Distinct.ToList
                End Function

                ''' <summary>
                ''' SPLITS THE GIVEN PHRASE UP INTO TWO PARTS by dividing word SplitPhrase(Userinput, "and",
                ''' Firstp, SecondP)
                ''' </summary>
                ''' <param name="PHRASE">      Sentence to be divided</param>
                ''' <param name="DIVIDINGWORD">String: Word to divide sentence by</param>
                ''' <param name="FIRSTPART">   String: firstpart of sentence to be populated</param>
                ''' <param name="SECONDPART">  String: Secondpart of sentence to be populated</param>
                ''' <remarks></remarks>
                Public Shared Sub SplitPhrase(ByVal PHRASE As String, ByRef DIVIDINGWORD As String, ByRef FIRSTPART As String, ByRef SECONDPART As String)
                    Dim POS As Short
                    POS = InStr(PHRASE, DIVIDINGWORD)
                    If (POS > 0) Then
                        FIRSTPART = Trim(Left(PHRASE, POS - 1))
                        SECONDPART = Trim(Right(PHRASE, Len(PHRASE) - POS - Len(DIVIDINGWORD) + 1))
                    Else
                        FIRSTPART = ""
                        SECONDPART = PHRASE
                    End If
                End Sub

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
                ''' Given a list of Entity-lists and a single entity-list
                ''' Extract a new list (AssociatedEntities)
                ''' </summary>
                ''' <param name="entityList"> a single entity-list ,
                ''' Possibly assert(value) discovered list of entitys, Or a particular intresting list</param>
                ''' <param name="storedEntityLists"></param>
                ''' <returns></returns>
                Public Shared Function GetAssociatedEntities(entityList As List(Of String), storedEntityLists As List(Of List(Of String))) As List(Of String)
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

                Public Shared Function GetEntityCountInText(ByVal text As String, ByVal entity As String) As Integer
                    Dim regex As New Regex(Regex.Escape(entity))
                    Return regex.Matches(text).Count
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
                    Return DbSubjectLst.Distinct.ToList
                End Function

            End Class

            Public Class Transform

                Public Shared Function RemoveEntitiesFromText(ByVal text As String, ByVal entities As List(Of String)) As String
                    For Each entity As String In entities
                        text = text.Replace(entity, String.Empty)
                    Next

                    Return text
                End Function

                Public Shared Function ReplaceEntities(sentence As String, ByRef ENTITYS As List(Of String)) As String
                    ' Replace discovered entities in the sentence with their entity type
                    For Each entity As String In ENTITYS
                        Dim entityType As String = entity.Substring(0, entity.IndexOf("("))
                        Dim entityValue As String = entity.Substring(entity.IndexOf("(") + 1, entity.Length - entity.IndexOf("(") - 2)
                        sentence = sentence.Replace(entityValue, entityType)
                    Next

                    Return sentence
                End Function

                ''' <summary>
                ''' Replaces the encapsulated [Entity Label] with an asterisk (*)
                ''' ie the [Entity] walked
                ''' </summary>
                ''' <param name="text">The text to be modified.</param>
                ''' <param name="Entitylabel">The label to replace the entities with.</param>
                ''' <returns>The text with entities replaced by the label.</returns>
                Public Shared Function ReplaceEntityLabel(ByRef text As String, ByRef EntityLabel As String, Value As String) As String
                    If text Is Nothing Then
                        Throw New ArgumentNullException("text")
                    End If
                    If EntityLabel Is Nothing Then
                        Throw New ArgumentNullException("EntityLabel")
                    End If
                    Dim str As String = text
                    str = str.Replace("[" & EntityLabel & "]", Value)
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
                        Str = Str.ToLower.Replace(item.ToLower, "[" & iLabel & "]")
                    Next
                    Return Str
                End Function

            End Class

        End Class
        Public Class EntailmentClassifier
            Public conclusionIndicators As String() = {"therefore", "thus", "consequently", "hence", "in conclusion", "Therefore", "Thus", "As a result"}
            Public hypothesisIndicators As String() = {"if", "when", "suppose that", "let's say", "assuming that", "Suppose", "Assume", "In the case that", "Given that"}
            Public premiseIndicators As String() = {"based on", "according to", "given", "assuming", "since", "According to", "Based on", "In light of", "Considering", "The evidence suggests"}
            Private premises As New List(Of String)
            Private hypotheses As New List(Of String)
            Private conclusions As New List(Of String)

            Public Sub New(conclusionIndicators() As String, hypothesisIndicators() As String, premiseIndicators() As String)
                If conclusionIndicators Is Nothing Then
                    Throw New ArgumentNullException(NameOf(conclusionIndicators))
                End If

                If hypothesisIndicators Is Nothing Then
                    Throw New ArgumentNullException(NameOf(hypothesisIndicators))
                End If

                If premiseIndicators Is Nothing Then
                    Throw New ArgumentNullException(NameOf(premiseIndicators))
                End If

                Me.conclusionIndicators = conclusionIndicators
                Me.hypothesisIndicators = hypothesisIndicators
                Me.premiseIndicators = premiseIndicators
            End Sub

            Public Sub New()
            End Sub

            Public Function IsPremise(ByVal sentence As String) As Boolean

                ' Check if any of the premise indicators are present in the sentence
                For Each indicator In premiseIndicators
                    If sentence.Contains(indicator) Then
                        Return True
                    End If
                Next

                Return False
            End Function

            Public Function IsHypothesis(ByVal sentence As String) As Boolean
                ' List of indicator phrases for hypotheses

                ' Check if any of the hypothesis indicators are present in the sentence
                For Each indicator In hypothesisIndicators
                    If sentence.Contains(indicator) Then
                        Return True
                    End If
                Next

                Return False
            End Function

            Public Function IsConclusion(ByVal sentence As String) As Boolean
                ' List of indicator phrases for conclusions

                ' Check if any of the conclusion indicators are present in the sentence
                For Each indicator In conclusionIndicators
                    If sentence.Contains(indicator) Then
                        Return True
                    End If
                Next

                Return False
            End Function

            Public Function Classify(ByVal document As String) As Dictionary(Of String, List(Of String))

                ' Define a list of possible end-of-sentence punctuation markers
                Dim endOfSentenceMarkers As String() = {".", "!", "?"}

                ' Split the document into sentences
                Dim sentences As String() = document.Split(endOfSentenceMarkers, StringSplitOptions.RemoveEmptyEntries)

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
                Dim classifiedSentences As New Dictionary(Of String, List(Of String))
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

            Public Function ClassifySentence(sentence As String, document As String) As String
                ' Rule-based sentence classification

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

                For Each item In premiseIndicators
                    Dim premisePattern As String = "(?i)(\b" & item & "\b.*?)"
                    If Regex.IsMatch(sentence, premisePattern) Then Return "Premise"
                Next
                For Each item In conclusionIndicators
                    Dim conclusionPattern As String = "(?i)(\b" & item & "\b.*?)"
                    If Regex.IsMatch(sentence, conclusionPattern) Then Return "Conclusion"
                Next
                For Each item In hypothesisIndicators
                    Dim hypothesisPattern As String = "(?i)(\b" & item & "\b.*?)"
                    If Regex.IsMatch(sentence, hypothesisPattern) Then Return "Hypothesis"
                Next

                Return "Unknown"
            End Function

            ''' <summary>
            ''' Attempts to Resolve any relational sentences
            ''' </summary>
            ''' <param name="document"></param>
            ''' <returns></returns>
            Public Function ClassifyAndResolve(ByVal document As String) As Dictionary(Of String, String)
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
                    If IsPremise(sentence) Then
                        premises.Add(index, sentence)
                    ElseIf IsHypothesis(sentence) Then
                        hypotheses.Add(index, sentence)
                    ElseIf IsConclusion(sentence) Then
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

            Public Function ResolveKnown(ByVal classifiedSentences As Dictionary(Of String, List(Of String))) As Dictionary(Of String, String)
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

        End Class
        Public Class Word2WordMatrix
            Private matrix As Dictionary(Of String, Dictionary(Of String, Integer))

            Public Sub New()
                matrix = New Dictionary(Of String, Dictionary(Of String, Integer))
            End Sub
            Public Shared Function CreateDataGridView(matrix As Dictionary(Of String, Dictionary(Of String, Double))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Object = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function
            Public Function iCoOccurrenceMatrix(text As String, entityList As List(Of String), windowSize As Integer) As Dictionary(Of String, Dictionary(Of String, Integer))
                Dim CoOccurrenceMatrix As New Dictionary(Of String, Dictionary(Of String, Integer))

                Dim words() As String = text.Split(" "c)
                For i As Integer = 0 To words.Length - 1
                    If entityList.Contains(words(i).ToLower()) Then
                        Dim entity As String = words(i)
                        If Not CoOccurrenceMatrix.ContainsKey(entity) Then
                            CoOccurrenceMatrix(entity) = New Dictionary(Of String, Integer)()
                        End If

                        For j As Integer = i - windowSize To i + windowSize
                            If j >= 0 AndAlso j < words.Length AndAlso i <> j AndAlso entityList.Contains(words(j).ToLower()) Then
                                Dim coOccurringEntity As String = words(j)
                                If Not CoOccurrenceMatrix(entity).ContainsKey(coOccurringEntity) Then
                                    CoOccurrenceMatrix(entity)(coOccurringEntity) = 0
                                End If

                                CoOccurrenceMatrix(entity)(coOccurringEntity) += 1
                            End If
                        Next
                    End If
                Next

                Return CoOccurrenceMatrix
            End Function

            Public Shared Function CreateDataGridView(matrix As Dictionary(Of String, Dictionary(Of String, Integer))) As DataGridView
                Dim dataGridView As New DataGridView()
                dataGridView.Dock = DockStyle.Fill
                dataGridView.AutoGenerateColumns = False
                dataGridView.AllowUserToAddRows = False

                ' Add columns to the DataGridView
                Dim wordColumn As New DataGridViewTextBoxColumn()
                wordColumn.HeaderText = "Word"
                wordColumn.DataPropertyName = "Word"
                dataGridView.Columns.Add(wordColumn)

                For Each contextWord As String In matrix.Keys
                    Dim contextColumn As New DataGridViewTextBoxColumn()
                    contextColumn.HeaderText = contextWord
                    contextColumn.DataPropertyName = contextWord
                    dataGridView.Columns.Add(contextColumn)
                Next

                ' Populate the DataGridView with the matrix data
                For Each word As String In matrix.Keys
                    Dim rowValues As New List(Of Object)()
                    rowValues.Add(word)

                    For Each contextWord As String In matrix.Keys
                        Dim count As Integer = If(matrix(word).ContainsKey(contextWord), matrix(word)(contextWord), 0)
                        rowValues.Add(count)
                    Next

                    dataGridView.Rows.Add(rowValues.ToArray())
                Next

                Return dataGridView
            End Function

            Public Sub AddDocument(document As String, contextWindow As Integer)
                Dim words As String() = document.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)

                For i As Integer = 0 To words.Length - 1
                    Dim currentWord As String = words(i)

                    If Not matrix.ContainsKey(currentWord) Then
                        matrix(currentWord) = New Dictionary(Of String, Integer)()
                    End If

                    For j As Integer = Math.Max(0, i - contextWindow) To Math.Min(words.Length - 1, i + contextWindow)
                        If i <> j Then
                            Dim contextWord As String = words(j)

                            If Not matrix(currentWord).ContainsKey(contextWord) Then
                                matrix(currentWord)(contextWord) = 0
                            End If

                            matrix(currentWord)(contextWord) += 1
                        End If
                    Next
                Next
            End Sub
            Public Shared Sub Main()
                ' Fill the matrix with your data
                Dim documents As List(Of String) = New List(Of String)()
                documents.Add("This is the first document.")
                documents.Add("The second document is here.")
                documents.Add("And this is the third document.")

                Dim contextWindow As Integer = 1
                Dim matrixBuilder As New Word2WordMatrix()

                For Each document As String In documents
                    matrixBuilder.AddDocument(document, contextWindow)
                Next

                Dim wordWordMatrix As Dictionary(Of String, Dictionary(Of String, Integer)) = matrixBuilder.GetWordWordMatrix()

                ' Create the DataGridView control
                Dim dataGridView As DataGridView = Word2WordMatrix.CreateDataGridView(wordWordMatrix)

                ' Create a form and add the DataGridView to it
                Dim form As New Form()
                form.Text = "Word-Word Matrix"
                form.Size = New Size(800, 600)
                form.Controls.Add(dataGridView)

                ' Display the form
                Application.Run(form)
            End Sub
            Public Function GetWordWordMatrix() As Dictionary(Of String, Dictionary(Of String, Integer))
                Return matrix
            End Function
        End Class

    End Namespace

    Public Class Summarise

        Public Function GenerateSummary(ByRef Text As String, ByRef Entitys As List(Of String)) As String
            ' Step 5: Generate the summary
            Return String.Join(vbNewLine, ExtractImportantSentencesInText(Text, Entitys, True, 2))
        End Function

        Public Function GenerateSummary(ByVal text As String, ByVal entities As List(Of String), ByVal numContextSentencesBefore As Integer, ByVal numContextSentencesAfter As Integer) As String
            ' Extract important sentences with context
            Dim importantSentences As List(Of String) = ExtractImportantSentencesInText(text, entities, numContextSentencesBefore, numContextSentencesAfter)

            ' Generate the summary
            Dim summary As String = String.Join(". ", importantSentences)

            Return summary
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
        Public Function ExtractImportantSentencesInText(ByRef Text As String,
                                                           EntityList As List(Of String),
                                                           Optional WithContext As Boolean = False,
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
        Public Function ExtractImportantSentencesInText(ByVal text As String, ByVal entityList As List(Of String), ByVal numContextSentencesBefore As Integer, ByVal numContextSentencesAfter As Integer) As List(Of String)
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
        Public Function ExtractImportantSentencesInText(ByRef Text As String, EntityList As List(Of String), Optional WithContext As Boolean = False,
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
        ''' Given an important Sentence Extract its surrounding context Sentences
        ''' </summary>
        ''' <param name="Text"></param>
        ''' <param name="ImportantSentence">Important Sentence to match</param>
        ''' <param name="ConTextInt">Number of Sentences Either Side</param>
        ''' <returns></returns>
        Public Function ExtractContextSentences(ByRef Text As String, ByRef ImportantSentence As String, ByRef ConTextInt As Integer) As List(Of String)
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
        Public Function ExtractContextSentences(ByVal text As String, ByVal importantSentence As String, ByVal numContextSentencesBefore As Integer, ByVal numContextSentencesAfter As Integer) As List(Of String)
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

        Public Function GenerateTextFromEntities(entities As List(Of String), storedSentences As List(Of String)) As String
            ' Implement your custom text generation logic here
            ' Generate text using the entities and stored sentences

            Dim generatedText As String = ""

            ' Example text generation logic
            For Each entity As String In entities
                Dim matchingSentences As List(Of String) = FindSentencesWithEntity(entity, storedSentences)

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

        Public Function FindSentencesWithEntity(entity As String, storedSentences As List(Of String)) As List(Of String)
            ' Implement your custom logic to find sentences that contain the given entity
            ' Return a list of sentences that match the entity

            Dim matchingSentences As New List(Of String)

            ' Example logic: Check if the entity appears in each stored sentence
            For Each sentence As String In storedSentences
                If sentence.Contains(entity) Then
                    matchingSentences.Add(sentence)
                End If
            Next

            Return matchingSentences
        End Function

    End Class

    Public Module Ext

        ''' <summary>
        ''' Writes the contents of an embedded resource embedded as Bytes to disk.
        ''' </summary>
        ''' <param name="BytesToWrite">Embedded resource</param>
        ''' <param name="FileName">    Save to file</param>
        ''' <remarks></remarks>
        <System.Runtime.CompilerServices.Extension()>
        Public Sub FileSave(ByVal BytesToWrite() As Byte, ByVal FileName As String)

            If IO.File.Exists(FileName) Then
                IO.File.Delete(FileName)
            End If

            Dim FileStream As New System.IO.FileStream(FileName, System.IO.FileMode.OpenOrCreate)
            Dim BinaryWriter As New System.IO.BinaryWriter(FileStream)

            BinaryWriter.Write(BytesToWrite)
            BinaryWriter.Close()
            FileStream.Close()
        End Sub
        ''' <summary>
        ''' Extracts words between  based on the before and after words
        ''' IE: THe cat sat on the mat (before The After The) output: cat sat on
        ''' </summary>
        ''' <param name="sentence"></param>
        ''' <param name="beforeWord"></param>
        ''' <param name="afterWord"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ExtractWordsBetween(sentence As String, beforeWord As String, afterWord As String) As List(Of String)
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

        <Extension>
        Public Function StartsWithAny(str As String, values As IEnumerable(Of String)) As Boolean
            For Each value As String In values
                If str.StartsWith(value) Then
                    Return True
                End If
            Next

            Return False
        End Function

    End Module
End Namespace