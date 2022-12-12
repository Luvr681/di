﻿namespace TagCloudContainer;

public class WordsReader
{
    private readonly Dictionary<string, Word> _words = new Dictionary<string, Word>();
    private IWordConfig _wordConfig = new WordConfig();
    private IMainFormConfig _mainFormConfig;
    
    public IEnumerable<Word> GetWordsFromFile(string fileName, bool needValidate, IMainFormConfig mainFormConfig)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("File name can not be null or empty");

        var filePath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, fileName);

        if (!File.Exists(filePath))
            throw new ApplicationException("File does not exists");

        _mainFormConfig = mainFormConfig;
        Read(filePath, needValidate);
        
        var wordsList = new List<Word>();
        foreach (var w in _words)
            wordsList.Add(w.Value);
        
        return _mainFormConfig.Random ? _wordConfig.ShuffleWords(wordsList)
            : wordsList.OrderByDescending(w => w.Weight);
    }

    private void Read(string filePath, bool needValidate)
    {
        if (!File.Exists(filePath))
            throw new ApplicationException("File not found!");

        var lines = File
            .ReadLines(filePath)
            .Distinct();
        lines = needValidate ? _wordConfig.Validate(lines) : lines;

        foreach (var word in lines)
            AddWord(word);
    }

    private void AddWord(string wordValue)
    {
        if (_words.ContainsKey(wordValue))
        {
            _words[wordValue].Weight++;
            return;
        }
        
        var word = new Word() { Value = wordValue, Weight = 1 };
        _words.Add(wordValue, word);
    }
}