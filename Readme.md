# Json.zip

Works as a utility to serialize object and compress the content.

## Usage

* Serialize & compress

```csharp
    var list = objectToBeSerialized;
    await JsonZipSerializer.Instance.SerializeAsync(list, "output.json.compressed").ConfigureAwait(false);
```

* Decompress & deserialize

```csharp
    var deserialized = await JsonZipSerializer.Instance.DeserializeAsync<Model>("output.json.compressed").ConfigureAwait(false);
```
