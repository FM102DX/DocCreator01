using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DocCreator01.Models;
using DocCreator01.ViewModels;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Helper interface for operations related to TextPart objects.
    /// </summary>
    public interface ITextPartHelper
    {
        TextPart CreateTextPart();

        List<TextPart> TextParts { get; }
        bool MoveTextPartUp(TextPart textPart);

        bool MoveTextPartDown(TextPart textPart);

        void RemoveTextPart(TextPart textPart);
        
        bool DecreaseTextPartLevel(TextPart textPart);

        bool IncreaseTextPartLevel(TextPart textPart);
        void RefreshTextPartData();
    }
}
