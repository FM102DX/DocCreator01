using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DocCreator01.Contracts;
using DocCreator01.Messages;
using DocCreator01.Models;
using ReactiveUI;

namespace DocCreator01.Services
{
    public class TextPartHelper : ITextPartHelper
    {
        private Project _project;
        public TextPartHelper(Project project)
        {
            _project = project;
        }
        public List<TextPart> TextParts=> _project.ProjectData.TextParts;
        public TextPart CreateTextPart()
        {
            return new TextPart
            {
                Id = Guid.NewGuid(),
                Text = $"Tab {_project.ProjectData.TextParts.Count + 1}",
                Name = _project.GetNewTextPartName(),
                IncludeInDocument = true
            };
        }

        public bool MoveTextPartUp(TextPart textPart)
        {
            int idx = TextParts.IndexOf(textPart);
            if (idx > 0)
            {
                TextParts.Move(idx, idx - 1);
                MessageBus.Current.SendMessage(new TextPartCollectionChangedMessage());
                return true;
            }
            return false;
        }

        public bool MoveTextPartDown(TextPart textPart)
        {
            int idx = TextParts.IndexOf(textPart);
            if (idx < TextParts.Count - 1 && idx >= 0)
            {
                TextParts.Move(idx, idx + 1);
                MessageBus.Current.SendMessage(new TextPartCollectionChangedMessage());
                return true;
            }
            return false;
        }

        public void RemoveTextPart(TextPart textPart)
        {
            TextParts.Remove(textPart);
            MessageBus.Current.SendMessage(new TextPartCollectionChangedMessage());
        }
        
        public bool DecreaseTextPartLevel(TextPart textPart)
        {
            if (textPart.Level > 1)
            {
                textPart.Level--;
                MessageBus.Current.SendMessage(new TextPartCollectionChangedMessage());
                return true;
            }
            return false;
        }
        
        public bool IncreaseTextPartLevel(TextPart textPart)
        {
            if (textPart.Level < 5)
            {
                textPart.Level++;
                MessageBus.Current.SendMessage(new TextPartCollectionChangedMessage());
                return true;
            }
            return false;
        }

        /* ---------- shared chunk helpers ---------- */
        public static bool IsChunkEmpty(TextPartChunk? chunk) =>
            chunk == null ||
            (string.IsNullOrWhiteSpace(chunk.Text) &&
             (chunk.ImageData == null || chunk.ImageData.Length == 0));

        public static TextPartChunk? AddEmptyChunk(TextPart? textPart)
        {
            if (textPart == null) return null;

            textPart.TextPartChunks ??= new List<TextPartChunk>();

            var newChunk = new TextPartChunk
            {
                Id   = Guid.NewGuid(),
                Text = string.Empty
            };
            textPart.TextPartChunks.Add(newChunk);
            return newChunk;
        }

        public static TextPartChunk? AddEmptyChunkIfNeeded(TextPart? textPart)
        {
            if (textPart == null) return null;

            textPart.TextPartChunks ??= new List<TextPartChunk>();

            if (textPart.TextPartChunks.Count == 0 ||
                !IsChunkEmpty(textPart.TextPartChunks.Last()))
            {
                return AddEmptyChunk(textPart);
            }

            return null;                    // nothing added
        }

        public void RefreshTextPartData()
        {
            NumerationHelper.ApplyNumeration(TextParts);
        }
    }
}
