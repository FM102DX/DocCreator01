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
        // Constructor no longer needs repository
        public TextPartHelper()
        {
        }

        public TextPart CreateTextPart(Project project)
        {
            return new TextPart
            {
                Id = Guid.NewGuid(),
                Text = $"Tab {project.ProjectData.TextParts.Count + 1}",
                Name = project.GetNewTextPartName(),
                IncludeInDocument = true
            };
        }

        public bool MoveTextPartUp(TextPart textPart, List<TextPart> textParts)
        {
            int idx = textParts.IndexOf(textPart);
            if (idx > 0)
            {
                textParts.Move(idx, idx - 1);
                MessageBus.Current.SendMessage(new GeneratedFilesUpdatedMessage());
                return true;
            }
            return false;
        }

        public bool MoveTextPartDown(TextPart textPart, List<TextPart> textParts)
        {
            int idx = textParts.IndexOf(textPart);
            if (idx < textParts.Count - 1 && idx >= 0)
            {
                textParts.Move(idx, idx + 1);
                return true;
            }
            return false;
        }

        public void RemoveTextPart(TextPart textPart, List<TextPart> textParts)
        {
            textParts.Remove(textPart);
        }
        
        public bool DecreaseTextPartLevel(TextPart textPart)
        {
            if (textPart.Level > 1)
            {
                textPart.Level--;
                return true;
            }
            return false;
        }
        
        public bool IncreaseTextPartLevel(TextPart textPart)
        {
            if (textPart.Level < 5)
            {
                textPart.Level++;
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
    }
}
