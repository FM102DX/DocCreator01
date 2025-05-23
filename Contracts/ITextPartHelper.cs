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
        /// <summary>
        /// Creates a new TextPart with default settings
        /// </summary>
        /// <param name="project">The current project to get a unique name from</param>
        /// <returns>A new TextPart instance</returns>
        TextPart CreateTextPart(Project project);


        /// <summary>
        /// Moves a TextPart up in the collection
        /// </summary>
        /// <param name="textPart">TextPart to move</param>
        /// <param name="textParts">Collection containing the TextPart</param>
        /// <param name="viewModels">Collection of view models to update</param>
        /// <returns>True if moved successfully, false otherwise</returns>
        bool MoveTextPartUp(TextPart textPart, List<TextPart> textParts);

        /// <summary>
        /// Moves a TextPart down in the collection
        /// </summary>
        /// <param name="textPart">TextPart to move</param>
        /// <param name="textParts">Collection containing the TextPart</param>
        /// <param name="viewModels">Collection of view models to update</param>
        /// <returns>True if moved successfully, false otherwise</returns>
        bool MoveTextPartDown(TextPart textPart, List<TextPart> textParts);

        void RemoveTextPart(TextPart textPart, List<TextPart> textParts);
        
        /// <summary>
        /// Decreases the level of a TextPart (move left in hierarchy)
        /// </summary>
        /// <param name="textPart">TextPart to modify</param>
        /// <returns>True if level was decreased, false if already at minimum level</returns>
        bool DecreaseTextPartLevel(TextPart textPart);
        
        /// <summary>
        /// Increases the level of a TextPart (move right in hierarchy)
        /// </summary>
        /// <param name="textPart">TextPart to modify</param>
        /// <returns>True if level was increased, false if already at maximum level</returns>
        bool IncreaseTextPartLevel(TextPart textPart);
    }
}
