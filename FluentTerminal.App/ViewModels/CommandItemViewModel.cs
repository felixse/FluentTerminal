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

        private List<string> _lastMatches;

        private bool _lastMatchesStartsBold;

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

            _commandWords = SimpleText.SplitWords().Select(w => w.ToLowerInvariant()).ToArray();

            _commandWordIndices = new int[_commandWords.Length];

            var idx = 0;

            for (var i = 0; i < _commandWords.Length; i++)
            {
                var word = _commandWords[i];

                idx = SimpleText.IndexOf(word, idx, StringComparison.OrdinalIgnoreCase);

                _commandWordIndices[i] = idx;

                idx += word.Length;
            }

            _lastMatches = new List<string> {SimpleText};

            CalculateMatchPrivate(null);

            ShowMatch(null, null);
        }

        #endregion Constructor

        #region Methods

        internal void CalculateMatch(string trimmedFilter, string[] lowercaseFilterWords)
        {
            if (!trimmedFilter.NullableEqualTo(_lastFilter))
            {
                try
                {
                    CalculateMatchPrivate(lowercaseFilterWords);

                    _lastFilter = trimmedFilter;
                }
                catch
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// The purpose of this method is to find matches between filter words (<paramref name="lowercaseFilterWords"/>),
        /// and the command words (<see cref="_commandWords"/>). I'm aware that the code is quite confusing, but it kinda
        /// has to be...
        /// </summary>
        private void CalculateMatchPrivate(string[] lowercaseFilterWords)
        {
            if (lowercaseFilterWords == null || lowercaseFilterWords.Length < 1)
            {
                _lastMatchesStartsBold = false;

                _lastMatches = new List<string> { SimpleText };

                return;
            }

            var matchedPairs = new List<Tuple<int, int, int, int>>();
            var multiOptionPairs = new Dictionary<int, List<Tuple<int, int, int, int>>>();

            for (var i = 0; i < lowercaseFilterWords.Length; i++)
            {
                var word = lowercaseFilterWords[i];

                Tuple<int, int, int, int> singleMatch = null;
                List<Tuple<int, int, int, int>> multiMatches = null;

                for (var j = 0; j < _commandWords.Length; j++)
                {
                    if (matchedPairs.Any(p => p.Item1 == j))
                    {
                        // The word is occupied
                        continue;
                    }

                    var idx = _commandWords[j].IndexOf(word, StringComparison.Ordinal);

                    if (idx < 0)
                    {
                        continue;
                    }

                    var match = Tuple.Create(j, i, idx, word.Length);

                    if (singleMatch == null)
                    {
                        singleMatch = match;
                    }
                    else if (multiMatches == null)
                    {
                        multiMatches = new List<Tuple<int, int, int, int>> {singleMatch, match};
                    }
                    else
                    {
                        multiMatches.Add(match);
                    }
                }

                if (singleMatch == null)
                {
                    _lastMatchesStartsBold = false;
                    _lastMatches = null;

                    return;
                }

                if (multiMatches == null)
                {
                    matchedPairs.Add(singleMatch);

                    var toRemove = multiOptionPairs.Where(kvp =>
                        kvp.Value.RemoveAll(p => p.Item1 == singleMatch.Item1) > 0 && kvp.Value.Count == 1).ToList();

                    foreach (var remove in toRemove)
                    {
                        var newSingle = remove.Value[0];

                        if (matchedPairs.Any(p => p.Item1 == newSingle.Item1))
                        {
                            _lastMatchesStartsBold = false;
                            _lastMatches = null;

                            return;
                        }

                        matchedPairs.Add(newSingle);

                        multiOptionPairs.Remove(remove.Key);
                    }
                }
                else
                {
                    multiOptionPairs.Add(i, multiMatches);
                }
            }

            if (multiOptionPairs.Any())
            {
                var byCommandWord = multiOptionPairs.SelectMany(kvp => kvp.Value).GroupBy(p => p.Item1)
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Item2).ToList());

                var newSingles = byCommandWord.Where(kvp => kvp.Value.Count == 1).Select(kvp => kvp.Value[0]).ToList();

                while (newSingles.Count > 0)
                {
                    matchedPairs.AddRange(newSingles);

                    foreach (var single in newSingles)
                    {
                        byCommandWord.Remove(single.Item1);
                    }

                    var newFilterWords = newSingles.Select(p => p.Item2).ToHashSet();

                    newSingles.Clear();

                    foreach (var kvp in byCommandWord)
                    {
                        var unsolvedWords = kvp.Value.Where(p => !newFilterWords.Contains(p.Item2)).ToList();

                        if (unsolvedWords.Count < 2)
                        {
                            newSingles.Add(unsolvedWords.FirstOrDefault() ?? kvp.Value.First());
                        }
                    }
                }

                //TODO: Finish this!!!
            }

            var matches = new List<string>();

            var next = 0;

            var lastAddedBold = false;

            foreach (var match in matchedPairs.OrderBy(p => p.Item1))
            {
                var start = _commandWordIndices[match.Item1] + match.Item3;

                if (next == 0)
                {
                    _lastMatchesStartsBold = start == 0;
                }

                if (start > next)
                {
                    var regular = SimpleText.Substring(next, start - next);

                    if (string.IsNullOrWhiteSpace(regular))
                    {
                        matches[matches.Count - 1] += regular;
                    }
                    else
                    {
                        matches.Add(regular);

                        lastAddedBold = false;
                    }
                }

                var bold = SimpleText.Substring(start, match.Item4);

                if (lastAddedBold)
                {
                    matches[matches.Count - 1] += bold;
                }
                else
                {
                    matches.Add(bold);

                    lastAddedBold = true;
                }

                next = start + match.Item4;
            }

            if (next < SimpleText.Length)
            {
                matches.Add(SimpleText.Substring(next));
            }

            _lastMatches = matches;
        }

        public void ShowMatch(string trimmedLowercaseFilter, string[] lowercaseFilterWords)
        {
            CalculateMatch(trimmedLowercaseFilter, lowercaseFilterWords);

            if (!(_lastMatches?.Any() ?? false))
            {
                RichTextBlock = null;
            }
            else
            {
                var rtb = new RichTextBlock();

                rtb.Blocks.Add(GetBlock());

                RichTextBlock = rtb;
            }
        }

        private Block GetBlock()
        {
            var p = new Paragraph();

            var isBold = _lastMatchesStartsBold;

            foreach (var part in _lastMatches)
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

        public override string ToString()
        {
            return ExecutedCommand.Value;
        }

        #endregion Methods
    }
}