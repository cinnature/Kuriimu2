﻿using System;
using System.Collections.Generic;
using System.IO;
using Kompression.LempelZiv.Matcher.Models;

namespace Kompression.LempelZiv.Matcher
{
    public class SuffixTreeMatcher : ILzMatcher
    {
        private readonly SuffixTree _tree;
        private readonly MatchType _matchType;

        public int MinMatchSize { get; }
        public int MaxMatchSize { get; }

        public SuffixTreeMatcher(MatchType matchType, int minMatchSize, int maxMatchSize)
        {
            _tree = new SuffixTree();
            _matchType = matchType;

            MinMatchSize = minMatchSize;
            MaxMatchSize = maxMatchSize;
        }

        public LzMatch[] FindMatches(Stream input)
        {
            var inputArray = ToArray(input);

            _tree.Build(inputArray, (int)input.Position);

            switch (_matchType)
            {
                case MatchType.Greedy:
                    return FindGreedyMatches(inputArray, (int)input.Position);
                case MatchType.OptimalParser:
                    return FindOptimalMatches(inputArray, (int)input.Position);
                default:
                    throw new InvalidOperationException($"Match type {_matchType} not supported.");
            }
        }

        private LzMatch[] FindGreedyMatches(byte[] input, int position)
        {
            var results = new List<LzMatch>();

            for (var i = Math.Max(position, 1); i < input.Length; i++)
            {
                var displacement = 0;
                var length = 0;
                _tree.FindLongestMatch(input, i, ref displacement, ref length);
                if (displacement > 0 && length > 0)
                {
                    results.Add(new LzMatch(i, displacement, length));
                    i += length - 1;
                }
            }

            return results.ToArray();
        }

        // TODO: Implement OptimalParser
        private LzMatch[] FindOptimalMatches(byte[] input, int position)
        {
            return null;
        }

        private byte[] ToArray(Stream input)
        {
            var bkPos = input.Position;
            var inputArray = new byte[input.Length];
            input.Read(inputArray, 0, inputArray.Length);
            input.Position = bkPos;

            return inputArray;
        }

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
                _tree.Dispose();
        }

        #endregion
    }
}