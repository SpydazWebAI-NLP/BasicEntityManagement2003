# iEntityHandler

iEntityHandler is a .NET library that provides functionalities for processing and analyzing text data, including entity detection, relationship resolution, similarity scoring, and information extraction. It is designed to be flexible and can be used in various natural language processing (NLP) tasks.

## Features

- Entity detection: Identify named entities such as names, locations, dates, and more in a given text.
- Relationship resolution: Resolve relationships between entities to extract meaningful information.
- Similarity scoring: Calculate similarity scores between phrases using cosine similarity and Jaccard similarity.
- Information extraction: Extract email addresses and phone numbers from text.
- Noun phrase extraction: Extract noun phrases from sentences.
- Verb phrase extraction: Extract verb phrases from sentences.
- Adjective phrase extraction: Extract adjective phrases from sentences.
- Text transformation: Replace entities in text with a specified label or asterisks.
- Sentence tagging: Assign tags to words or phrases based on certain criteria (e.g., part-of-speech, named entity).

## Usage

You can use the `iEntityHandler` library in your .NET projects by adding a reference to the assembly. Here's an example of how to use some of the functionalities:

```vb
' Entity detection
Dim text As String = "John Doe is a software engineer at XYZ Corp."
Dim detectedEntities As Dictionary(Of String, String) = iEntityHandler.iDetect.DetectNamedEntities(text)

' Relationship resolution
Dim relationships As Dictionary(Of String, String) = iEntityHandler.iResolve.ResolveRelationships(detectedEntities)

' Similarity scoring
Dim phrase1 As String = "The quick brown fox"
Dim phrase2 As String = "The lazy dog"
Dim cosineSimilarity As Double = iEntityHandler.iSearch.ComputeCosineSimilarity(phrase1, phrase2)
Dim jaccardSimilarity As Double = iEntityHandler.iSearch.ComputeJaccardSimilarity(phrase1, phrase2)

' Information extraction
Dim emailAddresses As List(Of String) = iEntityHandler.iSearch.ExtractEmailAddresses(text)
Dim phoneNumbers As List(Of String) = iEntityHandler.iSearch.ExtractPhoneNumbers(text)
```



# Requirements




## NET Framework 4.5 or higher.

License
This project is licensed under the MIT License.

## Contribution
Contributions are welcome! If you find any issues or have ideas for improvement, please open an issue or submit a pull request.

## Acknowledgments
Thank you to the developers and contributors of the underlying libraries and resources used in this project.