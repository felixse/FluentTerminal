using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels
{
    public class CommandItemViewModel : ViewModelBase
    {
        #region Fields

        private readonly string[] _commandWords;

        private readonly int[] _commandWordIndices;

        private string _lastFilter;

        #endregion Fields

        #region Properties

        public ExecutedCommand ExecutedCommand { get; }

        public string SimpleText { get; }

        private bool _isMatch = true;

        public bool IsMatch
        {
            get => _isMatch;
            private set => Set(ref _isMatch, value);
        }

        private RichTextBlock _richTextBlock;

        public RichTextBlock RichTextBlock
        {
            get => _richTextBlock;
            private set
            {
                if (Set(ref _richTextBlock, value))
                {
                    IsMatch = value != null;
                }
            }
        }

        #endregion Properties

        #region Constructor

        public CommandItemViewModel(ExecutedCommand command)
        {
            if (string.IsNullOrEmpty(command.Value))
            {
                throw new ArgumentException($"{nameof(ExecutedCommand.Value)} property is null or empty.",
                    nameof(command));
            }

            ExecutedCommand = command;

            SimpleText = command.Value.Trim();

            _commandWords = SimpleText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            _commandWordIndices = new int[_commandWords.Length];

            var idx = 0;

            for (var i = 0; i < _commandWords.Length; i++)
            {
                var word = _commandWords[i];

                idx = SimpleText.IndexOf(word, idx, StringComparison.OrdinalIgnoreCase);

                _commandWordIndices[i] = idx;

                idx += word.Length;
            }

            CreateRichTextBlock(new List<string>{SimpleText}, false);
        }

        #endregion Constructor

        #region Methods

        private void CreateRichTextBlock(List<string> commandParts, bool firstIsBold)
        {
            var rtb = new RichTextBlock();

            rtb.Blocks.Add(GetBlock(commandParts, firstIsBold));

            RichTextBlock = rtb;
        }

        private Block GetBlock(List<string> commandParts, bool firstIsBold)
        {
            var emptyPart = commandParts.FirstOrDefault(string.IsNullOrWhiteSpace);

            while (emptyPart != null)
            {
                int idx = commandParts.IndexOf(emptyPart);

                commandParts.RemoveAt(idx);

                if (idx > 0)
                {
                    while (idx < commandParts.Count)
                    {
                        var part = commandParts[idx];

                        commandParts.RemoveAt(idx);

                        emptyPart += part;

                        if (!string.IsNullOrWhiteSpace(part))
                        {
                            commandParts[idx - 1] += emptyPart;

                            break;
                        }
                    }
                }

                emptyPart = commandParts.FirstOrDefault(string.IsNullOrWhiteSpace);
            }

            var p = new Paragraph();

            var isBold = firstIsBold;

            foreach (var part in commandParts)
            {
                var run = new Run { Text = part };

                if (isBold)
                {
                    var bold = new Bold();

                    bold.Inlines.Add(run);

                    p.Inlines.Add(bold);
                }
                else
                {
                    p.Inlines.Add(run);
                }

                isBold = !isBold;
            }

            return p;
        }

        public void SetFilter(string filter, string[] filterWords)
        {
            if (filter.NullableEqualTo(_lastFilter))
            {
                // The same filter as the last one, so nothing to do.
                return;
            }

            _lastFilter = filter;

            if (string.IsNullOrEmpty(filter))
            {
                CreateRichTextBlock(new List<string> { SimpleText }, false);

                return;
            }

            var idx = SimpleText.IndexOf(filter, StringComparison.OrdinalIgnoreCase);

            if (idx >= 0)
            {
                List<string> commandParts = idx == 0
                    ? new List<string> {SimpleText.Substring(idx, filter.Length)}
                    : new List<string> {SimpleText.Substring(0, idx), SimpleText.Substring(idx, filter.Length)};

                if (idx + filter.Length < SimpleText.Length)
                {
                    commandParts.Add(SimpleText.Substring(idx + filter.Length));
                }

                CreateRichTextBlock(commandParts, idx == 0);

                return;
            }

            if (filterWords.Any(w => !SimpleText.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                // Word not found
                RichTextBlock = null;

                return;
            }

            var multiOptionPairs = new Dictionary<int, Dictionary<int, int>>();
            var finalPairs = new Dictionary<int, Tuple<int, int>>();

            for (int i = 0; i < filterWords.Length; i++)
            {
                var filterWord = filterWords[i];

                int matchingWord = -1;
                int matchingIndex = -1;

                Dictionary<int, int> matchingWords = null;

                for (int j = 0; j < _commandWords.Length; j++)
                {
                    if (finalPairs.ContainsKey(j))
                    {
                        // Word already used

                        continue;
                    }

                    idx = _commandWords[j].IndexOf(filterWord, StringComparison.OrdinalIgnoreCase);

                    if (idx < 0)
                        continue;

                    if (matchingWord < 0)
                    {
                        matchingWord = j;
                        matchingIndex = idx + _commandWordIndices[i];
                    }
                    else if (matchingWords == null)
                    {
                        matchingWords = new Dictionary<int, int>
                            {{matchingWord, matchingIndex}, {j, idx + _commandWordIndices[i]}};
                    }
                    else
                    {
                        matchingWords.Add(j, idx + _commandWordIndices[i]);
                    }
                }

                if (matchingWord < 0)
                {
                    // No matching word
                    RichTextBlock = null;

                    return;
                }

                if (matchingWords == null)
                {
                    if (finalPairs.ContainsKey(matchingWord))
                    {
                        // Word already used
                        RichTextBlock = null;

                        return;
                    }

                    finalPairs.Add(matchingWord, Tuple.Create(i, matchingIndex));

                    var newFinalPairs = multiOptionPairs
                        .Where(kvp => kvp.Value.Remove(matchingWord) && kvp.Value.Count < 2).ToList();

                    foreach (var newFinalPair in newFinalPairs)
                    {
                        if (finalPairs.ContainsKey(newFinalPair.Value.Keys.First()))
                        {
                            // Word already used
                            RichTextBlock = null;

                            return;
                        }

                        var kvp = newFinalPair.Value.First();

                        finalPairs.Add(kvp.Key, Tuple.Create(newFinalPair.Key, kvp.Value));

                        multiOptionPairs.Remove(newFinalPair.Key);
                    }
                }
                else
                {
                    multiOptionPairs.Add(i, matchingWords);
                }
            }

            if (multiOptionPairs.Any())
            {
                //TODO: Finish this!!!
            }

            {
                List<string> commandParts = new List<string>();

                var firstIsBold = false;

                var processedTo = 0;

                foreach (var kvp in finalPairs.OrderBy(p => p.Key))
                {
                    var start = kvp.Value.Item2;
                    var length = filterWords[kvp.Value.Item1].Length;

                    if (start == 0)
                    {
                        firstIsBold = true;
                    }
                    else if (processedTo < start)
                    {
                        commandParts.Add(SimpleText.Substring(processedTo, start - processedTo));
                    }

                    commandParts.Add(SimpleText.Substring(start, length));

                    processedTo = start + length;
                }

                CreateRichTextBlock(commandParts, firstIsBold);
            }
        }

        #endregion Methods
    }
}